using System.Collections.Generic;
using Actor;
using Actors;
using Jobs;
using JobSite;
using Station;
using UnityEngine;

namespace Priority
{
    public abstract class Priority_Manager
    {
        
    }
    
    public abstract class ComponentReference
    {
        public ulong ComponentID { get; }
        public ComponentReference(ulong componentID) => ComponentID = componentID;
        protected abstract object               _component { get; }
        public abstract    GameObject           GameObject { get; }
        public abstract    Priority_Data GetPriorityComponent();
    }
    public class ComponentReference_Actor : ComponentReference
    {
        public ulong ActorID => ComponentID;
        public ComponentReference_Actor(ulong actorID) : base(actorID) { }
        Actor_Component                    _actor;
        protected override object         _component => _actor ??= Actor_Manager.GetActor_Component(ComponentID);
        public             Actor_Component Actor_Component      => _component as Actor_Component;
        public Actor_Data ActorData => Actor_Component.ActorData;
        public override GameObject GameObject => Actor_Component.gameObject;
        public override Priority_Data GetPriorityComponent() => Actor_Component.ActorData.Priority;
    }
    public class ComponentReference_Station : ComponentReference
    {
        public ulong StationID => ComponentID;
        public ComponentReference_Station(ulong stationID) : base(stationID) { }
        Station_Component                    _station;
        protected override object            _component => _station ??= Station_Manager.GetStation_Component(StationID);
        public             Station_Component Station    => _component as Station_Component;
        public Station_Data StationData => Station.Station_Data;
        public override GameObject           GameObject                => Station.gameObject;
        public override Priority_Data GetPriorityComponent() => Station.JobSite.JobSiteData.PriorityData;
    }
    public class ComponentReference_Jobsite : ComponentReference
    {
        public ulong JobsiteID => ComponentID;
        public ComponentReference_Jobsite(ulong jobsiteID) : base(jobsiteID) { }
        JobSite_Component                    _jobSite;
        protected override object           _component => _jobSite ??= JobSite_Manager.GetJobSite_Component(JobsiteID);
        public             JobSite_Component JobSite    => _component as JobSite_Component;
        public JobSite_Data JobSiteData => JobSite.JobSiteData;
        public override GameObject           GameObject                => JobSite.gameObject;
        
        public override Priority_Data GetPriorityComponent() => JobSite.JobSiteData.PriorityData;
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