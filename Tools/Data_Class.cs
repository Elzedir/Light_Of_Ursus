using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Tools
{
    [Serializable]

    public abstract class Data_Class
    {
        [SerializeField] DataToDisplay _dataToDisplay;

        protected DataToDisplay DataToDisplay
        {
            get => _dataToDisplay ??= new DataToDisplay("Display Data");
            set => _dataToDisplay = value;
        }

        public abstract Dictionary<string, string> GetStringData();

        // We're passing through the DataTODisplay as the SubDataToDisplay, so we're just showing the same data again and again.
        // Try find a way to use this split system to display DataDisplay as First Data and SubData To Display as all other data.
        public abstract DataToDisplay GetDataToDisplay(bool toggleMissingDataDebugs);

        public virtual Dictionary<string, DataToDisplay> GetInteractableData(bool toggleMissingDataDebugs)
        {
            return new Dictionary<string, DataToDisplay>();
        }

        protected void _updateDataDisplay(DataToDisplay dataToDisplay,
            string title,
            bool toggleMissingDataDebugs,
            Dictionary<string, string> allStringData = null,
            DataToDisplay allInteractableData = null,
            DataToDisplay allSubData = null)
        {
            try
            {
                if (allStringData is not null)
                    dataToDisplay.AllStringData[title] = allStringData;

                if (allInteractableData is not null)
                    dataToDisplay.AllInteractableData[title] = allInteractableData;

                if (allSubData is null) return;
                
                dataToDisplay.AllSubData[title] = allSubData;
                allSubData.Title = title;

            }
            catch
            {
                if (!toggleMissingDataDebugs) return;

                if (string.IsNullOrEmpty(title))
                    Debug.LogError("Title is null.");

                if (dataToDisplay is null)
                    Debug.LogError($"DataToDisplay for {title} is null.");
            }
        }
    }
}