using System.Collections.Generic;
using WorkPosts;

namespace Station
{
    public abstract class Station_List
    {
        static Dictionary<uint, Station_Data> _defaultStations;
        public static Dictionary<uint, Station_Data> DefaultStations => _defaultStations ??= _initialiseDefaultStations();
        
        static Dictionary<uint, Station_Data> _initialiseDefaultStations()
        {
            return new Dictionary<uint, Station_Data>
            {
                {
                    1, new Station_Data(
                        stationID: 1,
                        stationName: StationName.Tree,
                        stationDescription: "Station 1 Description",
                        jobSiteID: 1,
                        allWorkPost_Data: new Dictionary<uint, WorkPost_Data>())
                },
                {
                    2, new Station_Data(
                        stationID: 2,
                        stationName: StationName.Sawmill,
                        stationDescription: "Station 2 Description",
                        jobSiteID: 1,
                        allWorkPost_Data: new Dictionary<uint, WorkPost_Data>())
                },
                {
                    3, new Station_Data(
                        stationID: 3,
                        stationName: StationName.Log_Pile,
                        stationDescription: "Station 3 Description",
                        jobSiteID: 1,
                        allWorkPost_Data: new Dictionary<uint, WorkPost_Data>())
                }
            };
        }
    }
}