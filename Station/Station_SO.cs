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
        public Object_Data<Station_Data>[]         Stations                        => Objects_Data;
        public Object_Data<Station_Data>           GetStation_Data(uint stationID) => GetObject_Data(stationID);
        Dictionary<uint, Station_Component>       _station_Components;
        public Dictionary<uint, Station_Component> Station_Components => _station_Components ??= _getExistingStation_Components();
        
        Dictionary<uint, Station_Component> _getExistingStation_Components()
        {
            return FindObjectsByType<Station_Component>(FindObjectsSortMode.None)
                                  .Where(station => Regex.IsMatch(station.name, @"\d+"))
                                  .ToDictionary(
                                      station => uint.Parse(new string(station.name.Where(char.IsDigit).ToArray())),
                                      station => station
                                  );
        }

        public Station_Component GetStation_Component(uint stationID)
        {
            if (stationID == 0)
            {
                Debug.LogError("StationID cannot be 0.");
                return null;
            }
            
            if (Station_Components.TryGetValue(stationID, out var component))
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

        public override void PopulateSceneData()
        {
            var physicalStations = _getExistingStation_Components();

            foreach (var station in Stations)
            {
                if (station?.DataObject is null || station.DataObjectID == 0) continue;

                if (physicalStations.TryGetValue(station.DataObject.StationID, out var physicalStation))
                {
                    physicalStation.Station_Data                     = station.DataObject;
                    Station_Components[station.DataObject.StationID] = physicalStation;
                    physicalStations.Remove(station.DataObject.StationID);
                    continue;
                }

                Debug.LogWarning($"Station with ID {station.DataObject.StationID} not found in the scene.");
            }

            foreach (var station in physicalStations)
            {
                UpdateStation(station.Key, station.Value.Station_Data);
            }
        }

        protected override Dictionary<uint, Object_Data<Station_Data>> _getDefaultDataObjects(bool initialisation = false)
        {
            if (_defaultDataObjects is null || !Application.isPlaying || initialisation)
                return _defaultDataObjects ??= _convertDictionaryToDataObject(Station_List.DefaultStations);
            
            if (Stations is null || Stations.Length == 0)
            {
                Debug.LogError("Stations is null or empty.");
                return _defaultDataObjects;
            }

            foreach (var station in Stations)
            {
                if (station?.DataObject is null || station.DataObject.StationID == 0) continue;

                if (!_defaultDataObjects.ContainsKey(station.DataObjectID))
                {
                    Debug.LogError($"Station with ID {station.DataObjectID} not found in DefaultStations.");
                    continue;
                }
                
                _defaultDataObjects[station.DataObjectID] = station;
            }
            
            return _defaultDataObjects;
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

        protected override Object_Data<Station_Data> _convertToDataObject(Station_Data dataObject)
        {
            return new Object_Data<Station_Data>(
                dataObjectID: dataObject.StationID,
                dataObject: dataObject,
                dataObjectTitle: $"{dataObject.StationID}: {dataObject.StationName}",
                getData_Display: dataObject.GetDataSO_Object);
        }
    }

    [CustomEditor(typeof(Station_SO))]
    public class Stations_SOEditor : Data_SOEditor<Station_Data>
    {
        public override Data_SO<Station_Data> SO => _so ??= (Station_SO)target;
    }
}