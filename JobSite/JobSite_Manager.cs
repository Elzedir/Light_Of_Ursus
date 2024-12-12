using System;
using System.Collections.Generic;
using System.Linq;
using EmployeePosition;
using Initialisation;
using UnityEditor;
using UnityEngine;

namespace JobSite
{
    public abstract class JobSite_Manager : IDataPersistence
    {
        const  string     _jobSite_SOPath = "ScriptableObjects/JobSite_SO";
        
        static JobSite_SO _jobSite_SO;
        static JobSite_SO JobSite_SO => _jobSite_SO ??= _getJobSite_SO();

        public void SaveData(SaveData saveData) =>
            saveData.SavedJobSiteData = new SavedJobSiteData(JobSite_SO.JobSites);

        public void LoadData(SaveData saveData)
        {
            if (saveData is null)
            {
                //Debug.Log("No SaveData found in LoadData.");
                return;
            }

            if (saveData.SavedJobSiteData == null)
            {
                //Debug.Log("No SavedJobSiteData found in SaveData.");
                return;
            }

            if (saveData.SavedJobSiteData.AllJobSiteData == null)
            {
                //Debug.Log("No AllJobSiteData found in SavedJobSiteData.");
                return;
            }

            if (saveData.SavedJobSiteData.AllJobSiteData.Length == 0)
            {
                //Debug.Log("AllJobSiteData count is 0.");
                return;
            }

            JobSite_SO.LoadSO(saveData.SavedJobSiteData.AllJobSiteData);
        }

        public static void OnSceneLoaded()
        {
            Manager_Initialisation.OnInitialiseManagerJobSite += _initialise;
        }

        static void _initialise()
        {
            JobSite_SO.PopulateSceneJobSites();
        }
        
        public static JobSite_Data GetJobSite_Data(uint jobSiteID)
        {
            return JobSite_SO.GetJobSite_Data(jobSiteID);
        }
        
        public static JobSite_Component GetJobSite_Component(uint jobSiteID)
        {
            return JobSite_SO.GetJobSite_Component(jobSiteID);
        }
        
        static JobSite_SO _getJobSite_SO()
        {
            var jobSite_SO = Resources.Load<JobSite_SO>(_jobSite_SOPath);
            
            if (jobSite_SO is not null) return jobSite_SO;
            
            Debug.LogError("JobSite_SO not found. Creating temporary JobSite_SO.");
            jobSite_SO = ScriptableObject.CreateInstance<JobSite_SO>();
            
            return jobSite_SO;
        }

        public static JobSite_Component GetNearestJobSite(Vector3 position, JobSiteName jobSiteName)
        {
            // Change so that you either pass through a city, or if not, then it will check nearest region, and give you nearest 
            // Region => City => Jobsite. Maybe flash a BoxCollider at increasing distances and check if it hits a city or region and
            // use that to calculate the nearest one.
            
            JobSite_Component nearestJobSite = null;

            var nearestDistance = float.MaxValue;

            foreach (var jobSite in JobSite_SO.JobSiteComponents.Values.Where(j => j.JobSiteName == jobSiteName))
            {
                var distance = Vector3.Distance(position, jobSite.transform.position);

                if (!(distance < nearestDistance)) continue;

                nearestJobSite  = jobSite;
                nearestDistance = distance;
            }

            return nearestJobSite;
        }

        public static uint GetUnusedJobSiteID()
        {
            return JobSite_SO.GetUnusedJobSiteID();
        }
        
        public static Dictionary<JobSiteName, List<EmployeePositionName>> EmployeeCanUseList = new()
        {
            {JobSiteName.Lumber_Yard, new List<EmployeePositionName>
            {
                EmployeePositionName.Logger,
                EmployeePositionName.Sawyer,
                EmployeePositionName.Hauler
                
            }},
            {JobSiteName.Smithy, new List<EmployeePositionName>
            {
                EmployeePositionName.Miner,
                EmployeePositionName.Smith,
                EmployeePositionName.Hauler
            }}
        };
    }
    
    public enum JobSiteName
    {
        None,

        Lumber_Yard,
        Smithy
    }
}
