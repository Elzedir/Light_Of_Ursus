using System.Collections.Generic;
using JobSite;

namespace Station
{
    public abstract class Station_List
    {
        public static readonly Dictionary<uint, Station_Data> DefaultStations = new()
        {
            {
                1, new Station_Data(
                    stationID: 1,
                    stationName: StationName.Tree,
                    stationDescription: "Station 1 Description",
                    jobsiteID: 1)
            },
            {
                2, new Station_Data(
                    stationID: 2,
                    stationName: StationName.Sawmill,
                    stationDescription: "Station 2 Description",
                    jobsiteID: 1)
            },
            {
                3, new Station_Data(
                    stationID: 3,
                    stationName: StationName.Log_Pile,
                    stationDescription: "Station 3 Description",
                    jobsiteID: 1)
            }
        };
    }
}