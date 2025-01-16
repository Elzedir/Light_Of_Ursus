using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tools
{
    [Serializable]

    public abstract class Data_Class
    {
        [SerializeField] DataToDisplay _dataTo_Display;

        public DataToDisplay GetData_Display(bool toggleMissingDataDebugs)
        {
            if (_dataTo_Display?.StringData is null || 
                _dataTo_Display.SubData is null ||
                _dataTo_Display.InteractableData is null)
            {
                _dataTo_Display = new DataToDisplay("Display Data");
            }
            
            return _dataTo_Display = GetSubData(toggleMissingDataDebugs, _dataTo_Display);
        }
        
        public abstract Dictionary<string, string> GetStringData();
        public abstract DataToDisplay GetSubData(bool toggleMissingDataDebugs, DataToDisplay dataToDisplay);

        public virtual Dictionary<string, DataToDisplay> GetInteractableData(bool toggleMissingDebugs,
            DataToDisplay dataToDisplay)
        {
            return new Dictionary<string, DataToDisplay>();
        }
        
        protected void _updateDataDisplay(ref DataToDisplay dataToDisplay,
            string title,
            Dictionary<string, string> stringData = null,
            Dictionary<string, DataToDisplay> subData = null,
            Dictionary<string, DataToDisplay> interactableData = null)
        {
            try
            {
                if (stringData is null && subData is null && interactableData is null)
                {
                    Debug.LogError("StringData, SubData, InteractableData are all null. Data will be empty.");
                }

                if (dataToDisplay.SubData.TryGetValue(title, out var data))
                {
                    data.StringData = stringData ?? new Dictionary<string, string>();
                    data.SubData = subData ?? new Dictionary<string, DataToDisplay>();
                    data.InteractableData = interactableData ?? new Dictionary<string, DataToDisplay>();
                    return;
                }

                dataToDisplay.SubData[title] = new DataToDisplay(title);
            }
            catch
            {
                Debug.Log($"Error in {title} Data");
            }
        }
    }
}