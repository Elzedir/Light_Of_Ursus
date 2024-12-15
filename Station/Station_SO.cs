using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Tools;
using UnityEditor;
using UnityEngine;

namespace Station
{
    [CreateAssetMenu(fileName = "Station_SO", menuName = "SOList/Station_SO")]
    [Serializable]
    public class Station_SO : Data_SO<Station_Data>
    {
        public Data_Object<Station_Data>[]         Stations                        => DataObjects;
        public Data_Object<Station_Data>           GetStation_Data(uint stationID) => GetDataObject_Master(stationID);
        public Dictionary<uint, Station_Component> StationComponents = new();

        public Station_Component GetStation_Component(uint stationID)
        {
            if (StationComponents.TryGetValue(stationID, out var component))
            {
                return component;
            }

            Debug.LogError($"Station with ID {stationID} not found in Station_SO.");
            return null;
        }

        public override uint GetDataObjectID(int id) => Stations[id].DataObject.StationID;

        public void UpdateStation(uint stationID, Station_Data station_Data) =>
            UpdateDataObject(stationID, station_Data);

        public void UpdateAllStations(Dictionary<uint, Station_Data> allStations) => UpdateAllDataObjects(allStations);

        public void PopulateSceneStations()
        {
            if (_defaultStations.Count == 0)
            {
                Debug.Log("No Default Stations Found");
            }

            var existingStations = FindObjectsByType<Station_Component>(FindObjectsSortMode.None)
                                     .Where(station => Regex.IsMatch(station.name, @"\d+"))
                                     .ToDictionary(
                                         station => uint.Parse(new string(station.name.Where(char.IsDigit).ToArray())),
                                         station => station
                                     );

            foreach (var station in Stations)
            {
                if (station?.DataObject is null) continue;
                
                if (existingStations.TryGetValue(station.DataObject.StationID, out var existingStation))
                {
                    existingStation.StationData = station.DataObject;
                    StationComponents[station.DataObject.StationID] = existingStation;
                    existingStations.Remove(station.DataObject.StationID);
                    continue;
                }
                
                Debug.LogWarning($"Station with ID {station.DataObject.StationID} not found in the scene.");
            }
            
            foreach (var station in existingStations)
            {
                if (DataObjectIndexLookup.ContainsKey(station.Key))
                {
                    Debug.LogWarning($"Station with ID {station.Key} wasn't removed from existingStations.");
                    continue;
                }
                
                Debug.LogWarning($"Station with ID {station.Key} does not have DataObject in Station_SO.");
            }
        }

        protected override Dictionary<uint, Data_Object<Station_Data>> _populateDefaultDataObjects()
        {
            var defaultStations = new Dictionary<uint, Station_Data>();

            foreach (var defaultStation in Station_List.DefaultStations)
            {
                defaultStations.Add(defaultStation.Key, defaultStation.Value);
            }

            return _convertDictionaryToDataObject(defaultStations);
        }

        static uint _lastUnusedStationID = 1;

        public uint GetUnusedStationID()
        {
            while (DataObjectIndexLookup.ContainsKey(_lastUnusedStationID))
            {
                _lastUnusedStationID++;
            }

            return _lastUnusedStationID;
        }

        Dictionary<uint, Data_Object<Station_Data>> _defaultStations => DefaultDataObjects;

        protected override Data_Object<Station_Data> _convertToDataObject(Station_Data data)
        {
            return new Data_Object<Station_Data>(
                dataObjectID: data.StationID,
                dataObject: data,
                dataObjectTitle: $"{data.StationID}: {data.StationName}",
                data_Display: data.DataSO_Object(ToggleMissingDataDebugs));
        }
    }

    [CustomEditor(typeof(Station_SO))]
    public class Stations_SOEditor : Data_SOEditor<Station_Data>
    {
        public override Data_SO<Station_Data> SO => _so ??= (Station_SO)target;
    }
}