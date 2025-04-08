using System.Collections.Generic;
using System.Linq;
using Baronies;
using Initialisation;
using Tools;
using UnityEngine;

namespace Counties
{
    public class County_Component : MonoBehaviour
    {
        public ulong ID => County_Data.ID;

        County_Data _county_Data;
        public County_Data County_Data => _county_Data ??= County_Manager.GetCounty_DataFromName(this);

        void Awake()
        {
            Manager_Initialisation.OnInitialiseCounties += _initialise;
        }

        void _initialise()
        {
            if (County_Data is null)
            {
                Debug.LogWarning($"County with name {name} not found in County_SO.");
                return;
            }
            
            County_Data.InitialiseCountyData();
        }

        public SerializableDictionary<ulong, Barony_Data> GetAllBaroniesInCounty() =>
            GetComponentsInChildren<Barony_Component>().ToSerializedDictionary(
                barony => barony.ID,
                barony => barony.Barony_Data);
    }
}
