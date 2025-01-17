using System;
using System.Collections.Generic;
using System.Linq;
using Tools;
using UnityEngine;

namespace Jobs
{
    [Serializable]
    public class Job_Data : Data_Class
    {
        public JobName              JobName;
        public string               JobDescription;
        public HashSet<JobTaskName> JobTasks;

        public Job_Data(JobName jobName, string jobDescription, HashSet<JobTaskName> jobTasks)
        {
            JobName        = jobName;
            JobDescription = jobDescription;
            JobTasks       = jobTasks;
        }

        public Job_Data(Job_Data jobData)
        {
            JobName        = jobData.JobName;
            JobDescription = jobData.JobDescription;
            JobTasks       = jobData.JobTasks;
        }

        public override Dictionary<string, string> GetStringData()
        {
            return new Dictionary<string, string>
            {
                { "Job ID", $"{(uint)JobName}" },
                { "Job Name", JobName.ToString() },
                { "Job Description", JobDescription }
            };
        }

        public override DataToDisplay GetSubData(bool toggleMissingDataDebugs)
        {
            _updateDataDisplay(ref _dataToDisplay,
                title: "Base Job Data",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: GetStringData());
            
            _updateDataDisplay(ref _dataToDisplay,
                title: "Job Tasks",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: JobTasks.ToDictionary(jobTask => $"{(uint)jobTask}", jobTask => $"{jobTask}"));

            return _dataToDisplay;
        }
    }
    
    [Serializable]
    public class Job
    {
        public readonly JobName JobName;
        public          uint    StationID;
        public          uint    WorkPostID;

        public void SetStationAndWorkPostID((uint StationID, uint WorkPostID) stationAndWorkPostID)
        {
            StationID  = stationAndWorkPostID.StationID;
            WorkPostID = stationAndWorkPostID.WorkPostID;
        }
        
        public ActivityPeriod ActivityPeriod;

        Job_Data                    _job_Data;
        public Job_Data                    Job_Data => _job_Data ??= Job_Manager.GetJob_Master(JobName);
        public HashSet<JobTaskName> JobTasks => Job_Data.JobTasks;
        
        public Job(JobName jobName, uint stationID, uint workPostID)
        {
            JobName    = jobName;
            StationID  = stationID;
            WorkPostID = workPostID;
        }
        
        // public IEnumerator PerformJob(ActorComponent actor)
        // {
        //     foreach(Task_Master task in JobTasks)
        //     {
        //         yield return task.GetTaskAction(actor, );
        //     }
        // }
    }
}