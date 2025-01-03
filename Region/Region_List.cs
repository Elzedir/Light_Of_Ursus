using System.Collections.Generic;
using Managers;

namespace Region
{
    public abstract class Region_List
    {
        static Dictionary<uint, Region_Data> _defaultRegions;
        public static Dictionary<uint, Region_Data> DefaultRegions => _defaultRegions ??= _initialiseDefaultRegions();
        
        static Dictionary<uint, Region_Data> _initialiseDefaultRegions()
        {
            return new Dictionary<uint, Region_Data>
            {
                {
                    1, new Region_Data(
                        regionID: 1,
                        regionName: "The Heartlands",
                        regionDescription: "The land of hearts",
                        regionFactionID: 0,
                        allCityIDs: new List<uint>
                        {
                            1
                        },
                        prosperityData: new ProsperityData(
                            currentProsperity: 50,
                            maxProsperity: 100,
                            baseProsperityGrowthPerDay: 1))
                }
            };
        }
    }
}