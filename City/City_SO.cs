using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DataPersistence;
using Tools;
using UnityEditor;
using UnityEngine;

namespace City
{
    [CreateAssetMenu(fileName = "City_SO", menuName = "SOList/City_SO")]
    [Serializable]
    public class City_SO : Data_Component_SO<City_Data, City_Component>
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

        protected override Dictionary<uint, Data<City_Data>> _getDefaultData() => 
            _convertDictionaryToData(City_List.DefaultCities);

        protected override Dictionary<uint, Data<City_Data>> _getSavedData()
        {
            Dictionary<uint, City_Data> savedData = new();
                
            try
            {
                savedData = DataPersistenceManager.DataPersistence_SO.CurrentSaveData.SavedCityData.AllCityData
                    .ToDictionary(city => city.CityID, actor => actor);
            }
            catch (Exception ex)
            {
                var saveData = DataPersistenceManager.DataPersistence_SO.CurrentSaveData;
                
                if (saveData == null)
                {
                    Debug.LogWarning("LoadData Error: CurrentSaveData is null.");
                }
                else if (saveData.SavedCityData == null)
                {
                    Debug.LogWarning($"LoadData Error: SavedCityData is null in CurrentSaveData (SaveID: {saveData.SavedProfileData.SaveDataID}).");
                }
                else if (saveData.SavedCityData.AllCityData == null)
                {
                    Debug.LogWarning($"LoadData Error: AllCityData is null in SavedCityData (SaveID: {saveData.SavedProfileData.SaveDataID}).");
                }
                else if (!saveData.SavedCityData.AllCityData.Any())
                {
                    Debug.LogWarning($"LoadData Warning: AllCityData is empty (SaveID: {saveData.SavedProfileData.SaveDataID}).");
                }

                Debug.LogError($"LoadData Exception: {ex.Message}\n{ex.StackTrace}");
            }
            
            return _convertDictionaryToData(savedData);
        }
        
        protected override Dictionary<uint, Data<City_Data>> _getSceneData() =>
            _convertDictionaryToData(_getSceneComponents().ToDictionary(kvp => kvp.Key, kvp => kvp.Value.CityData));

        static uint _lastUnusedCityID = 1;

        public uint GetUnusedCityID()
        {
            while (DataIndexLookup.ContainsKey(_lastUnusedCityID))
            {
                _lastUnusedCityID++;
            }

            return _lastUnusedCityID;
        }
        
        protected override Data<City_Data> _convertToData(City_Data data)
        {
            return new Data<City_Data>(
                dataID: data.CityID, 
                data_Object: data,
                dataTitle: $"{data.CityID}: {data.CityName}",
                getData_Display: data.GetData_Display);
        }

        public override void SaveData(SaveData saveData) =>
            saveData.SavedCityData = new SavedCityData(Cities.Select(city => city.Data_Object).ToArray());
    }

    [CustomEditor(typeof(City_SO))]
    public class City_SOEditor : Data_SOEditor<City_Data>
    {
        public override Data_SO<City_Data> SO => _so ??= (City_SO)target;
    }
}