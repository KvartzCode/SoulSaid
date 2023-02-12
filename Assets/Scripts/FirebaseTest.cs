using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using DBTables;


public class FirebaseTest : MonoBehaviour
{
    [SerializeField, ReadOnly]
    DBAPI db;

    public MessageLocation message = new MessageLocation();
    public LocationInfo lastKnownLocation;


    private void Start()
    {
        db = GetComponent<DBAPI>();
        LocationHandler.Instance.onLocationChanged += OnLocationUpdated;
    }

    private void Update()
    {
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            Debug.Log("pressed F");
            db.SaveMessage("Messages", message);
        }
    }


    public void OnLocationUpdated(LocationInfo info)
    {
        lastKnownLocation = info;
        message.Latitude = info.latitude;
        message.Longitude = info.longitude;
    }

    private void OnDestroy()
    {
        LocationHandler.Instance.onLocationChanged -= OnLocationUpdated;
    }
}

//public class FirebaseTest : MonoBehaviour
//{
//    FirebaseAuth auth;
//    FirebaseDatabase db;

//    MessageLocation savePosition;


//    void Start()
//    {
//        db = FirebaseDatabase.DefaultInstance;
//        savePosition = new MessageLocation();

//        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
//        {
//            if (task.Exception != null)
//                Debug.LogError(task.Exception);

//            auth = FirebaseAuth.DefaultInstance;

//            if (auth.CurrentUser == null)
//                AnonymousSignIn();
//        });
//    }

//    private void Update()
//    {
//        if (Input.GetKeyDown(KeyCode.Alpha1))
//        {
//            savePosition.pos = transform.position;
//            DataTest(auth.CurrentUser.UserId, JsonUtility.ToJson(savePosition));
//        }
//        if (Input.GetKeyDown(KeyCode.Alpha2))
//        {
//            LoadFromFirebase();
//        }

//        //if (Input.GetKeyDown(KeyCode.D))
//        //    DataTest(auth.CurrentUser.UserId, UnityEngine.Random.Range(0, 100).ToString());
//    }

//    private void AnonymousSignIn()
//    {
//        auth.SignInAnonymouslyAsync().ContinueWithOnMainThread(task =>
//        {
//            if (task.Exception != null)
//            {
//                Debug.LogWarning(task.Exception);
//            }
//            else
//            {
//                FirebaseUser newUser = task.Result;
//                Debug.LogFormat("User signed in successfully: {0} ({1})",
//                    newUser.DisplayName, newUser.UserId);
//            }
//        });
//    }

//    private void DataTest(string userID, string data)
//    {
//        Debug.Log("Trying to write data...");

//        db.RootReference.Child("users").Child(userID).SetRawJsonValueAsync(data).ContinueWithOnMainThread(task =>
//        {
//            if (task.Exception != null)
//                Debug.LogWarning(task.Exception);
//            else
//                Debug.Log("DataTestWrite: Complete");
//        });
//    }

//    private void LoadFromFirebase()
//    {
//        var db = FirebaseDatabase.DefaultInstance;
//        var userId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
//        db.RootReference.Child("users").Child(userId).GetValueAsync().ContinueWithOnMainThread(task =>
//        {
//            if (task.Exception != null)
//            {
//                Debug.LogError(task.Exception);
//            }

//            //here we get the result from our database.
//            DataSnapshot snap = task.Result;

//            //And send the json data to a function that can update our game.
//            Debug.Log(snap.GetRawJsonValue());

//            savePosition = JsonUtility.FromJson<MessageLocation>(snap.GetRawJsonValue());
//            transform.position = savePosition.pos;
//        });
//    }
//}