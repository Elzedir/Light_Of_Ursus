using System.Collections.Generic;
using Managers;
using UnityEngine;

namespace DataPersistence
{
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
                        DataPersistence_Manager.ChangeProfile(saveSlot.GetSaveSlotID());
                        DataPersistence_Manager.SetCurrentSaveData(new Save_Data(saveSlot.GetSaveSlotID(), saveSlot.GetSaveGameName()));
                        _saveGameAndLoadNewGame();
                    },
                    () => {
                        ActivateMenu(_menuMain);
                    }
                );
            }
            else
            {
                DataPersistence_Manager.ChangeProfile(saveSlot.GetSaveSlotID());
                DataPersistence_Manager.SetCurrentSaveData(new Save_Data(saveSlot.GetSaveSlotID(), saveSlot.GetSaveGameName()));
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
                        Manager_Game.S_Instance.LoadScene(Manager_Game.S_Instance.SceneName);
                    },
                    () => {
                        ActivateMenu(_menuMain);
                    }
                );
            }
            else
            {
                DataPersistence_Manager.ChangeProfile(saveSlot.GetSaveSlotID());
                DataPersistence_Manager.SetCurrentSaveData(new Save_Data(saveSlot.GetSaveSlotID(), saveSlot.GetSaveGameName()));
                _saveGameAndLoadNewGame();
            }
        }

        public void OnDeleteSaveGameClicked(SaveSlot saveSlot)
        {
            Manager_Game.FindTransformRecursively(transform.parent, "ConfirmationPanel").GetComponent<SaveSlot_Confirmation>().ActivateMenu(
                "Are you sure you want to clear this data?",
                () => {
                    DataPersistence_Manager.CurrentProfile.DeleteSave(saveSlot.GetSaveGameName());
                    ActivateMenu(_menuMain);
                },
                () => {
                    ActivateMenu(_menuMain);
                }
            );
        }

        private void _saveGameAndLoadNewGame()
        {
            DataPersistence_Manager.SaveGame("");
            _menuMain.NewGame();
        }

        public void ActivateMenu(Menu_Main menuMain)
        {
            _menuMain = menuMain;

            _saveSlots ??= new List<SaveSlot>();

            foreach (SaveSlot save in _saveSlots) Destroy(save.gameObject); _saveSlots.Clear();

            gameObject.SetActive(true);
            if (!_saveSlotParent) _saveSlotParent = Manager_Game.FindTransformRecursively(transform, "SavedGamesParent");
            if (!_saveSlot) _saveSlot = Manager_Game.FindTransformRecursively(transform, "SaveSlot").GetComponent<SaveSlot>();

            foreach (var saveData in DataPersistence_Manager.CurrentProfile.AllSavedData)
            {
                if (saveData.Key == "TheExister") continue;

                _saveSlots.Add(_createSaveSlot(saveData.Key, saveData.Value));
            }
        }

        SaveSlot _createSaveSlot(string profileID, Save_Data gameData)
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
}
