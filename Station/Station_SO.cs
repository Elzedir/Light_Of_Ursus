using System;
using System.Collections.Generic;
using System.Linq;
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
        public Dictionary<uint, Station_Component> Station_Components => _getSceneComponents();

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
            catch
            {
                var saveData = DataPersistenceManager.DataPersistence_SO.CurrentSaveData;
                
                if (ToggleMissingDataDebugs)
                {
                    Debug.LogWarning(saveData == null
                        ? "LoadData Error: CurrentSaveData is null."
                        : saveData.SavedStationData == null
                            ? $"LoadData Error: SavedStationData is null in CurrentSaveData (SaveID: {saveData.SavedProfileData.SaveDataID})."
                            : saveData.SavedStationData.AllStationData == null
                                ? $"LoadData Error: AllStationData is null in SavedStationData (SaveID: {saveData.SavedProfileData.SaveDataID})."
                                : !saveData.SavedStationData.AllStationData.Any()
                                    ? $"LoadData Warning: AllStationData is empty (SaveID: {saveData.SavedProfileData.SaveDataID})."
                                    : string.Empty);
                }
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
                getDataToDisplay: data.GetDataToDisplay);
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