using System;
using System.Collections.Generic;
using System.Linq;
using EmployeePosition;
using Initialisation;
using ScriptableObjects;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Station
{
    public abstract class Manager_Station : IDataPersistence
    {
        public static AllStations_SO                AllStations;
        public static Dictionary<uint, Station_Data> AllStationData = new();

        static uint _lastUnusedStationID = 1;

        public void SaveData(SaveData saveData) =>
            saveData.SavedStationData = new SavedStationData(AllStationData.Values.ToList());

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

            if (saveData.SavedStationData.AllStationData.Count == 0)
            {
                //Debug.Log("AllStationData count is 0.");
                return;
            }

            AllStationData = saveData.SavedStationData.AllStationData.ToDictionary(x => x.StationID);
        }

        public void OnSceneLoaded()
        {
            AllStations = Resources.Load<AllStations_SO>("ScriptableObjects/AllStations_SO");

            Manager_Initialisation.OnInitialiseManagerStation += _initialise;
        }

        void _initialise()
        {
            foreach (var station in Object.FindObjectsByType(typeof(Station_Component)))
            {
                if (station.StationData == null)
                {
                    Debug.Log($"Station: {station.name} does not have StationData.");
                    continue;
                }

                if (!AllStationComponents.ContainsKey(station.StationData.StationID))
                    AllStationComponents.Add(station.StationData.StationID, station);
                else
                {
                    if (AllStationComponents[station.StationData.StationID].gameObject == station.gameObject) continue;
                    else
                    {
                        throw new ArgumentException(
                            $"StationID {station.StationData.StationID} and name {station.name} already exists for station {AllStationComponents[station.StationData.StationID].name}");
                    }
                }

                if (!AllStationData.ContainsKey(station.StationData.StationID))
                {
                    //Debug.Log($"Station: {station.StationData.StationID}: {station.StationName} was not in AllStationData");
                    AddToAllStationData(station.StationData);
                }

                station.SetStationData(GetStationData(station.StationData.StationID));
            }

            foreach (var stationData in AllStationData.Values)
            {
                stationData.InitialiseStationData();
            }

            AllStations.AllStationData = AllStationData.Values.ToList();
        }

        public void AddToAllStationData(Station_Data stationData)
        {
            if (AllStationData.ContainsKey(stationData.StationID))
            {
                Debug.Log($"AllStationData already contains StationID: {stationData.StationID}");
                return;
            }

            AllStationData.Add(stationData.StationID, stationData);
        }

        public void UpdateAllStationData(Station_Data stationData)
        {
            if (!AllStationData.ContainsKey(stationData.StationID))
            {
                Debug.Log($"AllStationData does not contain StationID: {stationData.StationID}");
                return;
            }

            AllStationData[stationData.StationID] = stationData;
        }


        public static Station_Data GetStationData(uint stationID)
        {
            if (!AllStationData.ContainsKey(stationID))
            {
                Debug.Log($"Station: {stationID} is not in AllStationData list");
                return null;
            }

            return AllStationData[stationID];
        }

        public static Station_Component GetStation(uint stationID)
        {
            if (!AllStationComponents.ContainsKey(stationID))
            {
                Debug.Log($"Station: {stationID} is not in AllStationComponents list");
                return null;
            }

            return AllStationComponents[stationID];
        }

        public static void GetNearestStationToPosition(Vector3              position, StationName stationName,
                                                       out Station_Component nearestStation)
        {
            nearestStation = null;
            float nearestDistance = float.MaxValue;

            foreach (var station in AllStationComponents)
            {
                float distance = Vector3.Distance(position, station.Value.transform.position);

                if (distance < nearestDistance)
                {
                    nearestStation  = station.Value;
                    nearestDistance = distance;
                }
            }
        }

        public static uint GetStationID()
        {
            while (AllStationData.ContainsKey(_lastUnusedStationID))
            {
                _lastUnusedStationID++;
            }

            return _lastUnusedStationID;
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