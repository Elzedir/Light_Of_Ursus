using System.Linq;
using Actors;
using Buildings;
using Initialisation;
using Jobs;
using Managers;
using Tools;
using UnityEngine;
using UnityEngine.Serialization;

namespace Settlements
{
    public class Settlement_Component : MonoBehaviour
    {
        public ulong ID => Settlement_Data.ID;
        public Settlement_Data Settlement_Data;

        GameObject _SettlementSpawnZone;

        public GameObject SettlementSpawnZone => _SettlementSpawnZone ??=
            Manager_Game.FindTransformRecursively(transform, "SettlementSpawnZone").gameObject;

        public void SetSettlementData(Settlement_Data SettlementData)
        {
            Settlement_Data = SettlementData;
        }

        void Awake()
        {
            Manager_Initialisation.OnInitialiseSettlements += _initialise;
        }

        void _initialise()
        {
            var settlementData = Settlement_Manager.GetSettlement_DataFromName(this);

            if (settlementData is null)
            {
                Debug.LogWarning($"Settlement with name {name} not found in Settlement_SO.");
                return;
            }

            SetSettlementData(settlementData);

            settlementData.InitialiseSettlementData();
        }

        public SerializableDictionary<ulong, Building_Data> GetAllBuildingsInSettlement() =>
            GetComponentsInChildren<Building_Component>().ToSerializedDictionary(
                building => building.ID,
                building => building.Building_Data);
    }
}