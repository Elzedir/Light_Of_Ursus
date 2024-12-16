using System.Collections.Generic;
using Managers;

namespace City
{
    public abstract class City_List
    {
        public static readonly Dictionary<uint, City_Data> DefaultCities =
            new()
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