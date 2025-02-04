using System.Collections.Generic;
using System.Linq;
using Cities;
using Initialisation;
using Region;
using UnityEngine;

namespace Regions
{
    public class Region_Component : MonoBehaviour
    {
        public ulong RegionID => RegionData.RegionID;
        
        public Region_Data               RegionData;

        public void SetRegionData(Region_Data regionData)
        {
            RegionData = regionData;
        }

        void Awake()
        {
            Manager_Initialisation.OnInitialiseRegions += _initialise;
        }

        void _initialise()
        {
            var regionData = Region_Manager.GetRegion_DataFromName(this);
            
            if (regionData is null)
            {
                Debug.LogWarning($"Region with name {name} not found in Region_SO.");
                return;
            }
            
            SetRegionData(regionData);
            
            RegionData.InitialiseRegionData();
        }

        public List<City_Component> GetAllCitiesInRegion() => GetComponentsInChildren<City_Component>().ToList();
    }
}
