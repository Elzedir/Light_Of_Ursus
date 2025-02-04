using System.Collections.Generic;

namespace Station
{
    public abstract class Station_List
    {
        static Dictionary<ulong, Station_Data> s_defaultStations;
        public static Dictionary<ulong, Station_Data> S_DefaultStations => s_defaultStations ??= _initialiseDefaultStations();
        
        static Dictionary<ulong, Station_Data> _initialiseDefaultStations()
        {
            return new Dictionary<ulong, Station_Data>
            {
                {
                    1, new Station_Data(
                        stationID: 1,
                        stationName: StationName.Tree,
                        jobSiteID: 1)
                },
                {
                    2, new Station_Data(
                        stationID: 2,
                        stationName: StationName.Sawmill,
                        jobSiteID: 1)
                },
                {
                    3, new Station_Data(
                        stationID: 3,
                        stationName: StationName.Log_Pile,
                        jobSiteID: 1)
                }
            };
        }
    }
}