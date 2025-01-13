using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DataPersistence;
using Tools;
using UnityEditor;
using UnityEngine;

namespace Station
{
    [CreateAssetMenu(fileName = "Station_SO", menuName = "SOList/Station_SO")]
    [Serializable]
    public class Station_SO : Data_Component_SO<Station_Data, Station_Component>
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

        protected override Dictionary<uint, Data<Station_Data>> _getDefaultData() => 
            _convertDictionaryToData(Station_List.DefaultStations);

        protected override Dictionary<uint, Data<Station_Data>> _getSavedData()
        {
            Dictionary<uint, Station_Data> savedData = new();
            
            try
            {
                savedData = DataPersistenceManager.DataPersistence_SO.CurrentSaveData.SavedStationData.AllStationData
                    .ToDictionary(station => station.StationID, station => station);
            }
            catch (Exception ex)
            {
                var saveData = DataPersistenceManager.DataPersistence_SO.CurrentSaveData;
                
                if (saveData == null)
                {
                    Debug.LogWarning("LoadData Error: CurrentSaveData is null.");
                }
                else if (saveData.SavedStationData == null)
                {
                    Debug.LogWarning($"LoadData Error: SavedStationData is null in CurrentSaveData (SaveID: {saveData.SavedProfileData.SaveDataID}).");
                }
                else if (saveData.SavedStationData.AllStationData == null)
                {
                    Debug.LogWarning($"LoadData Error: AllStationData is null in SavedStationData (SaveID: {saveData.SavedProfileData.SaveDataID}).");
                }
                else if (!saveData.SavedStationData.AllStationData.Any())
                {
                    Debug.LogWarning($"LoadData Warning: AllStationData is empty (SaveID: {saveData.SavedProfileData.SaveDataID}).");
                }
                
                Debug.LogError($"LoadData Exception: {ex.Message}\n{ex.StackTrace}");
            }
            
            return _convertDictionaryToData(savedData);
        }

        protected override Dictionary<uint, Data<Station_Data>> _getSceneData() =>
            _convertDictionaryToData(_getSceneComponents().ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Station_Data));

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
                getData_Display: data.GetData_Display);
        }

        public override void SaveData(SaveData saveData) =>
            saveData.SavedStationData = new SavedStationData(Stations.Select(station => station.Data_Object).ToArray());
    }

    [CustomEditor(typeof(Station_SO))]
    public class Stations_SOEditor : Data_SOEditor<Station_Data>
    {
        public override Data_SO<Station_Data> SO => _so ??= (Station_SO)target;
    }
}