using System.Collections.Generic;
using Buildings;
using Cities;
using Tools;

namespace Baronies
{
    public class Barony_BuildingData
    {
        public BaronyType Type;
        public int MaxLevel;
        public Dictionary<int, int> BuildingSlotsPerLevel;
        
        SerializableDictionary<ulong, Building_Data> _allBuildings;

        public Barony_BuildingData(Barony_BuildingData data)
        {
            
        }

        public Barony_BuildingData()
        {
            
        }
    }
}