using System;
using System.Collections.Generic;
using System.Linq;
using ScriptableObjects;
using UnityEditor;
using UnityEngine;

namespace City
{
    [CreateAssetMenu(fileName = "AllCities_SO", menuName = "SOList/AllCities_SO")]
    [Serializable]
    public class City_SO : Base_SO<City_Component>
    {
        public City_Component[] Cities                         => Objects;
        public City_Data        GetCity_Data(uint      cityID) => GetObject_Master(cityID).CityData;
        public City_Component   GetCity_Component(uint cityID) => GetObject_Master(cityID);

        public City_Data[] Save_SO()
        {
            return Cities.Select(city => city.CityData).ToArray();
        }

        public void Load_SO(City_Data[] cityData)
        {
            foreach (var city in cityData)
            {
                if (!_city_Components.ContainsKey(city.CityID))
                {
                    Debug.LogError($"City with ID {city.CityID} not found in City_SO.");
                    continue;
                }

                _city_Components[city.CityID].CityData = city;
            }

            LoadSO(_city_Components.Values.ToArray());
        }

        public override uint GetObjectID(int id) => Cities[id].CityID;

        public void UpdateCity(uint cityID, City_Component city_Component) => UpdateObject(cityID, city_Component);
        public void UpdateAllCities(Dictionary<uint, City_Component> allCities) => UpdateAllObjects(allCities);

        public void PopulateSceneCities()
        {
            var allCityComponents = FindObjectsByType<City_Component>(FindObjectsSortMode.None);
            var allCityData =
                allCityComponents.ToDictionary(city => city.CityID);

            UpdateAllCities(allCityData);
            
            foreach (var city in Cities)
            {
                city.CityData.ProsperityData.SetProsperity(50);
                city.CityData.ProsperityData.MaxProsperity = 100;
            }
        }

        protected override Dictionary<uint, City_Component> _populateDefaultObjects()
        {
            return FindObjectsByType<City_Component>(FindObjectsSortMode.None).ToDictionary(
                city => city.CityID);
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

        Dictionary<uint, City_Component> _city_Components => DefaultObjects;
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
            return citySO.Cities.Select(c => c.CityData.CityName).ToArray();
        }

        void _drawCityAdditionalData(City_Component selectedCity)
        {
            EditorGUILayout.LabelField("City Data", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("City Name", selectedCity.CityData.CityName);
            EditorGUILayout.LabelField("City ID",   selectedCity.CityID.ToString());
            EditorGUILayout.LabelField("Region ID", selectedCity.CityData.RegionID.ToString());

            if (selectedCity.CityData.Population != null)
            {
                _showPopulation = EditorGUILayout.Toggle("Population", _showPopulation);

                if (_showPopulation)
                {
                    _drawPopulationDetails(selectedCity.CityData.Population);
                }
            }

            if (selectedCity.CityData.AllJobsiteIDs != null)
            {
                _showJobSites = EditorGUILayout.Toggle("JobSites", _showJobSites);

                if (_showJobSites)
                {
                    _drawJobSiteAdditionalData(selectedCity.CityData.AllJobsiteIDs);
                }
            }

            if (selectedCity.CityData.ProsperityData != null)
            {
                _showProsperity = EditorGUILayout.Toggle("Prosperity", _showProsperity);

                if (_showProsperity)
                {
                    _drawProsperityDetails(selectedCity.CityData.ProsperityData);
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
                    EditorGUILayout.LabelField("JobSite ID", jobSiteID.ToString());
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