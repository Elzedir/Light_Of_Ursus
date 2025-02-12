using System.Collections.Generic;
using System.Linq;
using Jobs;
using UnityEngine;

namespace Station
{
    public abstract class Station_Manager
    {
        const  string     _station_SOPath = "ScriptableObjects/Station_SO";
        
        static Station_SO s_allStations;
        static Station_SO AllStations => s_allStations ??= _getStation_SO();
        
        public static Station_Data GetStation_Data(ulong stationID) => 
            AllStations.GetStation_Data(stationID).Data_Object;
        
        public static Station_Data GetStation_DataFromName(Station_Component stationComponent) =>
            AllStations.GetDataFromName(stationComponent.name)?.Data_Object;
        
        public static Station_Component GetStation_Component(ulong stationID) => 
            AllStations.GetStation_Component(stationID);
        
        public static List<ulong> GetAllStationIDs() => AllStations.GetAllDataIDs();
        
        static Station_SO _getStation_SO()
        {
            var station_SO = Resources.Load<Station_SO>(_station_SOPath);
            
            if (station_SO is not null) return station_SO;
            
            Debug.LogError("Station_SO not found. Creating temporary Station_SO.");
            station_SO = ScriptableObject.CreateInstance<Station_SO>();
            
            return station_SO;
        }

        public static Station_Component GetNearestStation(Vector3 position, StationName stationName)
        {
            Station_Component nearestStation = null;

            var nearestDistance = float.MaxValue;

            foreach (var station in AllStations.Station_Components.Values.Where(s => s.StationName == stationName))
            {
                var distance = Vector3.Distance(position, station.transform.position);

                if (!(distance < nearestDistance)) continue;

                nearestStation  = station;
                nearestDistance = distance;
            }

            return nearestStation;
        }


        public static Dictionary<StationName, List<JobName>> WorkerCanUseList = new()
        {
            {StationName.Iron_Node, new List<JobName> { JobName.Miner} },
            {StationName.Anvil, new List<JobName> { JobName.Smith} },
            {StationName.Tree, new List<JobName> { JobName.Logger} },
            {StationName.Sawmill, new List<JobName> { JobName.Sawyer} },
            {StationName.Log_Pile, new List<JobName> { JobName.Logger, JobName.Sawyer} },
            {StationName.Fishing_Spot, new List<JobName> { JobName.Fisher} },
            {StationName.Farming_Plot, new List<JobName> { JobName.Farmer} },
            {StationName.Campfire, new List<JobName> { JobName.Cook} },
            {StationName.Tanning_Station, new List<JobName> { JobName.Tanner} }
        };
        
        public static void ClearSOData()
        {
            AllStations.ClearSOData();
        }
    }

    public enum StationName
    {
        None,
        
        IdleTemp,

        Iron_Node,

        Anvil,

        Tree,
        Sawmill,
        Log_Pile,

        Fishing_Spot,
        Farming_Plot,

        Campfire,

        Tanning_Station,
    }

    public enum StationType
    {
        None,
        
        Resource,
        Crafter,
        Storage,
        Recreation
    }
}