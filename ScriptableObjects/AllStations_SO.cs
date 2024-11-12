using System;
using System.Collections.Generic;
using System.Linq;
using Managers;
using UnityEditor;
using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "AllStations_SO", menuName = "SOList/AllStations_SO")]
    [Serializable]
    public class AllStations_SO : ScriptableObject
    {
        public List<StationData> AllStationData;

        public void SetAllStationData(List<StationData> allStationData)
        {
            AllStationData = allStationData;
        }

        public void LoadData(SaveData saveData)
        {
            AllStationData = saveData.SavedStationData.AllStationData;
        }

        public void ClearStationData()
        {
            AllStationData.Clear();
        }
    }

    [CustomEditor(typeof(AllStations_SO))]
    public class AllStationsSOEditor : Editor
    {
        int  _selectedStationIndex = -1;
        bool _showOperatingAreas;
        bool _showInventory;

        Vector2 _stationScrollPos;
        Vector2 _operatingAreaScrollPos;

        public override void OnInspectorGUI()
        {
            AllStations_SO allStationsSO = (AllStations_SO)target;

            if (GUILayout.Button("Clear Station Data"))
            {
                allStationsSO.ClearStationData();
                EditorUtility.SetDirty(allStationsSO);
            }

            EditorGUILayout.LabelField("All Stations", EditorStyles.boldLabel);
            _stationScrollPos     = EditorGUILayout.BeginScrollView(_stationScrollPos, GUILayout.Height(_getListHeight(allStationsSO.AllStationData.Count)));
            _selectedStationIndex = GUILayout.SelectionGrid(_selectedStationIndex, _getStationNames(allStationsSO), 1);
            EditorGUILayout.EndScrollView();

            if (_selectedStationIndex >= 0 && _selectedStationIndex < allStationsSO.AllStationData.Count)
            {
                var selectedStationData = allStationsSO.AllStationData[_selectedStationIndex];
                _drawStationAdditionalData(selectedStationData);
            }
        }

        string[] _getStationNames(AllStations_SO allStationsSO)
        {
            return allStationsSO.AllStationData.Select(s => s.StationID.ToString()).ToArray();
        }

        float _getListHeight(int itemCount)
        {
            return Mathf.Min(200, itemCount * 20);
        }

        void _drawStationAdditionalData(StationData selectedStationData)
        {
            EditorGUILayout.LabelField("Station Data", EditorStyles.boldLabel);
            //EditorGUILayout.LabelField("Station Name", selectedStationData.StationName.ToString());
            EditorGUILayout.LabelField("Station ID", selectedStationData.StationID.ToString());
            EditorGUILayout.LabelField("JobSite ID", selectedStationData.JobsiteID.ToString());

            if (selectedStationData.AllOperatingAreaIDs == null) return;
            
            _showOperatingAreas = EditorGUILayout.Toggle("Operating Areas", _showOperatingAreas);

            if (_showOperatingAreas)
            {
                _drawOperatingAreaAdditionalData(selectedStationData.AllOperatingAreaIDs);
            }

            _showInventory = EditorGUILayout.Toggle("Inventory", _showInventory);

            if (_showInventory)
            {
                _drawInventoryAdditionalData(selectedStationData.InventoryData);
            }

            // _showOrders = EditorGUILayout.Toggle("Orders", _showOrders);

            // if (_showOrders)
            // {
            //     DrawOrderAdditionalData(selectedStationData.Orders);
            // }
        }

        void _drawOperatingAreaAdditionalData(List<uint> allOperatingAreaData)
        {
            _operatingAreaScrollPos = EditorGUILayout.BeginScrollView(_operatingAreaScrollPos, GUILayout.Height(_getListHeight(allOperatingAreaData.Count)));

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

        void _drawInventoryAdditionalData(InventoryData inventoryData)
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