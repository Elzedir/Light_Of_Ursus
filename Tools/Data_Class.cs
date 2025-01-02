using System;
using UnityEngine;

namespace Tools
{
    [Serializable]

    public abstract class Data_Class
    {
        [SerializeField] Data_Display        _dataSO_Object;

        public Data_Display GetDataSO_Object(bool toggleMissingDataDebugs)
        {
            return _dataSO_Object = _getDataSO_Object(toggleMissingDataDebugs, _dataSO_Object);
        }
        
        protected abstract Data_Display _getDataSO_Object(bool toggleMissingDataDebugs, Data_Display dataSO_Object);
    }
}