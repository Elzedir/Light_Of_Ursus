using System;
using System.Collections.Generic;
using ActorActions;
using Careers;
using Inventory;
using Jobs;
using Priorities;
using Tools;

namespace Actors
{
    [Serializable]
    public class Actor_Data_Career : Priority_Class
    {
        public ComponentReference_Actor ActorReference => Reference as ComponentReference_Actor;
        
        public CareerName CareerName;
        public Job_Data Job;
        
        public Actor_Data_Career(ulong actorID, CareerName careerName, Job_Data job = null) : base(actorID, ComponentType.Actor)
        {
            CareerName = careerName;
            Job = job;
        }

        public Actor_Data_Career(Actor_Data_Career actorDataCareer) : base(actorDataCareer.ActorReference.ActorID, ComponentType.Actor)
        {
            CareerName = actorDataCareer.CareerName;
            Job = actorDataCareer.Job;
        }
        
        public override DataToDisplay GetDataToDisplay(bool toggleMissingDataDebugs)
        {
            _updateDataDisplay(DataToDisplay,
                title: "Career Data",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: GetStringData());
            
            // _updateDataDisplay(DataToDisplay,
            //     title: "Job Data",
            //     toggleMissingDataDebugs: toggleMissingDataDebugs,
            //     allSubData: JobID.GetDataToDisplay(toggleMissingDataDebugs));

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
            return Job is not null && Job.JobName != JobName.None 
                ? Job.JobActions
                : new List<ActorActionName> { ActorActionName.Idle };
        }
    }
}