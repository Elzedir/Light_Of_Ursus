using System.Collections.Generic;
using Priorities;

namespace Buildings
{
    public abstract class Building_PreExisting
    {
        static Dictionary<ulong, Building_Data> s_defaultBuildings;
        public static Dictionary<ulong, Building_Data> S_DefaultBuildings => s_defaultBuildings ??= _initialiseDefaultBuildings();
        
        static Dictionary<ulong, Building_Data> _initialiseDefaultBuildings()
        {
            return new Dictionary<ulong, Building_Data>
            {
                {
                    1, new Building_Data(
                        id: 1,
                        factionID: 0,
                        baronyID: 1,
                        ownerID: 0,
                        productionData: new ProductionData(1),
                        prosperityData: new Building_ProsperityData(),
                        priorityData: new Priority_Data_Building(1))
                }
            };
        }
    }
}