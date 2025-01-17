using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DataPersistence;
using Tools;
using UnityEditor;
using UnityEngine;

namespace JobSite
{
    [CreateAssetMenu(fileName = "JobSite_SO", menuName = "SOList/JobSite_SO")]
    [Serializable]
    public class JobSite_SO : Data_Component_SO<JobSite_Data, JobSite_Component>
    {
        public Data<JobSite_Data>[] JobSites                             => Data;
        public Data<JobSite_Data>        GetJobSite_Data(uint      jobSiteID) => GetData(jobSiteID);
        public Dictionary<uint, JobSite_Component> JobSite_Components => _getSceneComponents();

        public JobSite_Component GetJobSite_Component(uint jobSiteID)
        {
            if (JobSite_Components.TryGetValue(jobSiteID, out var component))
            {
                return component;
            }   
            
            Debug.LogError($"JobSite with ID {jobSiteID} not found in JobSite_SO.");
            return null;
        }

        public override uint GetDataID(int id) => JobSites[id].Data_Object.JobSiteID;

        public void UpdateJobSite(uint jobSiteID, JobSite_Data jobSite_Component) => UpdateData(jobSiteID, jobSite_Component);
        public void UpdateAllJobSites(Dictionary<uint, JobSite_Data> allJobSites) => UpdateAllData(allJobSites);

        protected override Dictionary<uint, Data<JobSite_Data>> _getDefaultData() =>
            _convertDictionaryToData(JobSite_List.DefaultJobSites);

        protected override Dictionary<uint, Data<JobSite_Data>> _getSavedData()
        {
            Dictionary<uint, JobSite_Data> savedData = new();

            try
            {
                savedData = DataPersistenceManager.DataPersistence_SO.CurrentSaveData.SavedJobSiteData.AllJobSiteData
                    .ToDictionary(jobSite => jobSite.JobSiteID, jobSite => jobSite);
            }
            catch
            {
                var saveData = DataPersistenceManager.DataPersistence_SO.CurrentSaveData;
                
                if (ToggleMissingDataDebugs)
                {
                    Debug.LogWarning(saveData == null
                        ? "LoadData Error: CurrentSaveData is null."
                        : saveData.SavedJobSiteData == null
                            ? $"LoadData Error: SavedJobSiteData is null in CurrentSaveData (SaveID: {saveData.SavedProfileData.SaveDataID})."
                            : saveData.SavedJobSiteData.AllJobSiteData == null
                                ? $"LoadData Error: AllJobSiteData is null in SavedJobSiteData (SaveID: {saveData.SavedProfileData.SaveDataID})."
                                : !saveData.SavedJobSiteData.AllJobSiteData.Any()
                                    ? $"LoadData Warning: AllJobSiteData is empty (SaveID: {saveData.SavedProfileData.SaveDataID})."
                                    : string.Empty);
                }
            }

            return _convertDictionaryToData(savedData);
        }

        protected override Dictionary<uint, Data<JobSite_Data>> _getSceneData() =>
            _convertDictionaryToData(_getSceneComponents().ToDictionary(kvp => kvp.Key, kvp => kvp.Value.JobSiteData));
        
        static uint _lastUnusedJobSiteID = 1;
        
        public uint GetUnusedJobSiteID()
        {
            while (DataIndexLookup.ContainsKey(_lastUnusedJobSiteID))
            {
                _lastUnusedJobSiteID++;
            }

            return _lastUnusedJobSiteID;
        }
        
        protected override Data<JobSite_Data> _convertToData(JobSite_Data data)
        {
            return new Data<JobSite_Data>(
                dataID: data.JobSiteID, 
                data_Object: data,
                dataTitle: $"{data.JobSiteID}: {data.JobSiteName}",
                getDataToDisplay: data.GetData_Display);
        }
        
        public override void SaveData(SaveData saveData) =>
            saveData.SavedJobSiteData = new SavedJobSiteData(JobSites.Select(jobSite => jobSite.Data_Object).ToArray());
    }

    [CustomEditor(typeof(JobSite_SO))]
    public class JobSites_SOEditor : Data_SOEditor<JobSite_Data>
    {
        public override Data_SO<JobSite_Data> SO => _so ??= (JobSite_SO)target;
    }
}