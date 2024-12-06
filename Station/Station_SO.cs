using System;
using System.Collections.Generic;
using System.Linq;
using Inventory;
using ScriptableObjects;
using UnityEditor;
using UnityEngine;

namespace Station
{
    [CreateAssetMenu(fileName = "AllStations_SO", menuName = "SOList/AllStations_SO")]
    [Serializable]
    public class Station_SO : Base_SO<Station_Component>
    {
        public Station_Component[] Stations                             => Objects;
        public Station_Data        GetStation_Data(uint      stationID) => GetObject_Master(stationID).StationData;
        public Station_Component   GetStation_Component(uint stationID) => GetObject_Master(stationID);

        public Station_Data[] Save_SO()
        {
            return Stations.Select(station => station.StationData).ToArray();
        }
        
        public void Load_SO(Station_Data[] stationData)
        {
            foreach (var station in stationData)
            {
                if (!_station_Components.ContainsKey(station.StationID))
                {
                    Debug.LogError($"Station with ID {station.StationID} not found in Station_SO.");
                    continue;
                }
                
                _station_Components[station.StationID].StationData = station;
            }

            LoadSO(_station_Components.Values.ToArray());
        }

        public override uint GetObjectID(int id) => Stations[id].StationID;

        public void UpdateStation(uint stationID, Station_Component station_Component) => UpdateObject(stationID, station_Component);
        public void UpdateAllStations(Dictionary<uint, Station_Component> allStations) => UpdateAllObjects(allStations);

        public void PopulateSceneStations()
        {
            var allStationComponents = FindObjectsByType<Station_Component>(FindObjectsSortMode.None);
            var allStationData =
                allStationComponents.ToDictionary(station => station.StationID);

            UpdateAllStations(allStationData);
        }

        protected override Dictionary<uint, Station_Component> _populateDefaultObjects()
        {
            return FindObjectsByType<Station_Component>(FindObjectsSortMode.None).ToDictionary(
                station => station.StationID);
        }
        
        static uint _lastUnusedStationID = 1;
        
        public uint GetUnusedStationID()
        {
            while (ObjectIndexLookup.ContainsKey(_lastUnusedStationID))
            {
                _lastUnusedStationID++;
            }

            return _lastUnusedStationID;
        }
        
        Dictionary<uint, Station_Component> _station_Components => DefaultObjects;
    }

    [CustomEditor(typeof(Station_SO))]
    public class Stations_SOEditor : Editor
    {
        int  _selectedStationIndex = -1;
        
        bool _showOperatingAreas;
        bool _showInventory;
        bool _showPriority; 

        Vector2 _stationScrollPos;
        Vector2 _operatingAreaScrollPos;

        public override void OnInspectorGUI()
        {
            var stationSO = (Station_SO)target;

            EditorGUILayout.LabelField("All Stations", EditorStyles.boldLabel);

            _stationScrollPos = EditorGUILayout.BeginScrollView(_stationScrollPos,
                GUILayout.Height(Mathf.Min(200, stationSO.Stations.Length * 20)));
            _selectedStationIndex = GUILayout.SelectionGrid(_selectedStationIndex, _getStationNames(stationSO), 1);
            
            EditorGUILayout.EndScrollView();

            if (_selectedStationIndex < 0 || _selectedStationIndex >= stationSO.Stations.Length) return;
            
            var selectedStation = stationSO.Stations[_selectedStationIndex];
            
            _drawStationAdditionalData(selectedStation);
        }

        string[] _getStationNames(Station_SO stationSO)
        {
            return stationSO.Stations.Select(s => s.StationID.ToString()).ToArray();
        }

        void _drawStationAdditionalData(Station_Component selectedStation)
        {
            EditorGUILayout.LabelField("Station Data", EditorStyles.boldLabel);
            
            EditorGUILayout.LabelField("Station Name", $"{selectedStation.StationName}");
            EditorGUILayout.LabelField("Station ID",   $"{selectedStation.StationID}");
            EditorGUILayout.LabelField("JobSite ID",   $"{selectedStation.StationData.JobsiteID}");

            if (selectedStation.StationData.AllOperatingAreaIDs != null)
            {
                _showOperatingAreas = EditorGUILayout.Toggle("Operating Areas", _showOperatingAreas);

                if (_showOperatingAreas)
                {
                    _drawOperatingAreaAdditionalData(selectedStation.StationData.AllOperatingAreaIDs);
                }                
            }
            
            if (selectedStation.StationData.InventoryData != null)
            {
                _showInventory = EditorGUILayout.Toggle("Inventory", _showInventory);

                if (_showInventory)
                {
                    _drawInventoryAdditionalData(selectedStation.StationData.InventoryData);
                }
            }
        }

        void _drawOperatingAreaAdditionalData(List<uint> allOperatingAreaData)
        {
            _operatingAreaScrollPos = EditorGUILayout.BeginScrollView(_operatingAreaScrollPos,
                GUILayout.Height(Mathf.Min(200, allOperatingAreaData.Count * 20)));

            try
            {
                foreach (var operatingAreaID in allOperatingAreaData)
                {
                    EditorGUILayout.LabelField("Operating Area Data", EditorStyles.boldLabel);
                    //EditorGUILayout.LabelField("Operating Area Name", operatingArea.OperatingAreaName.ToString());
                    EditorGUILayout.LabelField("Operating Area ID", operatingAreaID.ToString());
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error: {e.Message}");
            }
            finally
            {
                EditorGUILayout.EndScrollView();
            }
        }

        static void _drawInventoryAdditionalData(InventoryData inventoryData)
        {
            EditorGUILayout.LabelField("Inventory Data", EditorStyles.boldLabel);

            foreach (var inventoryItem in inventoryData.AllInventoryItems.Values)
            {
                EditorGUILayout.LabelField("Item ID",       inventoryItem.ItemID.ToString());
                EditorGUILayout.LabelField("Item Name",     inventoryItem.ItemName);
                EditorGUILayout.LabelField("Item Quantity", inventoryItem.ItemAmount.ToString());
            }
        }
    }
}