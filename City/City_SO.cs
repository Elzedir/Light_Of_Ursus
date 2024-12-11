using System;
using System.Collections.Generic;
using System.Linq;
using ScriptableObjects;
using UnityEditor;
using UnityEngine;

namespace City
{
    [CreateAssetMenu(fileName = "City_SO", menuName = "SOList/City_SO")]
    [Serializable]
    public class City_SO : Base_SO<City_Data>
    {
        public City_Data[]                         Cities                         => Objects;
        public City_Data                           GetCity_Data(uint      cityID) => GetObject_Master(cityID);
        public Dictionary<uint, City_Component> CityComponents = new();

        public City_Component GetCity_Component(uint cityID)
        {
            if (CityComponents.TryGetValue(cityID, out var component))
            {
                return component;
            }   
            
            Debug.LogError($"City with ID {cityID} not found in City_SO.");
            return null;
        }

        public override uint GetObjectID(int id) => Cities[id].CityID;

        public void UpdateCity(uint cityID, City_Data city_Data) => UpdateObject(cityID, city_Data);
        public void UpdateAllCities(Dictionary<uint, City_Data> allCities) => UpdateAllObjects(allCities);

        public void PopulateSceneCities()
        {
            var allCityComponents = FindObjectsByType<City_Component>(FindObjectsSortMode.None);
            var allCityData =
                allCityComponents.ToDictionary(city => city.CityID, city => city.CityData);

            UpdateAllCities(allCityData);
            
            foreach (var city in Cities)
            {
                city.ProsperityData.SetProsperity(50);
                city.ProsperityData.MaxProsperity = 100;
            }
        }

        protected override Dictionary<uint, City_Data> _populateDefaultObjects()
        {
            return FindObjectsByType<City_Component>(FindObjectsSortMode.None).ToDictionary(
                city => city.CityID, city => city.CityData); // Replace with City_List
        }

        static uint _lastUnusedCityID = 1;

        public uint GetUnusedCityID()
        {
            while (ObjectIndexLookup.ContainsKey(_lastUnusedCityID))
            {
                _lastUnusedCityID++;
            }

            return _lastUnusedCityID;
        }
    }

    [CustomEditor(typeof(City_SO))]
    public class City_SOEditor : Editor
    {
        int  _selectedCityIndex = -1;
        bool _showJobSites;
        bool _showPopulation;
        bool _showProsperity;

        Vector2 _cityScrollPos;
        Vector2 _jobSiteScrollPos;
        Vector2 _populationScrollPos;

        public override void OnInspectorGUI()
        {
            var citySO = (City_SO)target;

            EditorGUILayout.LabelField("All Cities", EditorStyles.boldLabel);
            _cityScrollPos = EditorGUILayout.BeginScrollView(_cityScrollPos,
                GUILayout.Height(Mathf.Min(200, citySO.Cities.Length * 20)));
            _selectedCityIndex = GUILayout.SelectionGrid(_selectedCityIndex, _getCityNames(citySO), 1);
            EditorGUILayout.EndScrollView();

            if (_selectedCityIndex < 0 || _selectedCityIndex >= citySO.Cities.Length) return;

            var selectedCityData = citySO.Cities[_selectedCityIndex];
            _drawCityAdditionalData(selectedCityData);
        }

        static string[] _getCityNames(City_SO citySO)
        {
            return citySO.Cities.Select(c => c.CityName).ToArray();
        }

        void _drawCityAdditionalData(City_Data selectedCityData)
        {
            EditorGUILayout.LabelField("City Data", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("City Name", $"{selectedCityData.CityName}");
            EditorGUILayout.LabelField("City ID",   $"{selectedCityData.CityID}");
            EditorGUILayout.LabelField("Region ID", $"{selectedCityData.RegionID}");

            if (selectedCityData.Population != null)
            {
                _showPopulation = EditorGUILayout.Toggle("Population", _showPopulation);

                if (_showPopulation)
                {
                    _drawPopulationDetails(selectedCityData.Population);
                }
            }

            if (selectedCityData.AllJobsiteIDs != null)
            {
                _showJobSites = EditorGUILayout.Toggle("JobSites", _showJobSites);

                if (_showJobSites)
                {
                    _drawJobSiteAdditionalData(selectedCityData.AllJobsiteIDs);
                }
            }

            if (selectedCityData.ProsperityData != null)
            {
                _showProsperity = EditorGUILayout.Toggle("Prosperity", _showProsperity);

                if (_showProsperity)
                {
                    _drawProsperityDetails(selectedCityData.ProsperityData);
                }
            }
        }

        void _drawPopulationDetails(PopulationData populationData)
        {
            EditorGUILayout.LabelField("Current Population",  $"{populationData.CurrentPopulation}");
            EditorGUILayout.LabelField("Expected Population", $"{populationData.ExpectedPopulation}");
            EditorGUILayout.LabelField("City ID",             $"{populationData.MaxPopulation}");

            EditorGUILayout.LabelField("All Citizens", EditorStyles.boldLabel);

            _populationScrollPos = EditorGUILayout.BeginScrollView(_populationScrollPos,
                GUILayout.Height(Mathf.Min(200, populationData.AllCitizenIDs.Count * 20)));

            foreach (var citizenID in populationData.AllCitizenIDs)
            {
                EditorGUILayout.LabelField($"- {citizenID}");
            }

            EditorGUILayout.EndScrollView();
        }

        void _drawJobSiteAdditionalData(List<uint> allJobSiteIDs)
        {
            _jobSiteScrollPos = EditorGUILayout.BeginScrollView(_jobSiteScrollPos,
                GUILayout.Height(Mathf.Min(200, allJobSiteIDs.Count * 20)));

            try
            {
                foreach (var jobSiteID in allJobSiteIDs)
                {
                    EditorGUILayout.LabelField("JobSite Data", EditorStyles.boldLabel);
                    //EditorGUILayout.LabelField("JobSite", jobSiteID.JobSiteName.ToString());
                    EditorGUILayout.LabelField("JobSite ID", $"{jobSiteID}");
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