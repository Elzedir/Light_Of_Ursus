using System;
using System.Collections.Generic;
using System.Linq;
using Tools;
using UnityEngine;
using UnityEngine.Serialization;

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
        
        protected override Data_Display _getDataSO_Object(bool toggleMissingDataDebugs)
        {
            var dataObjects = new List<Data_Display>();
            
            try
            {
                dataObjects.Add(new Data_Display(
                    title: "Base Job Data",
                    dataDisplayType: DataDisplayType.Item,
                    data: new List<string>
                    {
                        $"Job Name: {JobName}",
                        $"Job Description: {JobDescription}"
                    }));
            }
            catch
            {
                Debug.LogError("Error: Job Data not found.");
            }
            
            try
            {
                dataObjects.Add(new Data_Display(
                    title: "Job Tasks",
                    dataDisplayType: DataDisplayType.CheckBoxList,
                    data: JobTasks.Select(jobTask => jobTask.ToString()).ToList()));
            }
            catch
            {
                Debug.LogError("Error: Job Tasks not found.");
            }

            return new Data_Display(
                title: "Job Data",
                dataDisplayType: DataDisplayType.CheckBoxList,
                subData: dataObjects);
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