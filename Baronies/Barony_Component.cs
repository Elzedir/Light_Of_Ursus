using Cities;
using Initialisation;
using Managers;
using Settlements;
using Tools;
using UnityEngine;
using UnityEngine.Serialization;

namespace Baronies
{
    public class Barony_Component : MonoBehaviour
    {
        public ulong ID => _barony_Data.ID;
        Barony_Data _barony_Data;

        GameObject _baronySpawnZone;

        public Barony_Data Barony_Data => _barony_Data ??= Barony_Manager.GetBarony_DataFromName(this);
        public GameObject BaronySpawnZone => _baronySpawnZone ??=
            Manager_Game.FindTransformRecursively(transform, "BaronySpawnZone").gameObject;

        void Awake()
        {
            Manager_Initialisation.OnInitialiseSettlements += _initialise;
        }

        void _initialise()
        {
            if (Barony_Data?.ID is null or 0)
            {
                Debug.LogWarning($"Barony {Barony_Data?.ID}: {name} not found in Barony_SO.");
                return;
            }

            Barony_Data.InitialiseBaronyData();
        }

        public SerializableDictionary<ulong, Settlement_Data> GetAllSettlementsInBarony() =>
            GetComponentsInChildren<Settlement_Component>().ToSerializedDictionary(
                building => building.ID,
                building => building.Settlement_Data);
    }
}