using System;
using System.Collections.Generic;
using Tools;

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

        public override Dictionary<string, string> GetStringData()
        {
            return new Dictionary<string, string>
            {
                { "Current Prosperity", $"{CurrentProsperity}" },
                { "Max Prosperity", $"{MaxProsperity}" },
                { "Base Prosperity Growth Per Day", $"{BaseProsperityGrowthPerDay}" }
            };
        }

        public override DataToDisplay GetSubData(bool toggleMissingDataDebugs, DataToDisplay dataToDisplay)
        {
            _updateDataDisplay(ref dataToDisplay,
                title: "Prosperity Data",
                stringData: GetStringData());
            
            return dataToDisplay;
        }
    }
}