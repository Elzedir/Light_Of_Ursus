using System.Collections.Generic;
using Regions;
using UnityEngine;

namespace Region
{
    public class Region_Manager : MonoBehaviour
    {
        const string _region_SOPath = "ScriptableObjects/Region_SO";

        static Region_SO _allRegions;
        static Region_SO AllRegions => _allRegions ??= _getRegion_SO();
        
        public static Region_Data GetRegion_Data(ulong regionID)
        {
            return AllRegions.GetRegion_Data(regionID).Data_Object;
        }

        public static Region_Data GetRegion_DataFromName(Region_Component region_Component)
        {
            return AllRegions.GetDataFromName(region_Component.name)?.Data_Object;
        }
        
        public static Region_Component GetRegion_Component(ulong regionID)
        {
            return AllRegions.GetRegion_Component(regionID);
        }
        
        public static List<ulong> GetAllRegionIDs() => AllRegions.GetAllDataIDs(); 
        
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

            var nearestDistance = float.PositiveInfinity;

            foreach (var region in AllRegions.RegionComponents.Values)
            {
                var distance = Vector3.Distance(position, region.transform.position);

                if (!(distance < nearestDistance)) continue;

                nearestRegion  = region;
                nearestDistance = distance;
            }

            return nearestRegion;
        }
        
        public static void ClearSOData()
        {
            AllRegions.ClearSOData();
        }
    }
}