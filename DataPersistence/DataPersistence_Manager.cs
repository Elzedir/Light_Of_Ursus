using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DataPersistence
{
    public abstract class DataPersistence_Manager
    {
        const string _dataPersistence_SOPath = "ScriptableObjects/DataPersistence_SO";
        
        static DataPersistence_SO _dataPersistence_SO;
        static DataPersistence_SO DataPersistence_SO => _dataPersistence_SO ??= _getDataPersistence_SO();

        static DataPersistence_SO _getDataPersistence_SO()
        {
            var dataPersistence_SO = Resources.Load<DataPersistence_SO>(_dataPersistence_SOPath);

            if (dataPersistence_SO is not null) return dataPersistence_SO;

            Debug.LogError("DataPersistence_SO not found. Creating temporary DataPersistence_SO.");
            dataPersistence_SO = ScriptableObject.CreateInstance<DataPersistence_SO>();

            return dataPersistence_SO;
        }
        
        public static Save_Data CurrentSaveData => DataPersistence_SO.CurrentSaveData;
        public static Profile_Data CurrentProfile => DataPersistence_SO.CurrentProfile;
        public static Dictionary<ulong, Profile_Data> AllProfiles => DataPersistence_SO.AllProfiles;
        public static bool DeleteGameOnStart => DataPersistence_SO.DeleteGameOnStart;
        
        public static void SaveGame(string saveDataName) => DataPersistence_SO.SaveGame(saveDataName);
        public static void LoadGame(string saveDataName) => DataPersistence_SO.LoadGame(saveDataName);
        public static void ChangeProfile(ulong profileID) => DataPersistence_SO.ChangeProfile(profileID);
        public static void SetCurrentSaveData(Save_Data saveData) => DataPersistence_SO.SetCurrentSaveData(saveData);
        public static void DeleteTestSaveFile() => DataPersistence_SO.DeleteTestSaveFile();
        public static bool HasSaveData() => DataPersistence_SO.HasSaveData();
        public static IEnumerator AutoSave(float autoSaveTimeSeconds, int numberOfAutoSaves, bool autoSaveEnabled) =>
            DataPersistence_SO.AutoSave(autoSaveTimeSeconds, numberOfAutoSaves, autoSaveEnabled);
    }
}