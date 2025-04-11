using System.Collections.Generic;
using System.Linq;
using Baronies;
using Initialisation;
using UnityEngine;

namespace Counties
{
    public class County_Component : MonoBehaviour
    {
        public ulong ID => County_Data.ID;

        [SerializeField] County_Data _county_Data;
        public County_Data County_Data => _county_Data ??= County_Manager.GetCounty_DataFromName(this);

        void Awake()
        {
            Manager_Initialisation.OnInitialiseCounties += _initialise;
        }

        void _initialise()
        {
            if (County_Data?.ID is null or 0)
            {
                Debug.LogWarning($"County {County_Data?.ID}: {name} not found in County_SO.");
                return;
            }
            
            County_Data.InitialiseCountyData();
        }

        public Dictionary<ulong, Barony_Data> GetAllBaroniesInCounty() =>
            GetComponentsInChildren<Barony_Component>().ToDictionary(
                barony => barony.ID,
                barony => barony.Barony_Data);
    }
}
