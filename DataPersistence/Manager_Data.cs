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
            .OrderByDescending(p => DateTime.FromBinary(p.Value.LastUpdated))
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
            string profileDataFilePath = Path.Combine(directoryInfo.FullName, "profileData.json");

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

        string profileDataFilePath = Path.Combine(profilePath, "profileData.json");

        if (File.Exists(profileDataFilePath)) File.Delete(profileDataFilePath);

        string profileDataJson = JsonUtility.ToJson(profileData);

        File.WriteAllText(profileDataFilePath, profileDataJson);
    }

    public ProfileData LoadProfileData(string profileName)
    {
        string profileDataFilePath = Path.Combine(Application.persistentDataPath, profileName, "profileData.json");

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

        CurrentSaveData.LastUpdated = DateTime.Now.ToBinary();

        SaveProfileData(CurrentProfile);

        CurrentProfile.SaveData(saveDataName, CurrentSaveData, CurrentProfile.ProfileName);
    }

    public void LoadGame(string saveDataName)
    {
        if (_disableDataPersistence || CurrentProfile.ProfileName == "Create New Profile") return;

        saveDataName = saveDataName == "" ? GetLatestSave(CurrentProfile.ProfileName) : saveDataName;

        CurrentSaveData = CurrentProfile.LoadData(saveDataName, CurrentProfile.ProfileName);

        if (CurrentSaveData == null && _createNewSaveFileIfNull) CurrentSaveData = new SaveData(GetRandomProfileID(), CurrentProfile.ProfileName);

        if (CurrentSaveData == null) { Debug.Log("No data was found. A New Game needs to be started before data can be loaded."); return; }

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

        string fullPath = Path.Combine(Application.persistentDataPath, profileName, saveDataName, Manager_Data.SaveFileName);

        if (!File.Exists(fullPath)) return null;

        try
        {
            string dataToLoad = File.ReadAllText(fullPath);

            if (_useEncryption) dataToLoad = _encryptDecrypt(dataToLoad);

            return JsonUtility.FromJson<SaveData>(dataToLoad);
        }
        catch (Exception e)
        {
            if (allowRestoreFromBackup && _attemptRollback(fullPath))
            {
                Debug.LogWarning($"Failed to load data file. Attempting to roll back.\n{e}");
                return LoadData(saveDataName, profileName, false);
            }

            Debug.LogError($"Error occurred when trying to load file: {fullPath}. Backup did not work.\n{e}");
            return null;
        }
    }

    public void SaveData(string saveDataName, SaveData SaveData, string profileName, bool newProfile = false)
    {
        if (profileName == null || SceneManager.GetActiveScene().name == "Main_Menu" && !newProfile) return;

        if (string.IsNullOrEmpty(saveDataName)) saveDataName = $"NewSave_{AllSavedDatas.Count}";

        string fullPath = Path.Combine(Application.persistentDataPath, profileName, saveDataName, Manager_Data.SaveFileName);
        string backupFilePath = fullPath + _backupExtension;

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            string dataToStore = JsonUtility.ToJson(SaveData, true);

            if (_useEncryption) dataToStore = _encryptDecrypt(dataToStore);

            File.WriteAllText(fullPath, dataToStore);

            if (LoadData(saveDataName, profileName) != null) File.Copy(fullPath, backupFilePath, true);

            else throw new Exception("Save file could not be verified and backup could not be created.");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error occured when trying to save data to file: {fullPath} \n {e}.");
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
    public long LastUpdated;
    public int SaveDataID;
    public string SaveDataName;
    public int ProfileID;
    public string ProfileName;

    // All Game Info
    public SavedRegionData SavedRegionData;
    public SavedFactionData SavedFactionData;

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
        ProfileID = currentProfileID;
        ProfileName = currentProfileName;
    }
}

[Serializable]
public class SavedRegionData
{
    public List<RegionData> AllRegionData;

    public List<int> AllRegionIDs;
    public int LastUnusedRegionID = 0;

    public List<int> AllCityIDs;
    public int LastUnusedCityID = 1;

    public List<int> AllJobsiteIDs;
    public int LastUnusedJobsiteID = 1;

    public List<int> AllStationIDs;
    public int LastUnusedStationID = 1;

    public List<int> AllOperatingAreaIDs;
    public int LastUnusedOperatingAreaID = 1;

    public SavedRegionData(List<RegionData> allRegionData, List<int> allRegionIDs, int lastUnusedRegionID, List<int> allCityIDs, int lastUnusedCityID, List<int> allJobsiteIDs, int lastUnusedJobsiteID, List<int> allStationIDs, int lastUnusedStationID, List<int> allOperatingAreaIDs, int lastUnusedOperatingAreaID)
    {
        AllRegionData = allRegionData;
        AllRegionIDs = allRegionIDs;
        LastUnusedRegionID = lastUnusedRegionID;
        AllCityIDs = allCityIDs;
        LastUnusedCityID = lastUnusedCityID;
        AllJobsiteIDs = allJobsiteIDs;
        LastUnusedJobsiteID = lastUnusedJobsiteID;
        AllStationIDs = allStationIDs;
        LastUnusedStationID = lastUnusedStationID;
        AllOperatingAreaIDs = allOperatingAreaIDs;
        LastUnusedOperatingAreaID = lastUnusedOperatingAreaID;
    }
}

public class SavedFactionData
{
    public List<FactionData> AllFactionData;

    public List<int> AllFactionIDs;
    public int LastUnusedFactionID = 0;

    public List<int> AllActorIDs; //Can change later to a hashset for efficiency but for now need display
    public int LastUnusedActorID = 1;

    public SavedFactionData(List<FactionData> allFactionData, List<int> allFactionIDs, int lastUnusedFactionID, List<int> allActorIDs, int lastUnusedActorID)
    {
        AllFactionData = allFactionData;
        AllFactionIDs = allFactionIDs;
        LastUnusedFactionID = lastUnusedFactionID;
        AllActorIDs = allActorIDs;
        LastUnusedActorID = lastUnusedActorID;
    }
}