using System;
using System.Collections.Generic;
using System.Linq;
using City;
using Initialisation;
using UnityEngine;

namespace Region
{
    public class Region_Component : MonoBehaviour
    {
        public uint RegionID => RegionData.RegionID;
        
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
            RegionData.InitialiseRegionData();
        }

        public List<City_Component> GetAllCitiesInRegion() => GetComponentsInChildren<City_Component>().ToList();
    }
}
