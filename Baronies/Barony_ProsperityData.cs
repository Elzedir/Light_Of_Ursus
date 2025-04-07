using System;
using System.Collections.Generic;
using Tools;

namespace Cities
{
    [Serializable]
    public class Barony_ProsperityData : Data_Class
    {
        public float      CurrentProsperity;
        public float      MaxProsperity;
        public float      BaseProsperityGrowthPerDay;

        public Barony_ProsperityData(float currentProsperity, float maxProsperity, float baseProsperityGrowthPerDay)
        {
            CurrentProsperity          = currentProsperity;
            MaxProsperity              = maxProsperity;
            BaseProsperityGrowthPerDay = baseProsperityGrowthPerDay;
        }
        
        public Barony_ProsperityData(Barony_ProsperityData baronyProsperityData)
        {
            CurrentProsperity          = baronyProsperityData.CurrentProsperity;
            MaxProsperity              = baronyProsperityData.MaxProsperity;
            BaseProsperityGrowthPerDay = baronyProsperityData.BaseProsperityGrowthPerDay;
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
            // Eventually prosperity percent will take into account all levels, region, Barony, building, etc.
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