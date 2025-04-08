using System.Collections.Generic;
using Cities;

namespace Baronies
{
    public abstract class Barony_List
    {
        static Dictionary<ulong, Barony_Data> s_preExistingBaronies;
        public static Dictionary<ulong, Barony_Data> S_PreExistingBaronies => s_preExistingBaronies ??= _initialisePreExistingBaronies();

        static Dictionary<ulong, Barony_Data> _initialisePreExistingBaronies()
        {
            return new Dictionary<ulong, Barony_Data>
            {
                {
                    1, new Barony_Data
                    (
                        id: 1,
                        type: BaronyType.City,
                        name: "The Heartlands",
                        description: "The land of hearts",
                        countyID: 1,
                        buildings: new Barony_BuildingData(),
                        population: new Barony_PopulationData(
                            allCitizenIDList: new List<ulong>(),
                            maxPopulation: 100,
                            expectedPopulation: 50),
                        baronyProsperityData: new Barony_ProsperityData(
                            currentProsperity: 50,
                            maxProsperity: 100,
                            baseProsperityGrowthPerDay: 1)
                    )
                }
            };
        }
    }
}