using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Tools;
using UnityEditor;
using UnityEngine;

namespace Region
{
    [CreateAssetMenu(fileName = "Region_SO", menuName = "SOList/Region_SO")]
    [Serializable]
    public class Region_SO : Data_SO<Region_Data>
    {
        public Data<Region_Data>[]         Regions                            => Data;
        public Data<Region_Data>           GetRegion_Data(uint      regionID) => GetData(regionID);
        Dictionary<uint, Region_Component>        _region_Components;
        public Dictionary<uint, Region_Component> RegionComponents => _region_Components ??= _getExistingRegion_Components();

        Dictionary<uint, Region_Component> _getExistingRegion_Components()
        {
            return FindObjectsByType<Region_Component>(FindObjectsSortMode.None)
                                  .Where(station => Regex.IsMatch(station.name, @"\d+"))
                                  .ToDictionary(
                                      station => uint.Parse(new string(station.name.Where(char.IsDigit).ToArray())),
                                      station => station
                                  );
        }
        
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

        public override void PopulateSceneData()
        {
            if (_defaultRegions.Count == 0)
            {
                Debug.Log("No Default Regions Found");
            }
            
            var existingRegions = _getExistingRegion_Components();
            
            foreach (var region in Regions)
            {
                if (region?.Data_Object is null) continue;
                
                if (existingRegions.TryGetValue(region.Data_Object.RegionID, out var existingRegion))
                {
                    RegionComponents[region.Data_Object.RegionID] = existingRegion;
                    existingRegion.SetRegionData(region.Data_Object);
                    existingRegions.Remove(region.Data_Object.RegionID);
                    continue;
                }
                
                Debug.LogWarning($"Region with ID {region.Data_Object.RegionID} not found in the scene.");
            }
            
            foreach (var region in existingRegions)
            {
                if (DataIndexLookup.ContainsKey(region.Key))
                {
                    Debug.LogWarning($"Region with ID {region.Key} wasn't removed from existingRegions.");
                    continue;
                }
                
                Debug.LogWarning($"Region with ID {region.Key} does not have DataObject in Region_SO.");
            }
        }

        protected override Dictionary<uint, Data<Region_Data>> _getDefaultData(bool initialisation = false)
        {
            if (_defaultData is null || !Application.isPlaying || initialisation)
                return _defaultData ??= _convertDictionaryToData(Region_List.DefaultRegions);

            if (Regions is null || Regions.Length == 0)
            {
                Debug.LogError("Regions is null or empty.");
                return _defaultData;
            }

            foreach (var region in Regions)
            {
                if (region?.Data_Object is null) continue;
                
                if (!_defaultData.ContainsKey(region.Data_Object.RegionID))
                {
                    Debug.LogError($"Region with ID {region.Data_Object.RegionID} not found in DefaultRegions.");
                    continue;
                }
                
                _defaultData[region.Data_Object.RegionID] = region;
            }

            return _defaultData;
        }

        static uint _lastUnusedRegionID = 1;

        public uint GetUnusedRegionID()
        {
            while (DataIndexLookup.ContainsKey(_lastUnusedRegionID))
            {
                _lastUnusedRegionID++;
            }

            return _lastUnusedRegionID;
        }
        
        Dictionary<uint, Data<Region_Data>> _defaultRegions => DefaultData;
         
        protected override Data<Region_Data> _convertToData(Region_Data data)
        {
            return new Data<Region_Data>(
                dataID: data.RegionID, 
                data_Object: data,
                dataTitle: $"{data.RegionID}: {data.RegionName}",
                getData_Display: data.GetDataSO_Object);
        }
    }

    [CustomEditor(typeof(Region_SO))]
    public class Region_SOEditor : Data_SOEditor<Region_Data>
    {
        public override Data_SO<Region_Data> SO => _so ??= (Region_SO)target;
    }
}