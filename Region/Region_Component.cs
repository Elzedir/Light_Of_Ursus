using System.Collections.Generic;
using System.Linq;
using City;
using UnityEngine;

namespace Region
{
    public class RegionComponent : MonoBehaviour
    {
        public Region_Data RegionData;

        public List<City_Component> AllCitiesInRegion;

        public void Initialise()
        {
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
