using System.Collections.Generic;
using Regions;
using UnityEngine;

namespace Region
{
    public class County_Manager : MonoBehaviour
    {
        const string _region_SOPath = "ScriptableObjects/Region_SO";

        static Region_SO _allRegions;
        static Region_SO AllRegions => _allRegions ??= _getRegion_SO();
        
        public static County_Data GetRegion_Data(ulong regionID)
        {
            return AllRegions.GetRegion_Data(regionID).Data_Object;
        }

        public static County_Data GetRegion_DataFromName(County_Component county_Component)
        {
            return AllRegions.GetDataFromName(county_Component.name)?.Data_Object;
        }
        
        public static County_Component GetRegion_Component(ulong regionID)
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

        public static County_Component GetNearestRegion(Vector3 position)
        {
            County_Component nearestCounty = null;

            var nearestDistance = float.PositiveInfinity;

            foreach (var region in AllRegions.RegionComponents.Values)
            {
                var distance = Vector3.Distance(position, region.transform.position);

                if (!(distance < nearestDistance)) continue;

                nearestCounty  = region;
                nearestDistance = distance;
            }

            return nearestCounty;
        }
        
        public static void ClearSOData()
        {
            AllRegions.ClearSOData();
        }
    }
}