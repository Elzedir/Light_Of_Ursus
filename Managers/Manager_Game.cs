using FMODUnity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

[Serializable]
public enum GameState
{
    MainMenu,
    Playing,
    Paused,
    Cinematic,
    Loading,
    PlayerDead,
    Puzzle
}

public class Manager_Game : MonoBehaviour, IDataPersistence
{
    public static Manager_Game Instance;
    Manager_Audio _manager_Audio;
    public Manager_Audio Manager_Audio { get { return _getManager_Audio(); } private set { _manager_Audio = value; } }
    Manager_Audio _getManager_Audio() { if (_manager_Audio) return _manager_Audio; else return GameObject.Find("Main Camera").GetComponentInChildren<Manager_Audio>(); }

    Window_Text _window_Text;
    public Window_Text Window_Text { get { return _getWindow_Text(); } private set { _window_Text = value; } }
    Window_Text _getWindow_Text() { if (_window_Text) return _window_Text; else return FindTransformRecursively(GameObject.Find("UI").transform, "Window_Text").GetComponentInChildren<Window_Text>(); }

    [SerializeField] public GameState CurrentState;

    public Player Player;
    Vector3 _playerLastPosition;
    public Collider GroundCollider { get; private set; }

    public string LastScene;
    public string SceneName;

    public string MostRecentlyUpdatedProfile;

    Interactable_Puzzle _currentPuzzle;

    [field: SerializeField] public bool PlayerHasStaff { get; private set; }

    Transform _manager_Parent;

    

    [Header("Auto Saving Configuration")]
    [SerializeField] bool _autoSaveEnabled = false;
    [SerializeField] float _autoSaveTimeSeconds = 60f;
    [SerializeField] int _numberOfAutoSaves = 5;
    
