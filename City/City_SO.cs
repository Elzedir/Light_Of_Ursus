using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Tools;
using UnityEditor;
using UnityEngine;

namespace City
{
    [CreateAssetMenu(fileName = "City_SO", menuName = "SOList/City_SO")]
    [Serializable]
    public class City_SO : Data_SO<City_Data>
    {
        public Data_Object<City_Data>[]                         Cities                         => DataObjects;
        public Data_Object<City_Data>                           GetCity_Data(uint      cityID) => GetDataObject_Master(cityID);
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

        public override uint GetDataObjectID(int id) => Cities[id].DataObject.CityID;

        public void UpdateCity(uint cityID, City_Data city_Data) => UpdateDataObject(cityID, city_Data);
        public void UpdateAllCities(Dictionary<uint, City_Data> allCities) => UpdateAllDataObjects(allCities);

        public void PopulateSceneCities()
        {
            if (_defaultCities.Count == 0)
            {
                Debug.Log("No Default Cities Found");
            }
            
            var existingCities = FindObjectsByType<City_Component>(FindObjectsSortMode.None)
                                     .Where(station => Regex.IsMatch(station.name, @"\d+"))
                                     .ToDictionary(
                                         station => uint.Parse(new string(station.name.Where(char.IsDigit).ToArray())),
                                         station => station
                                     );
            
            foreach (var city in Cities)
            {
                if (city?.DataObject is null) continue;
                
                if (existingCities.TryGetValue(city.DataObject.CityID, out var existingCity))
                {
                    existingCity.CityData = city.DataObject;
                    CityComponents[city.DataObject.CityID] = existingCity;
                    existingCities.Remove(city.DataObject.CityID);
                    continue;
                }
                
                Debug.LogWarning($"City with ID {city.DataObject.CityID} not found in the scene.");
            }
            
            foreach (var city in existingCities)
            {
                if (DataObjectIndexLookup.ContainsKey(city.Key))
                {
                    Debug.LogWarning($"City with ID {city.Key} wasn't removed from existingCities.");
                    continue;
                }
                
                Debug.LogWarning($"City with ID {city.Key} does not have DataObject in City_SO.");
            }
        }

        protected override Dictionary<uint, Data_Object<City_Data>> _populateDefaultDataObjects()
        {
            var defaultCities = new Dictionary<uint, City_Data>();

            foreach (var defaultCity in City_List.DefaultCities)
            {
                defaultCities.Add(defaultCity.Key, defaultCity.Value);
            }

            return _convertDictionaryToDataObject(defaultCities);
        }

        static uint _lastUnusedCityID = 1;

        public uint GetUnusedCityID()
        {
            while (DataObjectIndexLookup.ContainsKey(_lastUnusedCityID))
            {
                _lastUnusedCityID++;
            }

            return _lastUnusedCityID;
        }
        
        Dictionary<uint, Data_Object<City_Data>> _defaultCities => DefaultDataObjects;
        
        protected override Data_Object<City_Data> _convertToDataObject(City_Data data)
        {
            return new Data_Object<City_Data>(
                dataObjectID: data.CityID, 
                dataObject: data,
                dataObjectTitle: $"{data.CityID}: {data.CityName}",
                data_Display: data.DataSO_Object(ToggleMissingDataDebugs));
        }
    }

    [CustomEditor(typeof(City_SO))]
    public class City_SOEditor : Data_SOEditor<City_Data>
    {
        public override Data_SO<City_Data> SO => _so ??= (City_SO)target;
    }
}