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
                    {
                        CityID = 1,
                        CityName = "Test City",
                        CityFactionID = 1,
                        RegionID = 1,
                        CityDescription = "City 1 Description",
                        Population = new PopulationData(
                            allCitizenIDList: new List<uint>(),
                            maxPopulation: 100,
                            expectedPopulation: 50),
                        ProsperityData = new ProsperityData(
                            currentProsperity: 50,
                            maxProsperity: 100,
                            baseProsperityGrowthPerDay: 1),
                        AllJobsiteIDs = new List<uint>()
                    }
                }
            };
    }
}