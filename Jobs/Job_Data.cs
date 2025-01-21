using System;
using System.Collections.Generic;
using System.Linq;
using Actor;
using ActorAction;
using Tools;

namespace Jobs
{
    [Serializable]
    public class Job_Data : Data_Class
    {
        public JobName              JobName;
        public string               JobDescription;
        public HashSet<ActorActionName> JobActions;

        public Job_Data(JobName jobName, string jobDescription, HashSet<ActorActionName> jobActions)
        {
            JobName        = jobName;
            JobDescription = jobDescription;
            JobActions       = jobActions;
        }

        public Job_Data(Job_Data jobData)
        {
            JobName        = jobData.JobName;
            JobDescription = jobData.JobDescription;
            JobActions       = jobData.JobActions;
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

        public override DataToDisplay GetDataToDisplay(bool toggleMissingDataDebugs)
        {
            _updateDataDisplay(DataToDisplay,
                title: "Base Job Data",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: GetStringData());
            
            _updateDataDisplay(DataToDisplay,
                title: "Job Tasks",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: JobActions.ToDictionary(jobTask => $"{(uint)jobTask}", jobTask => $"{jobTask}"));

            return DataToDisplay;
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
        public Job_Data                    Job_Data => _job_Data ??= Job_Manager.GetJob_Data(JobName);
        public HashSet<ActorActionName> JobActions => Job_Data.JobActions;
        
        public Job(JobName jobName, uint stationID, uint workPostID)
        {
            JobName    = jobName;
            StationID  = stationID;
            WorkPostID = workPostID;
        }
    }
}