using System.Collections.Generic;
using Managers;

namespace City
{
    public abstract class City_List
    {
        static Dictionary<ulong, City_Data> _defaultCities;
        public static Dictionary<ulong, City_Data> DefaultCities => _defaultCities ??= _initialiseDefaultCities();
        
        static Dictionary<ulong, City_Data> _initialiseDefaultCities()
        {
            return new Dictionary<ulong, City_Data>
            {
                {
                    1, new City_Data
                    (
                        cityID: 1,
                        cityName: "Test City",
                        cityDescription: "City 1 Description",
                        cityFactionID: 1,
                        regionID: 1,
                        allJobSiteIDs: new List<ulong>
                        {
                            1
                        },
                        population: new PopulationData(
                            allCitizenIDList: new List<ulong>(),
                            maxPopulation: 100,
                            expectedPopulation: 50),
                        prosperityData: new ProsperityData(
                            currentProsperity: 50,
                            maxProsperity: 100,
                            baseProsperityGrowthPerDay: 1)
                    )
                }
            };
        }
    }
}