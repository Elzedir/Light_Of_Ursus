using System;
using System.Collections.Generic;
using Actor;
using Jobsite;
using Managers;
using Station;
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
        ActionOrTask,
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

        Actor_Component                    _actor;
        protected override object         _component => _actor ??= Actor_Manager.GetActor(ComponentID);
        public             Actor_Component Actor      => _component as Actor_Component;

        public override GameObject GameObject => Actor.gameObject;
        public override PriorityComponent GetPriorityComponent() => Actor.DecisionMakerComponent.PriorityComponent;
    }

    public class ComponentReference_Station : ComponentReference
    {
        public uint StationID => ComponentID;
        public ComponentReference_Station(uint stationID) : base(stationID) { }

        Station_Component                    _station;
        protected override object            _component => _station ??= Station_Manager.GetStation_Component(StationID);
        public             Station_Component Station    => _component as Station_Component;

        public override GameObject           GameObject                => Station.gameObject;
        public override PriorityComponent GetPriorityComponent() => Station.Jobsite.PriorityComponent;
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