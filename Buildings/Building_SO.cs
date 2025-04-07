using System;
using System.Collections.Generic;
using System.Linq;
using DataPersistence;
using Tools;
using UnityEditor;
using UnityEngine;

namespace JobSites
{
    [CreateAssetMenu(fileName = "Building_SO", menuName = "SOList/Building_SO")]
    [Serializable]
    public class Building_SO : Data_Component_SO<Building_Data, Building_Component>
    {
        public Data<Building_Data>[] JobSites => Data;
        public Data<Building_Data>        GetJobSite_Data(ulong      jobSiteID) => GetData(jobSiteID);
        public Dictionary<ulong, Building_Component> JobSite_Components => _getSceneComponents();

        public Building_Component GetJobSite_Component(ulong jobSiteID)
        {
            if (jobSiteID == 0)
            {
                Debug.LogError("JobSiteID cannot be 0.");
                return null;
            }
            
            if (JobSite_Components.TryGetValue(jobSiteID, out var component))
            {
                return component;
            }   
            
            Debug.LogError($"JobSite with ID {jobSiteID} not found in JobSite_SO.");
            return null;
        }

        public void UpdateJobSite(ulong jobSiteID, Building_Data building_Component) => UpdateData(jobSiteID, building_Component);
        public void UpdateAllJobSites(Dictionary<ulong, Building_Data> allJobSites) => UpdateAllData(allJobSites);

        protected override Dictionary<ulong, Data<Building_Data>> _getDefaultData() =>
            _convertDictionaryToData(Building_List.S_DefaultJobSites);

        protected override Dictionary<ulong, Data<Building_Data>> _getSavedData()
        {
            Dictionary<ulong, Building_Data> savedData = new();

            try
            {
                savedData = DataPersistence_Manager.CurrentSaveData.SavedJobSiteData.AllJobSiteData
                    .ToDictionary(jobSite => jobSite.ID, jobSite => jobSite);
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

        protected override Dictionary<ulong, Data<Building_Data>> _getSceneData() =>
            _convertDictionaryToData(_getSceneComponents().ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Building_Data));
        
        protected override Data<Building_Data> _convertToData(Building_Data data)
        {
            return new Data<Building_Data>(
                dataID: data.ID, 
                data_Object: data,
                dataTitle: $"{data.ID}: {data.BuildingName}",
                getDataToDisplay: data.GetDataToDisplay);
        }
        
        public override void SaveData(Save_Data saveData) =>
            saveData.SavedJobSiteData = new SavedJobSiteData(JobSites.Select(jobSite => jobSite.Data_Object).ToArray());
    }

    [CustomEditor(typeof(Building_SO))]
    public class JobSites_SOEditor : Data_SOEditor<Building_Data>
    {
        public override Data_SO<Building_Data> SO => _so ??= (Building_SO)target;
    }
}