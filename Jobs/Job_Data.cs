using System;
using System.Collections.Generic;
using System.Linq;
using ActorActions;
using Tools;

namespace Jobs
{
    [Serializable]
    public class Job_Data : Data_Class
    {
        public JobName              JobName;
        public string               JobDescription;
        public List<ActorActionName> JobActions;

        public Job_Data(JobName jobName, string jobDescription, List<ActorActionName> jobActions)
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
                { "Job ID", $"{(ulong)JobName}" },
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
                allStringData: JobActions.ToDictionary(jobTask => $"{(ulong)jobTask}", jobTask => $"{jobTask}"));

            return DataToDisplay;
        }
    }
    
    [Serializable]
    public class Job
    {
        public readonly JobName JobName;
        public ulong JobSiteID;
        public ulong StationID;
        public ulong WorkPostID;

        public ActivityPeriod ActivityPeriod;
        
        Job_Data                    _job_Data;
        public Job_Data                    Job_Data => _job_Data ??= Job_Manager.GetJob_Data(JobName);
        public List<ActorActionName> JobActions => Job_Data.JobActions;
        
        public void SetStationAndWorkPostID((ulong StationID, ulong WorkPostID) stationAndWorkPostID)
        {
            StationID  = stationAndWorkPostID.StationID;
            WorkPostID = stationAndWorkPostID.WorkPostID;
        }
        
        public Job(JobName jobName, ulong jobSiteID, ulong stationID = 0, ulong workPostID = 0)
        {
            JobName    = jobName;
            JobSiteID = jobSiteID;
            StationID  = stationID;
            WorkPostID = workPostID;
        }
    }
}