using System.Collections.Generic;
using System.Linq;
using Jobs;
using UnityEngine;

namespace JobSites
{
    public abstract class JobSite_Manager
    {
        const  string     _jobSite_SOPath = "ScriptableObjects/JobSite_SO";
        
        static JobSite_SO s_jobSite_SO;
        static JobSite_SO S_JobSite_SO => s_jobSite_SO ??= _getJobSite_SO();
        
        public static JobSite_Data GetJobSite_Data(ulong jobSiteID)
        {
            return S_JobSite_SO.GetJobSite_Data(jobSiteID).Data_Object;
        }
        
        public static JobSite_Data GetJobSite_DataFromName(JobSite_Component jobSite_Component)
        {
            return S_JobSite_SO.GetDataFromName(jobSite_Component.name)?.Data_Object;
        }
        
        public static JobSite_Component GetJobSite_Component(ulong jobSiteID)
        {
            return S_JobSite_SO.GetJobSite_Component(jobSiteID);
        }
        
        public static List<ulong> GetAllJobSiteIDs() => S_JobSite_SO.GetAllDataIDs();
        
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

            var nearestDistance = float.PositiveInfinity;

            foreach (var jobSite in S_JobSite_SO.JobSite_Components.Values.Where(j => j.JobSiteName == jobSiteName))
            {
                var distance = Vector3.Distance(position, jobSite.transform.position);

                if (!(distance < nearestDistance)) continue;

                nearestJobSite  = jobSite;
                nearestDistance = distance;
            }

            return nearestJobSite;
        }
        
        public static Dictionary<JobSiteName, List<JobName>> EmployeeCanUseList = new()
        {
            {JobSiteName.Lumber_Yard, new List<JobName>
            {
                JobName.Logger,
                JobName.Sawyer,
                
            }},
            {JobSiteName.Smithy, new List<JobName>
            {
                JobName.Miner,
                JobName.Smith,
            }}
        };
        
        public static void ClearSOData()
        {
            S_JobSite_SO.ClearSOData();
        }
    }
    
    public enum JobSiteName
    {
        None,

        Lumber_Yard,
        Smithy
    }
}
