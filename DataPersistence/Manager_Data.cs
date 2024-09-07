using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using UnityEngine.SceneManagement;

public class Manager_Data : MonoBehaviour
{
    [Header("Debugging")]
    [SerializeField] bool _disableDataPersistence = false;
    [SerializeField] bool _createNewSaveFileIfNull = true;
    [SerializeField] bool _selectTestProfile = true;
    [SerializeField] string _currentTestSelectedProfileName = "test";

    public static string SaveFileName { get; private set; } = "Data.LightOfUrsus";

    [Header("File Storage Config")]
    [SerializeField] public bool _useEncryption;

    [Header("Auto Saving Configuration")]
    [SerializeField] bool _autoSaveEnabled = false;
    [SerializeField] float _autoSaveTimeSeconds = 60f;
    [SerializeField] int _numberOfAutoSaves = 5;

    public SaveData CurrentSaveData { get; private set; }
    public void SetCurrentSaveData(SaveData saveData) => CurrentSaveData = saveData;
    public bool HasSaveData() => CurrentSaveData != null;
    List<IDataPersistence> _dataPersistenceObjects { get { return new List<IDataPersistence>(FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None).OfType<IDataPersistence>()); } }
    public ProfileData CurrentProfile { get; private set; }
    public Dictionary<int, ProfileData> AllProfiles;
    public void SetAllProfiles(Dictionary<int, ProfileData> allProfiles) { AllProfiles = allProfiles; }
    Coroutine _autoSaveCoroutine;

    HashSet<int> _allProfileIDs = new();
    int _lastUnusedProfileID = 1;
    public int GetRandomProfileID()
    {
        while (!_allProfileIDs.Add(_lastUnusedProfileID))
        {
            _lastUnusedProfileID++;
        }

        return _lastUnusedProfileID;
    }

    public static Manager_Data Instance { get; private set; }

    public void OnSceneLoaded()
    {
        _initialise();

        Debug.Log("Loaded game data for profile: " + CurrentProfile.ProfileName);

        LoadGame("");

        if (_autoSaveCoroutine != null) StopCoroutine(_autoSaveCoroutine);
        _autoSaveCoroutine = StartCoroutine(_autoSave());
    }

    void _initialise()
    {
        if (Instance == null) { Instance = this; }

        if (_disableDataPersistence) Debug.LogWarning("Data Persistence is currently disabled!");

        AllProfiles = LoadAllProfiles();

        CurrentProfile = _initializeProfiles();
    }

    public void ChangeProfile(int profileID)
    {
        CurrentProfile = AllProfiles[profileID];

        if (CurrentProfile == null) { Debug.LogError("Profile not found."); return; }

        LoadGame("");
    }

    public void DeleteProfileData(ProfileData profileData)
    {
        profileData.DeleteProfile();
        _initializeProfiles();
        LoadGame("");
    }

    ProfileData _initializeProfiles()
    {
        // Load the profile ID's near here with the profiles.

        if (!_selectTestProfile) return GetMostRecentlyUpdatedProfile();

        Debug.LogWarning("Overrode selected profile id with test id: " + _currentTestSelectedProfileName);
        
        return new ProfileData(0, _currentTestSelectedProfileName, 0, null, _useEncryption);
    }

    public ProfileData GetMostRecentlyUpdatedProfile()
    {
        var mostRecentProfile = LoadAllProfilesLatestSave()
            .Where(p => p.Value != null)
            .OrderByDescending(p => DateTime.FromBinary(p.Value.SavedProfileData.LastUpdated))
            .Select(p => p.Key)
            .FirstOrDefault();

        if (mostRecentProfile == null)
        {
            mostRecentProfile = new ProfileData(GetRandomProfileID(), "Create New Profile", 0, null, _useEncryption);
        }

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

            _allProfileIDs.Add(profileData.ProfileID);
        }

        return allProfilesLatestSave;
    }

    public Dictionary<int, ProfileData> LoadAllProfiles()
    {
        Dictionary<int, ProfileData> allProfiles = new();

        Debug.Log("Loaded all profiles.");

        foreach (var directoryInfo in new DirectoryInfo(Application.persistentDataPath).EnumerateDirectories().Where(d => d.Name != "Unity"))
        {
            string profileDataFilePath = Path.Combine(directoryInfo.FullName, "ProfileData.json");

            if (File.Exists(profileDataFilePath))
            {
                string profileDataJson = File.ReadAllText(profileDataFilePath);
                ProfileData profileData = JsonUtility.FromJson<ProfileData>(profileDataJson);

                allProfiles.Add(profileData.ProfileID, profileData);
                _allProfileIDs.Add(profileData.ProfileID);
            }
            else
            {

                Debug.Log("ProfileData File didn't exist, created");

                var newProfileData = new ProfileData(GetRandomProfileID(), directoryInfo.Name, 0, null, _useEncryption);
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

            if (profileName != _currentTestSelectedProfileName) return null;
            else Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, CurrentProfile.ProfileName));
        }

        return new DirectoryInfo(profilePath)
            .EnumerateDirectories()
            .OrderByDescending(d => d?.LastWriteTime)
            .Select(d => d?.Name)
            .FirstOrDefault();
    }

    public void SaveGame(string saveDataName)
    {
        if (_disableDataPersistence) return;

        if (CurrentSaveData == null) { Debug.LogWarning("No data was found. A New Game needs to be started before data can be saved."); return; }

        foreach (IDataPersistence data in _dataPersistenceObjects) data.SaveData(CurrentSaveData);

        CurrentSaveData.SavedProfileData.LastUpdated = DateTime.Now.ToBinary();

        SaveProfileData(CurrentProfile);

        CurrentProfile.SaveData(saveDataName, CurrentSaveData, CurrentProfile.ProfileName);
    }

    public void LoadGame(string saveDataName)
    {
        if (_disableDataPersistence || CurrentProfile.ProfileName == "Create New Profile") return;

        Debug.Log("SaveGameName" + saveDataName);

        saveDataName = saveDataName == "" ? GetLatestSave(CurrentProfile.ProfileName) : saveDataName;

        CurrentSaveData = CurrentProfile.LoadData(saveDataName, CurrentProfile.ProfileName);

        Debug.Log($"CurrentSave file is {CurrentSaveData}");

        if (CurrentSaveData == null && _createNewSaveFileIfNull) CurrentSaveData = new SaveData(GetRandomProfileID(), CurrentProfile.ProfileName);

        if (CurrentSaveData == null) { Debug.Log("No data was found. A New Game needs to be started before data can be loaded."); return; }

        Debug.Log($"Loaded save Data: {CurrentSaveData}");

        foreach (IDataPersistence data in _dataPersistenceObjects) data.LoadData(CurrentSaveData);
    }

    void OnApplicationQuit()
    {
        Debug.Log("Data Saved");
        SaveGame("ExitSave");
    }

    IEnumerator _autoSave()
    {
        while (true)
        {
            yield return new WaitForSeconds(_autoSaveTimeSeconds);

            if (!_autoSaveEnabled) continue;

            SaveGame($"AutoSave_{CurrentProfile.AutoSaveCounter}");
            CurrentProfile.AutoSaveCounter = CurrentProfile.AutoSaveCounter % _numberOfAutoSaves + 1;
            Debug.Log("Auto Saved Game");
        }
    }
}

