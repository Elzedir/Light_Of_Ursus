using System.Collections.Generic;
using System.Linq;
using Cities;
using Initialisation;
using UnityEngine;

namespace Counties
{
    public class County_Component : MonoBehaviour
    {
        public ulong CountyID => CountyData.ID;
        
        public County_Data               CountyData;

        public void SetCountyData(County_Data countyData)
        {
            CountyData = countyData;
        }

        void Awake()
        {
            Manager_Initialisation.OnInitialiseRegions += _initialise;
        }

        void _initialise()
        {
            var regionData = County_Manager.GetRegion_DataFromName(this);
            
            if (regionData is null)
            {
                Debug.LogWarning($"Region with name {name} not found in Region_SO.");
                return;
            }
            
            SetCountyData(regionData);
            
            CountyData.InitialiseRegionData();
        }

        public List<Barony_Component> GetAllCitiesInRegion() => GetComponentsInChildren<Barony_Component>().ToList();
    }
}
