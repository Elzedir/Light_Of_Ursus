using System.Collections.Generic;
using UnityEngine;

namespace Jobs
{
    public abstract class Job_Manager
    {
        const string  _job_SOPath = "ScriptableObjects/Job_SO";
        
        static Job_SO s_allJobs;
        static Job_SO S_AllJobs => s_allJobs ??= _getJob_SO();

        public static List<Job_Data> AllJobData()
        {
            return S_AllJobs.AllJobData();
        }

        public static Job_Data GetJob_Data(JobName jobName) => S_AllJobs.GetJob_Data(jobName)?.Data_Object;   
        
        static Job_SO _getJob_SO()
        {
            var job_SO = Resources.Load<Job_SO>(_job_SOPath);
            
            if (job_SO is not null) return job_SO;
            
            Debug.LogError("Job_SO not found. Creating temporary Job_SO.");
            job_SO = ScriptableObject.CreateInstance<Job_SO>();
            
            return job_SO;
        }
        
        public static void ClearSOData()
        {
            S_AllJobs.ClearSOData();
        }
    }

    public enum JobName
    {
        None,
        
        Idle,
        
        Any,
        
        Unemployed,
        
        Wanderer,
        
        Hauler,
        
        Vendor,
        
        Guard,

        Researcher,

        Logger,
        Sawyer,
        
        Carpenter,

        Miner,
        
        Smith,
    
        Fisher,
        
        Farmer,
        
        Cook,
        
        Tanner
    }
    
    public enum ActivityPeriodName { None, Cathemeral, Nocturnal, Diurnal, Crepuscular }

    public abstract class ActivityPeriod
    {
        public ActivityPeriodName PeriodName;
    }
}