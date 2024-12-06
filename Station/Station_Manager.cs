using System.Collections.Generic;
using System.Linq;
using EmployeePosition;
using Initialisation;
using UnityEditor;
using UnityEngine;

namespace Station
{
    public abstract class Station_Manager : IDataPersistence
    {
        const  string     _stations_SOPath = "ScriptableObjects/Station_SO";
        
        static Station_SO _station_SO;
        static Station_SO Station_SO => _station_SO ??= _getOrCreateStation_SO();

        public void SaveData(SaveData saveData) =>
            saveData.SavedStationData = new SavedStationData(Station_SO.Save_SO());

        public void LoadData(SaveData saveData)
        {
            if (saveData == null)
            {
                //Debug.Log("No SaveData found in LoadData.");
                return;
            }

            if (saveData.SavedStationData == null)
            {
                //Debug.Log("No SavedStationData found in SaveData.");
                return;
            }

            if (saveData.SavedStationData.AllStationData == null)
            {
                //Debug.Log("No AllStationData found in SavedStationData.");
                return;
            }

            if (saveData.SavedStationData.AllStationData.Length == 0)
            {
                //Debug.Log("AllStationData count is 0.");
                return;
            }

            Station_SO.Load_SO(saveData.SavedStationData.AllStationData);
        }

        public static void OnSceneLoaded()
        {
            Manager_Initialisation.OnInitialiseManagerStation += _initialise;
        }

        static void _initialise()
        {
            Station_SO.PopulateSceneStations();
        }
        
        public static Station_Data GetStation_Data(uint stationID)
        {
            return Station_SO.GetStation_Data(stationID);
        }
        
        public static Station_Component GetStation_Component(uint stationID)
        {
            return Station_SO.GetStation_Component(stationID);
        }
        
        static Station_SO _getOrCreateStation_SO()
        {
            var station_SO = Resources.Load<Station_SO>(_stations_SOPath);
            
            if (station_SO is not null) return station_SO;
            
            station_SO = ScriptableObject.CreateInstance<Station_SO>();
            AssetDatabase.CreateAsset(station_SO, $"Assets/Resources/{_stations_SOPath}");
            AssetDatabase.SaveAssets();
            
            return station_SO;
        }

        public static Station_Component GetNearestStation(Vector3 position, StationName stationName)
        {
            Station_Component nearestStation = null;

            var nearestDistance = float.MaxValue;

            foreach (var station in Station_SO.Stations.Where(s => s.StationName == stationName))
            {
                var distance = Vector3.Distance(position, station.transform.position);

                if (!(distance < nearestDistance)) continue;

                nearestStation  = station;
                nearestDistance = distance;
            }

            return nearestStation;
        }

        public static uint GetUnusedStationID()
        {
            return Station_SO.GetUnusedStationID();
        }


        public static Dictionary<StationName, List<EmployeePositionName>> EmployeeCanUseList = new()
        {
            {StationName.Iron_Node, new List<EmployeePositionName> { EmployeePositionName.Miner} },
            {StationName.Anvil, new List<EmployeePositionName> { EmployeePositionName.Smith} },
            {StationName.Tree, new List<EmployeePositionName> { EmployeePositionName.Logger} },
            {StationName.Sawmill, new List<EmployeePositionName> { EmployeePositionName.Sawyer} },
            {StationName.Log_Pile, new List<EmployeePositionName> { EmployeePositionName.Hauler} },
            {StationName.Fishing_Spot, new List<EmployeePositionName> { EmployeePositionName.Fisher} },
            {StationName.Farming_Plot, new List<EmployeePositionName> { EmployeePositionName.Farmer} },
            {StationName.Campfire, new List<EmployeePositionName> { EmployeePositionName.Cook} },
            {StationName.Tanning_Station, new List<EmployeePositionName> { EmployeePositionName.Tanner} }
        };

        public static StationType GetStationType(StationName stationNAme)
        {
            if (_stationTypesByName.TryGetValue(stationNAme, out var type)) return type;
            
            Debug.Log($"StationType for {stationNAme} not found.");
            return StationType.None;
        }

        static readonly Dictionary<StationName, StationType> _stationTypesByName = new()
        {
            {
                StationName.Iron_Node, StationType.Resource
            },
            {
                StationName.Anvil, StationType.Crafter
            },
            {
                StationName.Tree, StationType.Resource
            },
            {
                StationName.Sawmill, StationType.Crafter
            },
            {
                StationName.Log_Pile, StationType.Storage
            },
            {
                StationName.Fishing_Spot, StationType.Resource
            },
            {
                StationName.Farming_Plot, StationType.Resource
            },
            {
                StationName.Campfire, StationType.Crafter
            },
            {
                StationName.Tanning_Station, StationType.Crafter
            }
        };
    }

    public enum StationName
    {
        None,

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
        Storage
    }
}