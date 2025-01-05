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
        public Data<JobSite_Data>[] JobSites                             => Data;
        public Data<JobSite_Data>        GetJobSite_Data(uint      jobSiteID) => GetData(jobSiteID);
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

        public override uint GetDataID(int id) => JobSites[id].Data_Object.JobSiteID;

        public void UpdateJobSite(uint jobSiteID, JobSite_Data jobSite_Component) => UpdateData(jobSiteID, jobSite_Component);
        public void UpdateAllJobSites(Dictionary<uint, JobSite_Data> allJobSites) => UpdateAllData(allJobSites);

        public override void PopulateSceneData()
        {
            if (_defaultJobSites.Count == 0)
            {
                Debug.Log("No Default JobSites Found");
            }

            var existingJobSites = _getExistingJobSite_Components();
            
            foreach (var jobSite in JobSites)
            {
                if (jobSite?.Data_Object is null) continue;
                
                if (existingJobSites.TryGetValue(jobSite.Data_Object.JobSiteID, out var existingJobSite))
                {
                    JobSite_Components[jobSite.Data_Object.JobSiteID] = existingJobSite;
                    existingJobSite.SetJobSiteData(jobSite.Data_Object);
                    existingJobSites.Remove(jobSite.Data_Object.JobSiteID);
                    continue;
                }
                
                Debug.LogWarning($"JobSite with ID {jobSite.Data_Object.JobSiteID} not found in the scene.");
            }
            
            foreach (var jobSite in existingJobSites)
            {
                if (DataIndexLookup.ContainsKey(jobSite.Key))
                {
                    Debug.LogWarning($"JobSite with ID {jobSite.Key} wasn't removed from existingJobSites.");
                    continue;
                }
                
                Debug.LogWarning($"JobSite with ID {jobSite.Key} does not have DataObject in JobSite_SO.");
            }
        }

        protected override Dictionary<uint, Data<JobSite_Data>> _getDefaultData(bool initialisation = false)
        {
            if (_defaultData is null || !Application.isPlaying || initialisation)
                return _defaultData ??= _convertDictionaryToData(JobSite_List.DefaultJobSites);

            if (JobSites is null || JobSites.Length == 0)
            {
                Debug.LogError("No JobSites Found in JobSite_SO.");
                return _defaultData;
            }

            foreach (var jobSite in JobSites)
            {
                if (jobSite?.Data_Object is null) continue;
                
                if (!_defaultData.ContainsKey(jobSite.Data_Object.JobSiteID))
                {
                    Debug.LogError($"JobSite with ID {jobSite.Data_Object.JobSiteID} not found in DefaultJobSites.");
                    continue;
                }
                
                _defaultData[jobSite.Data_Object.JobSiteID] = jobSite;
            }

            return _defaultData;
        }
        
        static uint _lastUnusedJobSiteID = 1;
        
        public uint GetUnusedJobSiteID()
        {
            while (DataIndexLookup.ContainsKey(_lastUnusedJobSiteID))
            {
                _lastUnusedJobSiteID++;
            }

            return _lastUnusedJobSiteID;
        }
        
        Dictionary<uint, Data<JobSite_Data>> _defaultJobSites => DefaultData;
        
        protected override Data<JobSite_Data> _convertToData(JobSite_Data data)
        {
            return new Data<JobSite_Data>(
                dataID: data.JobSiteID, 
                data_Object: data,
                dataTitle: $"{data.JobSiteID}: {data.JobSiteName}",
                getData_Display: data.GetDataSO_Object);
        }
    }

    [CustomEditor(typeof(JobSite_SO))]
    public class JobSites_SOEditor : Data_SOEditor<JobSite_Data>
    {
        public override Data_SO<JobSite_Data> SO => _so ??= (JobSite_SO)target;
    }
}