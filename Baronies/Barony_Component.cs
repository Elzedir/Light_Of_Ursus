using System.Collections.Generic;
using System.Linq;
using Buildings;
using Initialisation;
using Managers;
using UnityEngine;

namespace Cities
{
    public class City_Component : MonoBehaviour
    {
        public ulong               ID    => CityData.ID;
        public City_Data          CityData;

        public GameObject _citySpawnZone;
        
        public GameObject CitySpawnZone => _citySpawnZone ??= Manager_Game.FindTransformRecursively(transform, "CityEntranceSpawnZone").gameObject;

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
            
            CityData.InitialiseCityData();
        }

        public Dictionary<ulong, Building_Component> GetAllBuildingsInCity() =>
            GetComponentsInChildren<Building_Component>().ToDictionary(building => building.ID);
    }
}
