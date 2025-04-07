using System;
using System.Collections.Generic;
using System.Linq;
using Baronies;
using Buildings;
using Managers;
using Tools;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace Cities
{
    [Serializable]
    public class City_Data : Data_Class
    {
        public BaronyName BaronyName;
        public bool IsCapital;
        public Dictionary<int, int> BuildingSlotsPerLevel;
        
        public ulong ID;
        public ulong FactionID;
        public ulong RegionID;
        
        public string Name;
        public string Description;

        public int Size;

        [FormerlySerializedAs("_allJobSiteIDs")] [SerializeField] List<ulong> _allBuildingIDs;

        Barony_Component _barony;
        public Barony_BuildingData Buildings;
        public City_PopulationData Population;
        public City_ProsperityData Prosperity;

        const int c_maxCitySize = 5;
        
        public Barony_Component Barony => _barony ??= City_Manager.GetCity_Component(ID);

        Dictionary<ulong, Building_Component> _allJobSitesInCity;

        public Dictionary<ulong, Building_Component> AllJobSitesInCity
        {
            get
            {
                if (_allJobSitesInCity is not null && _allJobSitesInCity.Count != 0) return _allJobSitesInCity;

                return Barony.GetAllBuildingsInCity();
            }
        }

        public City_Data(ulong id, string name, string description, ulong factionID, ulong regionID,
            List<ulong> allBuildingIDs, City_PopulationData population, City_ProsperityData cityProsperityData = null)
        {
            ID = id;
            Name = name;
            Description = description;
            FactionID = factionID;
            RegionID = regionID;
            _allBuildingIDs = allBuildingIDs;
            Population = population;

            Prosperity = new City_ProsperityData(cityProsperityData);
        }

        public void InitialiseCityData()
        {
            _barony = City_Manager.GetCity_Component(ID);

            if (_barony is not null) return;

            Debug.LogWarning($"City with ID {ID} not found in City_SO.");
        }

        public override DataToDisplay GetDataToDisplay(bool toggleMissingDataDebugs)
        {
            _updateDataDisplay(DataToDisplay,
                title: "City Data",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: GetStringData());

            _updateDataDisplay(DataToDisplay,
                title: "Population Data",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allSubData: Population.GetDataToDisplay(toggleMissingDataDebugs));

            _updateDataDisplay(DataToDisplay,
                title: "City JobSites",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: AllJobSitesInCity.ToDictionary(
                    building => building.Key.ToString(),
                    building => building.Value.name));

            return DataToDisplay;
        }

        public override Dictionary<string, string> GetStringData()
        {
            return new Dictionary<string, string>
            {
                { "City ID", $"{ID}" },
                { "City Name", Name },
                { "City Faction ID", $"{FactionID}" },
                { "Region ID", $"{RegionID}" },
                { "City Description", Description }
            };
        }
    }


    [CustomPropertyDrawer(typeof(City_Data))]
    public class CityData_Drawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var cityName = property.FindPropertyRelative("CityName");
            label.text = !string.IsNullOrEmpty(cityName?.stringValue) ? cityName?.stringValue : "Unnamed City";

            EditorGUI.PropertyField(position, property, label, true);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }
    }
}