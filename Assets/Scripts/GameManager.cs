using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public enum InstanceState
{
    Initializing,
    Running,
    Paused,
    Stopped
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public InstanceState State { get; private set; }
    public bool IsEditModeActive { get; private set; } = false;

    [SerializeField] Button editButton;
    [SerializeField] List<GameObject> gameModeUIElements = new List<GameObject>();
    [SerializeField] List<GameObject> editModeUIElements = new List<GameObject>();

    [SerializeField] ARRaycastManager m_RaycastManager;

    GameObject selectedObject;
    List<ARRaycastHit> m_Hits = new();
    Camera cam;

    FirebaseAuth auth;


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

        cam = Camera.main;
    }

    private void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Exception != null)
            {
                Debug.LogError(task.Exception);
            }

            auth = FirebaseAuth.DefaultInstance;
            AnonymousSignIn();
        });

        editButton = editButton != null ? editButton : GameObject.Find("BTNEditMode").GetComponent<Button>();

        if (editButton == null)
        {
            Debug.LogError("Can't find edit button in scene!");
            return;
        }

        editButton.interactable = false;
        editButton.onClick.AddListener(delegate { ToggleEditMode(); });
    }

    void Update()
    {
        //ExampleSpawnMethod();

        //if (Input.GetKeyDown(KeyCode.F) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
        //    GetMessagesFromDB();
        //db.SaveMessage("Messages", message);

        //if (Input.touchCount > 0)
        //    CheckIfMessageWasSelected();
        ExampleSpawnMethod();
    }


    public void HandlerOrManagerStateChanged()
    {
        InstanceState spawnableManagerState = SpawnableManager.Instance.State;
        InstanceState locationHandlerState = LocationHandler.Instance.State;

        if (InstanceState.Running.EqualsAll(spawnableManagerState, locationHandlerState)
            && State == InstanceState.Initializing) //if all possible states are true and game not paused, game should run.
        {
            Debug.Log("ALL MANAGERS AND HANDLERS ARE RUNNING! SUCCESS!");
            State = InstanceState.Running;
            EnableEditButton();
        }
        else
        {
            if (InstanceState.Initializing.EqualsAll(spawnableManagerState, locationHandlerState))
            {
                Debug.Log("All managers are still initializing...");
                //TODO: do nothing?
            }
            else if (InstanceState.Stopped.EqualsAll(spawnableManagerState, locationHandlerState))
            {
                Debug.LogWarning("ALL MANAGERS ARE STOPPED!");
                //TODO: do nothing?
            }
            else
            {
                Debug.Log("One or more managers are still initializing...");
            }
        }

        Debug.Log($"SpawnableManager = [{spawnableManagerState}] \nLocationHandler = [{locationHandlerState}]");
    }

    //void CheckIfMessageWasSelected()
    //{
    //    if (selectedObject == null && Input.touchCount == 0 && Input.GetTouch(0).phase != TouchPhase.Began)
    //        return;

    //    Ray ray = cam.ScreenPointToRay(Input.GetTouch(0).position);
    //    Debug.DrawRay(cam.transform.position, ray.direction);

    //    if (m_RaycastManager.Raycast(Input.GetTouch(0).position, m_Hits))
    //    {
    //        Debug.Log("ray hit something");
    //        if (Physics.Raycast(ray, out RaycastHit hit))
    //        {
    //            Debug.LogWarning("ray hit something specific!");
    //            if (hit.collider.gameObject.CompareTag("Spawnable"))
    //            {
    //                selectedObject = hit.collider.GetComponent<MessageWorldObject>();
    //                selectedObject.DisplayText();
    //            }
    //            else
    //            {
    //                selectedObject.HideText();
    //                selectedObject = null;
    //            }
    //        }
    //    }
    //}

    void ExampleSpawnMethod()
    {
        if (Input.touchCount == 0)
            return;

        Debug.LogWarning("Input.touchCount != 0");

        RaycastHit hit;
        Ray ray = cam.ScreenPointToRay(Input.GetTouch(0).position);

        if (m_RaycastManager.Raycast(Input.GetTouch(0).position, m_Hits))
        {
            Debug.LogWarning("ray hit something");
            if (Input.GetTouch(0).phase == TouchPhase.Began && selectedObject == null)
            {
                if (Physics.Raycast(ray, out hit))
                {
                    Debug.LogWarning("ray hit something specific!");
                    if (hit.collider.gameObject.CompareTag("Spawnable"))
                    {
                        selectedObject = hit.collider.gameObject;
                        selectedObject.GetComponent<MessageWorldObject>().DisplayText();
                    }
                    else
                    {
                        selectedObject.GetComponent<MessageWorldObject>().HideText();
                        selectedObject = null;
                        //SpawnPrefab(m_Hits[0].pose.position);
                    }
                }
                else
                {
                    selectedObject.GetComponent<MessageWorldObject>().HideText();
                    selectedObject = null;
                }

            }
            else if (Input.GetTouch(0).phase == TouchPhase.Moved && selectedObject != null)
            {
                selectedObject.transform.position = m_Hits[0].pose.position;
            }
            if (Input.GetTouch(0).phase == TouchPhase.Ended)
            {
                selectedObject = null;
            }
        }
    }


    #region EditMode

    private void EnableEditButton()
    {
        if (auth.CurrentUser == null || State != InstanceState.Running)
            return;

        editButton.interactable = true;
    }

    private void ToggleEditMode()
    {
        IsEditModeActive = !IsEditModeActive;

        foreach (var item in editModeUIElements)
        {
            item.SetActive(IsEditModeActive);
        }

        foreach (var item in gameModeUIElements)
        {
            item.SetActive(!IsEditModeActive);
        }
    }

    public void ExitEditMode()
    {
        IsEditModeActive = false;

        foreach (var item in editModeUIElements)
        {
            item.SetActive(false);
        }

        foreach (var item in gameModeUIElements)
        {
            item.SetActive(true);
        }
    }

    #endregion

    #region DB methods

    private void AnonymousSignIn()
    {
        auth.SignInAnonymouslyAsync().ContinueWithOnMainThread(task => {
            if (task.Exception != null)
            {
                Debug.LogWarning(task.Exception);
            }
            else
            {
                SignedIn(task.Result);
            }
        });
    }

    private void SignedIn(FirebaseUser newUser)
    {
        Debug.LogFormat("User signed in successfully: {0} ({1})",
                    newUser.DisplayName, newUser.UserId);

        //Display who logged in
        if (newUser.DisplayName != "")
            Debug.Log("Logged in as: " + newUser.DisplayName);
        else if (newUser.Email != "")
            Debug.Log("Logged in as: " + newUser.Email);
        else
            Debug.Log("Logged in as: Anonymous User " + newUser.UserId);

        EnableEditButton();
    }

    #endregion
}
