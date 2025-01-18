using System.Collections.Generic;
using Priority;
using Station;
using Tools;
using UnityEngine;

namespace Items
{
    public class Item_PriorityStats : Data_Class
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

        public override Dictionary<string, string> GetStringData()
        {
            return new Dictionary<string, string>
            {
                {"Priority Stations", Priority_Stations.ToString()}
            };
        }

        public override DataToDisplay GetDataToDisplay(bool toggleMissingDataDebugs)
        {
            _updateDataDisplay(DataToDisplay,
                title: "Priority Stations",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: new Dictionary<string, string>
                {
                    {"Priority Stations", Priority_Stations.ToString()}
                });

            return DataToDisplay;
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