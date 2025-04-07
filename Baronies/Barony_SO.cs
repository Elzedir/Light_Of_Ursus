using System;
using System.Collections.Generic;
using System.Linq;
using DataPersistence;
using Tools;
using UnityEditor;
using UnityEngine;

namespace Cities
{
    [CreateAssetMenu(fileName = "City_SO", menuName = "SOList/City_SO")]
    [Serializable]
    public class City_SO : Data_Component_SO<Barony_Data, Barony_Component>
    {
        public Data<Barony_Data>[]                         Cities                         => Data;
        public Data<Barony_Data>                           GetCity_Data(ulong      cityID) => GetData(cityID);
        public Dictionary<ulong, Barony_Component> City_Components => _getSceneComponents();

        public Barony_Component GetCity_Component(ulong cityID)
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
        
        public void UpdateCity(ulong cityID, Barony_Data barony_Data) => UpdateData(cityID, barony_Data);
        public void UpdateAllCities(Dictionary<ulong, Barony_Data> allCities) => UpdateAllData(allCities);

        protected override Dictionary<ulong, Data<Barony_Data>> _getDefaultData() => 
            _convertDictionaryToData(Barony_List.DefaultCities);

        protected override Dictionary<ulong, Data<Barony_Data>> _getSavedData()
        {
            Dictionary<ulong, Barony_Data> savedData = new();
                
            try
            {
                savedData = DataPersistence_Manager.CurrentSaveData.SavedCityData.AllCityData
                    .ToDictionary(city => city.ID, city => city);
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
        
        protected override Dictionary<ulong, Data<Barony_Data>> _getSceneData() =>
            _convertDictionaryToData(_getSceneComponents().ToDictionary(kvp => kvp.Key, kvp => kvp.Value.BaronyData));
        
        protected override Data<Barony_Data> _convertToData(Barony_Data data)
        {
            return new Data<Barony_Data>(
                dataID: data.ID, 
                data_Object: data,
                dataTitle: $"{data.ID}: {data.Name}",
                getDataToDisplay: data.GetDataToDisplay);
        }

        public override void SaveData(Save_Data saveData) =>
            saveData.SavedCityData = new SavedCityData(Cities.Select(city => city.Data_Object).ToArray());
    }

    [CustomEditor(typeof(City_SO))]
    public class City_SOEditor : Data_SOEditor<Barony_Data>
    {
        public override Data_SO<Barony_Data> SO => _so ??= (City_SO)target;
    }
}