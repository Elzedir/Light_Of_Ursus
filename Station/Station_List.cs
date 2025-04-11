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
                        id: 1,
                        stationName: StationName.Tree,
                        buildingID: 1)
                },
                {
                    2, new Station_Data(
                        id: 2,
                        stationName: StationName.Sawmill,
                        buildingID: 1)
                },
                {
                    3, new Station_Data(
                        id: 3,
                        stationName: StationName.Log_Pile,
                        buildingID: 1)
                },
                {
                    4, new Station_Data(
                        id: 4,
                        stationName: StationName.IdleTemp,
                        buildingID: 1)
                }
            };
        }
    }
}