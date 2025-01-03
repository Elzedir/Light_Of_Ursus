using System;
using System.Collections.Generic;
using System.Linq;
using City;
using Managers;
using Relationships;
using Tools;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace Region
{
    [Serializable]
    public class Region_Data : Data_Class
    {
        public uint   RegionID;
        public string RegionName;
        public int    RegionFactionID;
        public string RegionDescription;

        Region_Component        _region_Component;
        public Region_Component Region_Component => _region_Component ??= Region_Manager.GetRegion_Component(RegionID);

        public ProsperityData ProsperityData;

        public           FactionName     Faction;
        [SerializeField] List<uint>      _allCityIDs;
        int                              _currentLength;
        Dictionary<uint, City_Component> _allCitiesInRegion;

        public Dictionary<uint, City_Component> AllCitiesInRegion
        {
            get
            {
                if (_allCitiesInRegion is not null && _allCitiesInRegion.Count != 0 &&
                    _allCitiesInRegion.Count == _currentLength) return _allCitiesInRegion;

                _currentLength = _allCitiesInRegion?.Count ?? 0;
                return Region_Component.GetAllCitiesInRegion().ToDictionary(city => city.CityID);
            }
        }

        public uint GetNearestCityInRegion(Vector3 position)
        {
            return AllCitiesInRegion
                   .OrderBy(city => Vector3.Distance(position, city.Value.transform.position))
                   .FirstOrDefault().Key;
        }

        // Call when a new city is formed.
        public void RefreshAllCities() => _currentLength = 0;

        public Region_Data(uint       regionID,   string regionName, string regionDescription, int regionFactionID,
                           List<uint> allCityIDs, ProsperityData prosperityData = null)
        {
            RegionID          = regionID;
            RegionName        = regionName;
            RegionDescription = regionDescription;
            RegionFactionID   = regionFactionID;
            _allCityIDs       = allCityIDs;

            ProsperityData = new ProsperityData(prosperityData);
        }

        public void InitialiseRegionData()
        {
            foreach (var city in AllCitiesInRegion
                         .Where(city_Component => !_allCityIDs.Contains(city_Component.Key)))
            {
                Debug.Log(
                    $"City_Component: {city.Value?.CityData?.CityID}: {city.Value?.CityData?.CityName} doesn't exist in DataList");
            }

            foreach (var cityID in _allCityIDs
                         .Where(cityID => !AllCitiesInRegion.ContainsKey(cityID)))
            {
                Debug.LogError($"City with ID {cityID} doesn't exist physically in Region: {RegionID}: {RegionName}");
            }
        }

        protected override Data_Display _getDataSO_Object(bool toggleMissingDataDebugs, Data_Display dataSO_Object)
        {
            if (dataSO_Object.Data is null && dataSO_Object.SubData is null)
                dataSO_Object = new Data_Display(
                    title: "Region Data",
                    dataDisplayType: DataDisplayType.List_CheckBox,
                    existingDataSO_Object: dataSO_Object,
                    subData: new Dictionary<string, Data_Display>(),
                    firstData: true);
                
            try
            {
                if (!dataSO_Object.SubData.TryGetValue("Base Region Data", out var baseRegionData))
                {
                    dataSO_Object.SubData["Base Region Data"] = new Data_Display(
                        title: "Base Region Data",
                        dataDisplayType: DataDisplayType.List_Item,
                        existingDataSO_Object: dataSO_Object,
                        data: new Dictionary<string, string>());    
                }
                
                if (baseRegionData is not null)
                {
                    baseRegionData.Data = new Dictionary<string, string>
                    {
                        { "Region ID", $"{RegionID}" },
                        { "Region Name", RegionName },
                        { "Region Faction ID", $"{RegionFactionID}" },
                        { "Region Description", RegionDescription },
                        { "Prosperity Data", ProsperityData.ToString() },
                        { "Faction", $"{Faction}" },
                        { "All City IDs", string.Join(", ", _allCityIDs) }
                    };
                }
            }
            catch
            {
                Debug.LogError("Error: Base Region Data not found.");
            }

            try
            {
                if (!dataSO_Object.SubData.TryGetValue("Region Cities", out var regionCities))
                {
                    dataSO_Object.SubData["Region Cities"] = new Data_Display(
                        title: "Region Cities",
                        dataDisplayType: DataDisplayType.List_Selectable,
                        existingDataSO_Object: dataSO_Object,
                        subData: new Dictionary<string, Data_Display>());
                }
                
                if (regionCities is not null)
                {
                    regionCities.Data = new Dictionary<string, string>
                    {
                        { "Current Prosperity", $"{ProsperityData.CurrentProsperity}" },
                        { "Max Prosperity", $"{ProsperityData.MaxProsperity}" },
                        { "Base Prosperity Growth", $"{ProsperityData.BaseProsperityGrowthPerDay}" }
                    };
                }
            }
            catch
            {
                Debug.LogError("Error: ProsperityData.");
            }

            return dataSO_Object;
        }
    }

    [CustomPropertyDrawer(typeof(Region_Data))]
    public class RegionData_Drawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var regionName = property.FindPropertyRelative("RegionName");
            label.text = !string.IsNullOrEmpty(regionName.stringValue) ? regionName.stringValue : "Unnamed Region";

            EditorGUI.PropertyField(position, property, label, true);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);

        }
    }
}