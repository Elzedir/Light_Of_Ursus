using System;
using System.Collections.Generic;
using System.Linq;
using Cities;
using Faction;
using Managers;
using Region;
using Tools;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace Regions
{
    [Serializable]
    public class Region_Data : Data_Class
    {
        public ulong   RegionID;
        public string RegionName;
        public int    RegionFactionID;
        public string RegionDescription;

        County_Component        _county_Component;
        public County_Component County_Component => _county_Component ??= Region_Manager.GetRegion_Component(RegionID);

        public           FactionName     Faction;
        [SerializeField] List<ulong>      _allCityIDs;
        int                              _currentLength;
        Dictionary<ulong, City_Component> _allCitiesInRegion;
        
        public Dictionary<ulong, City_Component> AllCitiesInRegion
        {
            get
            {
                if (_allCitiesInRegion is not null && _allCitiesInRegion.Count != 0 &&
                    _allCitiesInRegion.Count == _currentLength) return _allCitiesInRegion;

                _currentLength = _allCitiesInRegion?.Count ?? 0;
                return County_Component.GetAllCitiesInRegion().ToDictionary(city => city.ID);
            }
        }

        public ulong GetNearestCityInRegion(Vector3 position)
        {
            return AllCitiesInRegion
                   .OrderBy(city => Vector3.Distance(position, city.Value.transform.position))
                   .FirstOrDefault().Key;
        }

        // Call when a new city is formed.
        public void RefreshAllCities() => _currentLength = 0;

        public Region_Data(ulong       regionID,   string regionName, string regionDescription, int regionFactionID,
                           List<ulong> allCityIDs, City_ProsperityData cityProsperityData = null)
        {
            RegionID          = regionID;
            RegionName        = regionName;
            RegionDescription = regionDescription;
            RegionFactionID   = regionFactionID;
            _allCityIDs       = allCityIDs;
        }
        
        public void InitialiseRegionData()
        {
            _county_Component = Region_Manager.GetRegion_Component(RegionID);

            if (_county_Component is not null) return;
            
            Debug.LogWarning($"Region with ID {RegionID} not found in Region_SO.");
        }

        public override Dictionary<string, string> GetStringData()
        {
            return new Dictionary<string, string>
            {
                { "Region ID", $"{RegionID}" },
                { "Region Name", RegionName },
                { "Region Faction ID", $"{RegionFactionID}" },
                { "Region Description", RegionDescription },
                { "Faction", $"{Faction}" },
                { "All City IDs", string.Join(", ", _allCityIDs) }
            };
        }

        public override DataToDisplay GetDataToDisplay(bool toggleMissingDataDebugs)
        {
            _updateDataDisplay(DataToDisplay,
                title: "Base Region Data",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: GetStringData());

            _updateDataDisplay(DataToDisplay,
                title: "Region Cities",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: AllCitiesInRegion.ToDictionary(
                    city => city.Key.ToString(),
                    city => city.Value.name));

            return DataToDisplay;
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