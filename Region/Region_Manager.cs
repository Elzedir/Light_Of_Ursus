using System;
using System.Collections.Generic;
using System.Linq;
using Initialisation;
using UnityEditor;
using UnityEngine;

namespace Region
{
    public class Region_Manager : MonoBehaviour, IDataPersistence
    {
        const string _region_SOPath = "ScriptableObjects/Region_SO";

        static Region_SO _allRegions;
        static Region_SO AllRegions => _allRegions ??= _getOrCreateRegion_SO();
        
        public void SaveData(SaveData saveData) =>
            saveData.SavedRegionData = new SavedRegionData(AllRegions.Save_SO());

        public void LoadData(SaveData saveData)
        {
            if (saveData is null)
            {
                //Debug.Log("No SaveData found in LoadData.");
                return;
            }

            if (saveData.SavedRegionData == null)
            {
                //Debug.Log("No SavedRegionData found in SaveData.");
                return;
            }

            if (saveData.SavedRegionData.AllRegionData == null)
            {
                //Debug.Log("No AllRegionData found in SavedRegionData.");
                return;
            }

            if (saveData.SavedRegionData.AllRegionData.Length == 0)
            {
                //Debug.Log("AllRegionData count is 0.");
                return;
            }

            AllRegions.Load_SO(saveData.SavedRegionData.AllRegionData);
        }
        

        public static void OnSceneLoaded()
        {
            Manager_Initialisation.OnInitialiseManagerRegion += _initialise;
        }

        static void _initialise()
        {
            AllRegions.PopulateSceneRegions();
        }
        
        public static Region_Data GetRegion_Data(uint regionID)
        {
            return AllRegions.GetRegion_Data(regionID);
        }
        
        public static Region_Component GetRegion_Component(uint regionID)
        {
            return AllRegions.GetRegion_Component(regionID);
        }
        
        static Region_SO _getOrCreateRegion_SO()
        {
            var region_SO = Resources.Load<Region_SO>(_region_SOPath);
            
            if (region_SO is not null) return region_SO;
            
            region_SO = ScriptableObject.CreateInstance<Region_SO>();
            AssetDatabase.CreateAsset(region_SO, $"Assets/Resources/{_region_SOPath}");
            AssetDatabase.SaveAssets();
            
            return region_SO;
        }

        public static Region_Component GetNearestRegion(Vector3 position)
        {
            Region_Component nearestRegion = null;

            var nearestDistance = float.MaxValue;

            foreach (var region in AllRegions.Regions)
            {
                var distance = Vector3.Distance(position, region.transform.position);

                if (!(distance < nearestDistance)) continue;

                nearestRegion  = region;
                nearestDistance = distance;
            }

            return nearestRegion;
        }

        public static uint GetUnusedRegionID()
        {
            return AllRegions.GetUnusedRegionID();
        }
    }
}