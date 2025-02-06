using System;
using System.Collections.Generic;
using ActorActions;
using Careers;
using Inventory;
using Jobs;
using JobSites;
using Priorities;
using Priority;
using Tools;
using UnityEngine;

namespace Actors
{
    [Serializable]
    public class Actor_Data_Career : Priority_Class
    {
        public ComponentReference_Actor ActorReference => Reference as ComponentReference_Actor;
        
        public CareerName CareerName;
        
        [SerializeField] ulong _jobSiteID;
        
        JobSite_Component _jobSite;
        
        [SerializeField] Job _currentJob;

        public ulong JobSiteID
        {
            get => _jobSiteID;
            set
            {
                _jobSiteID = value;
                _jobSite = _jobSiteID is not 0 
                    ? JobSite_Manager.GetJobSite_Component(value) 
                    : null;
            }
        }
        
        public JobSite_Component JobSite => JobSiteID != 0
            ? _jobSite ??= JobSite_Manager.GetJobSite_Component(JobSiteID)
            : null;
        
        public Job CurrentJob => _currentJob ??= JobSite.GetActorJob(ActorReference.ActorID);
        
        public Actor_Data_Career(ulong actorID, CareerName careerName, ulong jobSiteID) : base(actorID, ComponentType.Actor)
        {
            CareerName = careerName;
            JobSiteID = jobSiteID;
        }

        public Actor_Data_Career(Actor_Data_Career actorDataCareer) : base(actorDataCareer.ActorReference.ActorID, ComponentType.Actor)
        {
            CareerName = actorDataCareer.CareerName;
            JobSiteID = actorDataCareer.JobSiteID;
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
                { "JobSiteID", $"{JobSiteID}" },
                { "Current Job", $"{CurrentJob?.JobName}" }
            };
        }

        public override List<ActorActionName> GetAllowedActions()
        {
            return CurrentJob?.JobActions ?? new List<ActorActionName> { ActorActionName.Idle };
        }
    }
}