using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DataPersistence
{
    [CreateAssetMenu(fileName = "DataPersistence_SO", menuName = "SOList/DataPersistence_SO")]
    [Serializable]
    public class DataPersistence_SO : ScriptableObject
    {
        public bool DeleteGameOnStart;
        public bool ClearAllSOsOnStart;

        [Header("Debugging")] public bool DisableDataPersistence;
        public bool CreateNewSaveFileIfNull = true;
        public bool SelectTestProfile = true;
        public string CurrentTestSelectedProfileName = "test";

        public static string SaveFileName { get; private set; } = "Data.LightOfUrsus";

        [Header("File Storage Config")] public bool UseEncryption;

        public Save_Data CurrentSaveData { get; private set; }
        public void SetCurrentSaveData(Save_Data saveData) => CurrentSaveData = saveData;
        public bool HasSaveData() => CurrentSaveData != null;

        HashSet<IDataPersistence> _allDataPersistenceObjects = new();

        [SerializeField] Profile_Data _currentProfile;

        public Profile_Data CurrentProfile => _currentProfile.ProfileID != 0
            ? _currentProfile
            : _currentProfile = GetMostRecentlyUpdatedProfile();
        
        public void SetCurrentProfile(Profile_Data profileData) => _currentProfile = profileData;

        Dictionary<ulong, Profile_Data> _allProfiles;
        public Dictionary<ulong, Profile_Data> AllProfiles =>_allProfiles ??= LoadAllProfiles();

        ulong _lastUnusedProfileID = 2;
        
        public static Action RegisterDataPersistenceObject;

        public ulong GetRandomProfileID()
        {
            while (AllProfiles.ContainsKey(_lastUnusedProfileID))
            {
                _lastUnusedProfileID++;
            }

            return _lastUnusedProfileID;
        }

        public void ChangeProfile(ulong profileID)
        {
            SetCurrentProfile(AllProfiles[profileID]);

            if (CurrentProfile == null)
            {
                Debug.LogError("Profile not found.");
                return;
            }

            LoadGame("");
        }

        public void DeleteProfileData(string profileName)
        {
            var profileData = LoadProfileData(profileName);

            if (profileData == null)
            {
                Debug.LogError("Profile not found.");
                return;
            }

            profileData.DeleteProfile();

            SetCurrentProfile(GetMostRecentlyUpdatedProfile());
            LoadGame("");
        }

        public Profile_Data GetMostRecentlyUpdatedProfile()
        {
            if (SelectTestProfile)
            {
                Debug.LogWarning("Overrode selected profile id with test id: " + CurrentTestSelectedProfileName);

                return new Profile_Data(1, CurrentTestSelectedProfileName, 0, UseEncryption);
            }

            var mostRecentProfile = LoadAllProfilesLatestSave()
                .Where(p => p.Value != null)
                .OrderByDescending(p => DateTime.FromBinary(p.Value.SavedProfileData.LastUpdated))
                .Select(p => p.Key)
                .FirstOrDefault() ?? new Profile_Data(GetRandomProfileID(), "Create New Profile", 0, UseEncryption);

            return mostRecentProfile;
        }

        public Dictionary<Profile_Data, Save_Data> LoadAllProfilesLatestSave()
        {
            Dictionary<Profile_Data, Save_Data> allProfilesLatestSave = new();

            foreach (var directoryInfo in new DirectoryInfo(Application.persistentDataPath).EnumerateDirectories()
                         .Where(d => d.Name != "Unity"))
            {
                var profileData = LoadProfileData(directoryInfo.Name);

                var latestSaveData = profileData.LoadData(GetLatestSave(directoryInfo.Name), directoryInfo.Name);

                if (latestSaveData == null) Debug.LogError($"SaveData is null for profile: {directoryInfo.Name}");

                allProfilesLatestSave.Add(profileData, latestSaveData);
            }

            return allProfilesLatestSave;
        }

        public Dictionary<ulong, Profile_Data> LoadAllProfiles()
        {
            Dictionary<ulong, Profile_Data> allProfiles = new();

            foreach (var directoryInfo in new DirectoryInfo(Application.persistentDataPath).EnumerateDirectories()
                         .Where(d => d.Name != "Unity"))
            {
                var profileDataFilePath = Path.Combine(directoryInfo.FullName, "ProfileData.json");

                if (File.Exists(profileDataFilePath))
                {
                    var profileDataJson = File.ReadAllText(profileDataFilePath);
                    var profileData = JsonUtility.FromJson<Profile_Data>(profileDataJson);

                    allProfiles.Add(profileData.ProfileID, profileData);
                }
                else
                {
                    //Debug.Log("ProfileData File didn't exist, created");

                    var newProfileData = new Profile_Data(1, directoryInfo.Name, 0, UseEncryption);
                    var newProfileDataJson = JsonUtility.ToJson(newProfileData);

                    File.WriteAllText(profileDataFilePath, newProfileDataJson);
                }
            }

            return allProfiles;
        }

        public void SaveProfileData(Profile_Data profileData)
        {
            var profilePath = Path.Combine(Application.persistentDataPath, profileData.ProfileName);

            if (!Directory.Exists(profilePath)) Directory.CreateDirectory(profilePath);

            var profileDataFilePath = Path.Combine(profilePath, "ProfileData.json");

            if (File.Exists(profileDataFilePath)) File.Delete(profileDataFilePath);

            var profileDataJson = JsonUtility.ToJson(profileData);

            File.WriteAllText(profileDataFilePath, profileDataJson);
        }

        public Profile_Data LoadProfileData(string profileName)
        {
            var profileDataFilePath = Path.Combine(Application.persistentDataPath, profileName, "ProfileData.json");

            if (!File.Exists(profileDataFilePath)) return null;

            var profileDataJson = File.ReadAllText(profileDataFilePath);

            return JsonUtility.FromJson<Profile_Data>(profileDataJson);
        }

        public string GetLatestSave(string profileName)
        {
            var profilePath = Path.Combine(Application.persistentDataPath, profileName);

            if (!Directory.Exists(profilePath))
            {
                Debug.LogWarning($"Directory not found: {profilePath}");

                if (profileName != CurrentTestSelectedProfileName) return null;
                Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, CurrentProfile.ProfileName));
            }

            return new DirectoryInfo(profilePath)
                .EnumerateDirectories()
                .OrderByDescending(d => d?.LastWriteTime)
                .Select(d => d?.Name)
                .FirstOrDefault();
        }

        public void SaveGame(string saveDataName)
        {
            if (DisableDataPersistence) return;

            if (CurrentSaveData == null)
            {
                Debug.LogWarning("No data was found. A New Game needs to be started before data can be saved.");
                return;
            }

            foreach (var data in _allDataPersistenceObjects) data.SaveData(CurrentSaveData);

            CurrentSaveData.SavedProfileData.LastUpdated = DateTime.Now.ToBinary();

            SaveProfileData(CurrentProfile);

            CurrentProfile.SaveData(saveDataName, CurrentSaveData, CurrentProfile.ProfileName);
        }

        public void LoadGame(string saveDataName)
        {
            if (DisableDataPersistence || CurrentProfile.ProfileName == "Create New Profile") return;

            if (string.IsNullOrEmpty(CurrentProfile.ProfileName))
            {
                Debug.LogError("Profile Name is null or empty.");
                return;
            }

            saveDataName = saveDataName == "" ? GetLatestSave(CurrentProfile.ProfileName) : saveDataName;

            CurrentSaveData = CurrentProfile.LoadData(saveDataName, CurrentProfile.ProfileName);

            if (CurrentSaveData == null && CreateNewSaveFileIfNull)
                CurrentSaveData = new Save_Data(GetRandomProfileID(), CurrentProfile.ProfileName);

            if (CurrentSaveData != null) return;

            Debug.Log("No data was found. A New Game needs to be started before data can be loaded.");
        }

        public Save_Data GetLatestSaveData()
        {
            if (string.IsNullOrEmpty(CurrentProfile.ProfileName))
            {
                Debug.LogError("Profile Name is null or empty.");
                return null;
            }

            CurrentSaveData =
                CurrentProfile.LoadData(GetLatestSave(CurrentProfile.ProfileName), CurrentProfile.ProfileName);

            if (CurrentSaveData != null) return CurrentSaveData;
            
            Debug.Log("No data was found.");
            return null;
        }

        public IEnumerator AutoSave(float autoSaveTimeSeconds, int numberOfAutoSaves, bool autoSaveEnabled)
        {
            //* Change the autosave feature to start and stop at specific times, like outside of menus or combat.
            while (true)
            {
                yield return new WaitForSeconds(autoSaveTimeSeconds);

                if (!autoSaveEnabled) continue;

                SaveGame($"AutoSave_{CurrentProfile.AutoSaveCounter}");
                CurrentProfile.AutoSaveCounter = CurrentProfile.AutoSaveCounter % numberOfAutoSaves + 1;
                Debug.Log("Auto Saved Game");
            }
        }

        public void DeleteTestSaveFile()
        {
            var fullPath = Path.Combine(Application.persistentDataPath, CurrentTestSelectedProfileName);

            try
            {
                if (Directory.Exists(fullPath))
                {
                    Directory.Delete(Path.GetDirectoryName(fullPath), true);
                    Debug.Log("Deleted Test Save File");
                }

                else Debug.LogWarning($"Tried to delete profile data, but data was not found: {fullPath}");
            }
            catch (Exception e)
            {
                Debug.LogError(
                    $"Failed to delete profile data for profileName: {CurrentTestSelectedProfileName} at path: {fullPath} \n {e}");
            }
        }
    }

    [CustomEditor(typeof(DataPersistence_SO))]
    public class DataPersistence_SOEditor : Editor
    {
        string _profileName = "";

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            DataPersistence_SO dataPersistenceSO = (DataPersistence_SO)target;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Profile Management", EditorStyles.boldLabel);

            _profileName = EditorGUILayout.TextField("Profile Name", _profileName);

            if (!GUILayout.Button("Load Profile")) return;
            
            if (dataPersistenceSO.AllProfiles.Values.Any(p => p.ProfileName == _profileName))
            {
                var profileData = dataPersistenceSO.LoadProfileData(_profileName);
                    
                if (profileData != null)
                {
                    dataPersistenceSO.SetCurrentProfile(profileData);
                    dataPersistenceSO.LoadGame("");
                    Debug.Log($"Profile '{_profileName}' loaded successfully.");
                }
                else
                {
                    Debug.LogError($"Profile '{_profileName}' could not be loaded.");
                }
            }
            else
            {
                Debug.LogError($"Profile '{_profileName}' does not exist.");
            }
        }
    }
    
    public interface IDataPersistence
    {
        void SaveData(Save_Data saveData);
    }
}