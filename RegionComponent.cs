using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RegionComponent : MonoBehaviour
{
    public RegionData RegionData;
    public BoxCollider RegionArea;

    public ProsperityComponent ProsperityComponent;

    public List<CityComponent> AllCitiesInRegion;

    public void Initialise()
    {
        RegionArea = GetComponent<BoxCollider>();

        CurrentDate.NewDay += _refreshRegion;

        ProsperityComponent = new ProsperityComponent(RegionData.Prosperity.CurrentProsperity);

        AllCitiesInRegion = GetAllCitiesInRegion();
    }

    public void SetRegionData(RegionData regionData)
    {
        RegionData = regionData;
    }

    void _refreshRegion()
    {
        // Refresh all cities in regiondata
    }

    public List<CityComponent> GetAllCitiesInRegion()
    {
        return Physics.OverlapBox(RegionArea.bounds.center, RegionArea.bounds.extents)
        .Select(collider => collider.GetComponent<CityComponent>()).Where(rc => rc != null).ToList();
    }

    public CityComponent GetNearestCityInRegion(Vector3 position)
    {
        return AllCitiesInRegion
        .OrderBy(city => Vector3.Distance(position, city.transform.position))
        .FirstOrDefault();
    }
}
