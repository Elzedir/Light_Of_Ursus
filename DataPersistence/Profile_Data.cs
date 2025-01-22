using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Actor;
using Faction;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DataPersistence
{
    [Serializable]
    public class Profile_Data
    {
        public uint ProfileID;
        public string ProfileName;
        Dictionary<string, Save_Data> _allSavedData;

        public Dictionary<string, Save_Data> AllSavedData => _allSavedData ??= GetAllSavedData();

        bool _useEncryption;
        public int AutoSaveCounter;
        readonly string _encryptionCodeWord = "word";
        readonly string _backupExtension = ".bak";

        public Profile_Data(uint profileID, string profileName, int autoSaveCounter, bool useEncryption)
        {
            ProfileID = profileID;
            ProfileName = profileName;
            AutoSaveCounter = autoSaveCounter;
            _useEncryption = useEncryption;
        }

        public Profile_Data(Profile_Data profileData)
        {
            ProfileID = profileData.ProfileID;
            ProfileName = profileData.ProfileName;
            _useEncryption = profileData._useEncryption;
            AutoSaveCounter = profileData.AutoSaveCounter;
            _encryptionCodeWord = profileData._encryptionCodeWord;
            _backupExtension = profileData._backupExtension;
        }

        public Dictionary<string, Save_Data> GetAllSavedData()
        {
            Dictionary<string, Save_Data> allSavedDatas = new();

            var profilePath = Path.Combine(Application.persistentDataPath, ProfileName);

            if (!Directory.Exists(profilePath))
            {
                Debug.LogWarning($"Directory not found: {profilePath}");
                return allSavedDatas;
            }

            foreach (var directoryInfo in new DirectoryInfo(profilePath).EnumerateDirectories())
            {
                Save_Data saveData = LoadData(directoryInfo.Name, ProfileName);

                if (saveData != null) allSavedDatas.Add(directoryInfo.Name, saveData);
            }

            return allSavedDatas;
        }

        public Save_Data LoadData(string saveDataName, string profileName, bool allowRestoreFromBackup = true)
        {
            if (string.IsNullOrEmpty(profileName) || string.IsNullOrEmpty(saveDataName)) return null;

            var profilePath = Path.Combine(Application.persistentDataPath, profileName);
            var profileDataPath = Path.Combine(profilePath, "ProfileData.json");

            var profileData = File.ReadAllText(profileDataPath);

            if (_useEncryption) profileData = _encryptDecrypt(profileData);

            var savedProfileData = JsonUtility.FromJson<Profile_Data>(profileData);

            var saveData = new Save_Data(savedProfileData.ProfileID, savedProfileData.ProfileName);

            var savePath = Path.Combine(profilePath, saveDataName);

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

        void _loadNestedRegionData(string savePath, Save_Data saveData)
        {
            var regionPath = Path.Combine(savePath, "Regions");
            var regionDataJson = JsonUtility.FromJson<SavedRegionData>(_fromJSON(regionPath, "RegionSaveData.json"));
            saveData.SavedRegionData = new SavedRegionData(regionDataJson?.AllRegionData);

            var cityPath = Path.Combine(regionPath, "Cities");
            var cityDataJson = JsonUtility.FromJson<SavedCityData>(_fromJSON(cityPath, "CitySaveData.json"));
            saveData.SavedCityData = new SavedCityData(cityDataJson?.AllCityData);

            var jobsitePath = Path.Combine(cityPath, "Jobsites");
            var jobsiteDataJson =
                JsonUtility.FromJson<SavedJobSiteData>(_fromJSON(jobsitePath, "JobsiteSaveData.json"));
            saveData.SavedJobSiteData = new SavedJobSiteData(jobsiteDataJson?.AllJobSiteData);

            var stationPath = Path.Combine(jobsitePath, "Stations");
            var stationDataJson =
                JsonUtility.FromJson<SavedStationData>(_fromJSON(stationPath, "StationSaveData.json"));
            saveData.SavedStationData = new SavedStationData(stationDataJson?.AllStationData);
        }

        void _loadNestedFactionData(string savePath, Save_Data saveData)
        {
            var allFactionsDirectoryPath = Path.Combine(savePath, "Factions");

            var factionSaveDataJson =
                JsonUtility.FromJson<SavedFactionData>(_fromJSON(allFactionsDirectoryPath, "AllFactionsSaveData.json"));

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
            var allSavedFactionData = new Faction_Data[allFactionDirectories.Length];

            for (var i = 0; i < allFactionDirectories.Length; i++)
            {
                var factionDirectoryPath = allFactionDirectories[i];

                var factionData =
                    JsonUtility.FromJson<Faction_Data>(_fromJSON(factionDirectoryPath, "FactionSaveData.json"));

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
                    var actorFiles = Directory.GetFiles(actorsPath, "Actor_*_SaveData.json");
                    var allSavedActorData = new Actor_Data[actorFiles.Length];

                    for (var j = 0; j < actorFiles.Length; j++)
                    {
                        var actorFile = actorFiles[j];
                        var actorDataJson = File.ReadAllText(actorFile);

                        if (_useEncryption) actorDataJson = _encryptDecrypt(actorDataJson);
                        var actorData = JsonUtility.FromJson<Actor_Data>(actorDataJson);

                        try
                        {
                            allSavedActorData[j] = actorData;
                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"Error occurred when trying to load actor data: {e}");
                        }
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
            var pathToLoad = Path.Combine(path, extension);
            if (!File.Exists(pathToLoad))
            {
                Debug.Log($"File does not exist for path: {pathToLoad}");
                return null;
            }

            var jsonData = File.ReadAllText(pathToLoad);
            if (_useEncryption) jsonData = _encryptDecrypt(jsonData);
            return jsonData;
        }

        public void SaveData(string saveDataName, Save_Data saveData, string profileName, bool newProfile = false)
        {
            if (profileName == null || SceneManager.GetActiveScene().name == "Main_Menu" && !newProfile) return;

            if (string.IsNullOrEmpty(saveDataName)) saveDataName = $"NewSave_{AllSavedData.Count}";

            string profilePath = Path.Combine(Application.persistentDataPath, profileName);
            string saveGamePath = Path.Combine(profilePath, saveDataName);

            try
            {
                Directory.CreateDirectory(profilePath);
                Directory.CreateDirectory(saveGamePath);

                _saveNestedRegionData(saveGamePath, saveData);

                _saveNestedFactionData(saveGamePath, saveData);

                if (LoadData(saveDataName, profileName) != null)
                {
                    var backupExtension = string.IsNullOrEmpty(_backupExtension) ? ".bak" : _backupExtension;
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

        void _saveNestedRegionData(string savePath, Save_Data saveData)
        {
            var regionPath = _toJSON(savePath, "Regions", "RegionSaveData.json", saveData.SavedRegionData);
            var cityPath = _toJSON(regionPath, "Cities", "CitySaveData.json", saveData.SavedCityData);
            var jobSitePath = _toJSON(cityPath, "JobSites", "JobSiteSaveData.json", saveData.SavedJobSiteData);
            _toJSON(jobSitePath, "Stations", "StationSaveData.json", saveData.SavedStationData);
        }

        void _saveNestedFactionData(string savePath, Save_Data saveData)
        {
            var factionDataPath =
                _toJSON(savePath, "Factions", "AllFactionsSaveData.json", saveData.SavedFactionData);

            var factionActorIDs = new HashSet<uint>();

            foreach (var factionData in saveData.SavedFactionData.AllFactionData)
            {
                var factionPath = _toJSON(factionDataPath, factionData.FactionName, "FactionSaveData.json",
                    factionData);

                var actorsInFaction = saveData.SavedActorData.AllActorData
                    .Where(actor => actor.ActorFactionID == factionData.FactionID).ToList();
                Directory.CreateDirectory(Path.Combine(factionPath, "Actors"));

                foreach (var actorData in actorsInFaction)
                {
                    _toJSON(factionPath, "Actors", $"Actor_{actorData.ActorID}_SaveData.json", actorData);
                    factionActorIDs.Add(actorData.ActorID);
                }
            }

            var factionlessActors = saveData.SavedActorData.AllActorData
                .Where(actor => !factionActorIDs.Contains(actor.ActorID)).ToList();

            foreach (var actorData in factionlessActors)
            {
                Debug.LogWarning($"Actor: {actorData.ActorID}: {actorData.ActorName} is factionless.");
                _toJSON(Path.Combine(factionDataPath, "Wanderers"), "Actors",
                    $"Actor_{actorData.ActorID}_SaveData.json", actorData);
            }
        }

        string _toJSON(string currentPath, string pathToAdd, string pathExtension, object data)
        {
            var path = Path.Combine(currentPath, pathToAdd);
            Directory.CreateDirectory(path);
            var jsonData = JsonUtility.ToJson(data, true);
            if (_useEncryption) jsonData = _encryptDecrypt(jsonData);
            File.WriteAllText(Path.Combine(path, pathExtension), jsonData);

            return path;
        }

        void _copyDirectory(string sourceDir, string destDir)
        {
            Directory.CreateDirectory(destDir);

            foreach (var file in Directory.GetFiles(sourceDir))
            {
                var destFile = Path.Combine(destDir, Path.GetFileName(file));
                File.Copy(file, destFile, true);
            }

            foreach (var subDir in Directory.GetDirectories(sourceDir))
            {
                var destSubDir = Path.Combine(destDir, Path.GetFileName(subDir));
                _copyDirectory(subDir, destSubDir);
            }
        }

        public void DeleteProfile()
        {
            var fullPath = Path.Combine(Application.persistentDataPath, ProfileName);

            try
            {
                if (Directory.Exists(fullPath)) Directory.Delete(Path.GetDirectoryName(fullPath), true);

                else Debug.LogWarning($"Tried to delete profile data, but data was not found: {fullPath}");
            }
            catch (Exception e)
            {
                Debug.LogError(
                    $"Failed to delete profile data for profileName: {ProfileName} at path: {fullPath} \n {e}");
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
                Debug.LogError(
                    $"Failed to delete profile data for profileName: {ProfileName} at path: {fullPath} \n {e}");
            }
        }

        string _encryptDecrypt(string data)
        {
            return new string(data.Select((c, i) => (char)(c ^ _encryptionCodeWord[i % _encryptionCodeWord.Length]))
                .ToArray());
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
}