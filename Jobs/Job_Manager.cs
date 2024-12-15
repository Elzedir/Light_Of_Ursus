using System;
using System.Collections;
using System.Collections.Generic;
using ScriptableObjects;
using Station;
using UnityEditor;
using UnityEngine;

namespace Jobs
{
    public abstract class Job_Manager
    {
        const string  _job_SOPath = "ScriptableObjects/Job_SO";
        
        static Job_SO _allJobs;
        static Job_SO AllJobs => _allJobs ??= _getJob_SO();

        public static Job_Data GetJob_Master(JobName jobName) => AllJobs.GetJob_Master(jobName).DataObject;
        
        public static void PopulateAllJobs()
        {
            AllJobs.PopulateDefaultJobs();
            // Then populate custom jobs.
        }
        
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
            AllJobs.ClearSOData();
        }
    }

    public enum JobName
    {
        Idle, 
        
        Wanderer,
        
        Hauler,
        
        Vendor,
        
        Guard,

        Researcher,

        Harvester,

        Logger,
        Sawmiller,
        
        Carpenter,

        Smith,
    
    }
    
    public enum ActivityPeriodName { Cathemeral, Nocturnal, Diurnal, Crepuscular }

    public abstract class ActivityPeriod
    {
        public ActivityPeriodName PeriodName;
    }
}