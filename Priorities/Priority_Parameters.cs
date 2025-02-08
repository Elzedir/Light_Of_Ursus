using System;
using System.Collections.Generic;
using System.Linq;
using Actor;
using Actors;
using Inventory;
using Items;
using JobSites;
using Station;
using UnityEngine;
using UnityEngine.Serialization;

namespace Priorities
{
    [Serializable]
    public class Priority_Parameters
    {
        public float DefaultPriorityValue = 0;
        
        public ulong ActorID_Source;
        public ulong ActorID_Target;
        public ulong JobSiteID_Source;
        public ulong JobSiteID_Target;
        
        public Station_Component HighestPriorityStation_Source;
        public Station_Component HighestPriorityStation_Target;
        
        [FormerlySerializedAs("AllStationID_Sources")] public List<Station_Component> AllStation_Sources;
        [FormerlySerializedAs("AllStationID_Targets")] public List<Station_Component> AllStation_Targets;
        
        public List<Item> Items;
        
        public Vector3 Position_Source;
        public Vector3 Position_Destination;
        
        public float DefaultMaxPriority;
        public float TotalDistance;
        public long TotalItems;
        
        public Actor_Component Actor_Component_Source => ActorID_Source != 0
        ? Actor_Manager.GetActor_Component(ActorID_Source)
        : null;
        
        public Actor_Component Actor_Component_Target => ActorID_Target != 0
            ? Actor_Manager.GetActor_Component(ActorID_Target)
            : null;
        
        public JobSite_Component JobSite_Component_Source =>  JobSiteID_Source != 0
            ? JobSite_Manager.GetJobSite_Component(JobSiteID_Source)
            : null;
        
        public JobSite_Component JobSite_Component_Target =>  JobSiteID_Target != 0
            ? JobSite_Manager.GetJobSite_Component(JobSiteID_Target)
            : null;
        
        public InventoryData Inventory_Hauler => ActorID_Source != 0 
            ? Actor_Component_Source.ActorData.InventoryData 
            : null;
    }
}