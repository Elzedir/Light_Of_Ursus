using System;
using System.Collections.Generic;
using System.Linq;
using ScriptableObjects;
using UnityEditor;
using UnityEngine;

namespace Region
{
    [CreateAssetMenu(fileName = "Region_SO", menuName = "SOList/Region_SO")]
    [Serializable]
    public class Region_SO : Base_SO<Region_Component>
    {
        public Region_Component[] Regions                         => Objects;
        public Region_Data        GetRegion_Data(uint      regionID) => GetObject_Master(regionID).RegionData;
        public Region_Component   GetRegion_Component(uint regionID) => GetObject_Master(regionID);

        public Region_Data[] Save_SO()
        {
            return Regions.Select(region => region.RegionData).ToArray();
        }

        public void Load_SO(Region_Data[] regionData)
        {
            foreach (var region in regionData)
            {
                if (!_region_Components.ContainsKey(region.RegionID))
                {
                    Debug.LogError($"Region with ID {region.RegionID} not found in Region_SO.");
                    continue;
                }

                _region_Components[region.RegionID].RegionData = region;
            }

            LoadSO(_region_Components.Values.ToArray());
        }

        public override uint GetObjectID(int id) => Regions[id].RegionID;

        public void UpdateRegion(uint regionID, Region_Component region_Component) => UpdateObject(regionID, region_Component);
        public void UpdateAllRegions(Dictionary<uint, Region_Component> allRegions) => UpdateAllObjects(allRegions);

        public void PopulateSceneRegions()
        {
            var allRegionComponents = FindObjectsByType<Region_Component>(FindObjectsSortMode.None);
            var allRegionData =
                allRegionComponents.ToDictionary(region => region.RegionID);

            UpdateAllRegions(allRegionData);
            
            foreach (var region in Regions)
            {
                region.RegionData.ProsperityData.SetProsperity(50);
                region.RegionData.ProsperityData.MaxProsperity = 100;
            }
        }

        protected override Dictionary<uint, Region_Component> _populateDefaultObjects()
        {
            return FindObjectsByType<Region_Component>(FindObjectsSortMode.None).ToDictionary(
                region => region.RegionID);
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

        Dictionary<uint, Region_Component> _region_Components => DefaultObjects;
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
            return regionSO.Regions.Select(c => c.RegionData.RegionName).ToArray();
        }

        void _drawRegionAdditionalData(Region_Component selectedRegion)
        {
            EditorGUILayout.LabelField("Region Data", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Region Name", selectedRegion.RegionData.RegionName);
            EditorGUILayout.LabelField("Region ID",   selectedRegion.RegionID.ToString());
            EditorGUILayout.LabelField("Region ID", selectedRegion.RegionData.RegionID.ToString());

            if (selectedRegion.RegionData.AllCityIDs != null)
            {
                _showCitys = EditorGUILayout.Toggle("Cities", _showCitys);

                if (_showCitys)
                {
                    _drawCityAdditionalData(selectedRegion.RegionData.AllCityIDs);
                }
            }

            if (selectedRegion.RegionData.ProsperityData != null)
            {
                _showProsperity = EditorGUILayout.Toggle("Prosperity", _showProsperity);

                if (_showProsperity)
                {
                    _drawProsperityDetails(selectedRegion.RegionData.ProsperityData);
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