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
        public Region_Data RegionData;

        public List<City_Component> AllCitiesInRegion;

        void Awake()
        {
            Manager_Initialisation.OnInitialiseRegions += _initialise;
        }

        void _initialise()
        {
            RegionData.InitialiseRegionData();
            
            AllCitiesInRegion = GetAllCitiesInRegion();

            AllCitiesInRegion.ForEach(city => city.SetRegionID(RegionData.RegionID));
        }

        public void SetRegionData(Region_Data regionData)
        {
            RegionData = regionData;
        }

        public List<City_Component> GetAllCitiesInRegion() => GetComponentsInChildren<City_Component>().ToList();

        public City_Component GetNearestCityInRegion(Vector3 position)
        {
            return AllCitiesInRegion
                   .OrderBy(city => Vector3.Distance(position, city.transform.position))
                   .FirstOrDefault();
        }
    }
}
