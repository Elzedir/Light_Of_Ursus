using System.Collections.Generic;
using System.Linq;
using Buildings;
using Initialisation;
using Managers;
using UnityEngine;
using UnityEngine.Serialization;

namespace Cities
{
    public class Barony_Component : MonoBehaviour
    {
        public ulong               ID    => BaronyData.ID;
        public Barony_Data          BaronyData;

        public GameObject _baronySpawnZone;
        
        public GameObject BaronySpawnZone => _baronySpawnZone ??= Manager_Game.FindTransformRecursively(transform, "BaronyEntranceSpawnZone").gameObject;

        public void SetBaronyData(Barony_Data baronyData)
        {
            BaronyData = baronyData;   
        }

        void Awake()
        {
            Manager_Initialisation.OnInitialiseCities += _initialise;
        }

        void _initialise()
        {
            var BaronyData = Barony_Manager.GetBarony_DataFromName(this);
            
            if (BaronyData is null)
            {
                Debug.LogWarning($"Barony with name {name} not found in Barony_SO.");
                return;
            }
            
            SetBaronyData(BaronyData);
            
            BaronyData.InitialiseBaronyData();
        }

        public Dictionary<ulong, Building_Component> GetAllBuildingsInBarony() =>
            GetComponentsInChildren<Building_Component>().ToDictionary(building => building.ID);
    }
}
