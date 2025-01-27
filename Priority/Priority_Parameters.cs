using System;
using System.Collections.Generic;
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
        
        public ulong StationID_Destination;
        public Station_Component Station_Component_Destination => Station_Manager.GetStation_Component(StationID_Destination);
        
        public ulong WorkPostID_Source;
        public WorkPost_Component WorkPost_Component_Source => Station_Component_Source.Station_Data.AllWorkPost_Components.GetValueOrDefault(WorkPostID_Source, null);

        public ulong WorkPostID_Destination;
        public WorkPost_Component WorkPost_Component_Destination => Station_Component_Destination.Station_Data.AllWorkPost_Components.GetValueOrDefault(WorkPostID_Destination, null); 
        
        public List<Item> Items;
        
        public Vector3 Position_Source;
        public Vector3 Position_Destination;
        
        public float DefaultMaxPriority;
        public float TotalDistance;
        public long TotalItems;
        public InventoryData Inventory_Hauler;
        public InventoryData Inventory_Target;
        
        public StationName StationType_Source;
        public StationName StationType_Destination;
        public HashSet<StationName> StationType_All;

        public Priority_Parameters(ulong actorID_Source = 0, ulong actorID_Target = 0, ulong jobSiteID_Source = 0,
            ulong jobSiteID_Target = 0, ulong stationID_Source = 0, ulong stationID_Destination = 0, ulong workPostID_Source = 0,
            ulong workPostID_Destination = 0, List<Item> items = null, Vector3 position_Source = default, Vector3 position_Destination = default,
            float defaultMaxPriority = 0, float totalDistance = 0, long totalItems = 0, InventoryData inventory_Hauler = null,
            InventoryData inventory_Target = null, StationName stationType_Source = StationName.None, StationName stationType_Destination = StationName.None,
            HashSet<StationName> stationType_All = null)
        {
            ActorID_Source = actorID_Source;
            ActorID_Target = actorID_Target;
            JobSiteID_Source = jobSiteID_Source;
            JobSiteID_Target = jobSiteID_Target;
            StationID_Source = stationID_Source;
            StationID_Destination = stationID_Destination;
            WorkPostID_Source = workPostID_Source;
            WorkPostID_Destination = workPostID_Destination;
            Items = items;
            Position_Source = position_Source;
            Position_Destination = position_Destination;
            DefaultMaxPriority = defaultMaxPriority;
            TotalDistance = totalDistance;
            TotalItems = totalItems;
            Inventory_Hauler = inventory_Hauler;
            Inventory_Target = inventory_Target;
            StationType_Source = stationType_Source;
            StationType_Destination = stationType_Destination;
            StationType_All = stationType_All;
        }
    }
}