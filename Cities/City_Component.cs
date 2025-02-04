using System;
using System.Collections.Generic;
using System.Linq;
using City;
using Initialisation;
using JobSites;
using Managers;
using UnityEngine;

namespace Cities
{
    public class City_Component : MonoBehaviour
    {
        public ulong               CityID    => CityData.CityID;
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
            var cityData = City_Manager.GetCity_DataFromName(this);
            
            if (cityData is null)
            {
                Debug.LogWarning($"City with name {name} not found in City_SO.");
                return;
            }
            
            SetCityData(cityData);
            
            CitySpawnZone = Manager_Game.FindTransformRecursively(transform, "CityEntranceSpawnZone").gameObject;
            
            CityData.InitialiseCityData();
        }

        public Dictionary<ulong, JobSite_Component> GetAllJobSitesInCity() =>
            GetComponentsInChildren<JobSite_Component>().ToDictionary(jobsite => jobsite.JobSiteID);
    }
}
