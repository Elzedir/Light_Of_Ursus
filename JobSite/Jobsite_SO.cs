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
        public Data<JobSite_Data>[] JobSites => Data;
        public Data<JobSite_Data>        GetJobSite_Data(ulong      jobSiteID) => GetData(jobSiteID);
        public Dictionary<ulong, JobSite_Component> JobSite_Components => _getSceneComponents();

        public JobSite_Component GetJobSite_Component(ulong jobSiteID)
        {
            if (jobSiteID == 0)
            {
                Debug.LogError("StationID cannot be 0.");
                return null;
            }
            
            if (JobSite_Components.TryGetValue(jobSiteID, out var component))
            {
                return component;
            }   
            
            Debug.LogError($"JobSite with ID {jobSiteID} not found in JobSite_SO.");
            return null;
        }

        public void UpdateJobSite(ulong jobSiteID, JobSite_Data jobSite_Component) => UpdateData(jobSiteID, jobSite_Component);
        public void UpdateAllJobSites(Dictionary<ulong, JobSite_Data> allJobSites) => UpdateAllData(allJobSites);

        protected override Dictionary<ulong, Data<JobSite_Data>> _getDefaultData() =>
            _convertDictionaryToData(JobSite_List.DefaultJobSites);

        protected override Dictionary<ulong, Data<JobSite_Data>> _getSavedData()
        {
            Dictionary<ulong, JobSite_Data> savedData = new();

            try
            {
                savedData = DataPersistence_Manager.CurrentSaveData.SavedJobSiteData.AllJobSiteData
                    .ToDictionary(jobSite => jobSite.JobSiteID, jobSite => jobSite);
            }
            catch
            {
                var saveData = DataPersistence_Manager.CurrentSaveData;
                
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

        protected override Dictionary<ulong, Data<JobSite_Data>> _getSceneData() =>
            _convertDictionaryToData(_getSceneComponents().ToDictionary(kvp => kvp.Key, kvp => kvp.Value.JobSiteData));
        
        protected override Data<JobSite_Data> _convertToData(JobSite_Data data)
        {
            return new Data<JobSite_Data>(
                dataID: data.JobSiteID, 
                data_Object: data,
                dataTitle: $"{data.JobSiteID}: {data.JobSiteName}",
                getDataToDisplay: data.GetDataToDisplay);
        }
        
        public override void SaveData(Save_Data saveData) =>
            saveData.SavedJobSiteData = new SavedJobSiteData(JobSites.Select(jobSite => jobSite.Data_Object).ToArray());
    }

    [CustomEditor(typeof(JobSite_SO))]
    public class JobSites_SOEditor : Data_SOEditor<JobSite_Data>
    {
        public override Data_SO<JobSite_Data> SO => _so ??= (JobSite_SO)target;
    }
}