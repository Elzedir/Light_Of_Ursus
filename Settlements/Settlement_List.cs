using System.Collections.Generic;
using Settlements;
using Cities;

namespace Settlements
{
    public abstract class Settlement_List
    {
        static Dictionary<ulong, Settlement_Data> s_preExistingSettlements;
        public static Dictionary<ulong, Settlement_Data> S_PreExistingSettlements => s_preExistingSettlements ??= _initialisePreExistingSettlements();

        static Dictionary<ulong, Settlement_Data> _initialisePreExistingSettlements()
        {
            return new Dictionary<ulong, Settlement_Data>
            {
                {
                    1, new Settlement_Data
                    (
                        id: 1,
                        type: SettlementType.City,
                        name: "The Heartlands",
                        description: "The land of hearts",
                        baronyID: 1,
                        buildings: new Settlement_Buildings(),
                        population: new Settlement_Population(
                            allCitizenIDList: new List<ulong>(),
                            maxPopulation: 100,
                            expectedPopulation: 50),
                        settlementProsperity: new Settlement_Prosperity(
                            currentProsperity: 50,
                            maxProsperity: 100,
                            baseProsperityGrowthPerDay: 1)
                    )
                }
            };
        }
    }
}