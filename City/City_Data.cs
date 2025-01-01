using System;
using System.Collections.Generic;
using System.Linq;
using JobSite;
using Managers;
using Tools;
using UnityEditor;
using UnityEngine;

namespace City
{
    [Serializable]
    public class City_Data : Data_Class
    {
        public uint   CityID;
        public string CityName;
        public uint   CityFactionID;
        public uint   RegionID;
        public string CityDescription;

        [SerializeField] List<uint> _allJobSiteIDs;
        
        City_Component        _city_Component;
        public City_Component City_Component => _city_Component ??= City_Manager.GetCity_Component(CityID);

        public PopulationData Population;
        
        public ProsperityData ProsperityData;
        
        int                              _currentLength;
        Dictionary<uint, JobSite_Component> _allJobSitesInCity;
        public Dictionary<uint, JobSite_Component> AllJobSitesInCity
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

        public City_Data(uint       cityID, string cityName, string cityDescription, uint cityFactionID, uint regionID,
                         List<uint> allJobSiteIDs, PopulationData population, ProsperityData prosperityData = null)
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
            foreach (var jobSite in AllJobSitesInCity
                                                 .Where(jobSite_Component => !_allJobSiteIDs.Contains(jobSite_Component.Key)))
            {
                Debug.Log($"JobSite_Component: {jobSite.Value?.JobSiteData?.JobSiteID}: {jobSite.Value?.JobSiteData?.JobSiteName} doesn't exist in DataList");
            }
            
            foreach (var jobSiteID in _allJobSiteIDs
                         .Where(jobSiteID => !AllJobSitesInCity.ContainsKey(jobSiteID)))
            {
                Debug.LogError($"JobSite with ID {jobSiteID} doesn't exist physically in City: {CityID}: {CityName}");
            }
        }

        protected override Data_Display _getDataSO_Object(bool toggleMissingDataDebugs, ref Data_Display dataSO_Object)
        {
            var dataObjects = dataSO_Object == null
                ? new Dictionary<string, Data_Display>()
                : new Dictionary<string, Data_Display>(dataSO_Object.SubData);
            
            try
            {
                dataObjects["City Data"] = new Data_Display(
                    title: "City Data",
                    dataDisplayType: DataDisplayType.Item,
                    dataSO_Object: dataSO_Object,
                    data: new Dictionary<string, string>
                    {
                        { "City ID", $"{CityID}" },
                        { "City Name", CityName },
                        { "City Faction ID", $"{CityFactionID}" },
                        { "Region ID", $"{RegionID}" },
                        { "City Description", CityDescription }
                    });
            }
            catch
            {
                Debug.LogError("Error in Base Career Data");
            }
            
            try
            {
                dataObjects["Population Data"] = new Data_Display(
                    title: "Population Data",
                    dataDisplayType: DataDisplayType.CheckBoxList,
                    dataSO_Object: dataSO_Object,
                    subData: Population.GetDataSO_Object(toggleMissingDataDebugs).SubData);
            }
            catch
            {
                Debug.LogError("Error in Base Career Data");
            }

            return dataSO_Object = new Data_Display(
                title: $"{CityID}: {CityName}",
                dataDisplayType: DataDisplayType.CheckBoxList,
                dataSO_Object: dataSO_Object,
                subData: dataObjects);
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
        public List<uint> AllCitizenIDList;

        public HashSet<uint> AllCitizenIDs = new();
        
        public PopulationData(List<uint> allCitizenIDList, float maxPopulation, float expectedPopulation)
        {
            foreach (var citizenID in allCitizenIDList)
            {
                AllCitizenIDs.Add(citizenID);
            }
            
            CurrentPopulation = allCitizenIDList.Count;
            MaxPopulation     = maxPopulation;
            ExpectedPopulation = expectedPopulation;
        }
        
        protected override Data_Display _getDataSO_Object(bool toggleMissingDataDebugs, ref Data_Display dataSO_Object)
        {
            var dataObjects = dataSO_Object == null
                ? new Dictionary<string, Data_Display>()
                : new Dictionary<string, Data_Display>(dataSO_Object.SubData);
            
            try
            {
                dataObjects["Population Data"] = new Data_Display(
                    title: "Population Data",
                    dataDisplayType: DataDisplayType.Item,
                    dataSO_Object: dataSO_Object,
                    data: new Dictionary<string, string>
                    {
                        { "Current Population", $"{CurrentPopulation}" },
                        { "Max Population", $"{MaxPopulation}" },
                        { "Expected Population", $"{ExpectedPopulation}" }
                    });
            }
            catch
            {
                Debug.LogError("Error in Base Career Data");
            }
            
            try
            {
                dataObjects["Citizen IDs"] = new Data_Display(
                    title: "Citizen IDs",
                    dataDisplayType: DataDisplayType.CheckBoxList,
                    dataSO_Object: dataSO_Object,
                    data: AllCitizenIDs.ToDictionary(citizenID => $"{citizenID}", citizenID => $"{citizenID}"));
            }
            catch
            {
                Debug.LogError("Error in Base Career Data");
            }

            return dataSO_Object = new Data_Display(
                title: "Population Data",
                dataDisplayType: DataDisplayType.CheckBoxList,
                dataSO_Object: dataSO_Object,
                subData: dataObjects);
        }
    }
}