using System;
using System.Collections.Generic;
using System.Linq;
using Initialisation;
using JobSite;
using Managers;
using UnityEngine;
using UnityEngine.Serialization;

namespace City
{
    public class City_Component : MonoBehaviour
    {
        public uint      CityID => CityData.CityID;
        public City_Data CityData;

        public GameObject CitySpawnZone;

        List<JobSite_Component> _allJobSitesInCity;
        public List<JobSite_Component> AllJobSitesInCity => _allJobSitesInCity ??= _getAllJobSitesInCity();

        void Awake()
        { 
            Manager_Initialisation.OnInitialiseCities += _initialise;
        }

        void _initialise()
        {
            CityData.InitialiseCityData();
            
            CitySpawnZone = Manager_Game.FindTransformRecursively(transform, "CityEntranceSpawnZone").gameObject;

            AllJobSitesInCity.ForEach(jobsite => jobsite.SetCityID(CityData.CityID));
        }

        public void SetCityData(City_Data cityData) => CityData = cityData;
        public void SetRegionID(uint     regionID) => CityData.RegionID = regionID;

        public void RefreshCity()
        {
            // Refresh all jobsites in citydata
        }

        List<JobSite_Component> _getAllJobSitesInCity() => GetComponentsInChildren<JobSite_Component>().ToList();
    }
}
