using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RegionComponent : MonoBehaviour
{
    public RegionData RegionData;

    public List<CityComponent> AllCitiesInRegion;

    public void Initialise()
    {
        AllCitiesInRegion = GetAllCitiesInRegion();

        AllCitiesInRegion.ForEach(city => city.SetRegionID(RegionData.RegionID));
    }

    public void SetRegionData(RegionData regionData)
    {
        RegionData = regionData;
    }

    public List<CityComponent> GetAllCitiesInRegion() => GetComponentsInChildren<CityComponent>().ToList();

    public CityComponent GetNearestCityInRegion(Vector3 position)
    {
        return AllCitiesInRegion
        .OrderBy(city => Vector3.Distance(position, city.transform.position))
        .FirstOrDefault();
    }
}
