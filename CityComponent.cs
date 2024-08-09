using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CityComponent : MonoBehaviour
{
    public CityData CityData;
    public BoxCollider CityArea;

    public GameObject CityEntranceSpawnZone;

    public List<Jobsite_Base> AllJobsites = new();
    public ProsperityComponent ProsperityComponent;

    void Awake()
    {
        CityArea = GetComponent<BoxCollider>();
        CityEntranceSpawnZone = Manager_Game.FindTransformRecursively(transform, "CityEntranceSpawnZone").gameObject;

        CurrentDate.NewDay += _refreshCity;
    }

    public void Initialise()
    {
        ProsperityComponent = new ProsperityComponent(CityData.Prosperity.CurrentProsperity);
        AllJobsites = _getJobsitesInArea();

        foreach(var job in AllJobsites)
        {
            job.Initialise(this);
        }
    }

    List<Jobsite_Base> _getJobsitesInArea()
    {
        return Physics.OverlapBox(CityArea.bounds.center, CityArea.bounds.extents)
        .Select(collider => collider.GetComponent<Jobsite_Base>())
        .Where(jobsite => jobsite != null)
        .ToList();
    }

    public void SetCityData(CityData cityData)
    {
        if (CityData.OverwriteDataInCity)
        {
            CityData = cityData;
        }
    }

    void _refreshCity()
    {
        


    }
}
