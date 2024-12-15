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
        public readonly  JobName JobName;
        [SerializeField] uint    _stationID;
        public           uint    StationID                    => _stationID;
        public           void    SetStationID(uint stationID) => _stationID = stationID;
        
        [SerializeField] uint _operatingAreaID;
        public           uint OperatingAreaID                          => _operatingAreaID;
        public           void SetOperatingAreaID(uint operatingAreaID) => _operatingAreaID = operatingAreaID;
        
        public ActivityPeriod ActivityPeriod;

        Job_Data                    _job_Data;
        Job_Data                    Job_Data => _job_Data ??= Job_Manager.GetJob_Master(JobName);
        public HashSet<JobTaskName> JobTasks => Job_Data.JobTasks;
        
        public Job(JobName jobName, uint stationID, uint operatingAreaID)
        {
            JobName          = jobName;
            _stationID       = stationID;
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
}