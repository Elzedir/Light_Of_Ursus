using System;
using System.Collections;
using System.Collections.Generic;
using Actors;
using ScriptableObjects;
using Station;
using UnityEditor;
using UnityEngine;

namespace Jobs
{
    public abstract class Manager_Job
    {
        const string  _allJobsSOPath = "ScriptableObjects/AllJobs_SO";
        
        static AllJobs_SO _allJobs;
        static AllJobs_SO AllJobs => _allJobs ??= _getOrCreateAllJobsSO();

        public static Job_Master GetJob_Master(JobName jobName) => AllJobs.GetJob_Master(jobName);
        
        public static void PopulateAllJobs()
        {
            AllJobs.PopulateDefaultJobs();
            // Then populate custom jobs.
        }
        
        static AllJobs_SO _getOrCreateAllJobsSO()
        {
            var allJobsSO = Resources.Load<AllJobs_SO>(_allJobsSOPath);
            
            if (allJobsSO is not null) return allJobsSO;
            
            allJobsSO = ScriptableObject.CreateInstance<AllJobs_SO>();
            AssetDatabase.CreateAsset(allJobsSO, $"Assets/Resources/{_allJobsSOPath}");
            AssetDatabase.SaveAssets();
            
            return allJobsSO;
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

    [Serializable]
    public class Job
    {
        public readonly  JobName JobName;
        [SerializeField] uint    _stationID;
        public           uint    StationID                    => _stationID;
        public           void    SetStationID(uint stationID) => _stationID = stationID;
        
        [SerializeField] uint    _operatingAreaID;
        public           uint    OperatingAreaID => _operatingAreaID;
        public           void    SetOperatingAreaID(uint operatingAreaID) => _operatingAreaID = operatingAreaID;
        
        public ActivityPeriod ActivityPeriod;

        Job_Master _job_Master;
        Job_Master Job_Master => _job_Master ??= Manager_Job.GetJob_Master(JobName);
        public HashSet<JobTaskName> JobTasks => Job_Master.JobTasks;
        
        public Job(JobName jobName, uint stationID, uint operatingAreaID)
        {
            JobName = jobName;
            _stationID = stationID;
            _operatingAreaID = operatingAreaID;
        }
        
        // public IEnumerator PerformJob(ActorComponent actor)
        // {
        //     foreach(Task_Master task in JobTasks)
        //     {
        //         yield return task.GetTaskAction(actor, );
        //     }
        // }
    }
    
    public enum ActivityPeriodName { Cathemeral, Nocturnal, Diurnal, Crepuscular }

    public abstract class ActivityPeriod
    {
        public ActivityPeriodName PeriodName;
    }

    [Serializable]
    public class Job_Master
    {
        public JobName           JobName;
        public string            JobDescription;
        public HashSet<JobTaskName> JobTasks;

        public Job_Master(JobName jobName, string jobDescription, HashSet<JobTaskName> jobTasks)
        {
            JobName        = jobName;
            JobDescription = jobDescription;
            JobTasks       = jobTasks;
        }

        public Job_Master(Job_Master jobMaster)
        {
            JobName        = jobMaster.JobName;
            JobDescription = jobMaster.JobDescription;
            JobTasks       = jobMaster.JobTasks;
        }
    }

    [CustomPropertyDrawer(typeof(Job_Master))]
    public class Job_Drawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var    stationNameProp = property.FindPropertyRelative("JobName");
            string stationName     = ((StationName)stationNameProp.enumValueIndex).ToString();

            label.text = !string.IsNullOrEmpty(stationName) ? stationName : "Unnamed Jobsite";

            EditorGUI.PropertyField(position, property, label, true);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);

        }
    }
}