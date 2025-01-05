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
        public Data<City_Data>[]                         Cities                         => Data;
        public Data<City_Data>                           GetCity_Data(uint      cityID) => GetData(cityID);
        Dictionary<uint, City_Component>                        _city_Components;
        public Dictionary<uint, City_Component> City_Components => _city_Components ??= _getExistingCity_Components();
        
        Dictionary<uint, City_Component> _getExistingCity_Components()
        {
            return FindObjectsByType<City_Component>(FindObjectsSortMode.None)
                                  .Where(city => Regex.IsMatch(city.name, @"\d+"))
                                  .ToDictionary(
                                      city => uint.Parse(new string(city.name.Where(char.IsDigit).ToArray())),
                                      city => city
                                  );
        }

        public City_Component GetCity_Component(uint cityID)
        {
            if (City_Components.TryGetValue(cityID, out var component))
            {
                return component;
            }   
            
            Debug.LogError($"City with ID {cityID} not found in City_SO.");
            return null;
        }

        public override uint GetDataID(int id) => Cities[id].Data_Object.CityID;

        public void UpdateCity(uint cityID, City_Data city_Data) => UpdateData(cityID, city_Data);
        public void UpdateAllCities(Dictionary<uint, City_Data> allCities) => UpdateAllData(allCities);

        public override void PopulateSceneData()
        {
            if (_defaultCities.Count == 0)
            {
                Debug.Log("No Default Cities Found");
            }
            
            var existingCities = _getExistingCity_Components();
            
            foreach (var city in Cities)
            {
                if (city?.Data_Object is null) continue;
                
                if (existingCities.TryGetValue(city.Data_Object.CityID, out var existingCity))
                {
                    City_Components[city.Data_Object.CityID] = existingCity;
                    existingCity.SetCityData(city.Data_Object);
                    existingCities.Remove(city.Data_Object.CityID);
                    continue;
                }
                
                Debug.LogWarning($"City with ID {city.Data_Object.CityID} not found in the scene.");
            }
            
            foreach (var city in existingCities)
            {
                if (DataIndexLookup.ContainsKey(city.Key))
                {
                    Debug.LogWarning($"City with ID {city.Key} wasn't removed from existingCities.");
                    continue;
                }
                
                Debug.LogWarning($"City with ID {city.Key} does not have DataObject in City_SO.");
            }
        }

        protected override Dictionary<uint, Data<City_Data>> _getDefaultData(bool initialisation = false)
        {
            if (_defaultData is null || !Application.isPlaying || initialisation)
                return _defaultData ??= _convertDictionaryToData(City_List.DefaultCities);

            if (Cities is null || Cities.Length == 0)
            {
                Debug.LogError("Cities is null or empty.");
                return _defaultData;
            }

            foreach (var city in Cities)
            {
                if (city?.Data_Object is null) continue;
                
                if (!_defaultData.ContainsKey(city.Data_Object.CityID))
                {
                    Debug.LogError($"City with ID {city.Data_Object.CityID} not found in City_List.");
                    continue;
                }
                
                _defaultData[city.Data_Object.CityID] = city;
            }
            
            return _defaultData;
        }

        static uint _lastUnusedCityID = 1;

        public uint GetUnusedCityID()
        {
            while (DataIndexLookup.ContainsKey(_lastUnusedCityID))
            {
                _lastUnusedCityID++;
            }

            return _lastUnusedCityID;
        }
        
        Dictionary<uint, Data<City_Data>> _defaultCities => DefaultData;
        
        protected override Data<City_Data> _convertToData(City_Data data)
        {
            return new Data<City_Data>(
                dataID: data.CityID, 
                data_Object: data,
                dataTitle: $"{data.CityID}: {data.CityName}",
                getData_Display: data.GetDataSO_Object);
        }
    }

    [CustomEditor(typeof(City_SO))]
    public class City_SOEditor : Data_SOEditor<City_Data>
    {
        public override Data_SO<City_Data> SO => _so ??= (City_SO)target;
    }
}