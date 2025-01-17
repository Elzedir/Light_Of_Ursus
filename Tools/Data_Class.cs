using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Tools
{
    [Serializable]

    public abstract class Data_Class
    {
        [SerializeField] protected DataToDisplay _dataToDisplay;

        public DataToDisplay GetData_Display(bool toggleMissingDataDebugs)
        {
            if (_dataToDisplay?.AllStringData is null ||
                _dataToDisplay.AllSubData is null ||
                _dataToDisplay.AllInteractableData is null)
            {
                _dataToDisplay = new DataToDisplay("Display Data");
            }

            return _dataToDisplay = GetSubData(toggleMissingDataDebugs);
        }

        public abstract Dictionary<string, string> GetStringData();

        // We're passing through the DataTODisplay as the SubDataToDisplay, so we're just showing the same data again and again.
        // Try find a way to use this split system to display DataDisplay as First Data and SubData To Display as all other data.
        public abstract DataToDisplay GetSubData(bool toggleMissingDataDebugs);

        public virtual Dictionary<string, DataToDisplay> GetInteractableData(bool toggleMissingDataDebugs)
        {
            return new Dictionary<string, DataToDisplay>();
        }

        protected void _updateDataDisplay(ref DataToDisplay dataToDisplay,
            string title,
            bool toggleMissingDataDebugs,
            Dictionary<string, string> allStringData = null,
            DataToDisplay allSubData = null,
            DataToDisplay allInteractableData = null)
        {
            try
            {
                if (allStringData is null && allSubData is null && allInteractableData is null)
                {
                    if (toggleMissingDataDebugs)
                        Debug.LogError("StringData, SubData, InteractableData are all null. Data will be empty.");
                }
                
                dataToDisplay.AllStringData[title] = allStringData;
                dataToDisplay.AllSubData[title] = allSubData;
                dataToDisplay.AllSubData[title].Title = title;
                dataToDisplay.AllInteractableData[title] = allInteractableData;
            }
            catch
            {
                if (toggleMissingDataDebugs)
                    Debug.Log($"Error in {title} Data");
            }
        }
    }
}