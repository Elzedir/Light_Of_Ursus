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
                        allWorkPost_Data: new Dictionary<uint, WorkPost_Data>
                        {
                            {
                                1, new WorkPost_Data(
                                    workPostID: 1,
                                    stationID: 1)
                            },
                            {
                                2, new WorkPost_Data(
                                    workPostID: 2,
                                    stationID: 1)
                            },
                            {
                                3, new WorkPost_Data(
                                    workPostID: 3,
                                    stationID: 1)
                            },
                            {
                                4, new WorkPost_Data(
                                    workPostID: 4,
                                    stationID: 1)
                            }
                        })
                },
                {
                    2, new Station_Data(
                        stationID: 2,
                        stationName: StationName.Sawmill,
                        stationDescription: "Station 2 Description",
                        jobSiteID: 1,
                        allWorkPost_Data: new Dictionary<uint, WorkPost_Data>
                        {
                            {
                                5, new WorkPost_Data(
                                    workPostID: 5,
                                    stationID: 2)
                            },
                            {
                                6, new WorkPost_Data(
                                    workPostID: 6,
                                    stationID: 2)
                            },
                            {
                                7, new WorkPost_Data(
                                    workPostID: 7,
                                    stationID: 2)
                            },
                            {
                                8, new WorkPost_Data(
                                    workPostID: 8,
                                    stationID: 2)
                            }
                        })
                },
                {
                    3, new Station_Data(
                        stationID: 3,
                        stationName: StationName.Log_Pile,
                        stationDescription: "Station 3 Description",
                        jobSiteID: 1,
                        allWorkPost_Data: new Dictionary<uint, WorkPost_Data>
                        {
                            {
                                9, new WorkPost_Data(
                                    workPostID: 9,
                                    stationID: 3)
                            },
                            {
                                10, new WorkPost_Data(
                                    workPostID: 10,
                                    stationID: 3)
                            },
                            {
                                11, new WorkPost_Data(
                                    workPostID: 11,
                                    stationID: 3)
                            },
                            {
                                12, new WorkPost_Data(
                                    workPostID: 12,
                                    stationID: 3)
                            }
                        })
                }
            };
        }
    }
}