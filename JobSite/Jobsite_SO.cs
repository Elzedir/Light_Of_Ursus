using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Tools;
using UnityEditor;
using UnityEngine;

namespace JobSite
{
    [CreateAssetMenu(fileName = "JobSite_SO", menuName = "SOList/JobSite_SO")]
    [Serializable]
    public class JobSite_SO : Data_SO<JobSite_Data>
    {
        public Object_Data<JobSite_Data>[] JobSites                             => Objects_Data;
        public Object_Data<JobSite_Data>        GetJobSite_Data(uint      jobSiteID) => GetObject_Data(jobSiteID);
        Dictionary<uint, JobSite_Component>     _jobSite_Components;
        public Dictionary<uint, JobSite_Component> JobSite_Components => _jobSite_Components ??= _getExistingJobSite_Components();
        
        Dictionary<uint, JobSite_Component> _getExistingJobSite_Components()
        {
            return FindObjectsByType<JobSite_Component>(FindObjectsSortMode.None)
                                  .Where(station => Regex.IsMatch(station.name, @"\d+"))
                                  .ToDictionary(
                                      station => uint.Parse(new string(station.name.Where(char.IsDigit).ToArray())),
                                      station => station
                                  );
        }

        public JobSite_Component GetJobSite_Component(uint jobSiteID)
        {
            if (JobSite_Components.TryGetValue(jobSiteID, out var component))
            {
                return component;
            }   
            
            Debug.LogError($"JobSite with ID {jobSiteID} not found in JobSite_SO.");
            return null;
        }

        public override uint GetDataObjectID(int id) => JobSites[id].DataObject.JobSiteID;

        public void UpdateJobSite(uint jobSiteID, JobSite_Data jobSite_Component) => UpdateDataObject(jobSiteID, jobSite_Component);
        public void UpdateAllJobSites(Dictionary<uint, JobSite_Data> allJobSites) => UpdateAllDataObjects(allJobSites);

        public override void PopulateSceneData()
        {
            if (_defaultJobSites.Count == 0)
            {
                Debug.Log("No Default JobSites Found");
            }

            var existingJobSites = _getExistingJobSite_Components();
            
            foreach (var jobSite in JobSites)
            {
                if (jobSite?.DataObject is null) continue;
                
                if (existingJobSites.TryGetValue(jobSite.DataObject.JobSiteID, out var existingJobSite))
                {
                    JobSite_Components[jobSite.DataObject.JobSiteID] = existingJobSite;
                    existingJobSite.SetJobSiteData(jobSite.DataObject);
                    existingJobSites.Remove(jobSite.DataObject.JobSiteID);
                    continue;
                }
                
                Debug.LogWarning($"JobSite with ID {jobSite.DataObject.JobSiteID} not found in the scene.");
            }
            
            foreach (var jobSite in existingJobSites)
            {
                if (DataObjectIndexLookup.ContainsKey(jobSite.Key))
                {
                    Debug.LogWarning($"JobSite with ID {jobSite.Key} wasn't removed from existingJobSites.");
                    continue;
                }
                
                Debug.LogWarning($"JobSite with ID {jobSite.Key} does not have DataObject in JobSite_SO.");
            }
        }

        protected override Dictionary<uint, Object_Data<JobSite_Data>> _getDefaultDataObjects(bool initialisation = false)
        {
            if (_defaultDataObjects is null || !Application.isPlaying || initialisation)
                return _defaultDataObjects ??= _convertDictionaryToDataObject(JobSite_List.DefaultJobSites);

            if (JobSites is null || JobSites.Length == 0)
            {
                Debug.LogError("No JobSites Found in JobSite_SO.");
                return _defaultDataObjects;
            }

            foreach (var jobSite in JobSites)
            {
                if (jobSite?.DataObject is null) continue;
                
                if (!_defaultDataObjects.ContainsKey(jobSite.DataObject.JobSiteID))
                {
                    Debug.LogError($"JobSite with ID {jobSite.DataObject.JobSiteID} not found in DefaultJobSites.");
                    continue;
                }
                
                _defaultDataObjects[jobSite.DataObject.JobSiteID] = jobSite;
            }

            return _defaultDataObjects;
        }
        
        static uint _lastUnusedJobSiteID = 1;
        
        public uint GetUnusedJobSiteID()
        {
            while (DataObjectIndexLookup.ContainsKey(_lastUnusedJobSiteID))
            {
                _lastUnusedJobSiteID++;
            }

            return _lastUnusedJobSiteID;
        }
        
        Dictionary<uint, Object_Data<JobSite_Data>> _defaultJobSites => DefaultDataObjects;
        
        protected override Object_Data<JobSite_Data> _convertToDataObject(JobSite_Data dataObject)
        {
            return new Object_Data<JobSite_Data>(
                dataObjectID: dataObject.JobSiteID, 
                dataObject: dataObject,
                dataObjectTitle: $"{dataObject.JobSiteID}: {dataObject.JobSiteName}",
                data_Display: dataObject.GetDataSO_Object(ToggleMissingDataDebugs));
        }
    }

    [CustomEditor(typeof(JobSite_SO))]
    public class JobSites_SOEditor : Data_SOEditor<JobSite_Data>
    {
        public override Data_SO<JobSite_Data> SO => _so ??= (JobSite_SO)target;
    }
}