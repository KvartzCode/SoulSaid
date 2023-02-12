using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DBTables;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;


public class DBAPI : MonoBehaviour
{
    FirebaseAuth auth;
    FirebaseDatabase db;


    void Start()
    {
#if UNITY_EDITOR
        FirebaseDatabase.DefaultInstance.SetPersistenceEnabled(false);
#endif
        db = FirebaseDatabase.DefaultInstance;

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Exception != null)
                Debug.LogError(task.Exception);

            auth = FirebaseAuth.DefaultInstance;

            if (auth.CurrentUser == null)
                AnonymousSignIn();
        });
    }


    private void AnonymousSignIn()
    {
        auth.SignInAnonymouslyAsync().ContinueWithOnMainThread(task => {
            if (task.Exception != null)
            {
                Debug.LogWarning(task.Exception);
            }
            else
            {
                FirebaseUser newUser = task.Result;
                Debug.LogFormat("User signed in successfully: {0} ({1})", newUser.DisplayName, newUser.UserId);
            }
        });
    }


    #region Account Methods

    private void SignInFirebase(string email, string password)
    {
        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.Exception != null)
            {
                Debug.LogWarning(task.Exception);
            }
            else
            {
                FirebaseUser newUser = task.Result;
                Debug.LogFormat("User signed in successfully: {0} ({1})",
                  newUser.DisplayName, newUser.UserId);
                //status.text = newUser.Email + " is signed in.";
            }
        });
    }

    private void RegisterNewUser(string email, string password)
    {
        Debug.Log("Starting Registration");
        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.Exception != null)
            {
                Debug.LogWarning(task.Exception);
            }
            else
            {
                FirebaseUser newUser = task.Result;
                Debug.LogFormat("User Registered: {0} ({1})",
                  newUser.DisplayName, newUser.UserId);
            }
        });
    }

    #endregion


    private void SaveData(string path, string data)
    {
        db.RootReference.Child(path).Push().SetRawJsonValueAsync(data).ContinueWithOnMainThread(task =>
        {
            if (task.Exception != null)
                Debug.LogWarning(task.Exception);

            Debug.Log($"Data saved to '{path}'");
        });
    }

    public void SaveMessage(string path, MessageLocation message)
    {
        SaveData(path, JsonUtility.ToJson(message));
    }

    /// <summary>
    /// returns 
    /// </summary>
    /// <param name="info"></param>
    /// <param name="maxDistance"></param>
    public List<MessageLocation> GetMessages(LocationInfo info, float maxDistance)
    {
        var db = FirebaseDatabase.DefaultInstance;
        var userId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        List<MessageLocation> messages = new List<MessageLocation>();

        db.RootReference.Child("Messages").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Exception != null)
            {
                Debug.LogError(task.Exception);
            }

            //here we get the result from our database.
            DataSnapshot snap = task.Result;

            //And send the json data to a function that can update our game.
            Debug.Log(snap.GetRawJsonValue());

            //IEnumerable<MessageLocation> test = snap.Children.Select(c => JsonUtility.FromJson<MessageLocation>(c.GetRawJsonValue()));
            IEnumerable<MessageLocation> test1 = JsonUtility.FromJson<IEnumerable<MessageLocation>>(snap.GetRawJsonValue());
            test1 = test1.Where(t1 => info.CalculateDistance(t1.Latitude, t1.Longitude) < maxDistance).Take(100);

            messages = test1.ToList();
            //var savePosition = JsonUtility.FromJson<MessageLocation>(snap.GetRawJsonValue());
            //transform.position = savePosition.pos;
        });

        return messages;
    }
}
