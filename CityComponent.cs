using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class CityComponent : MonoBehaviour
{
    public CityData CityData;
    public BoxCollider CityArea;

    public GameObject CitySpawnZone;
    public ProsperityComponent ProsperityComponent;

    public List<JobsiteComponent> AllJobsitesInCity;

    public void Initialise()
    {
        CityArea = GetComponent<BoxCollider>();
        CitySpawnZone = Manager_Game.FindTransformRecursively(transform, "CityEntranceSpawnZone").gameObject;

        ProsperityComponent = new ProsperityComponent(CityData.Prosperity.CurrentProsperity);

        AllJobsitesInCity = GetAllJobsitesInCity();
    }

    public void SetCityData(CityData cityData)
    {
        CityData = cityData;
    }

    public void RefreshCity()
    {
        // Refresh all jobsites in citydata
    }

    public List<JobsiteComponent> GetAllJobsitesInCity()
    {
        return Physics.OverlapBox(CityArea.bounds.center, CityArea.bounds.extents)
        .Select(collider => collider.GetComponent<JobsiteComponent>()).Where(jc => jc != null).ToList();
    }

    public JobsiteComponent GetNearestJobsiteInCity(Vector3 position, JobsiteName jobsiteName)
    {
        return AllJobsitesInCity
        .Where(jobsite => jobsite.JobsiteData.JobsiteName == jobsiteName)
        .OrderBy(jobsite => Vector3.Distance(position, jobsite.transform.position))
        .FirstOrDefault();
    }
}
