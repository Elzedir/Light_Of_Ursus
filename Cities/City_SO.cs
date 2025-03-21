using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Cities;
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
        public Data<City_Data>                           GetCity_Data(ulong      cityID) => GetData(cityID);
        public Dictionary<ulong, City_Component> City_Components => _getSceneComponents();

        public City_Component GetCity_Component(ulong cityID)
        {
            if (cityID == 0)
            {
                Debug.LogError("CityID cannot be 0.");
                return null;
            }
            
            if (City_Components.TryGetValue(cityID, out var component))
            {
                return component;
            }   
            
            Debug.LogError($"City with ID {cityID} not found in City_SO.");
            return null;
        }
        
        public void UpdateCity(ulong cityID, City_Data city_Data) => UpdateData(cityID, city_Data);
        public void UpdateAllCities(Dictionary<ulong, City_Data> allCities) => UpdateAllData(allCities);

        protected override Dictionary<ulong, Data<City_Data>> _getDefaultData() => 
            _convertDictionaryToData(City_List.DefaultCities);

        protected override Dictionary<ulong, Data<City_Data>> _getSavedData()
        {
            Dictionary<ulong, City_Data> savedData = new();
                
            try
            {
                savedData = DataPersistence_Manager.CurrentSaveData.SavedCityData.AllCityData
                    .ToDictionary(city => city.CityID, city => city);
            }
            catch
            {
                var saveData = DataPersistence_Manager.CurrentSaveData;
                
                if (ToggleMissingDataDebugs)
                {
                    Debug.LogWarning(saveData == null
                        ? "LoadData Error: CurrentSaveData is null."
                        : saveData.SavedCityData == null
                            ? $"LoadData Error: SavedCityData is null in CurrentSaveData (SaveID: {saveData.SavedProfileData.SaveDataID})."
                            : saveData.SavedCityData.AllCityData == null
                                ? $"LoadData Error: AllCityData is null in SavedCityData (SaveID: {saveData.SavedProfileData.SaveDataID})."
                                : !saveData.SavedCityData.AllCityData.Any()
                                    ? $"LoadData Warning: AllCityData is empty (SaveID: {saveData.SavedProfileData.SaveDataID})."
                                    : string.Empty);
                }
            }
            
            return _convertDictionaryToData(savedData);
        }
        
        protected override Dictionary<ulong, Data<City_Data>> _getSceneData() =>
            _convertDictionaryToData(_getSceneComponents().ToDictionary(kvp => kvp.Key, kvp => kvp.Value.CityData));
        
        protected override Data<City_Data> _convertToData(City_Data data)
        {
            return new Data<City_Data>(
                dataID: data.CityID, 
                data_Object: data,
                dataTitle: $"{data.CityID}: {data.CityName}",
                getDataToDisplay: data.GetDataToDisplay);
        }

        public override void SaveData(Save_Data saveData) =>
            saveData.SavedCityData = new SavedCityData(Cities.Select(city => city.Data_Object).ToArray());
    }

    [CustomEditor(typeof(City_SO))]
    public class City_SOEditor : Data_SOEditor<City_Data>
    {
        public override Data_SO<City_Data> SO => _so ??= (City_SO)target;
    }
}