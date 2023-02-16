using System.Collections;
using System.Collections.Generic;
using DBTables;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Android;
using URandom = UnityEngine.Random;


public enum LogLevel
{
    Info,
    Warning,
    Error,
    WriteInBuild
}

public class LocationHandler : MonoBehaviour
{
    public static LocationHandler Instance { get; private set; }
    public InstanceState State { get; private set; }

    public delegate void OnLocationChanged(LocationInfo locationInfo);
    public OnLocationChanged onLocationChanged;

    [SerializeField, Tooltip("Seconds before checking current location")]
    float refreshRate = 10;
    float timer = 0;

    [SerializeField] GameObject spawnableObjectPrefab;
    [SerializeField] List<MessageLocation> locations = new List<MessageLocation>(); // PLACEHOLDER FOR FIREBASE!
    //[SerializeField] bool locationFeedStarted = false;
    [SerializeField] TextMeshProUGUI statusText;

    public LocationInfo lastKnownLocation;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        State = InstanceState.Initializing;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        StartCoroutine(CheckPermissions());
    }

    private void Update()
    {
        if (timer > refreshRate)
        {
            if (State == InstanceState.Running && !lastKnownLocation.CompareLocationInfo(Input.location.lastData))
            {
                lastKnownLocation = Input.location.lastData;

                onLocationChanged?.Invoke(lastKnownLocation);
            }
            timer = 0;
        }
        timer += Time.deltaTime;
    }

    //public List<MessageLocation> GetNearbyLocations(float maxDistanceInMetres)
    //{
    //    var locations = this.locations; // change this to a firebase call ASAP!
    //    var nearbyLocations = new List<MessageLocation>();

    //    foreach (var location in locations)
    //    {
    //        var distance = lastKnownLocation.CalculateDistance(location.Latitude, location.Longitude);

    //        if (distance < maxDistanceInMetres)
    //            nearbyLocations.Add(location);
    //    }

    //    return nearbyLocations;
    //}

    public void AddToStatusText(string text, LogLevel logLevel = LogLevel.Info)
    {
#if UNITY_EDITOR
        if (statusText != null)
            statusText.text += $"[[{text}]]; ";
#elif PLATFORM_ANDROID
        if (statusText != null && logLevel == LogLevel.WriteInBuild)
            statusText.text += $"[[{text}]]; ";
#endif

        if (logLevel == LogLevel.Error)
            Debug.LogError(text);
        else if (logLevel == LogLevel.Warning)
            Debug.LogWarning(text);
        else
            Debug.Log(text);
    }

    //public float CalculateDistance(LocationInfo locationInfo)
    //{
    //    return CalculateDistance(locationInfo.latitude, locationInfo.longitude);
    //}

    ///// <summary>
    ///// Calculates the distance of 2 real world coordinates in metres.
    ///// </summary>
    ///// <returns>The distance in metres</returns>
    //public float CalculateDistance(float latitude, float longitude)
    //{
    //    float currentLat = lastKnownLocation.latitude;
    //    float currentLon = lastKnownLocation.longitude;

    //    float R = 6371000; // metres
    //    float omega1 = ((currentLat / 180) * Mathf.PI);
    //    float omega2 = ((latitude / 180) * Mathf.PI);
    //    float variacionomega1 = (((latitude - currentLat) / 180) * Mathf.PI);
    //    float variacionomega2 = (((longitude - currentLon) / 180) * Mathf.PI);
    //    float a = Mathf.Sin(variacionomega1 / 2) * Mathf.Sin(variacionomega1 / 2) +
    //                Mathf.Cos(omega1) * Mathf.Cos(omega2) *
    //                Mathf.Sin(variacionomega2 / 2) * Mathf.Sin(variacionomega2 / 2);
    //    float c = 2 * Mathf.Asin(Mathf.Sqrt(a));

    //    float d = R * c;

    //    AddToStatusText($"distance is: '{d}'");
    //    return d;
    //}

    //public void PlaceObjectNearby()
    //{
    //    Vector3 spawnPos = new Vector3();
    //    spawnPos.x = URandom.Range(transform.position.x - 10, transform.position.x + 10);
    //    spawnPos.y = transform.position.y;
    //    spawnPos.z = URandom.Range(transform.position.z + 5, transform.position.z + 10);
    //    var newLocationObject = Instantiate(spawnableObjectPrefab, spawnPos, transform.rotation);
    //    AddToStatusText($"Spawned new object at: '{newLocationObject.transform.position}'");
    //}

    IEnumerator CheckPermissions()
    {
#if PLATFORM_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            var callbacks = new PermissionCallbacks();
            callbacks.PermissionGranted += PermissionsGranted;
            Permission.RequestUserPermission(Permission.FineLocation, callbacks);
            yield break;
        }
