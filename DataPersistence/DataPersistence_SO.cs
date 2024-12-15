using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Actor;
using City;
using Faction;
using JobSite;
using OperatingArea;
using Region;
using Station;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace DataPersistence
{
    [CreateAssetMenu(fileName = "DataPersistence_SO", menuName = "SOList/DataPersistence_SO")]
    [Serializable]
    public class DataPersistence_SO : ScriptableObject
    {
        public bool DeleteGameOnStart;
        
        [Header("Debugging")]
        public bool DisableDataPersistence;
        public bool   CreateNewSaveFileIfNull        = true;
        public bool   SelectTestProfile              = true;
        public string CurrentTestSelectedProfileName = "test";

        public static string SaveFileName { get; private set; } = "Data.LightOfUrsus";

        [Header("File Storage Config")]
        public bool UseEncryption;

        public SaveData        CurrentSaveData                       { get; private set; }
        public void            SetCurrentSaveData(SaveData saveData) => CurrentSaveData = saveData;
        public bool            HasSaveData()                         => CurrentSaveData != null;
        List<IDataPersistence> _dataPersistenceObjects               { get { return new List<IDataPersistence>(FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None).OfType<IDataPersistence>()); } }
        [SerializeField] ProfileData     _currentProfile;
        public ProfileData CurrentProfile
        {
            get
            {
                if (_currentProfile.ProfileID == 0) return _currentProfile = GetMostRecentlyUpdatedProfile();
                return _currentProfile;
            }
            set => _currentProfile = value;
        }
        Dictionary<uint, ProfileData>        _allProfiles;
        public Dictionary<uint, ProfileData> AllProfiles { get { return _allProfiles ??= LoadAllProfiles(); } }

        uint _lastUnusedProfileID = 2;
        public uint GetRandomProfileID()
        {
            while (AllProfiles.ContainsKey(_lastUnusedProfileID))
            {
                _lastUnusedProfileID++;
            }

            return _lastUnusedProfileID;
        }

        public void ChangeProfile(uint profileID)
        {
            CurrentProfile = AllProfiles[profileID];

            if (CurrentProfile == null) { Debug.LogError("Profile not found."); return; }

            LoadGame("");
        }

        public void DeleteProfileData(string profileName)
        {
            var profileData = LoadProfileData(profileName);

            if (profileData == null) { Debug.LogError("Profile not found."); return; }

            profileData.DeleteProfile();
        
            CurrentProfile = GetMostRecentlyUpdatedProfile();
            LoadGame("");
        }

        public ProfileData GetMostRecentlyUpdatedProfile()
        {
            if (SelectTestProfile)
            {
                Debug.LogWarning("Overrode selected profile id with test id: " + CurrentTestSelectedProfileName);

                return new ProfileData(1, CurrentTestSelectedProfileName, 0, UseEncryption);
            }

            var mostRecentProfile = LoadAllProfilesLatestSave()
                                    .Where(p => p.Value != null)
                                    .OrderByDescending(p => DateTime.FromBinary(p.Value.SavedProfileData.LastUpdated))
                                    .Select(p => p.Key)
                                    .FirstOrDefault() ?? new ProfileData(GetRandomProfileID(), "Create New Profile", 0, UseEncryption);

            return mostRecentProfile;
        }

        public Dictionary<ProfileData, SaveData> LoadAllProfilesLatestSave()
        {
            Dictionary<ProfileData, SaveData> allProfilesLatestSave = new();

            foreach (var directoryInfo in new DirectoryInfo(Application.persistentDataPath).EnumerateDirectories().Where(d => d.Name != "Unity"))
            {
                var profileData = LoadProfileData(directoryInfo.Name);

                var latestSaveData = profileData.LoadData(GetLatestSave(directoryInfo.Name), directoryInfo.Name);

                if (latestSaveData == null) Debug.LogError($"SaveData is null for profile: {directoryInfo.Name}");

                allProfilesLatestSave.Add(profileData, latestSaveData);
            }

            return allProfilesLatestSave;
        }

        public Dictionary<uint, ProfileData> LoadAllProfiles()
        {
            Dictionary<uint, ProfileData> allProfiles = new();

            foreach (var directoryInfo in new DirectoryInfo(Application.persistentDataPath).EnumerateDirectories().Where(d => d.Name != "Unity"))
            {
                string profileDataFilePath = Path.Combine(directoryInfo.FullName, "ProfileData.json");

                if (File.Exists(profileDataFilePath))
                {
                    string      profileDataJson = File.ReadAllText(profileDataFilePath);
                    ProfileData profileData     = JsonUtility.FromJson<ProfileData>(profileDataJson);

                    allProfiles.Add(profileData.ProfileID, profileData);
                }
                else
                {
                    //Debug.Log("ProfileData File didn't exist, created");

                    var    newProfileData     = new ProfileData(1, directoryInfo.Name, 0, UseEncryption);
                    string newProfileDataJson = JsonUtility.ToJson(newProfileData);

                    File.WriteAllText(profileDataFilePath, newProfileDataJson);
                }
            }

            return allProfiles;
        }

        public void SaveProfileData(ProfileData profileData)
        {
            string profilePath = Path.Combine(Application.persistentDataPath, profileData.ProfileName);

            if (!Directory.Exists(profilePath)) Directory.CreateDirectory(profilePath);

            string profileDataFilePath = Path.Combine(profilePath, "ProfileData.json");

            if (File.Exists(profileDataFilePath)) File.Delete(profileDataFilePath);

            string profileDataJson = JsonUtility.ToJson(profileData);

            File.WriteAllText(profileDataFilePath, profileDataJson);
        }

        public ProfileData LoadProfileData(string profileName)
        {
            string profileDataFilePath = Path.Combine(Application.persistentDataPath, profileName, "ProfileData.json");

            if (!File.Exists(profileDataFilePath)) return null;

            string profileDataJson = File.ReadAllText(profileDataFilePath);

            return JsonUtility.FromJson<ProfileData>(profileDataJson);
        }

        public string GetLatestSave(string profileName)
        {
            string profilePath = Path.Combine(Application.persistentDataPath, profileName);

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

            if (CurrentSaveData == null) { Debug.LogWarning("No data was found. A New Game needs to be started before data can be saved."); return; }

            foreach (IDataPersistence data in _dataPersistenceObjects) data.SaveData(CurrentSaveData);

            CurrentSaveData.SavedProfileData.LastUpdated = DateTime.Now.ToBinary();

            SaveProfileData(CurrentProfile);

            CurrentProfile.SaveData(saveDataName, CurrentSaveData, CurrentProfile.ProfileName);
        }

        public void LoadGame(string saveDataName)
        {
            if (DisableDataPersistence || CurrentProfile.ProfileName == "Create New Profile") return;

            if (string.IsNullOrEmpty(CurrentProfile.ProfileName)) { Debug.LogError("Profile Name is null or empty."); return; }

            saveDataName = saveDataName == "" ? GetLatestSave(CurrentProfile.ProfileName) : saveDataName;

            CurrentSaveData = CurrentProfile.LoadData(saveDataName, CurrentProfile.ProfileName);

            if (CurrentSaveData == null && CreateNewSaveFileIfNull) CurrentSaveData = new SaveData(GetRandomProfileID(), CurrentProfile.ProfileName);

            if (CurrentSaveData == null) { Debug.Log("No data was found. A New Game needs to be started before data can be loaded."); return; }

            foreach (IDataPersistence data in _dataPersistenceObjects) data.LoadData(CurrentSaveData);
        }

        public SaveData GetLatestSaveData()
        {
            if (string.IsNullOrEmpty(CurrentProfile.ProfileName)) { Debug.LogError("Profile Name is null or empty."); return null; }

            CurrentSaveData = CurrentProfile.LoadData(GetLatestSave(CurrentProfile.ProfileName), CurrentProfile.ProfileName);

            if (CurrentSaveData == null) { Debug.Log("No data was found."); return null; }

            return CurrentSaveData;
        }

        public IEnumerator AutoSave(float autoSaveTimeSeconds, int numberOfAutoSaves, bool autoSaveEnabled)
        {
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
                Debug.LogError($"Failed to delete profile data for profileName: {CurrentTestSelectedProfileName} at path: {fullPath} \n {e}");
            }
        }
    }

    public interface IDataPersistence
    {
        void SaveData(SaveData data);
        void LoadData(SaveData data);
    }

    [Serializable]
    public class ProfileData
    {
        public uint                         ProfileID;
        public string                       ProfileName;
        Dictionary<string, SaveData>        _allSavedData;
        public Dictionary<string, SaveData> AllSavedData { get { return _allSavedData ??= GetAllSavedData(); } }
        bool                                _useEncryption;
        public   int                        AutoSaveCounter;
        readonly string                     _encryptionCodeWord = "word";
        readonly string                     _backupExtension    = ".bak";

        public ProfileData(uint profileID, string profileName, int autoSaveCounter, bool useEncryption)
        {
            ProfileID       = profileID;
            ProfileName     = profileName;
            AutoSaveCounter = autoSaveCounter;
            _useEncryption  = useEncryption;
        }

        public ProfileData(ProfileData profileData)
        {
            ProfileID           = profileData.ProfileID;
            ProfileName         = profileData.ProfileName;
            _useEncryption      = profileData._useEncryption;
            AutoSaveCounter     = profileData.AutoSaveCounter;
            _encryptionCodeWord = profileData._encryptionCodeWord;
            _backupExtension    = profileData._backupExtension;
        }

        public Dictionary<string, SaveData> GetAllSavedData()
        {
            Dictionary<string, SaveData> allSavedDatas = new();

            string profilePath = Path.Combine(Application.persistentDataPath, ProfileName);

            if (!Directory.Exists(profilePath))
            {
                Debug.LogWarning($"Directory not found: {profilePath}");
                return allSavedDatas;
            }

            foreach (var directoryInfo in new DirectoryInfo(profilePath).EnumerateDirectories())
            {
                SaveData saveData = LoadData(directoryInfo.Name, ProfileName);

                if (saveData != null) allSavedDatas.Add(directoryInfo.Name, saveData);
            }

            return allSavedDatas;
        }

        public SaveData LoadData(string saveDataName, string profileName, bool allowRestoreFromBackup = true)
        {
            if (string.IsNullOrEmpty(profileName) || string.IsNullOrEmpty(saveDataName)) return null;

            string profilePath     = Path.Combine(Application.persistentDataPath, profileName);
            string profileDataPath = Path.Combine(profilePath,                    "ProfileData.json");

            string profileData = File.ReadAllText(profileDataPath);

            if (_useEncryption) profileData = _encryptDecrypt(profileData);

            var savedProfileData = JsonUtility.FromJson<ProfileData>(profileData);

            SaveData saveData = new SaveData(savedProfileData.ProfileID, savedProfileData.ProfileName);

            string savePath = Path.Combine(profilePath, saveDataName);

            if (!Directory.Exists(savePath)) return null;

            try
            {
                _loadNestedRegionData(savePath, saveData);
                _loadNestedFactionData(savePath, saveData);

                return saveData;
            }
            catch (Exception e)
            {
                if (allowRestoreFromBackup && _attemptRollback(savePath))
                {
                    Debug.LogWarning($"Failed to load data file. Attempting to roll back.\n{e}");
                    return LoadData(saveDataName, profileName, false);
                }

                Debug.LogError($"Error occurred when trying to load file: {savePath}. Backup did not work.\n{e}");
                return null;
            }
        }

        void _loadNestedRegionData(string savePath, SaveData saveData)
        {
            var regionPath     = Path.Combine(savePath, "Regions");
            var    regionDataJson = JsonUtility.FromJson<SavedRegionData>(_fromJSON(regionPath, "RegionSaveData.json"));
            saveData.SavedRegionData = new SavedRegionData(regionDataJson?.AllRegionData);

            var cityPath     = Path.Combine(regionPath, "Cities");
            var    cityDataJson = JsonUtility.FromJson<SavedCityData>(_fromJSON(cityPath, "CitySaveData.json"));
            saveData.SavedCityData = new SavedCityData(cityDataJson?.AllCityData);

            var jobsitePath     = Path.Combine(cityPath, "Jobsites");
            var    jobsiteDataJson = JsonUtility.FromJson<SavedJobSiteData>(_fromJSON(jobsitePath, "JobsiteSaveData.json"));
            saveData.SavedJobSiteData = new SavedJobSiteData(jobsiteDataJson?.AllJobSiteData);

            var stationPath     = Path.Combine(jobsitePath, "Stations");
            var    stationDataJson = JsonUtility.FromJson<SavedStationData>(_fromJSON(stationPath, "StationSaveData.json"));
            saveData.SavedStationData = new SavedStationData(stationDataJson?.AllStationData);

            var operatingAreaPath     = Path.Combine(stationPath, "OperatingAreas");
            var    operatingAreaDataJson = JsonUtility.FromJson<SavedOperatingAreaData>(_fromJSON(operatingAreaPath, "OperatingAreaSaveData.json"));
            saveData.SavedOperatingAreaData = new SavedOperatingAreaData(operatingAreaDataJson?.AllOperatingAreaData);
        }

        void _loadNestedFactionData(string savePath, SaveData saveData)
        {
            var allFactionsDirectoryPath = Path.Combine(savePath, "Factions");

            var factionSaveDataJson = JsonUtility.FromJson<SavedFactionData>(_fromJSON(allFactionsDirectoryPath, "AllFactionsSaveData.json"));

            try
            {
                saveData.SavedFactionData = new SavedFactionData(factionSaveDataJson.AllFactionData);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error occurred when trying to load faction data: {e}");
                return;
            }
        
            var allFactionDirectories = Directory.GetDirectories(allFactionsDirectoryPath);
            var allSavedFactionData   = new Faction_Data[allFactionDirectories.Length];

            for(var i = 0; i < allFactionDirectories.Length; i++)
            {
                var factionDirectoryPath = allFactionDirectories[i];
            
                var factionData = JsonUtility.FromJson<Faction_Data>(_fromJSON(factionDirectoryPath, "FactionSaveData.json"));

                if (saveData.SavedFactionData.AllFactionData.All(f => f.FactionID != factionData.FactionID))
                {
                    try
                    {
                        allSavedFactionData[i] = factionData;
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Error occurred when trying to load faction data: {e}");
                        return;
                    }
                }

                var actorsPath = Path.Combine(factionDirectoryPath, "Actors");

                if (Directory.Exists(actorsPath))
                {
                    var actorFiles        = Directory.GetFiles(actorsPath, "Actor_*_SaveData.json");
                    var allSavedActorData = new Actor_Data[actorFiles.Length];

                    for (var j = 0; j < actorFiles.Length; j++)
                    {
                        var actorFile     = actorFiles[j];
                        var actorDataJson = File.ReadAllText(actorFile);
                    
                        if (_useEncryption) actorDataJson = _encryptDecrypt(actorDataJson);
                        var actorData                     = JsonUtility.FromJson<Actor_Data>(actorDataJson);

                        try { allSavedActorData[j] = actorData; }
                        catch (Exception e) { Debug.LogError($"Error occurred when trying to load actor data: {e}"); }
                    }

                    saveData.SavedActorData = new SavedActorData(allSavedActorData);
                }
                else
                {
                    Directory.CreateDirectory(actorsPath);
                    Debug.LogWarning($"No actors found for faction: {factionData.FactionName}");
                }
            }

            saveData.SavedFactionData.AllFactionData = allSavedFactionData;
        }

        string _fromJSON(string path, string extension)
        {
            string pathToLoad = Path.Combine(path, extension);
            if (!File.Exists(pathToLoad)) { Debug.Log($"File does not exist for path: {pathToLoad}"); return null; }
            string jsonData              = File.ReadAllText(pathToLoad);
            if (_useEncryption) jsonData = _encryptDecrypt(jsonData);
            return jsonData;
        }

        public void SaveData(string saveDataName, SaveData saveData, string profileName, bool newProfile = false)
        {
            if (profileName == null || SceneManager.GetActiveScene().name == "Main_Menu" && !newProfile) return;

            if (string.IsNullOrEmpty(saveDataName)) saveDataName = $"NewSave_{AllSavedData.Count}";

            string profilePath  = Path.Combine(Application.persistentDataPath, profileName);
            string saveGamePath = Path.Combine(profilePath,                    saveDataName);

            try
            {
                Directory.CreateDirectory(profilePath);
                Directory.CreateDirectory(saveGamePath);

                _saveNestedRegionData(saveGamePath, saveData);

                _saveNestedFactionData(saveGamePath, saveData);

                if (LoadData(saveDataName, profileName) != null)
                {
                    var    backupExtension     = string.IsNullOrEmpty(_backupExtension) ? ".bak" : _backupExtension;
                    string backupDirectoryPath = saveGamePath + backupExtension;
                    _copyDirectory(saveGamePath, backupDirectoryPath);
                }
                else
                {
                    throw new Exception("Save file could not be verified and backup could not be created.");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error occurred when trying to save data to file: {saveGamePath} \n {e}.");
            }
        }

        void _saveNestedRegionData(string savePath, SaveData saveData)
        {
            string regionPath  = _toJSON(savePath,    "Regions",  "RegionSaveData.json",  saveData.SavedRegionData);
            string cityPath    = _toJSON(regionPath,  "Cities",   "CitySaveData.json",    saveData.SavedCityData);
            string jobsitePath = _toJSON(cityPath,    "Jobsites", "JobsiteSaveData.json", saveData.SavedJobSiteData);
            string stationPath = _toJSON(jobsitePath, "Stations", "StationSaveData.json", saveData.SavedStationData);
            _toJSON(stationPath, "OperatingAreas", "OperatingAreaSaveData.json", saveData.SavedOperatingAreaData);
        }

        void _saveNestedFactionData(string savePath, SaveData saveData)
        {
            string factionDataPath = _toJSON(savePath, "Factions", "AllFactionsSaveData.json", saveData.SavedFactionData);

            HashSet<uint> factionActorIDs = new HashSet<uint>();

            foreach (var factionData in saveData.SavedFactionData.AllFactionData)
            {
                var factionPath = _toJSON(factionDataPath, factionData.FactionName, "FactionSaveData.json", factionData);

                var actorsInFaction = saveData.SavedActorData.AllActorData.Where(actor => actor.ActorFactionID == factionData.FactionID).ToList();
                Directory.CreateDirectory(Path.Combine(factionPath, "Actors"));

                foreach (var actorData in actorsInFaction)
                {
                    _toJSON(factionPath, "Actors", $"Actor_{actorData.ActorID}_SaveData.json", actorData);
                    factionActorIDs.Add(actorData.ActorID);
                }
            }

            var factionlessActors = saveData.SavedActorData.AllActorData.Where(actor => !factionActorIDs.Contains(actor.ActorID)).ToList();

            foreach (var actorData in factionlessActors)
            {
                Debug.LogWarning($"Actor: {actorData.ActorID}: {actorData.ActorName} is factionless.");
                _toJSON(Path.Combine(factionDataPath, "Wanderers"), "Actors", $"Actor_{actorData.ActorID}_SaveData.json", actorData);
            }
        }

        string _toJSON(string currentPath, string pathToAdd, string pathExtension, object data)
        {
            string path = Path.Combine(currentPath, pathToAdd);
            Directory.CreateDirectory(path);
            string jsonData              = JsonUtility.ToJson(data, true);
            if (_useEncryption) jsonData = _encryptDecrypt(jsonData);
            File.WriteAllText(Path.Combine(path, pathExtension), jsonData);

            return path;
        }

        void _copyDirectory(string sourceDir, string destDir)
        {
            Directory.CreateDirectory(destDir);

            foreach (string file in Directory.GetFiles(sourceDir))
            {
                string destFile = Path.Combine(destDir, Path.GetFileName(file));
                File.Copy(file, destFile, true);
            }

            foreach (string subdir in Directory.GetDirectories(sourceDir))
            {
                string destSubdir = Path.Combine(destDir, Path.GetFileName(subdir));
                _copyDirectory(subdir, destSubdir);
            }
        }

        public void DeleteProfile()
        {
            string fullPath = Path.Combine(Application.persistentDataPath, ProfileName);

            try
            {
                if (Directory.Exists(fullPath)) Directory.Delete(Path.GetDirectoryName(fullPath), true);

                else Debug.LogWarning($"Tried to delete profile data, but data was not found: {fullPath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to delete profile data for profileName: {ProfileName} at path: {fullPath} \n {e}");
            }
        }

        public void DeleteSave(string saveGameName)
        {
            string fullPath = Path.Combine(Application.persistentDataPath, ProfileName, saveGameName);

            try
            {
                if (Directory.Exists(fullPath)) Directory.Delete(fullPath, true);

                else Debug.LogWarning($"Tried to delete profile data, but data was not found: {fullPath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to delete profile data for profileName: {ProfileName} at path: {fullPath} \n {e}");
            }
        }

        string _encryptDecrypt(string data)
        {
            return new string(data.Select((c, i) => (char)(c ^ _encryptionCodeWord[i % _encryptionCodeWord.Length])).ToArray());
        }

        bool _attemptRollback(string fullPath)
        {
            var backupExtension = string.IsNullOrEmpty(_backupExtension) ? ".bak" : _backupExtension;

            string backupFilePath = fullPath + backupExtension;

            if (!File.Exists(backupFilePath))
            {
                Debug.LogError("Tried to roll back, but no backup file exists.");
                return false;
            }

            try
            {
                File.Copy(backupFilePath, fullPath, true);
                Debug.LogWarning($"Rolled back to backup file at: {backupFilePath}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error occured when trying to roll back to backup file at: {backupFilePath} \n {e}");
                return false;
            }
        }
    }

    [Serializable]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField] List<TKey>   _keys   = new();
        [SerializeField] List<TValue> _values = new();

        public void OnBeforeSerialize()
        {
            _keys.Clear();
            _values.Clear();

            foreach (KeyValuePair<TKey, TValue> pair in this)
            {
                _keys.Add(pair.Key);
                _values.Add(pair.Value);
            }
        }

        public void OnAfterDeserialize()
        {
            Clear();

            if (_keys.Count != _values.Count) { Debug.LogError($"Key count: {_keys.Count} does not match value count: {_values.Count}"); }

            for (int i = 0; i < _keys.Count; i++) Add(_keys[i], _values[i]);
        }
    }

    [Serializable]
    public class SaveData
    {
        // Profile Info
        public SavedProfileData SavedProfileData;    

        // All Game Info
        public                                            SavedRegionData        SavedRegionData;
        public                                            SavedCityData          SavedCityData;
        [FormerlySerializedAs("SavedJobsiteData")] public SavedJobSiteData       SavedJobSiteData;
        public                                            SavedStationData       SavedStationData;
        public                                            SavedOperatingAreaData SavedOperatingAreaData;
        public                                            SavedFactionData       SavedFactionData;
        public                                            SavedActorData         SavedActorData;
        public                                            SavedOrderData         SavedOrderData;

        // Player info
        public Vector3 PlayerPosition;
        public bool    StaffPickedUp;

        // Current Scene Info
        public string SceneName;
        public string LastScene;

        // public SerializableDictionary<string, string> QuestSaveData;
        public List<QuestData> QuestData;
        // Try replace with a list of serialised classes instead.
        //public SerializableDictionary<string, string> PuzzleSaveData;
        public List<PuzzleData> PuzzleData;
        // Try replace with a list of serialised classes instead.

        public SaveData(uint currentProfileID, string currentProfileName)
        {
            SavedProfileData = new SavedProfileData(currentProfileID, currentProfileName);
        }
    }

    [Serializable]
    public class SavedProfileData
    {
        public long   LastUpdated;
        public uint   SaveDataID;
        public string SaveDataName;
        public uint   ProfileID;
        public string ProfileName;

        public SavedProfileData(uint profileID, string profileName)
        {
            ProfileID   = profileID;
            ProfileName = profileName;
        }

        public SavedProfileData(SavedProfileData profileData)
        {
            LastUpdated  = profileData.LastUpdated;
            SaveDataID   = profileData.SaveDataID;
            SaveDataName = profileData.SaveDataName;
            ProfileID    = profileData.ProfileID;
            ProfileName  = profileData.ProfileName;
        }
    }

    [Serializable]
    public class SavedRegionData
    {
        public readonly Region_Data[] AllRegionData;

        public SavedRegionData(Region_Data[] allRegionData) => AllRegionData = allRegionData;
    }

    [Serializable]
    public class SavedCityData
    {
        public readonly City_Data[] AllCityData;

        public SavedCityData(City_Data[] allCityData) => AllCityData = allCityData;
    }

    [Serializable]
    public class SavedJobSiteData
    {
        public readonly JobSite_Data[] AllJobSiteData;

        public SavedJobSiteData(JobSite_Data[] allJobSiteData) => AllJobSiteData = allJobSiteData;
    }

    [Serializable]
    public class SavedStationData
    {
        public readonly Station_Data[] AllStationData;

        public SavedStationData(Station_Data[] allStationData) => AllStationData = allStationData;
    }

    [Serializable]
    public class SavedOperatingAreaData
    {
        public List<OperatingAreaData> AllOperatingAreaData;

        public SavedOperatingAreaData(List<OperatingAreaData> allOperatingAreaData) => AllOperatingAreaData = allOperatingAreaData;
    }

    [Serializable]
    public class SavedFactionData
    {
        public Faction_Data[] AllFactionData;
        public SavedFactionData(Faction_Data[] allFactionData) => AllFactionData = allFactionData;
    }

    public class SavedActorData
    {
        public readonly Actor_Data[] AllActorData;

        public SavedActorData(Actor_Data[] allActorData) => AllActorData = allActorData;
    }

    public class SavedOrderData
    {
        public readonly OrderData[] AllOrderData;

        public SavedOrderData(OrderData[] allOrderData) => AllOrderData = allOrderData;
    }

    public abstract class DataPersistenceManager
    {
        static        DataPersistence_SO _dataPersistence_SO;

        public static DataPersistence_SO DataPersistence_SO
        {
            get
            {
                return _dataPersistence_SO ??=
                    Resources.Load<DataPersistence_SO>("ScriptableObjects/DataPersistence_SO");
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

            if (GUILayout.Button("Load Profile"))
            {
                if (dataPersistenceSO.AllProfiles.Values.Any(p => p.ProfileName == _profileName))
                {
                    ProfileData profileData = dataPersistenceSO.LoadProfileData(_profileName);
                    if (profileData != null)
                    {
                        dataPersistenceSO.CurrentProfile = profileData;
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
    }
}