using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DBTables;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using Newtonsoft.Json;
using UnityEngine;


public class DBAPI : MonoBehaviour
{
    public delegate void OnSaveDelegate(string resultMessage);

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


    public void SaveMessage(string path, string text, OnSaveDelegate onSaveDelegate = null)
    {
        LocationInfo location = LocationHandler.Instance.lastKnownLocation;
        var message = new MessageLocation(auth.CurrentUser.UserId, text, location, DateTime.Now);
        var msg = JsonConvert.SerializeObject(message);

        SaveData(path, msg, onSaveDelegate);
    }

    private void SaveData(string path, string data, OnSaveDelegate onSaveDelegate = null)
    {
        DatabaseReference dbRef = db.RootReference.Child(path).Push();
        dbRef.SetRawJsonValueAsync(data).ContinueWithOnMainThread(task =>
        {
            if (task.Exception != null)
                Debug.LogWarning(task.Exception);

            Debug.Log($"Data saved to '{path}'");
            path = $"{path}/{dbRef.Key}/DateCreated";
            SaveTimeStamp(path, onSaveDelegate);
        });
    }

    private void SaveTimeStamp(string path, OnSaveDelegate onSaveDelegate = null)
    {
        db.RootReference.Child(path).SetValueAsync(ServerValue.Timestamp).ContinueWithOnMainThread(task =>
        {
            if (task.Exception != null)
            {
                Debug.LogWarning(task.Exception);
                onSaveDelegate?.Invoke(task.Exception.Message);
            }
            else
            {
                Debug.Log($"Data saved to '{path}'");
                onSaveDelegate?.Invoke("Success = " + task.IsCompletedSuccessfully.ToString());
            }
        });
    }

    /// <summary>
    /// Note to self:<br/>
    /// "Don't use firebase Realtime Database for anything other than a highscore system. You will have a bad time.
    /// Just use a normal DB where you actually can query something!"
    /// </summary>
    public void TestLoad()
    {
        var db = FirebaseDatabase.DefaultInstance;

        db.RootReference.Child("Messages").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Exception != null)
            {
                Debug.LogError(task.Exception);
                return;
            }

            DataSnapshot snap = task.Result;
            var testtt = snap.GetRawJsonValue();
            Debug.Log(testtt);

            if (task.Exception != null)
                Debug.LogWarning(task.Exception);

            var ListOfT = new List<MessageLocation>();

            foreach (var item in task.Result.Children)
            {
                ListOfT.Add(JsonUtility.FromJson<MessageLocation>(item.GetRawJsonValue()));
            }

            var testttttt = snap.Value;
            //snap.ChildrenCount
            Debug.LogWarning(JsonUtility.ToJson(testttttt));

            //IEnumerable<MessageLocation> messages = snap.Children.Select(c =>
            //{
            //    var m = JsonConvert.DeserializeObject<MessageLocation>(c.GetRawJsonValue());
            //    return LocationHandler.Instance.lastKnownLocation.CalculateDistance(m.Latitude, m.Longitude) < 50 ?
            //        m : null;
            //});

            //foreach (var msg in messages)
            //{
            //    //dostuff
            //    LocationHandler.Instance.AddToStatusText("\n" + msg.Text);
            //    LocationHandler.Instance.AddToStatusText("\n" + msg.DateCreated.ToString());
            //}
            //foreach (var msg in messages.ToList())
            //{
            //    //dostuff
            //    LocationHandler.Instance.AddToStatusText("\n" + msg.Text);
            //    LocationHandler.Instance.AddToStatusText("\n" + msg.DateCreated.ToString());
            //}

            //var test = JsonConvert.DeserializeObject<MessageLocation>(snap.GetRawJsonValue());
            //var testss = JsonConvert.DeserializeObject<MessageLocation>(snap.GetRawJsonValue());
            ////var tests = testss.ToList();

            //foreach (var test in testss)
            //{
            //    LocationHandler.Instance.AddToStatusText("\n"+test.Text);
            //    LocationHandler.Instance.AddToStatusText("\n"+test.DateCreated.ToString());
            //}

            //foreach (var test in tests)
            //{
            //    LocationHandler.Instance.AddToStatusText("\n" + test.Text);
            //    LocationHandler.Instance.AddToStatusText("\n" + test.DateCreated.ToString());
            //}
        });
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