#endif

        StartCoroutine(InitializeGPSService());
    }

    private void PermissionsGranted(string obj)
    {
        AddToStatusText($"Permission Granted: '{obj}'");
        StartCoroutine(InitializeGPSService());
    }

    IEnumerator InitializeGPSService() // Call CheckPermissions first! This won't work otherwise.
    {
        yield return new WaitForSecondsRealtime(5);

        if (!Input.location.isEnabledByUser)
        {
            AddToStatusText("GPS Not enabled by user");
            yield break;
        }

        Input.location.Start(5f, 0.5f);

        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            AddToStatusText("Initializing...");
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        if (maxWait < 1)
        {
            AddToStatusText("Service initialization time out");
            yield break;
        }

        if (Input.location.status == LocationServiceStatus.Failed)
        {
            AddToStatusText("Unable to determine device location");
            State = InstanceState.Stopped;
            GameManager.Instance.HandlerOrManagerStateChanged();
            yield break;
        }
        else //access granted
        {
            State = InstanceState.Running;
            GameManager.Instance.HandlerOrManagerStateChanged();
        }
    }

//    private IEnumerator StartLocationService(float desiredAccuracyInMeters = 10, float updateDistanceInMeters = 10)
//    {
//        AddToStatusText("Start of 'StartLocationService' method.");

//#if UNITY_EDITOR // Wait for Unity Remote to connect
//        yield return new WaitForSeconds(5);
//#endif
//#if PLATFORM_ANDROID
//        if (!Permission.HasUserAuthorizedPermission(Permission.CoarseLocation)) {
//            Permission.RequestUserPermission(Permission.CoarseLocation);
//        }

//        // First, check if user has location service enabled
//        if (!Input.location.isEnabledByUser) {
//            // TODO Failure
//            //Debug.LogFormat("Android and Location not enabled");
//            AddToStatusText("Android and Location not enabled");
//            yield break;
//        }
//#endif
//        yield return new WaitForSeconds(5);
//        if (Input.location.status == LocationServiceStatus.Running)
//            AddToStatusText("LocationServiceStatus is already running?");
//        if (!Permission.HasUserAuthorizedPermission(Permission.CoarseLocation))
//        {
//            Permission.RequestUserPermission(Permission.CoarseLocation);
//        }
//        if (!Input.location.isEnabledByUser) // Is location service enabled?
//        {
//            AddToStatusText("No locations enabled in the device", LogLevel.Warning);
//            yield break;
//        }
//        Input.location.Stop();
//        yield return new WaitForSeconds(10);
//        // Start service before querying location
//        Input.location.Start();

//#if UNITY_EDITOR
//        yield return new WaitForSeconds(5);
//#endif
//        yield return new WaitForSeconds(5);

//        int maxWait = 20; // Wait until service initializes
//        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
//        {
//            yield return new WaitForSeconds(1);
//            maxWait--;
//        }

//        if (maxWait < 1)
//        {
//            AddToStatusText("Service didn't initialize in 20 seconds");
//            yield break;
//        }

//        AddToStatusText($"Status = {Input.location.status}, starting again and wait for 15 seconds.");
//        maxWait = 15;

//        Input.location.Start(desiredAccuracyInMeters, updateDistanceInMeters);
//        AddToStatusText($"Status = {Input.location.status}");

//        while (Input.location.status == LocationServiceStatus.Stopped && maxWait > 0)
//        {
//            yield return new WaitForSecondsRealtime(1);
//            maxWait--;
//        }

//        if (maxWait < 1)
//        {
//            AddToStatusText("Service didn't initialize in 15 seconds");
//            yield break;
//        }

//        if (Input.location.status == LocationServiceStatus.Failed)
//        {
//            AddToStatusText("Unable to determine device location");
//            yield break;
//        }
//        else
//        {
//            float lat = Input.location.lastData.latitude;
//            float lon = Input.location.lastData.longitude;
//            float alt = Input.location.lastData.altitude;
//            float horAcc = Input.location.lastData.horizontalAccuracy;
//            float verAcc = Input.location.lastData.verticalAccuracy;
//            double timestamp = Input.location.lastData.timestamp;
//            Debug.Log($"latitude = '{lat}', longitude = '{lon}', altitude = '{alt}', horizontalAccuracy = '{horAcc}'," +
//                $" verticalAccuracy = '{verAcc}', timestamp = '{timestamp}', ");
//            AddToStatusText($"currentPos: latitude = '{lat}', longitude = '{lon}'");

//            locationFeedStarted = true;
//        }
//    }

    private void OnDestroy()
    {
        if (Input.location.status == LocationServiceStatus.Running)
            Input.location.Stop();

        StopAllCoroutines();
    }
}
