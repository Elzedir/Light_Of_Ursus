using System;
using System.Collections.Generic;
using System.Linq;
using ActorActions;
using Careers;
using Inventory;
using Jobs;
using JobSite;
using Priority;
using Station;
using Tools;
using UnityEngine;

namespace Actors
{
    [Serializable]
    public class Actor_Data_Career : Priority_Class
    {
        public ComponentReference_Actor ActorReference => Reference as ComponentReference_Actor;

        //* Need to rework the whole Career JobSiteID, assigning thing.
        
        public CareerName CareerName;
        public HashSet<JobName> AllJobs; //* Figure out how we make use of this later.
        
        public bool JobsActive = true;
        public ulong JobSiteID;
        JobSite_Component _jobSite;
        public JobSite_Component JobSite => _jobSite ??= JobSite_Manager.GetJobSite_Component(JobSiteID);
        
        [SerializeField] Job _currentJob;
        public Job CurrentJob => _currentJob ??= new Job(JobName.Idle, 0, 0);
        public HashSet<ActorActionName> CurrentJobActions =>
            Job_Manager.GetJob_Data(CurrentJob.JobName).JobActions.ToHashSet();
        
        public Actor_Data_Career(ulong actorID, CareerName careerName, HashSet<JobName> jobsNotFromCareer = null) : base(
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

        public HashSet<ActorActionName> AllJobActions => AllJobs
            .SelectMany(jobName => Job_Manager.GetJob_Data(jobName).JobActions).ToHashSet();

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

            Debug.LogWarning($"Job {jobName} does not exist in AllActorJobs.");
        }

        public void SetCurrentJob(Job job) => _currentJob = job;
        public bool HasCurrentJob() => CurrentJob != null && CurrentJob.JobName != JobName.Idle;
        public void StopCurrentJob() => _currentJob = null;
        public bool GetNewCurrentJob(ulong stationID = 0) => 
            CareerName != CareerName.Wanderer 
            && JobSite.GetNewCurrentJob(ActorReference.Actor_Component, stationID);
        
        public void SetJobSiteID(ulong jobSiteID) => JobSiteID = jobSiteID;

        public override List<ActorActionName> GetAllowedActions()
        {
            return CareerName switch
            {
                CareerName.Lumberjack => new List<ActorActionName>
                {
                    ActorActionName.Haul_Deliver,
                    ActorActionName.Haul_Fetch,
                    ActorActionName.Chop_Wood,
                    ActorActionName.Process_Logs
                },
                CareerName.Wanderer => new List<ActorActionName>
                {
                    ActorActionName.Wander
                },
                _ => new List<ActorActionName>()
            };
        }
    }
}