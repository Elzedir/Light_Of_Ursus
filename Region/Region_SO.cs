using System;
using System.Collections.Generic;
using System.Linq;
using ScriptableObjects;
using Tools;
using UnityEditor;
using UnityEngine;

namespace Region
{
    [CreateAssetMenu(fileName = "Region_SO", menuName = "SOList/Region_SO")]
    [Serializable]
    public class Region_SO : Base_SO<Region_Data>
    {
        public Region_Data[] Regions                         => Objects;
        public Region_Data        GetRegion_Data(uint      regionID) => GetObject_Master(regionID);
        public Dictionary<uint, Region_Component> RegionComponents = new();

        public Region_Component GetRegion_Component(uint regionID)
        {
            if (RegionComponents.TryGetValue(regionID, out var component))
            {
                return component;
            }   
            
            Debug.LogError($"Region with ID {regionID} not found in Region_SO.");
            return null;
        }

        public override uint GetObjectID(int id) => Regions[id].RegionID;

        public void UpdateRegion(uint regionID, Region_Data region_Data) => UpdateObject(regionID, region_Data);
        public void UpdateAllRegions(Dictionary<uint, Region_Data> allRegions) => UpdateAllObjects(allRegions);

        public void PopulateSceneRegions()
        {
            var allRegionComponents = FindObjectsByType<Region_Component>(FindObjectsSortMode.None);
            var allRegionData =
                allRegionComponents.ToDictionary(region => region.RegionID, region => region.RegionData);

            UpdateAllRegions(allRegionData);
            
            foreach (var region in Regions)
            {
                region.ProsperityData.SetProsperity(50);
                region.ProsperityData.MaxProsperity = 100;
            }
        }

        protected override Dictionary<uint, Region_Data> _populateDefaultObjects()
        {
            return FindObjectsByType<Region_Component>(FindObjectsSortMode.None).ToDictionary(
                region => region.RegionID, region => region.RegionData);
        }

        static uint _lastUnusedRegionID = 1;

        public uint GetUnusedRegionID()
        {
            while (ObjectIndexLookup.ContainsKey(_lastUnusedRegionID))
            {
                _lastUnusedRegionID++;
            }

            return _lastUnusedRegionID;
        }
    }

    [CustomEditor(typeof(Region_SO))]
    public class Region_SOEditor : Editor
    {
        int  _selectedRegionIndex = -1;
        bool _showCitys;
        bool _showPopulation;
        bool _showProsperity;

        Vector2 _regionScrollPos;
        Vector2 _cityScrollPos;
        Vector2 _populationScrollPos;

        public override void OnInspectorGUI()
        {
            var regionSO = (Region_SO)target;

            EditorGUILayout.LabelField("All Regions", EditorStyles.boldLabel);
            _regionScrollPos = EditorGUILayout.BeginScrollView(_regionScrollPos,
                GUILayout.Height(Mathf.Min(200, regionSO.Regions.Length * 20)));
            _selectedRegionIndex = GUILayout.SelectionGrid(_selectedRegionIndex, _getRegionNames(regionSO), 1);
            EditorGUILayout.EndScrollView();

            if (_selectedRegionIndex < 0 || _selectedRegionIndex >= regionSO.Regions.Length) return;

            var selectedRegionData = regionSO.Regions[_selectedRegionIndex];
            _drawRegionAdditionalData(selectedRegionData);
        }

        static string[] _getRegionNames(Region_SO regionSO)
        {
            return regionSO.Regions.Select(c => c.RegionName).ToArray();
        }

        void _drawRegionAdditionalData(Region_Data selectedRegionData)
        {
            EditorGUILayout.LabelField("Region Data", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Region Name", selectedRegionData.RegionName);
            EditorGUILayout.LabelField("Region ID",   selectedRegionData.RegionID.ToString());
            EditorGUILayout.LabelField("Region ID", selectedRegionData.RegionID.ToString());

            if (selectedRegionData.AllCityIDs != null)
            {
                _showCitys = EditorGUILayout.Toggle("Cities", _showCitys);

                if (_showCitys)
                {
                    _drawCityAdditionalData(selectedRegionData.AllCityIDs);
                }
            }

            if (selectedRegionData.ProsperityData != null)
            {
                _showProsperity = EditorGUILayout.Toggle("Prosperity", _showProsperity);

                if (_showProsperity)
                {
                    _drawProsperityDetails(selectedRegionData.ProsperityData);
                }
            }
        }

        void _drawCityAdditionalData(List<uint> allCityIDs)
        {
            _cityScrollPos = EditorGUILayout.BeginScrollView(_cityScrollPos,
                GUILayout.Height(Mathf.Min(200, allCityIDs.Count * 20)));

            try
            {
                foreach (var cityID in allCityIDs)
                {
                    EditorGUILayout.LabelField("City Data", EditorStyles.boldLabel);
                    //EditorGUILayout.LabelField("City", CityID.CityName.ToString());
                    EditorGUILayout.LabelField("City ID", cityID.ToString());
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

        static void _drawProsperityDetails(ProsperityData prosperityData)
        {
            EditorGUILayout.LabelField("Current Prosperity", $"{prosperityData.CurrentProsperity}");
            EditorGUILayout.LabelField("Max Prosperity",     $"{prosperityData.MaxProsperity}");
            EditorGUILayout.LabelField("Base Prosperity Growth Per Day",
                $"{prosperityData.BaseProsperityGrowthPerDay}");
        }
    }
}