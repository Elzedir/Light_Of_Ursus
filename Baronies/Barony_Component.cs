using Buildings;
using Cities;
using Initialisation;
using Managers;
using Tools;
using UnityEngine;
using UnityEngine.Serialization;

namespace Baronies
{
    public class Barony_Component : MonoBehaviour
    {
        public ulong ID => Barony_Data.ID;
        [FormerlySerializedAs("BaronyData")] public Barony_Data Barony_Data;

        GameObject _baronySpawnZone;

        public GameObject BaronySpawnZone => _baronySpawnZone ??=
            Manager_Game.FindTransformRecursively(transform, "BaronySpawnZone").gameObject;

        public void SetBaronyData(Barony_Data baronyData)
        {
            Barony_Data = baronyData;
        }

        void Awake()
        {
            Manager_Initialisation.OnInitialiseBaronies += _initialise;
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

        public SerializableDictionary<ulong, Building_Data> GetAllBuildingsInBarony() =>
            GetComponentsInChildren<Building_Component>().ToSerializedDictionary(
                building => building.ID,
                building => building.Building_Data);
    }
}