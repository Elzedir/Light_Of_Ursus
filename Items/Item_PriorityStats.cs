using System.Collections.Generic;
using Priority;
using Station;
using UnityEngine;

namespace Items
{
    public class Item_PriorityStats
    {
        public readonly Dictionary<PriorityImportance, List<StationName>> Priority_Stations;

        public Item_PriorityStats(Dictionary<PriorityImportance, List<StationName>> priority_Station = null)
        {
            Priority_Stations = priority_Station != null
                ? new Dictionary<PriorityImportance, List<StationName>>(priority_Station)
                : new Dictionary<PriorityImportance, List<StationName>>();
        }

        public Item_PriorityStats(Item_PriorityStats other)
        {
            Priority_Stations = other.Priority_Stations != null
                ? new Dictionary<PriorityImportance, List<StationName>>(other.Priority_Stations)
                : new Dictionary<PriorityImportance, List<StationName>>();
        }

        public PriorityImportance GetHighestStationPriority(List<StationName> allStations)
        {
            foreach (var priority in Priority_Stations.Keys)
            {
                foreach (var station in allStations)
                {
                    if (Priority_Stations[priority].Contains(station))
                    {
                        return priority;
                    }
                }

                Debug.Log($"Priority: {priority} not found in Priority_StationsForProduction");
            }

            Debug.LogError("No priority found for any station in Priority_StationsForProduction");

            return PriorityImportance.None;
        }

        public bool IsHighestPriorityStation(StationName currentStation, List<StationName> allStations)
        {
            PriorityImportance highestPriority = GetHighestStationPriority(allStations);

            return Priority_Stations[highestPriority].Contains(currentStation);
        }
    }
}