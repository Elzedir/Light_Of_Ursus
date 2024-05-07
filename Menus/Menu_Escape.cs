using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Menu_Escape : Menu_Base
{
    public static Menu_Escape Instance;
    Button _saveGameButton;
    Button _loadGameButton;

    public void Awake()
    {
        Instance = this;
    }

    public void Start()
    {
        _saveGameButton = GameObject.Find("SaveGameButton").GetComponent<Button>();
        _loadGameButton = GameObject.Find("LoadGameButton").GetComponent<Button>();

        _saveGameButton.onClick.AddListener(SaveGameOpen);
        _loadGameButton.onClick.AddListener(LoadGameOpen);

        gameObject.SetActive(false);
    }

    public void ToggleMenu()
    {
        if (!_isOpen) OpenMenu();
        else CloseMenu();
    }

    public override void OpenMenu()
    {
        gameObject.SetActive(true);
        _isOpen = true;
        Manager_Game.Instance.ChangeGameState(GameState.Paused);
    }
    public override void CloseMenu()
    {
        gameObject.SetActive(false);
        _isOpen = false;
        Manager_Game.Instance.ChangeGameState(GameState.Playing);
    }

    public void SaveGameOpen()
    {
        Manager_Game.FindTransformRecursively(transform.parent, "SaveAndLoadGamesPanel").gameObject.AddComponent<SaveAndLoadGames>().ActivateMenu("Save");
    }

    public void LoadGameOpen()
    {
        Manager_Game.FindTransformRecursively(transform.parent, "SaveAndLoadGamesPanel").gameObject.AddComponent<SaveAndLoadGames>().ActivateMenu("Load");
    }

    public void Settings()
    {
        //CloseMenu();
        //Menu_Settings.Instance.OpenMenu();
    }

    public void QuitToMainMenu()
    {
        SceneManager.LoadSceneAsync("Main Menu");
    }

    public void QuitToDesktop()
    {
        Application.Quit();
    }
}
