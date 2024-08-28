using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu_LoadGame : Menu_Base
{
    Menu_Main _menuMain;
    Transform _saveSlotParent;
    List<SaveSlot> _saveSlots;
    SaveSlot _saveSlot;

    public void OnNewGameSaveSlotClicked(SaveSlot saveSlot)
    {
        if (saveSlot.HasData)
        {
            Manager_Game.FindTransformRecursively(transform.parent, "ConfirmationPanel").GetComponent<SaveSlot_Confirmation>().ActivateMenu(
                "Starting a New Game with this slot will override the currently saved data. Are you sure?",
                () => {
                    Manager_Data.Instance.ChangeProfile(saveSlot.GetSaveSlotID(), saveSlot.GetSaveGameName());
                    Manager_Data.Instance.NewSaveData(saveSlot.GetSaveSlotID(), saveSlot.GetSaveGameName());
                    _saveGameAndLoadNewGame();
                },
                () => {
                    ActivateMenu(_menuMain);
                }
            );
        }
        else
        {
            Manager_Data.Instance.ChangeProfile(saveSlot.GetSaveSlotID(), saveSlot.GetSaveGameName());
            Manager_Data.Instance.NewSaveData(saveSlot.GetSaveSlotID(), saveSlot.GetSaveGameName());
            _saveGameAndLoadNewGame();
        }
    }

    public void OnLoadGameSaveSlotClicked(SaveSlot saveSlot)
    {
        if (saveSlot.HasData)
        {
            Manager_Game.FindTransformRecursively(transform.parent, "ConfirmationPanel").GetComponent<SaveSlot_Confirmation>().ActivateMenu(
                "Would you like to load this game?",
                () => {
                    Manager_Game.Instance.LoadScene(Manager_Game.Instance.SceneName);
                },
                () => {
                    ActivateMenu(_menuMain);
                }
            );
        }
        else
        {
            Manager_Data.Instance.ChangeProfile(saveSlot.GetSaveSlotID(), saveSlot.GetSaveGameName());
            Manager_Data.Instance.NewSaveData(saveSlot.GetSaveSlotID(), saveSlot.GetSaveGameName());
            _saveGameAndLoadNewGame();
        }
    }

    public void OnDeleteSaveGameClicked(SaveSlot saveSlot)
    {
        Manager_Game.FindTransformRecursively(transform.parent, "ConfirmationPanel").GetComponent<SaveSlot_Confirmation>().ActivateMenu(
                "Are you sure you want to clear this data?",
                () => {
                    Manager_Data.Instance.CurrentProfile.DeleteSave(saveSlot.GetSaveGameName());
                    ActivateMenu(_menuMain);
                },
                () => {
                    ActivateMenu(_menuMain);
                }
            );
    }

    private void _saveGameAndLoadNewGame()
    {
        Manager_Data.Instance.SaveGame();
        _menuMain.NewGame();
    }

    public void ActivateMenu(Menu_Main menuMain)
    {
        _menuMain = menuMain;

        if (_saveSlots == null) _saveSlots = new();

        foreach (SaveSlot save in _saveSlots) Destroy(save.gameObject); _saveSlots.Clear();

        gameObject.SetActive(true);
        if (!_saveSlotParent) _saveSlotParent = Manager_Game.FindTransformRecursively(transform, "SavedGamesParent");
        if (!_saveSlot) _saveSlot = Manager_Game.FindTransformRecursively(transform, "SaveSlot").GetComponent<SaveSlot>();

        foreach (var saveData in Manager_Data.Instance.GetAllSavedGames(Manager_Data.Instance.CurrentProfile))
        {
            if (saveData.SaveDataName == "TheExister") continue;

            _saveSlots.Add(_createSaveSlot(saveData.SaveDataName, saveData));
        }
    }

    SaveSlot _createSaveSlot(string profileID, SaveData gameData)
    {
        GameObject saveSlotGO = Instantiate(_saveSlot.gameObject, _saveSlotParent.transform);
        saveSlotGO.name = profileID;
        SaveSlot saveSlot = saveSlotGO.GetComponent<SaveSlot>();
        saveSlot.InitialiseSaveSlot(gameData, "Load", () => { }, () => { OnLoadGameSaveSlotClicked(saveSlot); }, () => { OnDeleteSaveGameClicked(saveSlot); } );
        return saveSlot;
    }

    public void DeactivateMenu()
    {
        gameObject.SetActive(false);
    }
}
