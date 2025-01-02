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
        public Object_Data<Region_Data>[]         Regions                            => Objects_Data;
        public Object_Data<Region_Data>           GetRegion_Data(uint      regionID) => GetObject_Data(regionID);
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

        public override uint GetDataObjectID(int id) => Regions[id].DataObject.RegionID;

        public void UpdateRegion(uint regionID, Region_Data region_Data) => UpdateDataObject(regionID, region_Data);
        public void UpdateAllRegions(Dictionary<uint, Region_Data> allRegions) => UpdateAllDataObjects(allRegions);

        public override void PopulateSceneData()
        {
            if (_defaultRegions.Count == 0)
            {
                Debug.Log("No Default Regions Found");
            }
            
            var existingRegions = _getExistingRegion_Components();
            
            foreach (var region in Regions)
            {
                if (region?.DataObject is null) continue;
                
                if (existingRegions.TryGetValue(region.DataObject.RegionID, out var existingRegion))
                {
                    RegionComponents[region.DataObject.RegionID] = existingRegion;
                    existingRegion.SetRegionData(region.DataObject);
                    existingRegions.Remove(region.DataObject.RegionID);
                    continue;
                }
                
                Debug.LogWarning($"Region with ID {region.DataObject.RegionID} not found in the scene.");
            }
            
            foreach (var region in existingRegions)
            {
                if (DataObjectIndexLookup.ContainsKey(region.Key))
                {
                    Debug.LogWarning($"Region with ID {region.Key} wasn't removed from existingRegions.");
                    continue;
                }
                
                Debug.LogWarning($"Region with ID {region.Key} does not have DataObject in Region_SO.");
            }
        }

        protected override Dictionary<uint, Object_Data<Region_Data>> _getDefaultDataObjects(bool initialisation = false)
        {
            if (_defaultDataObjects is null || !Application.isPlaying || initialisation)
                return _defaultDataObjects ??= _convertDictionaryToDataObject(Region_List.DefaultRegions);

            if (Regions is null || Regions.Length == 0)
            {
                Debug.LogError("Regions is null or empty.");
                return _defaultDataObjects;
            }

            foreach (var region in Regions)
            {
                if (region?.DataObject is null) continue;
                
                if (!_defaultDataObjects.ContainsKey(region.DataObject.RegionID))
                {
                    Debug.LogError($"Region with ID {region.DataObject.RegionID} not found in DefaultRegions.");
                    continue;
                }
                
                _defaultDataObjects[region.DataObject.RegionID] = region;
            }

            return _defaultDataObjects;
        }

        static uint _lastUnusedRegionID = 1;

        public uint GetUnusedRegionID()
        {
            while (DataObjectIndexLookup.ContainsKey(_lastUnusedRegionID))
            {
                _lastUnusedRegionID++;
            }

            return _lastUnusedRegionID;
        }
        
        Dictionary<uint, Object_Data<Region_Data>> _defaultRegions => DefaultDataObjects;
         
        protected override Object_Data<Region_Data> _convertToDataObject(Region_Data dataObject)
        {
            return new Object_Data<Region_Data>(
                dataObjectID: dataObject.RegionID, 
                dataObject: dataObject,
                dataObjectTitle: $"{dataObject.RegionID}: {dataObject.RegionName}",
                data_Display: dataObject.GetDataSO_Object(ToggleMissingDataDebugs));
        }
    }

    [CustomEditor(typeof(Region_SO))]
    public class Region_SOEditor : Data_SOEditor<Region_Data>
    {
        public override Data_SO<Region_Data> SO => _so ??= (Region_SO)target;
    }
}