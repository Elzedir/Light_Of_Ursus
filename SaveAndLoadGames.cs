using DataPersistence;
using Managers;
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
                DataPersistence_Manager.SaveGame(saveSlot.GetSaveGameName());
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
                DataPersistence_Manager.LoadGame(saveSlot.GetSaveGameName());
                Manager_Game.S_Instance.LoadScene(Manager_Game.S_Instance.SceneName);
            },
            () => { ActivateMenu(saveOrLoad); }
        );
    }

    public void OnDeleteSaveGameClicked(SaveSlot saveSlot, string saveOrLoad)
    {
        Manager_Game.FindTransformRecursively(transform.parent, "ConfirmationPanel").GetComponent<SaveSlot_Confirmation>().ActivateMenu(
                "Are you sure you want to clear this data?",
                () => {
                    DataPersistence_Manager.CurrentProfile.DeleteSave(saveSlot.GetSaveGameName());
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
        
        foreach (var saveData in DataPersistence_Manager.CurrentProfile.AllSavedData)
        {
            if (saveData.Key == "TheExister") continue;

            _createSaveSlot(saveData.Value, saveOrLoad);
        }
    }

    SaveSlot _createSaveSlot(Save_Data saveData, string saveOrLoad)
    {
        GameObject saveSlotGO = Instantiate(_saveSlot.gameObject, _saveSlotParent.transform);
        saveSlotGO.name = saveData.SavedProfileData.ProfileName;
        SaveSlot saveSlot = saveSlotGO.GetComponent<SaveSlot>();
        saveSlot.InitialiseSaveSlot(saveData,
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
                DataPersistence_Manager.SaveGame("");
                ActivateMenu("Save");
            },
            () => { ActivateMenu("Save"); }
        );
    }
}
