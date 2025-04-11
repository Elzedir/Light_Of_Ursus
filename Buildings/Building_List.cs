using System.Collections.Generic;
using Priorities;

namespace Buildings
{
    public abstract class Building_List
    {
        static Dictionary<ulong, Building_Data> s_defaultBuildings;
        public static Dictionary<ulong, Building_Data> S_DefaultBuildings => s_defaultBuildings ??= _initialiseDefaultBuildings();
        
        static Dictionary<ulong, Building_Data> _initialiseDefaultBuildings()
        {
            var buildings = new Dictionary<ulong, Building_Data>();

            const ulong lumberjack1ID = 200000;
            buildings.Add(lumberjack1ID, new Building_Data(
                id: lumberjack1ID,
                settlementID: 1,
                ownerID: 0,
                jobs: new Building_Jobs(),
                production: new Building_Production(lumberjack1ID),
                prosperity: new Building_Prosperity(),
                priorityData: new Priority_Data_Building(lumberjack1ID)));

            return buildings;
        }
    }
}