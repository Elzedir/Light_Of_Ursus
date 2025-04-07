using System;
using System.Collections.Generic;
using System.Linq;
using DataPersistence;
using Tools;
using UnityEditor;
using UnityEngine;

namespace Counties
{
    [CreateAssetMenu(fileName = "Region_SO", menuName = "SOList/Region_SO")]
    [Serializable]
    public class County_SO : Data_Component_SO<County_Data, County_Component>
    {
        public Data<County_Data>[]         Regions                            => Data;
        public Data<County_Data>           GetRegion_Data(ulong      regionID) => GetData(regionID);
        public Dictionary<ulong, County_Component> RegionComponents => _getSceneComponents();
        
        public County_Component GetRegion_Component(ulong regionID)
        {
            if (RegionComponents.TryGetValue(regionID, out var component))
            {
                return component;
            }   
            
            Debug.LogError($"Region with ID {regionID} not found in Region_SO.");
            return null;
        }

        public void UpdateRegion(ulong regionID, County_Data county_Data) => UpdateData(regionID, county_Data);
        public void UpdateAllRegions(Dictionary<ulong, County_Data> allRegions) => UpdateAllData(allRegions);

        protected override Dictionary<ulong, Data<County_Data>> _getDefaultData() =>
            _convertDictionaryToData(County_List.DefaultCounties);

        protected override Dictionary<ulong, Data<County_Data>> _getSavedData()
        {
            Dictionary<ulong, County_Data> savedData = new();

            try
            {
                savedData = DataPersistence_Manager.CurrentSaveData.SavedCountyData.AllCountyData
                    .ToDictionary(region => region.ID, region => region);
            }
            catch
            {
                var saveData = DataPersistence_Manager.CurrentSaveData;

                if (ToggleMissingDataDebugs)
                {
                    Debug.LogWarning(saveData == null
                        ? "LoadData Error: CurrentSaveData is null."
                        : saveData.SavedCountyData == null
                            ? $"LoadData Error: SavedRegionData is null in CurrentSaveData (SaveID: {saveData.SavedProfileData.SaveDataID})."
                            : saveData.SavedCountyData.AllCountyData == null
                                ? $"LoadData Error: AllRegionData is null in SavedRegionData (SaveID: {saveData.SavedProfileData.SaveDataID})."
                                : !saveData.SavedCountyData.AllCountyData.Any()
                                    ? $"LoadData Warning: AllRegionData is empty (SaveID: {saveData.SavedProfileData.SaveDataID})."
                                    : string.Empty);
                }
            }

            return _convertDictionaryToData(savedData);
        }

        protected override Dictionary<ulong, Data<County_Data>> _getSceneData() =>
            _convertDictionaryToData(_getSceneComponents().ToDictionary(kvp => kvp.Key, kvp => kvp.Value.CountyData));
         
        protected override Data<County_Data> _convertToData(County_Data data)
        {
            return new Data<County_Data>(
                dataID: data.ID, 
                data_Object: data,
                dataTitle: $"{data.ID}: {data.Name}",
                getDataToDisplay: data.GetDataToDisplay);
        }
        
        public override void SaveData(Save_Data saveData) =>
            saveData.SavedCountyData = new SavedCountyData(Regions.Select(region => region.Data_Object).ToArray());
    }

    [CustomEditor(typeof(County_SO))]
    public class Region_SOEditor : Data_SOEditor<County_Data>
    {
        public override Data_SO<County_Data> SO => _so ??= (County_SO)target;
    }
}