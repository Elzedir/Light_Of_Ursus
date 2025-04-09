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
        
        SerializableDictionary<ulong, Building_Data> _allBuildings;
        
        public SerializableDictionary<ulong, Building_Data> AllBuildings
        {
            get
            {
                if (_allBuildings is not null && _allBuildings.Count != 0) return _allBuildings;

                return _allBuildings = Settlement_Data.Settlement.GetAllBuildingsInSettlement();
            }
        }

        public Settlement_Buildings(Settlement_Buildings data)
        {
            
        }

        public Settlement_Buildings()
        {
            
        }

        public void OnProgressDay()
        {
            foreach (var building in AllBuildings.Values)
            {
                building.OnProgressDay();
            }
        }

        public float GenerateIncome(float liegeTaxRate)
        {
            var income = 0f;
            
            foreach (var building in AllBuildings.Values)
            {
                income += building.GenerateIncome(liegeTaxRate);
            }
            
            return income;
        }
    }
}