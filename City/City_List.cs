using System.Collections.Generic;
using Managers;

namespace City
{
    public abstract class City_List
    {
        static Dictionary<uint, City_Data> _defaultCities;
        public static Dictionary<uint, City_Data> DefaultCities => _defaultCities ??= _initialiseDefaultCities();
        
        static Dictionary<uint, City_Data> _initialiseDefaultCities()
        {
            return new Dictionary<uint, City_Data>
            {
                {
                    1, new City_Data
                    (
                        cityID: 1,
                        cityName: "Test City",
                        cityDescription: "City 1 Description",
                        cityFactionID: 1,
                        regionID: 1,
                        allJobSiteIDs: new List<uint>
                        {
                            1
                        },
                        population: new PopulationData(
                            allCitizenIDList: new List<uint>(),
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