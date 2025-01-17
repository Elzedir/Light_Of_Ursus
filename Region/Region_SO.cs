using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
        public Data<Region_Data>           GetRegion_Data(uint      regionID) => GetData(regionID);
        public Dictionary<uint, Region_Component> RegionComponents => _getSceneComponents();
        
        public Region_Component GetRegion_Component(uint regionID)
        {
            if (RegionComponents.TryGetValue(regionID, out var component))
            {
                return component;
            }   
            
            Debug.LogError($"Region with ID {regionID} not found in Region_SO.");
            return null;
        }

        public override uint GetDataID(int id) => Regions[id].Data_Object.RegionID;

        public void UpdateRegion(uint regionID, Region_Data region_Data) => UpdateData(regionID, region_Data);
        public void UpdateAllRegions(Dictionary<uint, Region_Data> allRegions) => UpdateAllData(allRegions);

        protected override Dictionary<uint, Data<Region_Data>> _getDefaultData() =>
            _convertDictionaryToData(Region_List.DefaultRegions);

        protected override Dictionary<uint, Data<Region_Data>> _getSavedData()
        {
            Dictionary<uint, Region_Data> savedData = new();

            try
            {
                savedData = DataPersistenceManager.DataPersistence_SO.CurrentSaveData.SavedRegionData.AllRegionData
                    .ToDictionary(region => region.RegionID, region => region);
            }
            catch
            {
                var saveData = DataPersistenceManager.DataPersistence_SO.CurrentSaveData;

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

        protected override Dictionary<uint, Data<Region_Data>> _getSceneData() =>
            _convertDictionaryToData(_getSceneComponents().ToDictionary(kvp => kvp.Key, kvp => kvp.Value.RegionData));

        static uint _lastUnusedRegionID = 1;

        public uint GetUnusedRegionID()
        {
            while (DataIndexLookup.ContainsKey(_lastUnusedRegionID))
            {
                _lastUnusedRegionID++;
            }

            return _lastUnusedRegionID;
        }
         
        protected override Data<Region_Data> _convertToData(Region_Data data)
        {
            return new Data<Region_Data>(
                dataID: data.RegionID, 
                data_Object: data,
                dataTitle: $"{data.RegionID}: {data.RegionName}",
                getDataToDisplay: data.GetData_Display);
        }
        
        public override void SaveData(SaveData saveData) =>
            saveData.SavedRegionData = new SavedRegionData(Regions.Select(region => region.Data_Object).ToArray());
    }

    [CustomEditor(typeof(Region_SO))]
    public class Region_SOEditor : Data_SOEditor<Region_Data>
    {
        public override Data_SO<Region_Data> SO => _so ??= (Region_SO)target;
    }
}