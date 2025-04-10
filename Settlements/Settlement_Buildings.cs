using System.Collections.Generic;
using Buildings;
using Tools;

namespace Settlements
{
    public class Settlement_Buildings
    {
        public Settlement_Data Settlement_Data;
        public int MaxLevel;
        public Dictionary<int, int> BuildingSlotsPerLevel;
        
        Dictionary<ulong, Building_Plot> _allBuildings;
        
        public Dictionary<Building_Plot> AllBuildingPlots
        {
            get
            {
                if (_allBuildings is not null && _allBuildings.Count != 0) return _allBuildings;

                return _allBuildings = Settlement_Data.Settlement.GetAllBuildingPlotsInSettlement();
            }
        }

        public Settlement_Buildings(Settlement_Buildings data, Settlement_Data settlement_Data)
        {
            Settlement_Data = settlement_Data;
            MaxLevel = data.MaxLevel;
        }

        public Settlement_Buildings()
        {
            
        }

        public void OnProgressDay()
        {
            foreach (var buildingPlot in AllBuildingPlots)
            {
                buildingPlot.Building.OnProgressDay();
            }
        }

        public float GenerateIncome(float liegeTaxRate)
        {
            var income = 0f;
            
            foreach (var buildingPlot in AllBuildingPlots)
            {
                income += buildingPlot.GenerateIncome(liegeTaxRate);
            }
            
            return income;
        }
    }
}