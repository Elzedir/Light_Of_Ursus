using System.Collections.Generic;
using Actor;
using Jobs;
using JobSite;
using Station;
using UnityEngine;

namespace Priority
{
    public abstract class Priority_Manager
    {
        public static readonly List<ActorActionName> BasePriorityActorActions = new()
        {
            ActorActionName.Idle,
            ActorActionName.Perform_JobTask
        };
        
        public static readonly List<JobTaskName> BasePriorityJobTasks = new()
        {
            JobTaskName.Idle,
            JobTaskName.Fetch_Items,
            JobTaskName.Deliver_Items,
            JobTaskName.Chop_Wood,
            JobTaskName.Process_Logs
        };
    }
    
    public abstract class ComponentReference
    {
        public uint ComponentID { get; }
        public ComponentReference(uint componentID) => ComponentID = componentID;
        protected abstract object               _component { get; }
        public abstract    GameObject           GameObject { get; }
        public abstract    Priority_Data GetPriorityComponent();
    }
    public class ComponentReference_Actor : ComponentReference
    {
        public uint ActorID => ComponentID;
        public ComponentReference_Actor(uint actorID) : base(actorID) { }
        Actor_Component                    _actor;
        protected override object         _component => _actor ??= Actor_Manager.GetActor_Component(ComponentID);
        public             Actor_Component Actor_Component      => _component as Actor_Component;
        public override GameObject GameObject => Actor_Component.gameObject;
        public override Priority_Data GetPriorityComponent() => Actor_Component.DecisionMakerComponent.PriorityData;
    }
    public class ComponentReference_Station : ComponentReference
    {
        public uint StationID => ComponentID;
        public ComponentReference_Station(uint stationID) : base(stationID) { }
        Station_Component                    _station;
        protected override object            _component => _station ??= Station_Manager.GetStation_Component(StationID);
        public             Station_Component Station    => _component as Station_Component;
        public override GameObject           GameObject                => Station.gameObject;
        public override Priority_Data GetPriorityComponent() => Station.JobSite.JobSiteData.PriorityData;
    }
    public class ComponentReference_Jobsite : ComponentReference
    {
        public uint JobsiteID => ComponentID;
        public ComponentReference_Jobsite(uint jobsiteID) : base(jobsiteID) { }
        JobSite_Component                    _jobSite;
        protected override object           _component => _jobSite ??= JobSite_Manager.GetJobSite_Component(JobsiteID);
        public             JobSite_Component JobSite    => _component as JobSite_Component;
        public override GameObject           GameObject                => JobSite.gameObject;
        
        public override Priority_Data GetPriorityComponent() => JobSite.JobSiteData.PriorityData;
    }
    
    public enum PriorityParameterName
    {
        None,

        // At some point, figure out how we want to apply maxPriority, maybe per parameter? Like every TotalItems, TotalDistance, etc. has an attached maxPriority.
        DefaultMaxPriority,
        
        Jobsite_Component,
        Worker,
        CurrentStationType,
        AllStationTypes,
        
        Worker_Component,
        Target_Component,
        
        Total_Items,
        Total_Distance,
    }

    public enum PriorityImportance
    {
        None,

        Critical,
        High,
        Medium,
        Low,
    }
}