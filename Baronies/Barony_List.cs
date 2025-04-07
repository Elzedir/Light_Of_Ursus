using System.Collections.Generic;
using Managers;

namespace Cities
{
    public abstract class Barony_List
    {
        static Dictionary<ulong, Barony_Data> s_defaultCities;
        public static Dictionary<ulong, Barony_Data> DefaultCities => s_defaultCities ??= _initialiseDefaultCities();
        
        static Dictionary<ulong, Barony_Data> _initialiseDefaultCities()
        {
            return new Dictionary<ulong, Barony_Data>
            {
                {
                    1, new Barony_Data
                    (
                        id: 1,
                        name: "The Heartlands",
                        description: "The land of hearts",
                        factionID: 1,
                        regionID: 1,
                        allBuildingIDs: new List<ulong>
                        {
                            1
                        },
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