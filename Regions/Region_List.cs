using System.Collections.Generic;
using Managers;
using Regions;

namespace Region
{
    public abstract class Region_List
    {
        static Dictionary<ulong, Region_Data> _defaultRegions;
        public static Dictionary<ulong, Region_Data> DefaultRegions => _defaultRegions ??= _initialiseDefaultRegions();
        
        static Dictionary<ulong, Region_Data> _initialiseDefaultRegions()
        {
            return new Dictionary<ulong, Region_Data>
            {
                {
                    1, new Region_Data(
                        regionID: 1,
                        regionName: "The Heartlands",
                        regionDescription: "The land of hearts",
                        regionFactionID: 0,
                        allCityIDs: new List<ulong>
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