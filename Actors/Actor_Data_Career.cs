using System;
using System.Collections.Generic;
using System.Linq;
using ActorAction;
using Careers;
using Inventory;
using Jobs;
using JobSite;
using Priority;
using Tools;
using UnityEngine;

namespace Actor
{
    [Serializable]
    public class Actor_Data_Career : Priority_Updater
    {
        public Actor_Data_Career(uint actorID, CareerName careerName, HashSet<JobName> jobsNotFromCareer = null) : base(
            actorID,
            ComponentType.Actor)
        {
            CareerName = careerName;
            AllJobs = jobsNotFromCareer ?? new HashSet<JobName>();

            foreach (var job in Career_Manager.GetCareer_Master(careerName).CareerBaseJobs)
            {
                AddJob(job);
            }
        }

        public Actor_Data_Career(Actor_Data_Career actorDataCareer) : base(actorDataCareer.ActorReference.ActorID, ComponentType.Actor)
        {
            CareerName = actorDataCareer.CareerName;
            AllJobs = new HashSet<JobName>(actorDataCareer.AllJobs);
        }
        
        public override DataToDisplay GetDataToDisplay(bool toggleMissingDataDebugs)
        {
            _updateDataDisplay(DataToDisplay,
                title: "Career Data",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: GetStringData());

            return DataToDisplay;
        }

        public override Dictionary<string, string> GetStringData()
        {
            return new Dictionary<string, string>
            {
                { "Career Name", $"{CareerName}" },
                { "Jobs Active", $"{JobsActive}" },
                { "JobSiteID", $"{JobSiteID}" },
                { "Current Job", $"{CurrentJob?.JobName}" }
            };
        }

        public ComponentReference_Actor ActorReference => Reference as ComponentReference_Actor;

        public CareerName CareerName;

        public void SetCareer(CareerName careerName, bool changeAllCareerJobs = true)
        {
            CareerName = careerName;

            if (!changeAllCareerJobs) return;

            AllJobs.Clear();

            var careerJobs = Career_Manager.GetCareer_Master(careerName).CareerBaseJobs;

            foreach (var job in careerJobs)
            {
                AddJob(job);
            }
        }

        List<JobName> _allJobs; // For saving purposes
        public HashSet<JobName> AllJobs;

        public HashSet<ActorActionName> AllJobActions =>
            AllJobs.SelectMany(jobName => Job_Manager.GetJob_Data(jobName).JobActions).ToHashSet();

        public void AddJob(JobName jobName)
        {
            // Find a way to use actorData to exclude jobs that are not allowed due to status and conditions like paralyzed, or
            // personalities.

            if (AllJobs.Add(jobName)) return;

            //Debug.LogWarning($"Job {jobName} already exists in AllActorJobs.");
        }

        public void RemoveJob(JobName jobName)
        {
            if (AllJobs.Remove(jobName)) return;

            //Debug.LogWarning($"Job {jobName} does not exist in AllActorJobs.");
        }

        [SerializeField] Job _currentJob;
        public Job CurrentJob => _currentJob ??= new Job(JobName.Idle, 0, 0);

        public HashSet<ActorActionName> CurrentJobActions =>
            Job_Manager.GetJob_Data(CurrentJob.JobName).JobActions.ToHashSet();

        public void SetCurrentJob(Job job)
        {
            _currentJob = job;
        }

        public bool HasCurrentJob() => CurrentJob != null && CurrentJob.JobName != JobName.Idle;

        public void StopCurrentJob() => _currentJob = null;

        public bool GetNewCurrentJob(uint stationID = 0)
        {
            return CareerName != CareerName.Wanderer &&
                   JobSite.GetNewCurrentJob(ActorReference.Actor_Component, stationID);
        }

        public bool JobsActive = true;
        public void ToggleDoJobs(bool jobsActive) => JobsActive = jobsActive;

        public uint JobSiteID;
        JobSite_Component _jobSite;
        public JobSite_Component JobSite => _jobSite ??= JobSite_Manager.GetJobSite_Component(JobSiteID);
        public void SetJobSiteID(uint jobSiteID) => JobSiteID = jobSiteID;

        protected override bool _priorityChangeNeeded(object dataChanged)
        {
            return false;
        }

        protected override Dictionary<PriorityUpdateTrigger, Dictionary<PriorityParameterName, object>>
            _priorityParameterList { get; set; } = new();

        public override List<ActorActionName> GetAllowedActions()
        {
            if ((!JobsActive || !HasCurrentJob()) && !GetNewCurrentJob())
            {
                return new List<ActorActionName> {ActorActionName.Idle};
            }
            
            //* Populate list based on Job
            return new List<ActorActionName>
            {
                ActorActionName.Idle,
                ActorActionName.Craft,
                ActorActionName.Process,
                ActorActionName.Haul
            };
        }
    }
}