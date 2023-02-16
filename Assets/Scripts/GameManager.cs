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

    public InstanceState state;

    [SerializeField]
    Button editButton;
    private bool _IsEditModeActive = false;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        state = InstanceState.Initializing;
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        editButton = editButton != null ? editButton : FindObjectsOfType<Button>().FirstOrDefault(b => b.name == "BTNEditMode");
        editButton.interactable = false;
        editButton.onClick.AddListener(delegate { ToggleEditMode(); });
    }

    public void HandlerOrManagerStateChanged()
    {
        if (InstanceState.Running.EqualsAll(SpawnableManager.Instance.state, LocationHandler.Instance.State)
            && state == InstanceState.Initializing) //if all possible states are true and game not paused, game should run.
        {
            state = InstanceState.Running;
            editButton.interactable = true;
        }
        else
        {
            if (InstanceState.Initializing.EqualsAll(SpawnableManager.Instance.state, LocationHandler.Instance.State))
            {
                //TODO: do nothing?
            }
        }
    }

    public void ToggleEditMode()
    {
        _IsEditModeActive = !_IsEditModeActive;
    }


    ///// <summary>
    ///// WARNING!<br/>
    ///// ONLY <see cref="LocationHandler"/> SHOULD CALL THIS METHOD! GAME LOGIC CAN BREAK IF CALLED INCORRECTLY!
    ///// </summary>
    //public void LocationHandlerActiveState(bool state)
    //{
    //    _IsLocationHandlerActive = state;
    //}
}
