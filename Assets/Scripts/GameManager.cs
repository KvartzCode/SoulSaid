using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

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

    private void Start()
    {
        editButton = editButton != null ? editButton : GameObject.Find("BTNEditMode").GetComponent<Button>();

        if (editButton == null)
        {
            Debug.LogError("Can't find edit button in scene!");
            return;
        }

        editButton.interactable = false;
        editButton.onClick.AddListener(delegate { ToggleEditMode(); });
    }

    public void HandlerOrManagerStateChanged()
    {
        if (InstanceState.Running.EqualsAll(SpawnableManager.Instance.State, LocationHandler.Instance.State)
            && State == InstanceState.Initializing) //if all possible states are true and game not paused, game should run.
        {
            Debug.Log("ALL MANAGERS AND HANDLERS ARE RUNNING! SUCCESS!");
            State = InstanceState.Running;
            editButton.interactable = true;
        }
        else
        {
            if (InstanceState.Initializing.EqualsAll(SpawnableManager.Instance.State, LocationHandler.Instance.State))
            {
                Debug.Log("All managers are still initializing...");
                //TODO: do nothing?
            }
            else if (InstanceState.Stopped.EqualsAll(SpawnableManager.Instance.State, LocationHandler.Instance.State))
            {
                Debug.LogWarning("ALL MANAGERS ARE STOPPED!");
                //TODO: do nothing?
            }
            else
            {
                Debug.Log("One or more managers are still initializing...");
            }
        }

        Debug.Log($"SpawnableManager = [{SpawnableManager.Instance.State}] \nLocationHandler = [{LocationHandler.Instance.State}]");
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
}
