using System;
using System.Collections.Generic;
using Tools;

namespace Buildings
{
    [Serializable]
    public class Building_ProsperityData : Data_Class
    {
        public Building_ProsperityData()
        {
            
        }

        public Building_ProsperityData(Building_ProsperityData prosperityData)
        {
            
        }
        
        public float GetProsperityPercentage()
        {
            // Later, base prosperity off owner gold
            return 0.5f;
        }
        
        public override Dictionary<string, string> GetStringData()
        {
            return new Dictionary<string, string>
            {
                { "Current Prosperity", $"Placeholder" },
                { "Max Prosperity", $"PaceHolder" },
                { "Base Prosperity Growth Per Day", $"Placeholder" }
            };
        }

        public override DataToDisplay GetDataToDisplay(bool toggleMissingDataDebugs)
        {
            _updateDataDisplay(DataToDisplay,
                title: "Prosperity Data",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: GetStringData());
            
            return DataToDisplay;
        }
    }
}