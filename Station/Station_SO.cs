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
        public Data<Station_Data>[]         Stations                        => Data;
        public Data<Station_Data>           GetStation_Data(uint stationID) => GetData(stationID);
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

        public override uint GetDataID(int id) => Stations[id].Data_Object.StationID;

        public void UpdateStation(uint stationID, Station_Data station_Data) =>
            UpdateData(stationID, station_Data);

        public void UpdateAllStations(Dictionary<uint, Station_Data> allStations) => UpdateAllData(allStations);

        public override void PopulateSceneData()
        {
            var physicalStations = _getExistingStation_Components();

            foreach (var station in Stations)
            {
                if (station?.Data_Object is null || station.DataID == 0) continue;

                if (physicalStations.TryGetValue(station.Data_Object.StationID, out var physicalStation))
                {
                    physicalStation.Station_Data                     = station.Data_Object;
                    Station_Components[station.Data_Object.StationID] = physicalStation;
                    physicalStations.Remove(station.Data_Object.StationID);
                    continue;
                }

                Debug.LogWarning($"Station with ID {station.Data_Object.StationID} not found in the scene.");
            }

            foreach (var station in physicalStations)
            {
                UpdateStation(station.Key, station.Value.Station_Data);
            }
        }

        protected override Dictionary<uint, Data<Station_Data>> _getDefaultData(bool initialisation = false)
        {
            if (_defaultData is null || !Application.isPlaying || initialisation)
                return _defaultData ??= _convertDictionaryToData(Station_List.DefaultStations);
            
            if (Stations is null || Stations.Length == 0)
            {
                Debug.LogError("Stations is null or empty.");
                return _defaultData;
            }

            foreach (var station in Stations)
            {
                if (station?.Data_Object is null || station.Data_Object.StationID == 0) continue;

                if (!_defaultData.ContainsKey(station.DataID))
                {
                    Debug.LogError($"Station with ID {station.DataID} not found in DefaultStations.");
                    continue;
                }
                
                _defaultData[station.DataID] = station;
            }
            
            return _defaultData;
        }

        static uint _lastUnusedStationID = 1;

        public uint GetUnusedStationID()
        {
            while (DataIndexLookup.ContainsKey(_lastUnusedStationID))
            {
                _lastUnusedStationID++;
            }

            return _lastUnusedStationID;
        }

        protected override Data<Station_Data> _convertToData(Station_Data data)
        {
            return new Data<Station_Data>(
                dataID: data.StationID,
                data_Object: data,
                dataTitle: $"{data.StationID}: {data.StationName}",
                getData_Display: data.GetDataSO_Object);
        }
    }

    [CustomEditor(typeof(Station_SO))]
    public class Stations_SOEditor : Data_SOEditor<Station_Data>
    {
        public override Data_SO<Station_Data> SO => _so ??= (Station_SO)target;
    }
}