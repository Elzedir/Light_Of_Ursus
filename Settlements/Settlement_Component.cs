using System.Collections.Generic;
using System.Linq;
using Buildings;
using Initialisation;
using Managers;
using UnityEngine;

namespace Settlements
{
    public class Settlement_Component : MonoBehaviour
    {
        public ulong ID => Settlement_Data.ID;
        public Settlement_Data Settlement_Data;

        GameObject _settlementSpawnZone;

        // public Settlement_Data Settlement_Data => _settlement_Data ??= 
        //     Settlement_Manager.GetSettlement_DataFromName(this);
        public GameObject SettlementSpawnZone => _settlementSpawnZone ??=
            Manager_Game.FindTransformRecursively(transform, "SettlementSpawnZone").gameObject;

        void Awake()
        {
            Manager_Initialisation.OnInitialiseSettlements += _initialise;
        }

        void _initialise()
        {
            Settlement_Data = Settlement_Manager.GetSettlement_DataFromName(this);
            
            if (Settlement_Data?.ID is null or 0)
            {
                Debug.LogWarning($"Settlement with name {name} not found in Settlement_SO.");
                return;
            }

            Settlement_Data.InitialiseSettlementData();
        }

        public Dictionary<ulong, Building_Plot> GetAllBuildingPlotsInSettlement() =>
            GetComponentsInChildren<Building_Plot>().ToDictionary(
                building => building.ID,
                building => building);
    }
}