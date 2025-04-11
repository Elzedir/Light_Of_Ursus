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
        public ulong ID;
        [SerializeField] Settlement_Data _settlement_Data;

        GameObject _settlementSpawnZone;

        public Settlement_Data Settlement_Data => _settlement_Data ??= 
            Settlement_Manager.GetSettlement_DataFromName(this);
        public GameObject SettlementSpawnZone => _settlementSpawnZone ??=
            Manager_Game.FindTransformRecursively(transform, "SettlementSpawnZone").gameObject;

        void Awake()
        {
            Manager_Initialisation.OnInitialiseSettlements += _initialise;
        }

        void _initialise()
        {
            if (Settlement_Data?.ID is null or 0)
            {
                Debug.LogWarning($"Settlement with name {name} not found in Settlement_SO.");
                return;
            }

            Settlement_Data.InitialiseSettlementData(ID);
        }

        public Dictionary<ulong, Building_Data> GetAllBuildingsInSettlement() =>
            GetComponentsInChildren<Building_Component>().ToDictionary(
                building => building.ID,
                building => building.Building_Data);
    }
}