using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Tools
{
    [Serializable]

    public abstract class Data_Class
    {
        [SerializeField] Data_Display        _data_Display;

        public Data_Display GetData_Display(bool toggleMissingDataDebugs)
        {
            return _data_Display = _getDataSO_Object(toggleMissingDataDebugs, _data_Display);
        }
        
        protected abstract Data_Display _getDataSO_Object(bool toggleMissingDataDebugs, Data_Display dataSO_Object);
    }
}