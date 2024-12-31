using System;

namespace Tools
{
    [Serializable]

    public abstract class Data_Class
    {
        Data_Display        _dataSO_Object;
        public Data_Display DataSO_Object(bool toggleMissingDataDebugs) => _getDataSO_Object(toggleMissingDataDebugs, _dataSO_Object);
        
        protected abstract Data_Display _getDataSO_Object(bool toggleMissingDataDebugs, Data_Display dataSO_Object);
    }
}