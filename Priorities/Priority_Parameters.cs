using System;
using System.Collections.Generic;
using System.Linq;
using Actor;
using Actors;
using Inventory;
using Items;
using JobSite;
using Station;
using UnityEngine;

namespace Priorities
{
    [Serializable]
    public class Priority_Parameters
    {
        public ulong ActorID_Source;
        public Actor_Component Actor_Component_Source => Actor_Manager.GetActor_Component(ActorID_Source);
        
        public ulong ActorID_Target;
        public Actor_Component Actor_Component_Target => Actor_Manager.GetActor_Component(ActorID_Target);
        
        public ulong JobSiteID_Source;
        public JobSite_Component JobSite_Component_Source => JobSite_Manager.GetJobSite_Component(JobSiteID_Source);
        
        public ulong JobSiteID_Target;
        public JobSite_Component JobSite_Component_Target => JobSite_Manager.GetJobSite_Component(JobSiteID_Target);
        
        public ulong StationID_Source;
        public Station_Component Station_Component_Source => Station_Manager.GetStation_Component(StationID_Source);
        
        public ulong StationID_Target;
        public Station_Component Station_Component_Target => Station_Manager.GetStation_Component(StationID_Target); 
        
        public List<Item> Items;
        
        public Vector3 Position_Source;
        public Vector3 Position_Destination;
        
        public float DefaultMaxPriority;
        public float TotalDistance;
        public long TotalItems;
        
        public InventoryData Inventory_Hauler => Actor_Component_Source.ActorData.InventoryData;
        public InventoryData Inventory_Target => Station_Component_Target.Station_Data.InventoryData;
        
        public StationName StationName_Source => StationID_Source != 0 ? Station_Component_Source.StationName : StationName.None;
        public StationName StationName_Target => StationID_Target != 0 ? Station_Component_Target.StationName : StationName.None;
        public HashSet<StationName> StationType_All => JobSite_Component_Source.JobSiteData.AllStationComponents.Values
            .Select(station => station.StationName).ToHashSet();
    }
}