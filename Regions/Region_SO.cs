using System;
using System.Collections.Generic;
using System.Linq;
using DataPersistence;
using Tools;
using UnityEditor;
using UnityEngine;

namespace Region
{
    [CreateAssetMenu(fileName = "Region_SO", menuName = "SOList/Region_SO")]
    [Serializable]
    public class Region_SO : Data_Component_SO<Region_Data, Region_Component>
    {
        public Data<Region_Data>[]         Regions                            => Data;
        public Data<Region_Data>           GetRegion_Data(ulong      regionID) => GetData(regionID);
        public Dictionary<ulong, Region_Component> RegionComponents => _getSceneComponents();
        
        public Region_Component GetRegion_Component(ulong regionID)
        {
            if (RegionComponents.TryGetValue(regionID, out var component))
            {
                return component;
            }   
            
            Debug.LogError($"Region with ID {regionID} not found in Region_SO.");
            return null;
        }

        public void UpdateRegion(ulong regionID, Region_Data region_Data) => UpdateData(regionID, region_Data);
        public void UpdateAllRegions(Dictionary<ulong, Region_Data> allRegions) => UpdateAllData(allRegions);

        protected override Dictionary<ulong, Data<Region_Data>> _getDefaultData() =>
            _convertDictionaryToData(Region_List.DefaultRegions);

        protected override Dictionary<ulong, Data<Region_Data>> _getSavedData()
        {
            Dictionary<ulong, Region_Data> savedData = new();

            try
            {
                savedData = DataPersistence_Manager.CurrentSaveData.SavedRegionData.AllRegionData
                    .ToDictionary(region => region.RegionID, region => region);
            }
            catch
            {
                var saveData = DataPersistence_Manager.CurrentSaveData;

                if (ToggleMissingDataDebugs)
                {
                    Debug.LogWarning(saveData == null
                        ? "LoadData Error: CurrentSaveData is null."
                        : saveData.SavedRegionData == null
                            ? $"LoadData Error: SavedRegionData is null in CurrentSaveData (SaveID: {saveData.SavedProfileData.SaveDataID})."
                            : saveData.SavedRegionData.AllRegionData == null
                                ? $"LoadData Error: AllRegionData is null in SavedRegionData (SaveID: {saveData.SavedProfileData.SaveDataID})."
                                : !saveData.SavedRegionData.AllRegionData.Any()
                                    ? $"LoadData Warning: AllRegionData is empty (SaveID: {saveData.SavedProfileData.SaveDataID})."
                                    : string.Empty);
                }
            }

            return _convertDictionaryToData(savedData);
        }

        protected override Dictionary<ulong, Data<Region_Data>> _getSceneData() =>
            _convertDictionaryToData(_getSceneComponents().ToDictionary(kvp => kvp.Key, kvp => kvp.Value.RegionData));
         
        protected override Data<Region_Data> _convertToData(Region_Data data)
        {
            return new Data<Region_Data>(
                dataID: data.RegionID, 
                data_Object: data,
                dataTitle: $"{data.RegionID}: {data.RegionName}",
                getDataToDisplay: data.GetDataToDisplay);
        }
        
        public override void SaveData(Save_Data saveData) =>
            saveData.SavedRegionData = new SavedRegionData(Regions.Select(region => region.Data_Object).ToArray());
    }

    [CustomEditor(typeof(Region_SO))]
    public class Region_SOEditor : Data_SOEditor<Region_Data>
    {
        public override Data_SO<Region_Data> SO => _so ??= (Region_SO)target;
    }
}