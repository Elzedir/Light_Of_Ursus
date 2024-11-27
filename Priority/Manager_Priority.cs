using System;
using System.Collections.Generic;
using Actors;
using Jobsite;
using Managers;
using UnityEngine;

namespace Priority
{
    public class Manager_Priority : MonoBehaviour
    {
    
    }
    
    public enum PriorityParameterName
    {
        None,

        // At some point, figure out how we want to apply maxPriority, maybe per parameter? Like every TotalItems, TotalDistance, etc. has an attached maxPriority.
        DefaultPriority,
        TotalItems,
        TotalDistance,
        InventoryHauler,
        InventoryTarget,
        CurrentStationType,
        AllStationTypes,
        Jobsite,
    }

    public enum PriorityImportance
    {
        None,

        Critical,
        High,
        Medium,
        Low,
    }

    public abstract class ComponentReference
    {
        public uint ComponentID { get; private set; }
        public ComponentReference(uint componentID) => ComponentID = componentID;

        protected abstract object               _component { get; }
        public abstract    GameObject           GameObject { get; }
        public abstract    PriorityComponent GetPriorityComponent();
    }

    public class ComponentReference_Actor : ComponentReference
    {
        public uint ActorID => ComponentID;
        public ComponentReference_Actor(uint actorID) : base(actorID) { }

        ActorComponent                    _actor;
        protected override object         _component => _actor ??= Manager_Actor.GetActor(ComponentID);
        public             ActorComponent Actor      => _component as ActorComponent;

        public override GameObject GameObject => Actor.gameObject;
        public override PriorityComponent GetPriorityComponent() => Actor.DecisionMakerComponent.PriorityComponent;
    }

    public class ComponentReference_Station : ComponentReference
    {
        public uint StationID => ComponentID;
        public ComponentReference_Station(uint stationID) : base(stationID) { }

        StationComponent                    _station;
        protected override object           _component => _station ??= Manager_Station.GetStation(StationID);
        public             StationComponent Station    => _component as StationComponent;

        public override GameObject           GameObject                => Station.gameObject;
        
        public override PriorityComponent GetPriorityComponent() => Station.PriorityComponent;
    }

    public class ComponentReference_Jobsite : ComponentReference
    {
        public uint JobsiteID => ComponentID;
        public ComponentReference_Jobsite(uint jobsiteID) : base(jobsiteID) { }

        JobsiteComponent                    _jobsite;
        protected override object           _component => _jobsite ??= Manager_Jobsite.GetJobsite(JobsiteID);
        public             JobsiteComponent Jobsite    => _component as JobsiteComponent;

        public override GameObject           GameObject                => Jobsite.gameObject;
        
        public override PriorityComponent GetPriorityComponent() => Jobsite.PriorityComponent;
    }
}