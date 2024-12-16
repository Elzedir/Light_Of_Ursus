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
        public Dictionary<uint, JobSite_Component> JobSiteComponents = new();

        public JobSite_Component GetJobSite_Component(uint jobSiteID)
        {
            if (JobSiteComponents.TryGetValue(jobSiteID, out var component))
            {
                return component;
            }   
            
            Debug.LogError($"JobSite with ID {jobSiteID} not found in JobSite_SO.");
            return null;
        }

        public override uint GetDataObjectID(int id) => JobSites[id].DataObject.JobSiteID;

        public void UpdateJobSite(uint jobSiteID, JobSite_Data jobSite_Component) => UpdateDataObject(jobSiteID, jobSite_Component);
        public void UpdateAllJobSites(Dictionary<uint, JobSite_Data> allJobSites) => UpdateAllDataObjects(allJobSites);

        public void PopulateSceneJobSites()
        {
            if (_defaultJobSites.Count == 0)
            {
                Debug.Log("No Default JobSites Found");
            }
            
            var existingJobSites = FindObjectsByType<JobSite_Component>(FindObjectsSortMode.None)
                                     .Where(station => Regex.IsMatch(station.name, @"\d+"))
                                     .ToDictionary(
                                         station => uint.Parse(new string(station.name.Where(char.IsDigit).ToArray())),
                                         station => station
                                     );
            
            foreach (var jobSite in JobSites)
            {
                if (jobSite?.DataObject is null) continue;
                
                if (existingJobSites.TryGetValue(jobSite.DataObject.JobSiteID, out var existingJobSite))
                {
                    JobSiteComponents[jobSite.DataObject.JobSiteID] = existingJobSite;
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

        protected override Dictionary<uint, Object_Data<JobSite_Data>> _populateDefaultDataObjects()
        {
            var defaultJobSites = new Dictionary<uint, JobSite_Data>();

            foreach (var defaultJobSite in JobSite_List.DefaultJobSites)
            {
                defaultJobSites.Add(defaultJobSite.Key, defaultJobSite.Value);
            }

            return _convertDictionaryToDataObject(defaultJobSites);
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
        
        protected override Object_Data<JobSite_Data> _convertToDataObject(JobSite_Data data)
        {
            return new Object_Data<JobSite_Data>(
                dataObjectID: data.JobSiteID, 
                dataObject: data,
                dataObjectTitle: $"{data.JobSiteID}: {data.JobSiteName}",
                data_Display: data.DataSO_Object(ToggleMissingDataDebugs));
        }
    }

    [CustomEditor(typeof(JobSite_SO))]
    public class JobSites_SOEditor : Data_SOEditor<JobSite_Data>
    {
        public override Data_SO<JobSite_Data> SO => _so ??= (JobSite_SO)target;
    }
}