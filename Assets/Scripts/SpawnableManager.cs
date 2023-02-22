using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DBTables;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using URandom = UnityEngine.Random;


[Serializable]
public class CustomLocation
{
    public MessageLocation locationObject;
    public MessageWorldObject messageWorldObject;
    public bool isSpawned = false; //does nothing atm, may remove later.


    public CustomLocation(MessageLocation messageLocation, MessageWorldObject messageWorldObject, bool isSpawned = false)
    {
        locationObject = messageLocation;
        this.messageWorldObject = messageWorldObject;
        this.isSpawned = isSpawned;
    }
}

public class SpawnableManager : MonoBehaviour
{
    public static SpawnableManager Instance { get; private set; }
    public InstanceState State { get; private set; }

    [SerializeField, Tooltip("How many metres from the device to look for saved messages")]
    float detectionDistance = 50;
    [SerializeField, Tooltip("Distance in metres the device needs to travel before looking for new messages")]
    float updateDistance = 50;
    [SerializeField, Tooltip("How many new messages to spawn in close to device when conditions are met")]
    float maxNewInstancedMessages = 10;

    [SerializeField] Camera cam;

    [SerializeField] GameObject spawnablePrefab;
    [SerializeField] ARRaycastManager m_RaycastManager;
    List<ARRaycastHit> m_Hits = new();

    List<CustomLocation> locations = new();
    MessageWorldObject spawnedObject;
    LocationInfo lastLocationCheckpoint;

    //public MessageLocation message = new(); //only for testing purposes

    //[SerializeField, ReadOnly]
    //DBAPI db;
    FirebaseDatabase db;
    [SerializeField] TMP_InputField inputField;
    [SerializeField] TMP_Text statusText;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        State = InstanceState.Initializing;
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        if (cam == null)
            cam = Camera.main;

        db = FirebaseDatabase.DefaultInstance;

