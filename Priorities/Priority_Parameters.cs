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
        public ulong StationID_Source;
        public ulong StationID_Target;
        
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
        
        public Station_Component Station_Component_Source =>  StationID_Source != 0
            ? Station_Manager.GetStation_Component(StationID_Source)
            : null;
        
        public Station_Component Station_Component_Target =>  StationID_Target != 0
            ? Station_Manager.GetStation_Component(StationID_Target)
            : null; 
        
        public InventoryData Inventory_Hauler => ActorID_Source != 0 
            ? Actor_Component_Source.ActorData.InventoryData 
            : null;
        
        public InventoryData Inventory_Source => StationID_Source != 0 
            ? Station_Component_Source.Station_Data.InventoryData 
            : null;
        
        public InventoryData Inventory_Target => StationID_Target != 0 
            ? Station_Component_Target.Station_Data.InventoryData 
            : null;
        
        public StationName StationName_Source => StationID_Source != 0 
            ? Station_Component_Source.StationName 
            : StationName.None;
        
        public StationName StationName_Target => StationID_Target != 0 
            ? Station_Component_Target.StationName 
            : StationName.None;
        
        public HashSet<StationName> StationType_All => JobSiteID_Source != 0 
            ? JobSite_Component_Source.JobSite_Data.AllStations.Values.Select(station => station.StationName).ToHashSet() 
            : null;
    }
}