    Coroutine _autoSaveCoroutine;

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); } else if (Instance != this) Destroy(gameObject);

        string currentScene = SceneManager.GetActiveScene().name;

        if (string.IsNullOrEmpty(LastScene)) LastScene = currentScene;

        if (currentScene == "Main_Menu") CurrentState = GameState.MainMenu;

        if (currentScene != "Main_Menu" || currentScene != "Puzzle") GroundCollider = GameObject.Find("Ground").GetComponent<Collider>();

        Manager_Spawner.OnPuzzleStatesRestored += OnPuzzleStatesRestored;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(_initialiseManagers());
    }

    IEnumerator _initialiseManagers()
    {
        if (SceneManager.GetActiveScene().name == "Main_Menu") yield break;

        if (GameObject.Find("Manager_Parent") != null) yield break;

        yield return null;

        _manager_Parent = new GameObject("Manager_Parent").transform;

        Manager_Item.Initialise();
        _createManager("Manager_TickRate", _manager_Parent).AddComponent<Manager_TickRate>().OnSceneLoaded();
        _createManager("Manager_Dialogue", _manager_Parent).AddComponent<Manager_Dialogue>().OnSceneLoaded();
        _createManager("Manager_FloatingText", _manager_Parent).AddComponent<Manager_FloatingText>().OnSceneLoaded();
        _createManager("Manager_Cutscene", _manager_Parent).AddComponent<Manager_Cutscene>().OnSceneLoaded();
        _createManager("Manager_Spawner", _manager_Parent).AddComponent<Manager_Spawner>().OnSceneLoaded();
        _createManager("Manager_Ability", _manager_Parent).AddComponent<Manager_Ability>().OnSceneLoaded();
        _createManager("Manager_Job", _manager_Parent).AddComponent<Manager_Job>().OnSceneLoaded();
        _createManager("Manager_Recipe", _manager_Parent).AddComponent<Manager_Recipe>().OnSceneLoaded();
        
        Manager_Career.Initialise();
        Manager_DateAndTime.Initialise();
        Manager_Personality.Initialise();

        _createManager("Manager_Faction", _manager_Parent).AddComponent<Manager_Faction>().OnSceneLoaded();
        _createManager("Manager_Region", _manager_Parent).AddComponent<Manager_Region>().OnSceneLoaded();
        _createManager("Manager_City", _manager_Parent).AddComponent<Manager_City>().OnSceneLoaded();
        _createManager("Manager_Jobsite", _manager_Parent).AddComponent<Manager_Jobsite>().OnSceneLoaded();
        _createManager("Manager_Station", _manager_Parent).AddComponent<Manager_Station>().OnSceneLoaded();
        _createManager("Manager_Actor", _manager_Parent).AddComponent<Manager_Actor>().OnSceneLoaded();
        _createManager("Manager_Order", _manager_Parent).AddComponent<Manager_Order>().OnSceneLoaded();

        DataPersistenceManager.DataPersistence_SO.LoadGame("");

        Manager_Initialisation.InitialiseFactions();
        Manager_Initialisation.InitialiseRegions();
        Manager_Initialisation.InitialiseActors();
        Manager_Initialisation.InitialiseJobsites();

        Debug_Visualiser.Instance.Initialise();

        if (_autoSaveCoroutine != null) StopCoroutine(_autoSaveCoroutine);
        _autoSaveCoroutine = StartCoroutine(DataPersistenceManager.DataPersistence_SO.AutoSave(_autoSaveTimeSeconds, _numberOfAutoSaves, _autoSaveEnabled));

        GameObject _createManager(string name, Transform parent)
        {
            GameObject manager = new GameObject(name);
            manager.transform.parent = parent;
            return manager;
        }
    }

    void OnDestroy()
    {
        Manager_Spawner.OnPuzzleStatesRestored -= OnPuzzleStatesRestored;
    }

    public void SaveData(SaveData data)
    {
        // data.CurrentProfileName = Manager_Data.Instance.GetActiveProfile().Name;
        // data.SceneName = SceneManager.GetActiveScene().name;
        // data.StaffPickedUp = PlayerHasStaff;
        // data.LastScene = LastScene;
    }

    public void LoadData(SaveData data)
    {
        PlayerHasStaff = data.StaffPickedUp;
        LastScene = data.LastScene;
        if (SceneManager.GetActiveScene().name == "Main_Menu") SceneName = data.SceneName;
        else SceneName = SceneManager.GetActiveScene().name;
        _playerLastPosition = data.PlayerPosition;
    }

    public static Transform FindTransformRecursively(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name) return child;

            Transform result = FindTransformRecursively(child, name);

            if (result != null) return result;
        }
        return null;
    }

    public void LoadScene(string nextScene = null, Interactable_Puzzle puzzle = null)
    {
        DataPersistenceManager.DataPersistence_SO.SaveGame("");

        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene != "Puzzle") LastScene = currentScene;

        if (nextScene == "Puzzle" && puzzle == null) { Debug.Log("Cannot load Puzzle Scene without a puzzle"); return; }
        if (nextScene == "Puzzle") { _playerLastPosition = Player.transform.position; _currentPuzzle = puzzle; }

        else if (nextScene == null && currentScene != "Puzzle") { Debug.Log("Cannot load null scene."); return; }
        else if (nextScene == null) nextScene = LastScene;

        SceneManager.LoadSceneAsync(nextScene);
        SceneManager.sceneLoaded += OnSceneLoaded;

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            StartCoroutine(loadScene());

            IEnumerator loadScene()
            {
                yield return null;

                if (scene.name != nextScene) { Debug.Log("Did not load the correct scene."); yield break; }

                if (scene.name == "Puzzle")
                {
                    ChangeGameState(GameState.Puzzle);
                    Manager_Puzzle.Instance.Puzzle = _currentPuzzle;
                    Manager_Puzzle.Instance.LoadPuzzle();
                }
                else
                {
                    ChangeGameState(GameState.Playing);
                    Player = FindFirstObjectByType<Player>();
                    if (currentScene == "Main_Menu") Player.transform.position = _playerLastPosition;
                    else Player.transform.position = currentScene == "Puzzle" ? _playerLastPosition : FindTransformRecursively(GameObject.Find("Spawners").transform, currentScene).position;
                }

                LastScene = currentScene;
            }

            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    void OnPuzzleStatesRestored()
    {
        if (LastScene == "Puzzle" && _currentPuzzle.PuzzleData != null) Manager_Spawner.Instance.CompletePuzzle(_currentPuzzle.PuzzleData.PuzzleID);
    }

    public void StartNewGame()
    {
        LastScene = SceneManager.GetActiveScene().name;
        SceneManager.LoadSceneAsync("Ursus_Cave");
        SceneManager.sceneLoaded += OnSceneLoaded;
        PlayerHasStaff = false;

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != "Ursus_Cave")
            {
                Debug.Log("Did not load the correct scene.");
                return;
            }

            ChangeGameState(GameState.Playing);

            StartCoroutine(StartSequence());

            IEnumerator StartSequence()
            {
                Manager_Audio.PlaySong(RuntimeManager.PathToEventReference("event:/Test_03"));
                Manager_Audio.LocalParameters[3].SetValue(1);
                Manager_Audio.LocalParameters[0].SetValue(0.6f);
                Manager_Audio.LocalParameters[2].SetValue(1);

                yield return StartCoroutine(Manager_Cutscene.Instance.PlayCutscene(GameObject.Find("Ursus_Cave_Intro").GetComponent<PlayableDirector>()));
                yield return StartCoroutine(Window_Text.UpdateText("Chapter 1: Water"));
            }

            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    public void ChangeGameState(GameState newState)
    {
        switch (CurrentState)
        {
            case GameState.Playing:
                break;
            case GameState.Paused:
                break;
            case GameState.Loading:
                // hide the loading screen
                // switch to the next level
                break;
            case GameState.Cinematic:
                // Hide the cinematic
                // Resume player controls
                break;
            case GameState.PlayerDead:
                // Reset the player
                break;
        }

        CurrentState = newState;

        switch (CurrentState)
        {
            case GameState.Playing:
                UnityEngine.Time.timeScale = 1f;
                // enbale player controls
                // resume normal sound and music
                break;
            case GameState.Paused:
                UnityEngine.Time.timeScale = 0f;
                // disable player controls
                // play pause music and stop game sounds
                break;
            case GameState.Loading:
                // play a loading screen and slowly load the map in
                // begin loading the next level
                break;
            case GameState.Cinematic:
                // Play the cinematic
                // Stop player controls
                break;
            case GameState.PlayerDead:
                // Stop the player controls and enemies moving
                // Play game over sound effect
                break;
        }
    }
    public void SetPlayer()
    {
        Player = FindFirstObjectByType<Player>();
    }

    public void PickUpStaff()
    {
        if (!PlayerHasStaff)
        {
            PlayerHasStaff = true;

            StartCoroutine(Player.PickUpStaffAction());
        }
    }

    public static List<Vector3> GetAllObstacles(Vector3 moverPosition)
    {
        List<Vector3> obstaclePositions = new();

        GetObstacles(moverPosition, "Wall", obstaclePositions);
        GetObstacles(moverPosition, "Water", obstaclePositions);

        return obstaclePositions;
    }

    static void GetObstacles(Vector3 moverPosition, string layerName, List<Vector3> obstaclePositions)
    {
        int layerMask = 1 << LayerMask.NameToLayer(layerName);
        Collider[] colliders = Physics.OverlapSphere(moverPosition, Mathf.Max(Manager_Game.Instance.GroundCollider.bounds.size.x, Manager_Game.Instance.GroundCollider.bounds.size.z), layerMask);

        foreach (Collider collider in colliders)
        {
            obstaclePositions.Add(collider.transform.position);
        }
    }

    void OnApplicationQuit()
    {
        Debug.Log("Data Saved");
        DataPersistenceManager.DataPersistence_SO.SaveGame("ExitSave");
    }
}
