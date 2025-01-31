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
using UnityEngine.Serialization;
using WorkPosts;

namespace Priority
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
        
        public ulong WorkPostID_Source;
        public WorkPost_Component WorkPost_Component_Source => Station_Component_Source.Station_Data.AllWorkPost_Components.GetValueOrDefault(WorkPostID_Source, null);

        public ulong WorkPostID_Target;
        public WorkPost_Component WorkPost_Component_Target => Station_Component_Target.Station_Data.AllWorkPost_Components.GetValueOrDefault(WorkPostID_Target, null); 
        
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

        public Priority_Parameters(ulong actorID_Source = 0, ulong actorID_Target = 0, ulong jobSiteID_Source = 0,
            ulong jobSiteID_Target = 0, ulong stationID_Source = 0, ulong stationID_Target = 0, ulong workPostID_Source = 0,
            ulong workPostID_Target = 0, List<Item> items = null, Vector3 position_Source = default, Vector3 position_Destination = default,
            float defaultMaxPriority = 0, float totalDistance = 0, long totalItems = 0)
        {
            ActorID_Source = actorID_Source;
            ActorID_Target = actorID_Target;
            JobSiteID_Source = jobSiteID_Source;
            JobSiteID_Target = jobSiteID_Target;
            StationID_Source = stationID_Source;
            StationID_Target = stationID_Target;
            WorkPostID_Source = workPostID_Source;
            WorkPostID_Target = workPostID_Target;
            Items = items;
            Position_Source = position_Source;
            Position_Destination = position_Destination;
            DefaultMaxPriority = defaultMaxPriority;
            TotalDistance = totalDistance;
            TotalItems = totalItems;
        }
    }
}