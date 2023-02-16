using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Android;

//[ExecuteAlways]
public class GetLocation : MonoBehaviour
{
    [ReadOnly]
    public double latitude;
    [ReadOnly]
    public double longitude;

    private Coroutine routine;

    private void Start()
    {
        routine = StartCoroutine(CheckPermissions());
    }

    private void OnDestroy()
    {
        if (routine != null)
            StopCoroutine(routine);
    }

    IEnumerator CheckPermissions()
    {
#if PLATFORM_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            var callbacks = new PermissionCallbacks();
            callbacks.PermissionGranted += Callbacks_PermissionGranted;
            Permission.RequestUserPermission(Permission.FineLocation, callbacks);
            yield break;
        }
#endif

        StartCoroutine(InitializeGPSService());
        yield break;
    }

    private void Callbacks_PermissionGranted(string obj)
    {
        Debug.Log($"Permission Granted: '{obj}'");
        StartCoroutine(InitializeGPSService());
    }

    IEnumerator InitializeGPSService() // Call CheckPermissions first! This won't work otherwise.
    {
        yield return new WaitForSecondsRealtime(5);
        //is the location service available on the device or not
        if (!Input.location.isEnabledByUser)
        {
            Debug.LogWarning("GPS Not enabled by user");
            yield break;
        }
        //start the location service before querying location
        Input.location.Start(5f, 0.5f);

        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            Debug.Log("Initializing...");
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        if (maxWait < 1)
        {
            Debug.LogWarning("Service initialization time out");
            yield break;
        }

        //service failed
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.LogWarning("Unable to determine device location");
            yield break;
        }
        else
        {
            var location = Input.location; // if things works use this instead.
            int maxAttempts = 30;

            while (Input.location.lastData.latitude == 0 && Input.location.lastData.longitude == 0 && maxAttempts > 0)
            {
                yield return new WaitForSecondsRealtime(1);
                maxAttempts--;
            }

            longitude = Input.location.lastData.longitude;
            latitude = Input.location.lastData.latitude;

            Debug.LogWarning($"latitude = '{latitude}'\nlongitude = '{longitude}'");
            //GetLocation.SetLocation(Input.location.lastData.latitude, Input.location.lastData.longitude);
            //Destroy(this);
        }
    }
}


//public class GetLocation : EditorWindow
//{
//    private static readonly Vector2Int size = new Vector2Int(250, 200);
//    private static Test test;

//    private static double _latitude;
//    private static double _longitude;

    
//    public static void SetLocation(double latitude, double longitude)
//    {
//        _latitude = latitude;
//        _longitude = longitude;
//    }

//    [MenuItem("Extensions/GetLocation")]
//    public static void ShowWindow()
//    {
//        EditorWindow window = GetWindow<GetLocation>();
//        window.minSize = size;
//        window.maxSize = size;
//    }

//    private void OnGUI()
//    {
//        if (GUILayout.Button("test"))
//        {
//            Debug.Log("test");
//            if (test == null)
//            {
//                //test = Instantiate(new Test());
//                test = new GameObject().AddComponent<Test>();
//                test.name = "GetLocation Object";
//                //test = obj.AddComponent<Test>();
//            }
//        }

//        EditorGUILayout.LabelField("latitude");
//        EditorGUILayout.DoubleField(_latitude);

//        EditorGUILayout.LabelField("longitude");
//        EditorGUILayout.DoubleField(_longitude);
//    }
//}
