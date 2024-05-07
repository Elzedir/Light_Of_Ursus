using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveAndLoadGames : MonoBehaviour
{
    Transform _saveSlotParent;
    SaveSlot _saveSlot;

    public void OnSaveGameSaveSlotClicked(SaveSlot saveSlot, string saveOrLoad)
    {
        Manager_Game.FindTransformRecursively(transform.parent, "ConfirmationPanel").GetComponent<SaveSlot_Confirmation>().ActivateMenu(
            "Would you like to save this game?",
            () =>
            {
                Manager_Data.Instance.SaveGame(saveSlot.GetSaveGameName());
                ActivateMenu(saveOrLoad);
            },
            () => { ActivateMenu(saveOrLoad); }
        );
    }

    public void OnLoadGameSaveSlotClicked(SaveSlot saveSlot, string saveOrLoad)
    {
        Manager_Game.FindTransformRecursively(transform.parent, "ConfirmationPanel").GetComponent<SaveSlot_Confirmation>().ActivateMenu(
            "Would you like to load this game?",
            () =>
            {
                Manager_Data.Instance.LoadGame(saveSlot.GetSaveGameName());
                Manager_Game.Instance.LoadScene(Manager_Game.Instance.SceneName);
            },
            () => { ActivateMenu(saveOrLoad); }
        );
    }

    public void OnDeleteSaveGameClicked(SaveSlot saveSlot, string saveOrLoad)
    {
        Manager_Game.FindTransformRecursively(transform.parent, "ConfirmationPanel").GetComponent<SaveSlot_Confirmation>().ActivateMenu(
                "Are you sure you want to clear this data?",
                () => {
                    Manager_Data.Instance.GetActiveProfile().DeleteSave(saveSlot.GetSaveGameName());
                    ActivateMenu(saveOrLoad);
                },
                () => { ActivateMenu(saveOrLoad); }
            );
    }

    public void ActivateMenu(string saveOrLoad)
    {
        if (!_saveSlotParent) _saveSlotParent = Manager_Game.FindTransformRecursively(transform.parent, "SavedGamesParent");
        if (!_saveSlot) _saveSlot = Manager_Game.FindTransformRecursively(transform.parent, "SaveSlot").GetComponent<SaveSlot>();

        foreach (Transform save in _saveSlotParent) Destroy(save.gameObject);

        gameObject.SetActive(true);
        
        foreach (var saveGame in Manager_Data.Instance.GetAllSavedGames(Manager_Data.Instance.GetActiveProfile()))
        {
            if (saveGame.Key == "TheExister") continue;

            _createSaveSlot(saveGame.Key, saveGame.Value, saveOrLoad);
        }
    }

    SaveSlot _createSaveSlot(string profileID, GameData gameData, string saveOrLoad)
    {
        GameObject saveSlotGO = Instantiate(_saveSlot.gameObject, _saveSlotParent.transform);
        saveSlotGO.name = profileID;
        SaveSlot saveSlot = saveSlotGO.GetComponent<SaveSlot>();
        saveSlot.InitialiseSaveSlot(profileID, gameData,
            saveOrLoad,
            () => { OnSaveGameSaveSlotClicked(saveSlot, saveOrLoad); },
            () => { OnLoadGameSaveSlotClicked(saveSlot, saveOrLoad); }, 
            () => { OnDeleteSaveGameClicked(saveSlot, saveOrLoad); }
            );
        return saveSlot;
    }

    public void DeactivateMenu()
    {
        gameObject.SetActive(false);
    }

    public void CreateNewSave()
    {
        Manager_Game.FindTransformRecursively(transform.parent, "ConfirmationPanel").GetComponent<SaveSlot_Confirmation>().ActivateMenu(
            "Would you like to create a new save?",
            () =>
            {
                Manager_Data.Instance.SaveGame("");
                ActivateMenu("Save");
            },
            () => { ActivateMenu("Save"); }
        );
    }
}