public interface IDataPersistence
{
    void SaveData(SaveData data);
    void LoadData(SaveData data);
}

public class ProfileData
{
    public int ProfileID;
    public string ProfileName;
    public Dictionary<string, SaveData> AllSavedDatas;
    bool _useEncryption = false;
    public int AutoSaveCounter;
    readonly string _encryptionCodeWord = "word";
    readonly string _backupExtension = ".bak";

    public ProfileData(int profileID, string profileName, int autoSaveCounter, Dictionary<string, SaveData> allSavedDatas, bool useEncryption)
    {
        ProfileName = profileName;
        AutoSaveCounter = autoSaveCounter;
        AllSavedDatas = allSavedDatas ?? new Dictionary<string, SaveData>();
        _useEncryption = useEncryption;
    }

    public ProfileData(ProfileData profileData)
    {
        ProfileID = profileData.ProfileID;
        ProfileName = profileData.ProfileName;
        AllSavedDatas = new(profileData.AllSavedDatas);
        _useEncryption = profileData._useEncryption;
        AutoSaveCounter = profileData.AutoSaveCounter;
        _encryptionCodeWord = profileData._encryptionCodeWord;
        _backupExtension = profileData._backupExtension;
    }

    public SaveData LoadData(string saveDataName, string profileName, bool allowRestoreFromBackup = true)
    {
        if (string.IsNullOrEmpty(profileName) || string.IsNullOrEmpty(saveDataName)) return null;

        string profilePath = Path.Combine(Application.persistentDataPath, profileName);
        string profileDataPath = Path.Combine(profilePath, "ProfileData.json");

        string profileData = File.ReadAllText(profileDataPath);

        Debug.Log($"Has data to load.");

        if (_useEncryption) profileData = _encryptDecrypt(profileData);

        var savedProfileData = JsonUtility.FromJson<ProfileData>(profileData);

        SaveData saveData = new SaveData(savedProfileData.ProfileID, savedProfileData.ProfileName);

        string savePath = Path.Combine(profilePath, saveDataName);

        Debug.Log($"Loading data from file: {savePath}");

        if (!Directory.Exists(savePath)) return null;

        Debug.Log($"Directory exists: {savePath}");

        try
        {
            Debug.Log($"Loading data from directory: {savePath}");

            LoadNestedRegionData(savePath, saveData);

            Debug.Log($"Loaded region data from directory: {savePath}");

            LoadNestedFactionData(savePath, saveData);

            Debug.Log($"Loaded faction data from directory: {savePath}");

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

    private void LoadNestedRegionData(string path, SaveData saveData)
    {
        string regionPath = Path.Combine(path, "Regions");
        var regionDataJSON = JsonUtility.FromJson<SavedRegionData>(_fromJSON(regionPath, "RegionSaveData.json"));
        saveData.SavedRegionData = new SavedRegionData(regionDataJSON?.AllRegionData);

        string cityPath = Path.Combine(regionPath, "Cities");
        var cityDataJSON = JsonUtility.FromJson<SavedCityData>(_fromJSON(cityPath, "CitySaveData.json"));
        saveData.SavedCityData = new SavedCityData(cityDataJSON?.AllCityData);

        string jobsitePath = Path.Combine(cityPath, "Jobsites");
        var jobsiteDataJSON = JsonUtility.FromJson<SavedJobsiteData>(_fromJSON(jobsitePath, "JobsiteSaveData.json"));
        saveData.SavedJobsiteData = new SavedJobsiteData(jobsiteDataJSON?.AllJobsiteData);

        string stationPath = Path.Combine(jobsitePath, "Stations");
        var stationDataJSON = JsonUtility.FromJson<SavedStationData>(_fromJSON(stationPath, "StationSaveData.json"));
        saveData.SavedStationData = new SavedStationData(stationDataJSON?.AllStationData);

        string operatingAreaPath = Path.Combine(stationPath, "OperatingAreas");
        var operatingAreaDataJSON = JsonUtility.FromJson<SavedOperatingAreaData>(_fromJSON(operatingAreaPath, "OperatingAreaSaveData.json"));
        saveData.SavedOperatingAreaData = new SavedOperatingAreaData(operatingAreaDataJSON?.AllOperatingAreaData);
    }

    private void LoadNestedFactionData(string path, SaveData saveData)
    {
        string factionDataPath = Path.Combine(path, "Factions");
        var factionDataJSON = JsonUtility.FromJson<SavedFactionData>(_fromJSON(factionDataPath, "FactionSaveData.json"));
        saveData.SavedFactionData = new SavedFactionData(factionDataJSON?.AllFactionData);

        foreach (var factionDir in Directory.GetDirectories(factionDataPath))
        {
            string factionPath = Path.Combine(factionDir, "FactionSaveData.json");

            if (File.Exists(path))
            {
                var factionData = JsonUtility.FromJson<FactionData>(_fromJSON(factionDir, "FactionSaveData.json"));
                saveData.SavedFactionData.AllFactionData.Add(factionData);

                string actorsPath = Path.Combine(factionDir, "Actors");
                if (Directory.Exists(actorsPath))
                {
                    foreach (var actorFile in Directory.GetFiles(actorsPath, "Actor_*_SaveData.json"))
                    {
                        string actorDataJson = File.ReadAllText(actorFile);
                        if (_useEncryption) actorDataJson = _encryptDecrypt(actorDataJson);
                        var actorData = JsonUtility.FromJson<ActorData>(actorDataJson);
                        saveData.SavedActorData.AllActorData.Add(actorData);
                    }
                }
            }
        }

        string factionlessPath = Path.Combine(factionDataPath, "Factionless", "Actors");
        if (Directory.Exists(factionlessPath))
        {
            foreach (var actorFile in Directory.GetFiles(factionlessPath, "Actor_*_SaveData.json"))
            {
                string actorDataJson = File.ReadAllText(actorFile);
                if (_useEncryption) actorDataJson = _encryptDecrypt(actorDataJson);
                var actorData = JsonUtility.FromJson<ActorData>(actorDataJson);
                saveData.SavedActorData.AllActorData.Add(actorData);
            }
        }
    }

    string _fromJSON(string path, string extension)
    {
        string pathToLoad = Path.Combine(path, extension);
        if (!File.Exists(pathToLoad)) { Debug.Log($"File does not exist for path: {pathToLoad}"); return null; }
        string jsonData = File.ReadAllText(pathToLoad);
        if (_useEncryption) jsonData = _encryptDecrypt(jsonData);
        return jsonData;
    }

    public void SaveData(string saveDataName, SaveData saveData, string profileName, bool newProfile = false)
    {
        if (profileName == null || SceneManager.GetActiveScene().name == "Main_Menu" && !newProfile) return;

        if (string.IsNullOrEmpty(saveDataName)) saveDataName = $"NewSave_{AllSavedDatas.Count}";

        string profilePath = Path.Combine(Application.persistentDataPath, profileName);
        string saveGamePath = Path.Combine(profilePath, saveDataName);

        try
        {
            Directory.CreateDirectory(profilePath);
            Directory.CreateDirectory(saveGamePath);

            _saveNestedRegionData(saveGamePath, saveData);

            Debug.Log($"Saved region data to file: {saveGamePath}");

            _saveNestedFactionData(saveGamePath, saveData);

            Debug.Log($"Saved faction data to file: {saveGamePath}");

            if (LoadData(saveDataName, profileName) != null)
            {
                Debug.Log($"Verified save file at: {saveGamePath}");
                
                string backupDirectoryPath = saveGamePath + _backupExtension;
                _copyDirectory(saveGamePath, backupDirectoryPath);
                Debug.Log($"Created backup directory at: {backupDirectoryPath}");
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
        string regionPath = _toJSON(savePath, "Regions", "RegionSaveData.json", saveData.SavedRegionData);
        string cityPath = _toJSON(regionPath, "Cities", "CitySaveData.json", saveData.SavedCityData);
        string jobsitePath = _toJSON(cityPath, "Jobsites", "JobsiteSaveData.json", saveData.SavedJobsiteData);
        string stationPath = _toJSON(jobsitePath, "Stations", "StationSaveData.json", saveData.SavedStationData);
        _toJSON(stationPath, "OperatingAreas", "OperatingAreaSaveData.json", saveData.SavedOperatingAreaData);
    }

    void _saveNestedFactionData(string savePath, SaveData saveData)
    {
        string factionDataPath = _toJSON(savePath, "Factions", "FactionSaveData.json", saveData.SavedFactionData);

        HashSet<int> factionActorIDs = new HashSet<int>();

        foreach (var factionData in saveData.SavedFactionData.AllFactionData)
        {
            var factionPath = _toJSON(factionDataPath, factionData.FactionName, "FactionSaveData.json", factionData);

            var actorsInFaction = saveData.SavedActorData.AllActorData.Where(actor => actor.ActorFactionID == factionData.FactionID).ToList();

            foreach (var actorData in actorsInFaction)
            {
                _toJSON(factionPath, "Actors", $"Actor_{actorData.ActorID}_SaveData.json", actorData);
                factionActorIDs.Add(actorData.ActorID);
            }
        }

        var factionlessActors = saveData.SavedActorData.AllActorData.Where(actor => !factionActorIDs.Contains(actor.ActorID)).ToList();
        var factionlessPath = Path.Combine(factionDataPath, "Factionless");
        Directory.CreateDirectory(factionlessPath);

        foreach (var actorData in factionlessActors)
        {
            _toJSON(factionlessPath, "Actors", $"Actor_{actorData.ActorID}_SaveData.json", actorData);
        }
    }

    string _toJSON(string currentPath, string pathToAdd, string pathExtension, object data)
    {
        string path = Path.Combine(currentPath, pathToAdd);
        Directory.CreateDirectory(path);
        string jsonData = JsonUtility.ToJson(data, true);
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
        string backupFilePath = fullPath + _backupExtension;

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
    [SerializeField] List<TKey> _keys = new();
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

        for (int i = 0; i < _keys.Count; i++) this.Add(_keys[i], _values[i]);
    }
}

[Serializable]
public class SaveData
{
    // Profile Info
    public SavedProfileData SavedProfileData;    

    // All Game Info
    public SavedRegionData SavedRegionData;
    public SavedCityData SavedCityData;
    public SavedJobsiteData SavedJobsiteData;
    public SavedStationData SavedStationData;
    public SavedOperatingAreaData SavedOperatingAreaData;
    public SavedFactionData SavedFactionData;
    public SavedActorData SavedActorData;

    // Player info
    public Vector3 PlayerPosition;
    public bool StaffPickedUp;

    // Current Scene Info
    public string SceneName;
    public string LastScene;

    // public SerializableDictionary<string, string> QuestSaveData;
    public List<QuestData> QuestData;
    // Try replace with a list of serialised classes instead.
    //public SerializableDictionary<string, string> PuzzleSaveData;
    public List<PuzzleData> PuzzleData;
    // Try replace with a list of serialised classes instead.

    public SaveData(int currentProfileID, string currentProfileName)
    {
        SavedProfileData = new SavedProfileData(currentProfileID, currentProfileName);
    }
}

[Serializable]
public class SavedProfileData
{
    public long LastUpdated;
    public int SaveDataID;
    public string SaveDataName;
    public int ProfileID;
    public string ProfileName;

    public SavedProfileData(int profileID, string profileName)
    {
        ProfileID = profileID;
        ProfileName = profileName;
    }

    public SavedProfileData(SavedProfileData profileData)
    {
        LastUpdated = profileData.LastUpdated;
        SaveDataID = profileData.SaveDataID;
        SaveDataName = profileData.SaveDataName;
        ProfileID = profileData.ProfileID;
        ProfileName = profileData.ProfileName;
    }
}

[Serializable]
public class SavedRegionData
{
    public List<RegionData> AllRegionData = new();

    public SavedRegionData(List<RegionData> allRegionData) => AllRegionData = allRegionData;
}

[Serializable]
public class SavedCityData
{
    public List<CityData> AllCityData = new();

    public SavedCityData(List<CityData> allCityData) => AllCityData = allCityData;
}

[Serializable]
public class SavedJobsiteData
{
    public List<JobsiteData> AllJobsiteData = new();

    public SavedJobsiteData(List<JobsiteData> allJobsiteData) => AllJobsiteData = allJobsiteData;
}

[Serializable]
public class SavedStationData
{
    public List<StationData> AllStationData = new();

    public SavedStationData(List<StationData> allStationData) => AllStationData = allStationData;
}

[Serializable]
public class SavedOperatingAreaData
{
    public List<OperatingAreaData> AllOperatingAreaData = new();

    public SavedOperatingAreaData(List<OperatingAreaData> allOperatingAreaData) => AllOperatingAreaData = allOperatingAreaData;
}

[Serializable]
public class SavedFactionData
{
    public List<FactionData> AllFactionData = new();
    public SavedFactionData(List<FactionData> allFactionData) => AllFactionData = allFactionData;
}

public class SavedActorData
{
    public List<ActorData> AllActorData = new();

    public SavedActorData(List<ActorData> allActorData) => AllActorData = allActorData;
}