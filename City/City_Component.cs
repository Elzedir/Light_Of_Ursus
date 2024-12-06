using System.Collections.Generic;
using System.Linq;
using JobSite;
using Managers;
using UnityEngine;

namespace City
{
    public class CityComponent : MonoBehaviour
    {
        public CityData CityData;

        public GameObject CitySpawnZone;

        public List<JobSite_Component> AllJobsitesInCity;

        public void Initialise()
        {
            CitySpawnZone = Manager_Game.FindTransformRecursively(transform, "CityEntranceSpawnZone").gameObject;

            AllJobsitesInCity = GetAllJobsitesInCity();
            AllJobsitesInCity.ForEach(jobsite => jobsite.SetCityID(CityData.CityID));
        }

        public void SetCityData(CityData cityData) => CityData = cityData;
        public void SetRegionID(uint     regionID) => CityData.RegionID = regionID;

        public void RefreshCity()
        {
            // Refresh all jobsites in citydata
        }

        public List<JobSite_Component> GetAllJobsitesInCity() => GetComponentsInChildren<JobSite_Component>().ToList();

        public JobSite_Component GetNearestJobsiteInCity(Vector3 position, JobSiteName jobSiteName)
        {
            return AllJobsitesInCity
                   .Where(jobsite => jobsite.JobSiteName == jobSiteName)
                   .OrderBy(jobsite => Vector3.Distance(position, jobsite.transform.position))
                   .FirstOrDefault();
        }
    }
}
