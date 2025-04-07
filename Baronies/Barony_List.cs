using System.Collections.Generic;
using Managers;

namespace Cities
{
    public abstract class City_List
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
                        name: "Test City",
                        description: "City 1 Description",
                        factionID: 1,
                        regionID: 1,
                        allBuildingIDs: new List<ulong>
                        {
                            1
                        },
                        population: new City_PopulationData(
                            allCitizenIDList: new List<ulong>(),
                            maxPopulation: 100,
                            expectedPopulation: 50),
                        cityProsperityData: new City_ProsperityData(
                            currentProsperity: 50,
                            maxProsperity: 100,
                            baseProsperityGrowthPerDay: 1)
                    )
                }
            };
        }
    }
    
    public class Barony_ListChange
    {
        static Dictionary<BaronyName, Barony_Data> s_defaultCounties;
        public static Dictionary<BaronyName, Barony_Data> DefaultCounties => s_defaultCounties ??= _initialiseDefaultCounties();
        
        static Dictionary<BaronyName, Barony_Data> _initialiseDefaultCounties()
        {
            return new Dictionary<BaronyName, Barony_Data>
            {
                {
                    BaronyName.Hamlet, new Barony_Data(
                        BaronyID: 1,
                        BaronyName: "The Heartlands",
                        BaronyDescription: "The land of hearts",
                        BaronyFactionID: 0,
                        allCityIDs: new List<ulong>
                        {
                            1
                        })
                }
            };
        }
}