using System.Collections;
using System.Collections.Generic;
using System.Linq;
using City;
using UnityEngine;

public class RegionComponent : MonoBehaviour
{
    public RegionData RegionData;

    public List<City_Component> AllCitiesInRegion;

    public void Initialise()
    {
        AllCitiesInRegion = GetAllCitiesInRegion();

        AllCitiesInRegion.ForEach(city => city.SetRegionID(RegionData.RegionID));
    }

    public void SetRegionData(RegionData regionData)
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
