using System;
using System.Collections.Generic;
using Tools;
using UnityEngine;

namespace Managers
{
    public class Manager_Prosperity
    {
    
    }

    [Serializable]
    public class ProsperityData : Data_Class
    {
        public float      CurrentProsperity;
        public float      MaxProsperity;
        public float      BaseProsperityGrowthPerDay;

        public ProsperityData(float currentProsperity, float maxProsperity, float baseProsperityGrowthPerDay)
        {
            CurrentProsperity          = currentProsperity;
            MaxProsperity              = maxProsperity;
            BaseProsperityGrowthPerDay = baseProsperityGrowthPerDay;
        }
        
        public ProsperityData(ProsperityData prosperityData)
        {
            CurrentProsperity          = prosperityData.CurrentProsperity;
            MaxProsperity              = prosperityData.MaxProsperity;
            BaseProsperityGrowthPerDay = prosperityData.BaseProsperityGrowthPerDay;
        }

        public void ChangeProsperity(float prosperityChange)
        {
            CurrentProsperity += Math.Min(prosperityChange, MaxProsperity - CurrentProsperity);
        }

        public void SetProsperity(float prosperity)
        {
            CurrentProsperity = prosperity >= 0 ? prosperity : 0;
        }

        public float GetProsperityPercentage()
        {
            // Eventually prosperity percent will take into account all levels, region, city, jobsite, etc.
            return Math.Min((CurrentProsperity / Math.Max(MaxProsperity, 1)) + 0.1f, 1);
        }

        public void OnTick()
        {
            ChangeProsperity(_getProsperityGrowth());
        }

        public float _getProsperityGrowth()
        {
            if (CurrentProsperity > MaxProsperity) return Math.Max(MaxProsperity * 0.05f, 1);
            if (CurrentProsperity == MaxProsperity) return 0;

            return BaseProsperityGrowthPerDay; // Add modifiers afterwards.
        }
        
        protected override Data_Display _getDataSO_Object(bool toggleMissingDataDebugs, Data_Display dataSO_Object)
        {
            if (dataSO_Object.Data is null && dataSO_Object.SubData is null)
                dataSO_Object = new Data_Display(
                    title: "Prosperity Data",
                    dataDisplayType: DataDisplayType.List_CheckBox,
                    data: new Dictionary<string, string>());

            try
            {
                if (!dataSO_Object.SubData.TryGetValue("Prosperity Data", out var prosperityData))
                {
                    dataSO_Object.SubData["Prosperity Data"] = new Data_Display(
                        title: "Prosperity Data",
                        dataDisplayType: DataDisplayType.List_Item,
                        data: new Dictionary<string, string>());
                }
                
                if (prosperityData is not null)
                {
                    prosperityData.Data = new Dictionary<string, string>
                    {
                        { "Current Prosperity", $"{CurrentProsperity}" },
                        { "Max Prosperity", $"{MaxProsperity}" },
                        { "Base Prosperity Growth Per Day", $"{BaseProsperityGrowthPerDay}" }
                    };
                }
            }
            catch
            {
                Debug.LogError("Error in Prosperity Data");
            }
            
            return dataSO_Object;
        }
    }
}