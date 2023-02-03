using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DBTables;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using URandom = UnityEngine.Random;


[Serializable]
public class CustomLocation
{
    public MessageLocation locationObject;
    public bool isSpawned = false;

    public CustomLocation(MessageLocation messageLocation, bool isSpawned = false)
    {
        locationObject = messageLocation;
        this.isSpawned = isSpawned;
    }
}

public class SpawnableManager : MonoBehaviour
{
    public bool isSpawnModeActive = false;

    [SerializeField, Tooltip("How many metres from the device to look for saved messages")]
    float detectionDistance = 50;
    [SerializeField, Tooltip("Distance in metres the device needs to travel before looking for new messages")]
    float updateDistance = 50;

    [SerializeField] Camera cam;

    [SerializeField] GameObject spawnablePrefab;
    [SerializeField] ARRaycastManager m_RaycastManager;
    List<ARRaycastHit> m_Hits = new List<ARRaycastHit>();

    List<CustomLocation> locations = new List<CustomLocation>();
    GameObject spawnedObject;
    LocationInfo lastLocationCheckpoint;


    void Start()
    {
        if (cam == null)
            cam = Camera.main;

        spawnedObject = null;
    }

    void OnEnable()
    {
        LocationHandler.Instance.onLocationChanged += OnLocationChanged;
    }

    void OnDisable()
    {
        LocationHandler.Instance.onLocationChanged -= OnLocationChanged;
    }

    void Update()
    {
        ExampleSpawnMethod();
    }

    void OnLocationChanged(LocationInfo info)
    {
        var distance = info.CalculateDistance(lastLocationCheckpoint);
        LocationHandler.Instance.AddToStatusText($"distance is: '{distance}'");

        if (info.CalculateDistance(lastLocationCheckpoint) < updateDistance)
            return;
        else
            lastLocationCheckpoint = info;

        LocationHandler.Instance.AddToStatusText("Location changed: " + JsonConvert.SerializeObject(info),
            LogLevel.Warning);

        var nearbyLocations = LocationHandler.Instance.GetNearbyLocations(detectionDistance);

        foreach (var location in nearbyLocations)
        {
            if (locations.Any(l => l.locationObject == location))
                continue;

            SpawnNearby();
            locations.Add(new CustomLocation(location));
        }
        Debug.LogWarning(JsonConvert.SerializeObject(locations));
    }

    void SpawnNearby()
    {
        float randomDistance = URandom.Range(5f, 10f);
        Vector3 randomPos = cam.transform.position.GetRandomPointOnHorizontalCircle(randomDistance);
        SpawnPrefab(randomPos);
    }

    void ExampleSpawnMethod()
    {
        if (Input.touchCount == 0)
            return;

        RaycastHit hit;
        Ray ray = cam.ScreenPointToRay(Input.GetTouch(0).position);

        if (m_RaycastManager.Raycast(Input.GetTouch(0).position, m_Hits))
        {
            if (Input.GetTouch(0).phase == TouchPhase.Began && spawnedObject == null)
            {
                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.collider.gameObject.CompareTag("Spawnable"))
                    {
                        spawnedObject = hit.collider.gameObject;
                    }
                    else
                    {
                        //SpawnPrefab(m_Hits[0].pose.position);
                    }
                }

            }
            else if (Input.GetTouch(0).phase == TouchPhase.Moved && spawnedObject != null)
            {
                spawnedObject.transform.position = m_Hits[0].pose.position;
            }
            if (Input.GetTouch(0).phase == TouchPhase.Ended)
            {
                spawnedObject = null;
            }
        }
    }

    private void SpawnPrefab(Vector3 spawnPosition)
    {
        if (isSpawnModeActive)
            spawnedObject = Instantiate(spawnablePrefab, spawnPosition, Quaternion.identity);
    }
}
