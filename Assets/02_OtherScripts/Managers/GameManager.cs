using UnityEngine.SceneManagement;
using UnityEngine;

[DisallowMultipleComponent]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public static AudioManager AudioManager { get; private set; }
    public static TimerManager TimerManager { get; private set; }
    public static SaveManager SaveManager { get; private set; }
    public static EventManager EventManager { get; private set; }
    public static InputManager InputManager { get; private set; }

    private Manager[] activeManagers;

    private void Awake()
    {
        SingletonSetup();
        ManagerSetup();
    }

    private void SingletonSetup()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(Instance);
        }
    }

    private void ManagerSetup()
    {
        AudioManager = new AudioManager();
        TimerManager = new TimerManager();
        SaveManager = new SaveManager();
        EventManager = new EventManager();
        InputManager = new InputManager();

        activeManagers = new Manager[] { 
            AudioManager, 
            TimerManager, 
            SaveManager,
            EventManager,
            InputManager
        };

        foreach (Manager manager in activeManagers)
        {
            manager.Setup();
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene loadedScene, LoadSceneMode loadSceneMode)
    {
        foreach (Manager manager in activeManagers)
        {
            manager.OnSceneLoad();
        }
    }
    
    private void FixedUpdate()
    {
        foreach (Manager manager in activeManagers)
        {
            manager.OnFixedUpdate();
        }
    }

    #region ContextMenu
    [ContextMenu("SaveManager/Save")] public void Save() => SaveManager?.Save();
    [ContextMenu("SaveManager/Load")] public void Load() => SaveManager?.Load();
    [ContextMenu("SaveManager/DeleteSave")] public void DeleteSave() => SaveManager?.DeleteSave();

    #endregion
}