        spawnedObject = null;
        LocationHandler.Instance.onLocationChanged += OnLocationChanged;
        StartCoroutine(WaitForFirstVerifiedLocation());
    }

    void Update()
    {
        //ExampleSpawnMethod();

        //if (Input.GetKeyDown(KeyCode.F) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
        //    GetMessagesFromDB();
        //db.SaveMessage("Messages", message);

        if (Input.touchCount > 0)
            CheckIfMessageWasSelected();
    }

    IEnumerator WaitForFirstVerifiedLocation()
    {
        //yield return new WaitForSecondsRealtime(5);
        int tries = 60;
        while (LocationHandler.Instance.lastKnownLocation.IsZero() && tries-- > 0)
        {
            yield return new WaitForSecondsRealtime(1);
        }

        if (tries > 0)
            State = InstanceState.Running;
        else
            State = InstanceState.Stopped;

        Debug.LogWarning($"SpawnableManager state = [{State}]");
        GameManager.Instance.HandlerOrManagerStateChanged();
    }


    void OnLocationChanged(LocationInfo info)
    {
        if (GameManager.Instance.State != InstanceState.Running)
            return;

        var distance = info.CalculateDistance(lastLocationCheckpoint);
        LocationHandler.Instance.AddToStatusText($"distance is: '{distance}'");

        if (distance < updateDistance)
            return;
        else
            lastLocationCheckpoint = info;

        LocationHandler.Instance.AddToStatusText("Location changed: " + JsonConvert.SerializeObject(info), LogLevel.Info);
        GetMessagesFromDB();
    }


    void CheckIfMessageWasSelected()
    {
        if (spawnedObject == null && Input.touchCount == 0 && Input.GetTouch(0).phase != TouchPhase.Began)
            return;

        Ray ray = cam.ScreenPointToRay(Input.GetTouch(0).position);

        if (m_RaycastManager.Raycast(Input.GetTouch(0).position, m_Hits))
        {
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.gameObject.CompareTag("Spawnable"))
                {
                    spawnedObject = hit.collider.GetComponent<MessageWorldObject>();
                    spawnedObject.DisplayText();
                }
                else
                {
                    spawnedObject.HideText();
                    spawnedObject = null;
                }
            }
        }
    }

    GameObject SpawnNearby()
    {
        float randomDistance = URandom.Range(5f, 10f);
        Vector3 randomPos = cam.transform.position.GetRandomPointOnHorizontalCircle(randomDistance);

        bool rayHit = Physics.Raycast(randomPos, Vector3.down, out RaycastHit hit);
        randomPos = rayHit ? hit.point : randomPos;

        GameObject newObject = Instantiate(spawnablePrefab, randomPos, Quaternion.identity);
        return newObject;
    }

    //void ExampleSpawnMethod()
    //{
    //    if (Input.touchCount == 0)
    //        return;

    //    RaycastHit hit;
    //    Ray ray = cam.ScreenPointToRay(Input.GetTouch(0).position);

    //    if (m_RaycastManager.Raycast(Input.GetTouch(0).position, m_Hits))
    //    {
    //        if (Input.GetTouch(0).phase == TouchPhase.Began && spawnedObject == null)
    //        {
    //            if (Physics.Raycast(ray, out hit))
    //            {
    //                if (hit.collider.gameObject.CompareTag("Spawnable"))
    //                {
    //                    spawnedObject = hit.collider.gameObject;
    //                }
    //                else
    //                {
    //                    //SpawnPrefab(m_Hits[0].pose.position);
    //                }
    //            }

    //        }
    //        else if (Input.GetTouch(0).phase == TouchPhase.Moved && spawnedObject != null)
    //        {
    //            spawnedObject.transform.position = m_Hits[0].pose.position;
    //        }
    //        if (Input.GetTouch(0).phase == TouchPhase.Ended)
    //        {
    //            spawnedObject = null;
    //        }
    //    }
    //}


    #region DB methods

    public void GetMessagesFromDB()
    {
        if (GameManager.Instance.IsEditModeActive)
            return;

        db.RootReference.Child("Messages").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Exception != null)
            {
                Debug.LogError(task.Exception);
                return;
            }

            DataSnapshot snap = task.Result;

            if (task.Exception != null)
                Debug.LogWarning(task.Exception);

            var nearbyLocations = new List<MessageLocation>();

            foreach (var item in task.Result.Children)
            {
                MessageLocation message = JsonUtility.FromJson<MessageLocation>(item.GetRawJsonValue());

                if (lastLocationCheckpoint.CalculateDistance(message.Latitude, message.Longitude) > detectionDistance)
                {
                    nearbyLocations.Add(message);

                    if (nearbyLocations.Count < maxNewInstancedMessages)
                        break;
                }
            }

            Debug.LogWarning(JsonConvert.SerializeObject(nearbyLocations));

            foreach (var location in nearbyLocations)
            {
                if (locations.Any(l => l.locationObject == location))
                    continue;

                var newLocation = SpawnNearby();
                if (newLocation != null)
                {
                    var worldObject = newLocation.GetComponent<MessageWorldObject>();
                    Debug.LogError("Spawnable object have no MessageWorldObject component!");
                    locations.Add(new CustomLocation(location, worldObject, true));
                }
            }
        });
    }


    public void SaveMessageToDB()
    {
        if (!GameManager.Instance.IsEditModeActive)
        {
            Debug.LogWarning("SaveMessageToDB was called but IsEditModeActive is false!");
            return;
        }

        var path = "Messages";
        var auth = FirebaseAuth.DefaultInstance;
        LocationInfo location = LocationHandler.Instance.lastKnownLocation;

        var message = new MessageLocation(auth.CurrentUser.UserId, inputField.text, location);
        var msg = JsonConvert.SerializeObject(message);

        SaveData(path, msg);
    }

    private void SaveData(string path, string data)
    {
        DatabaseReference dbRef = db.RootReference.Child(path).Push();
        dbRef.SetRawJsonValueAsync(data).ContinueWithOnMainThread(task =>
        {
            if (task.Exception != null)
            {
                Debug.LogWarning(task.Exception);
                statusText.text = task.Exception.Message;
                return;
            }

            Debug.Log($"Data saved to '{path}'");
            path = $"{path}/{dbRef.Key}/DateCreated";
            SaveTimeStamp(path);
        });
    }

    private void SaveTimeStamp(string path)
    {
        db.RootReference.Child(path).SetValueAsync(ServerValue.Timestamp).ContinueWithOnMainThread(task =>
        {
            if (task.Exception != null)
            {
                Debug.LogWarning(task.Exception);
                statusText.text += $"\n{task.Exception.Message}";
            }
            else
            {
                Debug.Log($"Data saved to '{path}'");
                GameManager.Instance.ExitEditMode();
            }
        });
    }

    #endregion


    private void OnDestroy()
    {
        LocationHandler.Instance.onLocationChanged -= OnLocationChanged;
        StopAllCoroutines();
    }
}
