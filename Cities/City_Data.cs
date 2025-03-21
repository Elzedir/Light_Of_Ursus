using System;
using System.Collections.Generic;
using System.Linq;
using Cities;
using JobSites;
using Managers;
using Tools;
using UnityEditor;
using UnityEngine;

namespace City
{
    [Serializable]
    public class City_Data : Data_Class
    {
        public ulong   CityID;
        public string CityName;
        public ulong   CityFactionID;
        public ulong   RegionID;
        public string CityDescription;

        [SerializeField] List<ulong> _allJobSiteIDs;
        
        City_Component        _city_Component;
        public City_Component City_Component => _city_Component ??= City_Manager.GetCity_Component(CityID);

        public PopulationData Population;
        
        public ProsperityData ProsperityData;
        
        int                              _currentLength;
        Dictionary<ulong, JobSite_Component> _allJobSitesInCity;
        public Dictionary<ulong, JobSite_Component> AllJobSitesInCity
        {
            get
            {
                if (_allJobSitesInCity is not null && _allJobSitesInCity.Count != 0 && _allJobSitesInCity.Count == _currentLength) return _allJobSitesInCity;
                
                _currentLength = _allJobSitesInCity?.Count ?? 0;
                return City_Component.GetAllJobSitesInCity();
            }
        }
        
        // Call when a new city is formed.
        public void RefreshAllJobSites() => _currentLength = 0;

        public City_Data(ulong       cityID, string cityName, string cityDescription, ulong cityFactionID, ulong regionID,
                         List<ulong> allJobSiteIDs, PopulationData population, ProsperityData prosperityData = null)
        {
            CityID          = cityID;
            CityName        = cityName;
            CityDescription = cityDescription;
            CityFactionID   = cityFactionID;
            RegionID        = regionID;
            _allJobSiteIDs  = allJobSiteIDs;
            Population      = population;

            ProsperityData = new ProsperityData(prosperityData);
        }

        public void InitialiseCityData()
        {
            _city_Component = City_Manager.GetCity_Component(CityID);

            if (_city_Component is not null) return;
            
            Debug.LogWarning($"City with ID {CityID} not found in City_SO.");
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
                    jobSite => jobSite.Key.ToString(),
                    jobSite => jobSite.Value.name));

            return DataToDisplay;
        }

        public override Dictionary<string, string> GetStringData()
        {
            return new Dictionary<string, string>
            {
                { "City ID", $"{CityID}" },
                { "City Name", CityName },
                { "City Faction ID", $"{CityFactionID}" },
                { "Region ID", $"{RegionID}" },
                { "City Description", CityDescription }
            };
        }
    }


    [CustomPropertyDrawer(typeof(City_Data))]
    public class CityData_Drawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var cityName = property.FindPropertyRelative("CityName");
            label.text = !string.IsNullOrEmpty(cityName.stringValue) ? cityName.stringValue : "Unnamed City";

            EditorGUI.PropertyField(position, property, label, true);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }
    }

    [Serializable]
    public class PopulationData : Data_Class
    {
        public float      CurrentPopulation;
        public float      MaxPopulation;
        public float      ExpectedPopulation;
        public List<ulong> AllCitizenIDList;

        public HashSet<ulong> AllCitizenIDs = new();
        
        public PopulationData(List<ulong> allCitizenIDList, float maxPopulation, float expectedPopulation)
        {
            foreach (var citizenID in allCitizenIDList)
            {
                AllCitizenIDs.Add(citizenID);
            }
            
            CurrentPopulation = allCitizenIDList.Count;
            MaxPopulation     = maxPopulation;
            ExpectedPopulation = expectedPopulation;
        }
        
        public override DataToDisplay GetDataToDisplay(bool toggleMissingDataDebugs)
        {
            _updateDataDisplay(DataToDisplay,
                title: "Population Data",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: GetStringData());

            _updateDataDisplay(DataToDisplay,
                title: "Citizen IDs",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: AllCitizenIDs.ToDictionary(citizenID => $"{citizenID}", citizenID => $"{citizenID}"));

            return DataToDisplay;
        }

        public override Dictionary<string, string> GetStringData()
        {
            return new Dictionary<string, string>
            {
                { "Current Population", $"{CurrentPopulation}" },
                { "Max Population", $"{MaxPopulation}" },
                { "Expected Population", $"{ExpectedPopulation}" }
            };
        }
    }
}