using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Ability;
using Actor;
using Careers;
using City;
using DataPersistence;
using DateAndTime;
using Faction;
using FMODUnity;
using Initialisation;
using Jobs;
using JobSite;
using Personality;
using Recipes;
using Region;
using Station;
using TickRates;
using Tools;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

namespace Managers
{
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
        static        Manager_Game _instance;

        public static Manager_Game Instance =>
            _instance ??= GameObject.Find("Manager_Game").GetComponent<Manager_Game>();

        Manager_Audio _manager_Audio;

        public Manager_Audio Manager_Audio =>
            _manager_Audio ??= GameObject.Find("Main Camera").GetComponentInChildren<Manager_Audio>();

        Window_Text _window_Text;

        public Window_Text Window_Text => _window_Text ??=
            FindTransformRecursively(GameObject.Find("UI").transform, "Window_Text")
                .GetComponentInChildren<Window_Text>();

        [SerializeField] public GameState CurrentState;

        public Player   Player;
        Vector3         _playerLastPosition;
        public Collider GroundCollider { get; private set; }

        public string LastScene;
        public string SceneName;

        Interactable_Puzzle _currentPuzzle;

        [field: SerializeField] public bool PlayerHasStaff { get; private set; }

        Transform _manager_Parent;

    

        [Header("Auto Saving Configuration")]
        [SerializeField] bool _autoSaveEnabled;
        [SerializeField] float _autoSaveTimeSeconds = 60f;
        [SerializeField] int   _numberOfAutoSaves   = 5;
    
        Coroutine _autoSaveCoroutine;

