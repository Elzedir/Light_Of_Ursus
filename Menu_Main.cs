using System.IO;
using System.Linq;
using DataPersistence;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Menu_Main : MonoBehaviour
{
    Menu_LoadGame _loadGamePanel;
    Transform _profileContainer;

    void Start()
    {
        Manager_Game.FindTransformRecursively(transform, "ProfileText").GetComponent<TextMeshProUGUI>().text =
            DataPersistence_Manager.CurrentProfile.ProfileName;
    }

    public void Continue()
    {
        if (!DataPersistence_Manager.HasSaveData()) { Debug.LogWarning("Manager_Data has no game data."); return; }

        DataPersistence_Manager.ChangeProfile(DataPersistence_Manager.CurrentProfile.ProfileID);
        Manager_Game.S_Instance.LoadScene(Manager_Game.S_Instance.SceneName);
    }

    public void NewGame()
    {
        var currentProfile = DataPersistence_Manager.CurrentProfile;
        DataPersistence_Manager.SetCurrentSaveData(new Save_Data(currentProfile.ProfileID, currentProfile.ProfileName));
        Manager_Game.S_Instance.StartNewGame();
    }

    public void LoadGame()
    {
        if (_loadGamePanel == null) { Debug.Log("Load found"); _loadGamePanel = Manager_Game.FindTransformRecursively(transform.parent, "LoadGamePanel").gameObject.GetComponent<Menu_LoadGame>(); }

        Debug.Log("Load called");

        _loadGamePanel.ActivateMenu(this);
    }

    public void Settings()
    {
        Manager_Settings.Instance.OpenMenu();
    }

    public void Exit()
    {
        Application.Quit();
    }

    public void OpenSwitchProfilePanel()
    {
        if (!_profileContainer) _profileContainer = Manager_Game.FindTransformRecursively(transform.parent, "ProfileContainer");

        GameObject switchProfilePanel= Manager_Game.FindTransformRecursively(transform.parent, "SwitchProfilePanel").gameObject;
        switchProfilePanel.SetActive(true);
        var allProfiles = DataPersistence_Manager.AllProfiles.Where(p => p.Value.ProfileName != "Unity").Select(p => p.Value).ToList();

        foreach (Profile_Data profile in allProfiles)
        {
            GameObject profileGO = new GameObject(profile.ProfileName);
            RectTransform rectTransform = profileGO.AddComponent<RectTransform>();
            profileGO.transform.SetParent(_profileContainer);
            ProfileUI profileUI = profileGO.AddComponent<ProfileUI>();
            profileUI.Initialise(this, profile.ProfileName);
        }
    }

    public void CloseSwitchProfile()
    {
        foreach (Transform child in _profileContainer)
        {
            Destroy(child.gameObject);
        }

        Manager_Game.FindTransformRecursively(transform.parent, "SwitchProfilePanel").gameObject.SetActive(false);
    }

    public void SwitchProfile(uint profileID)
    {
        DataPersistence_Manager.ChangeProfile(profileID);
        CloseSwitchProfile();
        Manager_Game.FindTransformRecursively(transform, "ProfileText").GetComponent<TextMeshProUGUI>().text =
            DataPersistence_Manager.CurrentProfile.ProfileName;
    }

    public void CreateNewProfile()
    {
        Manager_Game.FindTransformRecursively(transform.parent, "CreateNewProfilePanel").gameObject.SetActive(true);
    }

    public void Create()
    {
        int returnCheck = 0;
        if (returnCheck == 0) return; // Just return here so we can't use this function.
        // Save data is saving to TheExister, which we need to see if we still need it and how we can remove it from the system.

        string profileName = Manager_Game.FindTransformRecursively(transform.parent, "InputField").gameObject.GetComponent<InputField>().text;

        if (profileName == "") { OpenWarningPopup("Profile name is empty."); return; }

        foreach (DirectoryInfo directoryInfo in new DirectoryInfo(Application.persistentDataPath).EnumerateDirectories())
        {
            if (directoryInfo.Name == profileName) { OpenWarningPopup("Profile already exists."); return; }
        }

        Manager_Game.FindTransformRecursively(transform.parent, "CreateNewProfilePanel").gameObject.SetActive(false);

        //ProfileData newProfile = new ProfileData(profileName, 0, null, Manager_Data.Instance._useEncryption);

        //newProfile.SaveData("TheExister", new SaveData(profileID, profileName), profileName, true);

        CloseSwitchProfile();
        OpenSwitchProfilePanel();
    }

    public void CloseCreateNewProfilePanel()
    {
        Manager_Game.FindTransformRecursively(transform.parent, "CreateNewProfilePanel").gameObject.SetActive(false);
    }

    public void OpenWarningPopup(string text)
    {
        GameObject warningPanel = Manager_Game.FindTransformRecursively(transform.parent, "WarningPanel").gameObject;
        warningPanel.SetActive(true);
        Manager_Game.FindTransformRecursively(warningPanel.transform, "Warning").GetComponentInChildren<TextMeshProUGUI>().text = text;
    }

    public void CloseWarningPopup()
    {
        Manager_Game.FindTransformRecursively(transform.parent, "WarningPanel").gameObject.SetActive(false);
    }

    public void Directional()
    {
        _runTestPuzzle(PuzzleSet.Directional);
    }

    public void AntiDirectional()
    {
        _runTestPuzzle(PuzzleSet.AntiDirectional);
    }

    public void XOYXOY()
    {
        _runTestPuzzle(PuzzleSet.XOYXOY);
    }

    public void FlappyInvaders()
    {
        _runTestPuzzle(PuzzleSet.FlappyInvaders);
    }

    public void MouseMaze()
    {
        _runTestPuzzle(PuzzleSet.MouseMaze);
    }

    public void IceWall()
    {
        _runTestPuzzle(PuzzleSet.IceWall);
    }

    void _runTestPuzzle(PuzzleSet puzzleSet)
    {
        Transform puzzleTransform = Manager_Game.FindTransformRecursively(transform, puzzleSet.ToString());
        Interactable_Puzzle puzzle = puzzleTransform.GetComponent<Interactable_Puzzle>();

        SceneManager.LoadSceneAsync("Puzzle");
        SceneManager.sceneLoaded += OnSceneLoaded;

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Manager_Game.S_Instance.ChangeGameState(GameState.Puzzle);
            Manager_Puzzle.Instance.Puzzle = puzzle;
            Manager_Puzzle.Instance.LoadPuzzle();

            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
}
