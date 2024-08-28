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
    [SerializeField] bool _createNewSaveFileIfNull = false;
    [SerializeField] bool _selectTestProfile = false;
    [SerializeField] string _currentTestSelectedProfileName = "test";

    public static string SaveFileName { get; private set; } = "Data.LightOfUrsus";

    [Header("File Storage Config")]
    [SerializeField] public bool _useEncryption;

    [Header("Auto Saving Configuration")]
    [SerializeField] bool _autoSaveEnabled = false;
    [SerializeField] float _autoSaveTimeSeconds = 60f;
    [SerializeField] int _numberOfAutoSaves = 5;

    SaveData _currentSaveData;
    public bool HasGameData() => _currentSaveData != null;
    List<IDataPersistence> _dataPersistenceObjects { get { return new List<IDataPersistence>(FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None).OfType<IDataPersistence>()); } }
    public ProfileData CurrentProfile { get; private set; }
    public List<ProfileData> AllProfiles { get { return LoadAllProfilesLatestSave().Keys.ToList(); } }
    Coroutine _autoSaveCoroutine;

    HashSet<int> _allProfileIDs;
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

        LoadGame();

        if (_autoSaveCoroutine != null) StopCoroutine(_autoSaveCoroutine);
        _autoSaveCoroutine = StartCoroutine(_autoSave());
    }

    void _initialise()
    {
        if (Instance == null) { Instance = this; }

        if (_disableDataPersistence) Debug.LogWarning("Data Persistence is currently disabled!");

        CurrentProfile = _initializeProfiles();
    }

    public void ChangeProfile(int profileID, string profileName)
    {
        CurrentProfile = GetProfile(profileName, profileID);

        if (CurrentProfile == null) { Debug.LogError("Profile not found."); return; }

        LoadGame();
    }

    public void DeleteProfileData(ProfileData profileData)
    {
        profileData.DeleteProfile();
        _initializeProfiles();
        LoadGame();
    }

    ProfileData _initializeProfiles()
    {
        // Load the profile ID's near here with the profiles.

        if (!_selectTestProfile) return GetMostRecentlyUpdatedProfileID();

        Debug.LogWarning("Overrode selected profile id with test id: " + _currentTestSelectedProfileName);
        return new ProfileData(_currentTestSelectedProfileName, 0, null, _useEncryption);
    }

    public ProfileData GetMostRecentlyUpdatedProfileID()
    {
        var mostRecentProfile = LoadAllProfilesLatestSave()
            .Where(p => p.Value != null)
            .OrderByDescending(p => DateTime.FromBinary(p.Value.LastUpdated))
            .Select(p => p.Key)
            .FirstOrDefault();

        if (mostRecentProfile == null)
        {
            mostRecentProfile = new ProfileData("Create New Profile", 0, null, _useEncryption);
            mostRecentProfile.SetProfileID(GetRandomProfileID());
        }

        return mostRecentProfile;
    }

    public Dictionary<ProfileData, SaveData> LoadAllProfilesLatestSave()
    {
        Dictionary<ProfileData, SaveData> allProfilesLatestSave = new();

        foreach (var directoryInfo in new DirectoryInfo(Application.persistentDataPath).EnumerateDirectories().Where(d => d.Name != "Unity"))
        {
            var profileData = LoadProfile(directoryInfo.Name);

            var latestSaveData = profileData.LoadData(GetLatestSave(directoryInfo.Name), directoryInfo.Name);

            if (latestSaveData == null) Debug.LogError($"SaveData is null for profile: {directoryInfo.Name}");

            allProfilesLatestSave.Add(profileData, latestSaveData);
        }

        return allProfilesLatestSave;
    }

    public ProfileData LoadProfile(string profileName)
    {
        IEnumerable<DirectoryInfo> saveGameDirectories = new DirectoryInfo(Path.Combine(Application.persistentDataPath, profileName)).EnumerateDirectories();

        return new ProfileData(
            profileName,
            saveGameDirectories.Where(d => d.Name.Contains("AutoSave")).Count(),
            saveGameDirectories.Where(d => !d.Name.Contains("AutoSave")).Select(d => d.Name).ToList(),
            _useEncryption
            );
    }

    public ProfileData GetProfile(string profileName, int profileID)
    {
        return AllProfiles.Find(p => p.ProfileName == profileName && p.ProfileID == profileID);
    }

    public string GetLatestSave(string profileName)
    {
        return new DirectoryInfo(Path.Combine(Application.persistentDataPath, profileName))
            .EnumerateDirectories()
            .OrderByDescending(d => d.LastWriteTime)
            .Select(d => d.Name)
            .FirstOrDefault();
    }

    public void NewSaveData(int profileID, string profileName)
    {
        _currentSaveData = new SaveData(profileID, profileName);
    }

    public void SaveGame(string saveGameName = "")
    {
        if (_disableDataPersistence) return;

        if (_currentSaveData == null) { Debug.LogWarning("No data was found. A New Game needs to be started before data can be saved."); return; }

        foreach (IDataPersistence data in _dataPersistenceObjects) data.SaveData(_currentSaveData);

        _currentSaveData.LastUpdated = DateTime.Now.ToBinary();

        CurrentProfile.SaveData(saveGameName, _currentSaveData, CurrentProfile.ProfileName);
    }

    public void LoadGame(string saveGame = "")
    {
        if (_disableDataPersistence || CurrentProfile.ProfileName == "Create New Profile") return;

        saveGame = saveGame == "" ? GetLatestSave(CurrentProfile.ProfileName) : saveGame;

        _currentSaveData = CurrentProfile.LoadData(saveGame, CurrentProfile.ProfileName);

        if (_currentSaveData == null && _createNewSaveFileIfNull) NewSaveData(GetRandomProfileID(), CurrentProfile.ProfileName);

        if (_currentSaveData == null) { Debug.Log("No data was found. A New Game needs to be started before data can be loaded."); return; }

        foreach (IDataPersistence data in _dataPersistenceObjects) data.LoadData(_currentSaveData);
    }

    void OnApplicationQuit()
    {
        Debug.Log("Data Saved");
        SaveGame("ExitSave");
    }

    public List<SaveData> GetAllSavedGames(ProfileData profile) { return profile.AllSavedDatas.Select(saveGame => profile.LoadData(saveGame, profile.ProfileName)).ToList(); }

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
    public List<string> AllSavedDatas;
    bool _useEncryption = false;
    public int AutoSaveCounter;
    readonly string _encryptionCodeWord = "word";
    readonly string _backupExtension = ".bak";

    public ProfileData(string profileName, int autoSaveCounter, List<string> allSavedDatas, bool useEncryption)
    {
        ProfileName = profileName;
        AutoSaveCounter = autoSaveCounter;
        if (allSavedDatas != null) AllSavedDatas = new(allSavedDatas);
        else AllSavedDatas = new();
        _useEncryption = useEncryption;
    }

    public void SetProfileID(int profileID)
    {
        ProfileID = profileID;
    }

    public SaveData LoadData(string savedGame, string profileName, bool allowRestoreFromBackup = true)
    {
        if (profileName == null) return null;

        string fullPath = Path.Combine(Application.persistentDataPath, profileName, savedGame, Manager_Data.SaveFileName);

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
                return LoadData(savedGame, profileName, false);
            }

            Debug.LogError($"Error occurred when trying to load file: {fullPath}. Backup did not work.\n{e}");
            return null;
        }
    }

    public void SaveData(string savedGameName, SaveData SaveData, string profileName, bool newProfile = false)
    {
        if (profileName == null || SceneManager.GetActiveScene().name == "Main_Menu" && !newProfile) return;

        if (string.IsNullOrEmpty(savedGameName)) savedGameName = $"NewSave_{AllSavedDatas.Count}";

        string fullPath = Path.Combine(Application.persistentDataPath, profileName, savedGameName, Manager_Data.SaveFileName);
        string backupFilePath = fullPath + _backupExtension;

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            string dataToStore = JsonUtility.ToJson(SaveData, true);

            if (_useEncryption) dataToStore = _encryptDecrypt(dataToStore);

            File.WriteAllText(fullPath, dataToStore);

            if (LoadData(savedGameName, profileName) != null) File.Copy(fullPath, backupFilePath, true);

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
    public AllRegions_SO AllRegions_SO;
    public AllFactions_SO AllFactions_SO;

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