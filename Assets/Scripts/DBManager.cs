using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Database;
using UnityEngine.UI;
using System.Security.Cryptography;
using System;
using System.Text;
using TMPro;

public class DBManager : MonoBehaviour
{
    public DatabaseReference userRef;
    public TMP_InputField userNameInput, passwordInput, ageInput;
    public Toggle maleTogle, famaleTogle;

    public TextMeshProUGUI confirmText;
    public TextMeshProUGUI warningText;


    private void Awake()
    {
        Firebase.FirebaseApp.Create();
    }
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Initilization());

    }


    private IEnumerator Initilization()
    {
        confirmText.text = "Init Baslangic";
        var task = FirebaseApp.CheckAndFixDependenciesAsync();
        confirmText.text = "After Task";
        while (!task.IsCompleted)
        {
            yield return null;
        }
        confirmText.text = "After While";
        if (task.IsCanceled || task.IsFaulted)
        {
            Debug.LogError("Databas Error: " + task.Exception);
        }


        var dependencyStatus = task.Result;

        if (dependencyStatus == DependencyStatus.Available)
        {
            userRef = FirebaseDatabase.DefaultInstance.GetReference("Users");
            Debug.Log("init complated");
            confirmText.text = "init complated";
        }
        else
        {
            Debug.LogError("Database Error: ");
            confirmText.text = "Database Error: ";
        }
    }



    public void SaveUser()
    {
        string username = userNameInput.text;
        string password = passwordInput.text;
        string hashPassword;
        string age = ageInput.text;
        string gender;

        if (maleTogle.isOn)
        {
            gender = "Male";
        }
        else
        {
            gender = "Female";
        }


        Debug.Log(EncodePassword(password, PasswordSalt));
        hashPassword = EncodePassword(password, PasswordSalt);

        Dictionary<string, object> user = new Dictionary<string, object>();
        user["username"] = username;
        user["password"] = password;
        user["hashPassword"] = hashPassword;
        user["age"] = age;
        user["gender"] = gender;
        confirmText.text = "AFTER DICTIONARY ";

        string key = userRef.Push().Key;

        userRef.Child(key).UpdateChildrenAsync(user);
    }

    public void GetData()
    {
        StartCoroutine(GetUserData());
    }


    public IEnumerator GetUserData()
    {
        string name = userNameInput.text;

        var task = userRef.Child(name).GetValueAsync();
        while (!task.IsCompleted)
        {
            yield return null;
        }

        if (task.IsCanceled || task.IsFaulted)
        {
            Debug.LogError("Databas Error: " + task.Exception);
            yield break;
        }

        DataSnapshot snapshot = task.Result;

        foreach (DataSnapshot user in snapshot.Children)
        {
            if (user.Key == "password")
            {
                Debug.Log("Password: " + user.Value.ToString());

            }

            if (user.Key == "username")
            {
                Debug.Log("Username: " + user.Value.ToString());

            }
        }
    }

    #region HADING AND SALTING
    private string PasswordSalt
    {
        get
        {
            var rng = new RNGCryptoServiceProvider();
            var buff = new byte[32];
            rng.GetBytes(buff);
            return Convert.ToBase64String(buff);
        }
    }

    private string EncodePassword(string password, string salt)
    {
        byte[] bytes = Encoding.Unicode.GetBytes(password);
        byte[] src = Encoding.Unicode.GetBytes(salt);
        byte[] dst = new byte[src.Length + bytes.Length];
        Buffer.BlockCopy(src, 0, dst, 0, src.Length);
        Buffer.BlockCopy(bytes, 0, dst, src.Length, bytes.Length);
        HashAlgorithm algorithm = HashAlgorithm.Create("SHA1");
        byte[] inarray = algorithm.ComputeHash(dst);
        return Convert.ToBase64String(inarray);
    }
    #endregion
}
