using System.Collections.Generic;
using Managers;

namespace Region
{
    public abstract class Region_List
    {
        public static readonly Dictionary<uint, Region_Data> DefaultRegions = new()
        {
            {
                1, new Region_Data(
                    regionID: 1,
                    regionName: "The Heartlands",
                    regionDescription: "The land of hearts",
                    regionFactionID: 0,
                    prosperityData: new ProsperityData(
                        currentProsperity: 50, 
                        maxProsperity: 100,
                        baseProsperityGrowthPerDay: 1))
            }
        };
    }
}