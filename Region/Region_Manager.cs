using System.Linq;
using DataPersistence;
using Initialisation;
using UnityEditor;
using UnityEngine;

namespace Region
{
    public class Region_Manager : MonoBehaviour, IDataPersistence
    {
        const string _region_SOPath = "ScriptableObjects/Region_SO";

        static Region_SO _allRegions;
        static Region_SO AllRegions => _allRegions ??= _getRegion_SO();
        
        public void SaveData(SaveData saveData) =>
            saveData.SavedRegionData = new SavedRegionData(AllRegions.Regions.Select(region => region.DataObject).ToArray());

        public void LoadData(SaveData saveData)
        {
            try
            {
                AllRegions.LoadSO(saveData.SavedRegionData.AllRegionData);
            }
            catch
            {
                if (saveData is null)
                {
                    Debug.LogError("No SaveData found in LoadData.");
                    return;
                }

                if (saveData.SavedRegionData is null)
                {
                    Debug.LogError("No SavedRegionData found in SaveData.");
                    return;
                }

                if (saveData.SavedRegionData.AllRegionData is null)
                {
                    Debug.LogError("No AllRegionData found in SavedRegionData.");
                    return;
                }
                
                Debug.LogError("AllRegionData count is 0.");
            }
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
            return AllRegions.GetRegion_Data(regionID).DataObject;
        }
        
        public static Region_Component GetRegion_Component(uint regionID)
        {
            return AllRegions.GetRegion_Component(regionID);
        }
        
        static Region_SO _getRegion_SO()
        {
            var region_SO = Resources.Load<Region_SO>(_region_SOPath);
            
            if (region_SO is not null) return region_SO;
            
            Debug.LogError("Region_SO not found. Creating temporary Region_SO.");
            region_SO = ScriptableObject.CreateInstance<Region_SO>();
            
            return region_SO;
        }

        public static Region_Component GetNearestRegion(Vector3 position)
        {
            Region_Component nearestRegion = null;

            var nearestDistance = float.MaxValue;

            foreach (var region in AllRegions.RegionComponents.Values)
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
        
        public static void ClearSOData()
        {
            AllRegions.ClearSOData();
        }
    }
}