using System;
using System.Collections.Generic;
using System.Linq;
using Initialisation;
using JobSite;
using Managers;
using UnityEngine;

namespace City
{
    public class City_Component : MonoBehaviour
    {
        public uint               CityID    => CityData.CityID;
        public City_Data          CityData;

        public GameObject CitySpawnZone;

        public void SetCityData(City_Data cityData)
        {
            CityData = cityData;   
        }

        void Awake()
        {
            Manager_Initialisation.OnInitialiseCities += _initialise;
        }

        void _initialise()
        {
            CityData.InitialiseCityData();
            
            CitySpawnZone = Manager_Game.FindTransformRecursively(transform, "CityEntranceSpawnZone").gameObject;
        }

        public Dictionary<uint, JobSite_Component> GetAllJobSitesInCity() =>
            GetComponentsInChildren<JobSite_Component>().ToDictionary(jobsite => jobsite.JobSiteID);
    }
}
