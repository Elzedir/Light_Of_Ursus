using System.Collections.Generic;
using UnityEngine;

namespace Counties
{
    public class County_Manager : MonoBehaviour
    {
        const string _region_SOPath = "ScriptableObjects/Region_SO";

        static County_SO s_allCounties;
        static County_SO AllCounties => s_allCounties ??= _getRegion_SO();
        
        public static County_Data GetRegion_Data(ulong regionID)
        {
            return AllCounties.GetRegion_Data(regionID).Data_Object;
        }

        public static County_Data GetRegion_DataFromName(County_Component county_Component)
        {
            return AllCounties.GetDataFromName(county_Component.name)?.Data_Object;
        }
        
        public static County_Component GetRegion_Component(ulong regionID)
        {
            return AllCounties.GetRegion_Component(regionID);
        }
        
        public static List<ulong> GetAllCountyIDs() => AllCounties.GetAllDataIDs(); 
        
        static County_SO _getRegion_SO()
        {
            var region_SO = Resources.Load<County_SO>(_region_SOPath);
            
            if (region_SO is not null) return region_SO;
            
            Debug.LogError("Region_SO not found. Creating temporary Region_SO.");
            region_SO = ScriptableObject.CreateInstance<County_SO>();
            
            return region_SO;
        }

        public static County_Component GetNearestRegion(Vector3 position)
        {
            County_Component nearestCounty = null;

            var nearestDistance = float.PositiveInfinity;

            foreach (var region in AllCounties.RegionComponents.Values)
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
            AllCounties.ClearSOData();
        }
    }
}