        void Awake()
        {
            var currentScene = SceneManager.GetActiveScene().name;

            if (string.IsNullOrEmpty(LastScene)) LastScene = currentScene;

            if (currentScene == "Main_Menu") CurrentState = GameState.MainMenu;

            if (currentScene != "Main_Menu" || currentScene != "Puzzle") GroundCollider = GameObject.Find("Ground").GetComponent<Collider>();

            Manager_Spawner.OnPuzzleStatesRestored += OnPuzzleStatesRestored;

            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        
        public void SetTime(InputAction.CallbackContext context)
        {
            if (!context.performed) return;
        
            switch (context.control.name)
            {
                case "numpadMinus":
                    Manager_DateAndTime.DecreaseTimeScale();
                    break;
                case "numpadPlus":
                    Manager_DateAndTime.IncreaseTimeScale();
                    break;
                case "numpad0":
                    Manager_DateAndTime.ToggleTimeScale();
                    break;
                default:
                    Debug.LogWarning($"{context.control.name} key pressed");
                    break;
            }
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (DataPersistenceManager.DataPersistence_SO.DeleteGameOnStart) DataPersistenceManager.DataPersistence_SO.DeleteTestSaveFile();
            
            SaveAndLoadSO.ClearAllSOs(); // Only do on game start, but refine since it will be called on every scene load
            // which eliminates the whole purpose of the scriptable object persisting between scenes.
            
            StartCoroutine(_initialiseManagers());
        }

        IEnumerator _initialiseManagers()
        {
            if (SceneManager.GetActiveScene().name == "Main_Menu") yield break;

            if (GameObject.Find("Manager_Parent") != null) yield break;

            yield return null;

            _manager_Parent = new GameObject("Manager_Parent").transform;

            _createManager("Manager_TickRate",     _manager_Parent).AddComponent<Manager_TickRate>().OnSceneLoaded();
            _createManager("Manager_Dialogue",     _manager_Parent).AddComponent<Manager_Dialogue>().OnSceneLoaded();
            _createManager("Manager_FloatingText", _manager_Parent).AddComponent<Manager_FloatingText>().OnSceneLoaded();
            _createManager("Manager_Cutscene",     _manager_Parent).AddComponent<Manager_Cutscene>().OnSceneLoaded();
            _createManager("Manager_Spawner",      _manager_Parent).AddComponent<Manager_Spawner>().OnSceneLoaded();
            
            Recipe_Manager.PopulateAllRecipes();
            Job_Manager.PopulateAllJobs();
            ActorPreset_Manager.PopulateAllActorDataPresets();
            Career_Manager.PopulateAllCareers();
            Manager_DateAndTime.Initialise();
            
            _createManager("Manager_Order",   _manager_Parent).AddComponent<Manager_Order>().OnSceneLoaded();

            DataPersistenceManager.DataPersistence_SO.LoadGame("");
            
            Faction_Manager.OnSceneLoaded();
            Actor_Manager.OnSceneLoaded();

            Region_Manager.OnSceneLoaded();
            City_Manager.OnSceneLoaded();
            JobSite_Manager.OnSceneLoaded();
            Station_Manager.OnSceneLoaded();
            
            Manager_Initialisation.InitialiseManagers();
            
            yield return null;
            
            Manager_Initialisation.InitialiseFactions();
            Manager_Initialisation.InitialiseActors();
            
            yield return null;
            
            Manager_Initialisation.InitialiseRegions();
            Manager_Initialisation.InitialiseCities();
            Manager_Initialisation.InitialiseJobSites();
            Manager_Initialisation.InitialiseStations();

            if (_autoSaveCoroutine != null) StopCoroutine(_autoSaveCoroutine);
            _autoSaveCoroutine = StartCoroutine(DataPersistenceManager.DataPersistence_SO.AutoSave(_autoSaveTimeSeconds, _numberOfAutoSaves, _autoSaveEnabled));
        }
        
        GameObject _createManager(string managerName, Transform parent)
        {
            var manager = new GameObject(managerName)
            {
                transform =
                {
                    parent = parent
                }
            };
            return manager;
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
            LastScene      = data.LastScene;
            SceneName = SceneManager.GetActiveScene().name == "Main_Menu" ? data.SceneName : SceneManager.GetActiveScene().name;
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
            SceneManager.sceneLoaded += _onSceneLoaded;
            return;

            void _onSceneLoaded(Scene scene, LoadSceneMode mode)
            {
                StartCoroutine(_loadScene());

                SceneManager.sceneLoaded -= _onSceneLoaded;
                return;

                IEnumerator _loadScene()
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
                        else Player.transform.position                             = currentScene == "Puzzle" ? _playerLastPosition : FindTransformRecursively(GameObject.Find("Spawners").transform, currentScene).position;
                    }

                    LastScene = currentScene;
                }
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
            SceneManager.sceneLoaded += _onSceneLoaded;
            PlayerHasStaff           =  false;
            return;

            void _onSceneLoaded(Scene scene, LoadSceneMode mode)
            {
                if (scene.name != "Ursus_Cave")
                {
                    Debug.Log("Did not load the correct scene.");
                    return;
                }

                ChangeGameState(GameState.Playing);

                StartCoroutine(_startSequence());

                SceneManager.sceneLoaded -= _onSceneLoaded;
                return;

                IEnumerator _startSequence()
                {
                    Manager_Audio.PlaySong(RuntimeManager.PathToEventReference("event:/Test_03"));
                    Manager_Audio.LocalParameters[3].SetValue(1);
                    Manager_Audio.LocalParameters[0].SetValue(0.6f);
                    Manager_Audio.LocalParameters[2].SetValue(1);

                    yield return StartCoroutine(Manager_Cutscene.Instance.PlayCutscene(GameObject.Find("Ursus_Cave_Intro").GetComponent<PlayableDirector>()));
                    yield return StartCoroutine(Window_Text.UpdateText("Chapter 1: Water"));
                }
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

            _getObstacles(moverPosition, "Wall",  obstaclePositions);
            _getObstacles(moverPosition, "Water", obstaclePositions);

            return obstaclePositions;
        }

        static void _getObstacles(Vector3 moverPosition, string layerName, List<Vector3> obstaclePositions)
        {
            var        layerMask = 1 << LayerMask.NameToLayer(layerName);
            var colliders = Physics.OverlapSphere(moverPosition, Mathf.Max(Instance.GroundCollider.bounds.size.x, Instance.GroundCollider.bounds.size.z), layerMask);

            obstaclePositions.AddRange(colliders.Select(collider => collider.transform.position));
        }

        void OnApplicationQuit()
        {
            Debug.Log("Data Saved");
            DataPersistenceManager.DataPersistence_SO.SaveGame("ExitSave");
        }
    }
}