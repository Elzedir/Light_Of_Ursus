using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Jobsite;
using Managers;
using UnityEngine;

public class CityComponent : MonoBehaviour
{
    public CityData CityData;

    public GameObject CitySpawnZone;

    public List<JobsiteComponent> AllJobsitesInCity;

    public void Initialise()
    {
        CitySpawnZone = Manager_Game.FindTransformRecursively(transform, "CityEntranceSpawnZone").gameObject;

        AllJobsitesInCity = GetAllJobsitesInCity();
        AllJobsitesInCity.ForEach(jobsite => jobsite.SetCityID(CityData.CityID));
    }

    public void SetCityData(CityData cityData) => CityData = cityData;
    public void SetRegionID(uint regionID) => CityData.RegionID = regionID;

    public void RefreshCity()
    {
        // Refresh all jobsites in citydata
    }

    public List<JobsiteComponent> GetAllJobsitesInCity() => GetComponentsInChildren<JobsiteComponent>().ToList();

    public JobsiteComponent GetNearestJobsiteInCity(Vector3 position, JobsiteName jobsiteName)
    {
        return AllJobsitesInCity
        .Where(jobsite => jobsite.JobsiteData.JobsiteName == jobsiteName)
        .OrderBy(jobsite => Vector3.Distance(position, jobsite.transform.position))
        .FirstOrDefault();
    }
}
