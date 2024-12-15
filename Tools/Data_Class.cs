using System;
using UnityEngine.Serialization;

namespace Tools
{
    [Serializable]

    public abstract class Data_Class
    {
        Data_Display        _dataSO_Object;
        public Data_Display DataSO_Object(bool toggleMissingDataDebugs) => _dataSO_Object ??= _getDataSO_Object(toggleMissingDataDebugs);

        protected abstract Data_Display _getDataSO_Object(bool toggleMissingDataDebugs);
    }
}