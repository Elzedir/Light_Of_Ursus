using System;
using System.Collections.Generic;
using System.Linq;
using DataPersistence;
using Tools;
using UnityEditor;
using UnityEngine;

namespace Counties
{
    [CreateAssetMenu(fileName = "County_SO", menuName = "SOList/County_SO")]
    [Serializable]
    public class County_SO : Data_Component_SO<County_Data, County_Component>
    {
        public Data<County_Data>[]         Counties                            => Data;
        public Data<County_Data>           GetCounty_Data(ulong      countyID) => GetData(countyID);
        public Dictionary<ulong, County_Component> CountyComponents => _getSceneComponents();
        
        public County_Component GetCounty_Component(ulong countyID)
        {
            if (countyID == 0)
            {
                Debug.LogError("CountyID cannot be 0.");
                return null;
            }
            
            if (CountyComponents.TryGetValue(countyID, out var component))
            {
                return component;
            }   
            
            Debug.LogError($"County with ID {countyID} not found in County_SO.");
            return null;
        }

        public void UpdateCounty(ulong countyID, County_Data county_Data) => UpdateData(countyID, county_Data);
        public void UpdateAllCounties(Dictionary<ulong, County_Data> allCounties) => UpdateAllData(allCounties);

        protected override Dictionary<ulong, Data<County_Data>> _getDefaultData() =>
            _convertDictionaryToData(County_List.DefaultCounties);

        protected override Dictionary<ulong, Data<County_Data>> _getSavedData()
        {
            Dictionary<ulong, County_Data> savedData = new();

            try
            {
                savedData = DataPersistence_Manager.CurrentSaveData.SavedCountyData.AllCountyData
                    .ToDictionary(county => county.ID, county => county);
            }
            catch
            {
                var saveData = DataPersistence_Manager.CurrentSaveData;

                if (ToggleMissingDataDebugs)
                {
                    Debug.LogWarning(saveData == null
                        ? "LoadData Error: CurrentSaveData is null."
                        : saveData.SavedCountyData == null
                            ? $"LoadData Error: SavedCountyData is null in CurrentSaveData (SaveID: {saveData.SavedProfileData.SaveDataID})."
                            : saveData.SavedCountyData.AllCountyData == null
                                ? $"LoadData Error: AllCountyData is null in SavedCountyData (SaveID: {saveData.SavedProfileData.SaveDataID})."
                                : !saveData.SavedCountyData.AllCountyData.Any()
                                    ? $"LoadData Warning: AllCountyData is empty (SaveID: {saveData.SavedProfileData.SaveDataID})."
                                    : string.Empty);
                }
            }

            return _convertDictionaryToData(savedData);
        }

        protected override Dictionary<ulong, Data<County_Data>> _getSceneData() =>
            _convertDictionaryToData(_getSceneComponents().ToDictionary(kvp => kvp.Key, kvp => kvp.Value.County_Data));
         
        protected override Data<County_Data> _convertToData(County_Data data)
        {
            return new Data<County_Data>(
                dataID: data.ID, 
                data_Object: data,
                dataTitle: $"{data.ID}: {data.Name}",
                getDataToDisplay: data.GetDataToDisplay);
        }
        
        public override void SaveData(Save_Data saveData) =>
            saveData.SavedCountyData = new SavedCountyData(Counties.Select(county => county.Data_Object).ToArray());
    }

    [CustomEditor(typeof(County_SO))]
    public class County_SOEditor : Data_SOEditor<County_Data>
    {
        public override Data_SO<County_Data> SO => _so ??= (County_SO)target;
    }
}