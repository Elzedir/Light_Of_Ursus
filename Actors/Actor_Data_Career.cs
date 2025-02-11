using System;
using System.Collections.Generic;
using ActorActions;
using Careers;
using Inventory;
using Jobs;
using JobSites;
using Priorities;
using Tools;

namespace Actors
{
    [Serializable]
    public class Actor_Data_Career : Priority_Class
    {
        public ComponentReference_Actor ActorReference => Reference as ComponentReference_Actor;
        
        public CareerName CareerName;
        public Job CurrentJob;
        
        public Actor_Data_Career(ulong actorID, CareerName careerName, ulong jobSiteID = 0) : base(actorID, ComponentType.Actor)
        {
            CareerName = careerName;
            CurrentJob = jobSiteID == 0 
                ? JobSite_Manager.GetJobSite_Component(jobSiteID).GetActorJob(actorID)
                : null;
        }

        public Actor_Data_Career(Actor_Data_Career actorDataCareer) : base(actorDataCareer.ActorReference.ActorID, ComponentType.Actor)
        {
            CareerName = actorDataCareer.CareerName;
            CurrentJob = actorDataCareer.CurrentJob;
        }
        
        public override DataToDisplay GetDataToDisplay(bool toggleMissingDataDebugs)
        {
            _updateDataDisplay(DataToDisplay,
                title: "Career Data",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: GetStringData());
            
            _updateDataDisplay(DataToDisplay,
                title: "Job Data",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allSubData: CurrentJob.GetDataToDisplay(toggleMissingDataDebugs));

            return DataToDisplay;
        }

        public override Dictionary<string, string> GetStringData()
        {
            return new Dictionary<string, string>
            {
                { "Career Name", $"{CareerName}" }
            };
        }

        public override List<ActorActionName> GetAllowedActions()
        {
            return CurrentJob is not null && CurrentJob.JobName != JobName.None 
                ? CurrentJob.JobActions
                : new List<ActorActionName> { ActorActionName.Idle };
        }
    }
}