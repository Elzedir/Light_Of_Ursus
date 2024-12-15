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
                DataPersistenceManager.DataPersistence_SO.SaveGame(saveSlot.GetSaveGameName());
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
                DataPersistenceManager.DataPersistence_SO.LoadGame(saveSlot.GetSaveGameName());
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
                    DataPersistenceManager.DataPersistence_SO.CurrentProfile.DeleteSave(saveSlot.GetSaveGameName());
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
        
        foreach (var saveData in DataPersistenceManager.DataPersistence_SO.CurrentProfile.AllSavedData)
        {
            if (saveData.Key == "TheExister") continue;

            _createSaveSlot(saveData.Value, saveOrLoad);
        }
    }

    SaveSlot _createSaveSlot(SaveData saveData, string saveOrLoad)
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
                DataPersistenceManager.DataPersistence_SO.SaveGame("");
                ActivateMenu("Save");
            },
            () => { ActivateMenu("Save"); }
        );
    }
}
