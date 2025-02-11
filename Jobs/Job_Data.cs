using System;
using System.Collections.Generic;
using System.Linq;
using ActorActions;
using Actors;
using Station;
using TickRates;
using Tools;
using UnityEngine;
using WorkPosts;

namespace Jobs
{
    public class Job_Data : Data_Class
    {
        public readonly JobName              JobName;
        public readonly string               JobDescription;
        public readonly List<ActorActionName> JobActions;

        public Job_Data(JobName jobName, string jobDescription, List<ActorActionName> jobActions)
        {
            JobName        = jobName;
            JobDescription = jobDescription;
            JobActions       = jobActions;
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
    public class Job : Data_Class
    {
        public JobName JobName;
        public ulong JobSiteID;
        public ulong StationID;
        public ulong WorkPostID;
        [SerializeField] ulong _actorID;
        public bool            IsWorkerMovingToWorkPost;

        public ulong ActorID
        {
            get => _actorID;
            set
            {
                _actorID = value;
                _actor = _actorID != 0
                    ? Actor_Manager.GetActor_Component(value)
                    : null;
                
                IsWorkerMovingToWorkPost = false;
            }
        }

        Actor_Component _actor;
        public Actor_Component Actor => _actor ??=
            _actorID != 0
                ? Actor_Manager.GetActor_Component(_actorID)
                : null;

        public ActivityPeriod ActivityPeriod;
        
        Job_Data                    _job_Data;
        public Job_Data                    Job_Data => _job_Data ??= Job_Manager.GetJob_Data(JobName);
        Station_Component _station;
        public Station_Component Station => _station ??= Station_Manager.GetStation_Component(StationID);
        WorkPost_Component _workPost;
        public WorkPost_Component WorkPost => _workPost ??= Station.Station_Data.GetWorkPost(WorkPostID);
        public List<ActorActionName> JobActions => Job_Data.JobActions;
        
        public Job(JobName jobName, ulong jobSiteID, ulong actorID, ulong stationID = 0, ulong workPostID = 0)
        {
            JobSiteID = jobSiteID;
            ActorID = actorID;
            StationID  = stationID;
            WorkPostID = workPostID;
        }
        
        public Job(Job job)
        {
            JobName    = job.JobName;
            JobSiteID = job.JobSiteID;
            ActorID = job.ActorID;
            StationID  = job.StationID;
            WorkPostID = job.WorkPostID;
        }

        public TickRateName CurrentTickRateName;
        public void OnTick()
        {
            Station.Station_Data.OnTick();
        }
        
        public override Dictionary<string, string> GetStringData()
        {
            return new Dictionary<string, string>
            {
                { "Job ID", $"{JobName}" },
                { "Job Name", $"{JobName}" },
                { "Job Site ID", $"{JobSiteID}" },
                { "Station ID", $"{StationID}" },
                { "Actor ID", $"{ActorID}" },
                { "Is Worker Moving To WorkPost", $"{IsWorkerMovingToWorkPost}" }
            };
        }

        public override DataToDisplay GetDataToDisplay(bool toggleMissingDataDebugs)
        {
            _updateDataDisplay(DataToDisplay,
                title: "Base Job Data",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: GetStringData());

            return DataToDisplay;
        }
    }
}