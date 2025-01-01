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
        
        protected override Data_Display _getDataSO_Object(bool toggleMissingDataDebugs, ref Data_Display dataSO_Object)
        {
            var dataObjects = dataSO_Object == null
                ? new Dictionary<string, Data_Display>()
                : new Dictionary<string, Data_Display>(dataSO_Object.SubData);
            
            try
            {
                dataObjects["Base Job Data"] = new Data_Display(
                    title: "Base Job Data",
                    dataDisplayType: DataDisplayType.Item,
                    dataSO_Object: dataSO_Object,
                    data: new Dictionary<string, string>
                    {
                        { "Job ID", $"{(uint)JobName}" },
                        { "Job Name", JobName.ToString() },
                        { "Job Description", JobDescription }
                    });
            }
            catch
            {
                Debug.LogError("Error: Job Data not found.");
            }
            
            try
            {
                dataObjects["Job Tasks"] = new Data_Display(
                    title: "Job Tasks",
                    dataDisplayType: DataDisplayType.CheckBoxList,
                    dataSO_Object: dataSO_Object,
                    data: JobTasks.ToDictionary(jobTask => $"{(uint)jobTask}", jobTask => $"{jobTask}"));
            }
            catch
            {
                Debug.LogError("Error: Job Tasks not found.");
            }
            
            return dataSO_Object = new Data_Display(
                title: "Job Data",
                dataDisplayType: DataDisplayType.CheckBoxList,
                dataSO_Object: dataSO_Object,
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