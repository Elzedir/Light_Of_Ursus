using System;
using System.Collections.Generic;
using System.Linq;
using ActorActions;
using Items;
using Station;
using UnityEngine;

namespace Priorities
{
    public abstract class Priority_Generator
    {
        const float _defaultMaxPriority = 10;

        static float _addPriorityIfAboveTarget(float current, float target, float maxPriority)
            => Math.Clamp(current - target, 0, maxPriority);

        static float _addPriorityIfBelowTarget(float current, float target, float maxPriority)
            => Math.Clamp(target - current, 0, maxPriority);

        static float _addPriorityIfNotEqualTarget(float current, float target, float maxPriority)
            => Math.Clamp(Math.Abs(current - target), 0, maxPriority);

        protected static float _addPriorityIfOutsideRange(float current, float min, float max, float maxPriority)
            => current > min || current < max
                ? 0
                : Math.Clamp(Math.Min(Math.Abs(current - min), Math.Abs(current - max)), 0, maxPriority);

        protected static float _addPriorityIfInsideRange(float current, float min, float max, float maxPriority)
            => current < min || current > max
                ? 0
                : Math.Clamp(Math.Max(Math.Abs(current - min), Math.Abs(current - max)), 0, maxPriority);
        
            //* Test these priority calculations.They don't seem right. Especially when total is 0...
        
        static float _addPriorityIfAbovePercent(float current, float total, float targetPercentage,
            float maxPriority)
            => total == 0 
                ? Math.Clamp((current - targetPercentage / 100) * maxPriority, 0, maxPriority) 
                : Math.Clamp((current / total - targetPercentage / 100) * maxPriority, 0, maxPriority);

        static float _addPriorityIfBelowPercent(float current, float total, float targetPercentage,
            float maxPriority)
            => total == 0
                ? Math.Clamp((targetPercentage / 100 - current) * maxPriority, 0, maxPriority)
                : Math.Clamp((targetPercentage / 100 - current / total) * maxPriority, 0, maxPriority);

        static float _addPriorityIfNotEqualPercent(float current, float total, float targetPercentage,
            float maxPriority)
            => total == 0
                ? Math.Clamp(Math.Abs(current - targetPercentage / 100) * maxPriority, 0, maxPriority)
                : Math.Clamp(Math.Abs(current / total - targetPercentage / 100) * maxPriority, 0, maxPriority);

        protected static float _addPriorityIfOutsidePercentRange(float current, float total, float min, float max,
            float maxPriority)
        {
            current = current / total * 100;
            return (current < min || current > max)
                ? Math.Clamp(Math.Min(Math.Abs(current - min), Math.Abs(current - max)), 0, maxPriority)
                : 0;
        }

        protected static float _addPriorityIfInsidePercentRange
            (float current, float total, float min, float max, float maxPriority)
        {
            current = current / total * 100;

            return (current >= min && current <= max)
                ? Math.Clamp(Math.Max(Math.Abs(current - min), Math.Abs(current - max)), 0, maxPriority)
                : 0;
        }

        protected static float _moreItemsDesired_Target(List<Item> items, float target, float maxPriority)
        {
            return _addPriorityIfBelowTarget(Item.GetItemListTotal_CountAllItems(items), target, maxPriority);
        }

        protected static float _lessItemsDesired_Target(List<Item> items, float target, float maxPriority)
        {
            return _addPriorityIfAboveTarget(Item.GetItemListTotal_CountAllItems(items), target, maxPriority);
        }

        protected static float _exactItemsDesired_Target(List<Item> items, float target, float maxPriority)
        {
            return _addPriorityIfNotEqualTarget(Item.GetItemListTotal_CountAllItems(items), target, maxPriority);
        }

        protected static float _moreItemsDesired_Percent(List<Item> items, float total, float maxPriority)
        {
            return _addPriorityIfAbovePercent(Item.GetItemListTotal_CountAllItems(items), total, 0, maxPriority);
        }

        protected static float _lessItemsDesired_Percent(List<Item> items, float total, float maxPriority)
        {
            return _addPriorityIfBelowPercent(Item.GetItemListTotal_CountAllItems(items), total, 100, maxPriority);
        }

        protected static float _exactItemsDesired_Percent(List<Item> items, float total, float maxPriority)
        {
            return _addPriorityIfNotEqualPercent(Item.GetItemListTotal_CountAllItems(items), total, 100, maxPriority);
        }

        protected static float _moreDistanceDesired_Target(Vector3 currentPosition, Vector3 targetPosition,
            float target, float maxPriority)
        {
            return _addPriorityIfBelowTarget(Vector3.Distance(currentPosition, targetPosition), target, maxPriority);
        }

        protected static float _lessDistanceDesired_Target(Vector3 currentPosition, Vector3 targetPosition,
            float target, float maxPriority)
        {
            return _addPriorityIfAboveTarget(Vector3.Distance(currentPosition, targetPosition), target, maxPriority);
        }

        protected static float _exactDistanceDesired_Target(Vector3 currentPosition, Vector3 targetPosition,
            float target, float maxPriority)
        {
            return _addPriorityIfNotEqualTarget(Vector3.Distance(currentPosition, targetPosition), target, maxPriority);
        }

        protected static float _moreDistanceDesired_Percent(Vector3 currentPosition, Vector3 targetPosition, float total,
            float maxPriority)
        {
            return _addPriorityIfAbovePercent(Vector3.Distance(currentPosition, targetPosition), total, 0, maxPriority);
        }

        protected static float _lessDistanceDesired_Percent(Vector3 currentPosition, Vector3 targetPosition, float total,
            float maxPriority)
        {
            return _addPriorityIfBelowPercent(Vector3.Distance(currentPosition, targetPosition), total, 100,
                maxPriority);
        }

        protected static float _exactDistanceDesired_Percent(Vector3 currentPosition, Vector3 targetPosition, float total,
            float maxPriority)
        {
            return _addPriorityIfNotEqualPercent(Vector3.Distance(currentPosition, targetPosition), total, 100,
                maxPriority);
        }

        public static float GeneratePriority(ulong priorityID, Priority_Parameters priority_Parameters)
        {
            if (priority_Parameters == null)
                throw new Exception("Priority_Parameters should not be null.");

            if (priority_Parameters.DefaultMaxPriority == 0)
                priority_Parameters.DefaultMaxPriority = _defaultMaxPriority;

            switch (priorityID)
            {
                case (ulong)ActorActionName.Idle:
                    return 1;
                case (ulong)ActorActionName.Haul:
                    return _generateHaulPriority(priority_Parameters);
                case (ulong)ActorActionName.Chop_Wood:
                    return _generateChop_WoodPriority(priority_Parameters);
                case (ulong)ActorActionName.Process_Logs:
                    return _generateProcess_LogsPriority(priority_Parameters);
                case (ulong)ActorActionName.Wander:
                    return 0;
                default:
                    Debug.LogError($"ActorAction: {(ActorActionName)priorityID} not found.");
                    return 0;
            }
        }

        static float _generateHaulPriority(Priority_Parameters priority_Parameters)
        {
            float highestPriority = 0;
            var allStationNames = priority_Parameters.JobSite_Component_Source.GetStationNames();
            
            if (priority_Parameters.Inventory_Hauler is null)
            {
                foreach (var stationToFetchFrom in priority_Parameters.AllStation_Sources)
                {
                    foreach (var stationToDeliverTo in priority_Parameters.AllStation_Targets)
                    {
                        var itemsToDeliver = stationToDeliverTo
                            .GetItemsToDeliverToThisStation(stationToFetchFrom.Station_Data.InventoryData);
                        
                        var correctItemsToDeliver = itemsToDeliver
                            .Where(item => new Item(item.Key, item.Value)
                                .Item_Data.ItemPriorityStats
                                .IsHighestPriorityStation(stationToDeliverTo.StationName, allStationNames))
                            .ToDictionary(x => x.Key, x => x.Value);
                        
                        if (correctItemsToDeliver.Count == 0) continue;
                        
                        priority_Parameters.Items = Item.GetListItemFromDictionary(correctItemsToDeliver);
                        priority_Parameters.Position_Source = stationToFetchFrom.transform.position;
                        priority_Parameters.Position_Destination = stationToDeliverTo.transform.position;
                        
                        var priority = GeneratePriority(priority_Parameters, Item.GetListItemFromDictionary(itemsToDeliver),
                            _moreItemsDesired_Percent, _lessDistanceDesired_Percent);

                        if (priority <= highestPriority) continue;
                        
                        highestPriority = priority;
                    }
                }
                
                return highestPriority;
            }
            
            foreach (var stationToFetchFrom in priority_Parameters.AllStation_Sources)
            {
                foreach (var stationToDeliverTo in priority_Parameters.AllStation_Targets)
                {
                    var itemsToDeliverFromActor = priority_Parameters.Inventory_Hauler
                        .GetItemsToDeliverFromThisActor(stationToDeliverTo.Station_Data.InventoryData);
                    
                    var itemsToDeliverFromStation = stationToDeliverTo
                        .GetItemsToDeliverToThisStation(stationToFetchFrom.Station_Data.InventoryData);

                    var itemsToDeliver = itemsToDeliverFromActor
                        .Concat(itemsToDeliverFromStation)
                        .ToDictionary(x => x.Key, x => x.Value);
                        
                    var correctItemsToDeliver = itemsToDeliver
                        .Where(item => new Item(item.Key, item.Value)
                            .Item_Data.ItemPriorityStats
                            .IsHighestPriorityStation(stationToDeliverTo.StationName, allStationNames))
                        .ToDictionary(x => x.Key, x => x.Value);
                        
                    if (correctItemsToDeliver.Count == 0) continue;
                        
                    priority_Parameters.Items = Item.GetListItemFromDictionary(correctItemsToDeliver);
                    priority_Parameters.Position_Source = stationToFetchFrom.transform.position;
                    priority_Parameters.Position_Destination = stationToDeliverTo.transform.position;
                        
                    var priority = GeneratePriority(priority_Parameters, Item.GetListItemFromDictionary(itemsToDeliver),
                        _moreItemsDesired_Percent, _lessDistanceDesired_Percent);

                    if (priority <= highestPriority) continue;
                        
                    highestPriority = priority;
                }
            }
                
            return highestPriority;
        }

        static float _generateChop_WoodPriority(Priority_Parameters priority_Parameters)
        {
            float highestPriority = 0;
            
            foreach(var station in priority_Parameters.AllStation_Sources)
            {
                if (station.StationName != StationName.Tree)
                {
                    Debug.LogError($"Station {station.StationName} is not a tree.");
                    continue;
                }
                
                priority_Parameters.Position_Source = priority_Parameters.Inventory_Hauler?.Reference.GameObject.transform.position ?? Vector3.zero;
                priority_Parameters.Position_Destination = station.transform.position;
                
                var priority = GeneratePriority(
                    priority_Parameters, 
                    new List<Item>(), 
                    _lessItemsDesired_Percent, 
                    _lessDistanceDesired_Percent);
                
                //* For now, temporary solution to reduce priority by the number of logs the character has.
                //* But later, we can change it to be a percentage of the total storage space, or a previously stated percentage value.

                if (priority_Parameters.Inventory_Hauler is not null
                    && priority_Parameters.Inventory_Hauler.AllInventoryItems.TryGetValue(1100, out var existingLogs))
                    priority -= Math.Max(0, existingLogs.ItemAmount);

                if (priority <= highestPriority) continue;
                
                highestPriority = priority;
            }
            
            return highestPriority;
        }

        static float _generateProcess_LogsPriority(Priority_Parameters priority_Parameters)
        {
            float highestPriority = 0;
            
            foreach (var station in priority_Parameters.AllStation_Sources)
            {
                if (station.StationName != StationName.Sawmill)
                {
                    Debug.LogError($"Station {station.StationName} is not a sawmill.");
                    continue;
                }
                
                priority_Parameters.Position_Source = station.transform.position;
                priority_Parameters.Position_Destination = priority_Parameters.Inventory_Hauler?.Reference.GameObject.transform.position ?? Vector3.zero;

                if (priority_Parameters.Inventory_Hauler is null) return 0;
                
                var itemsToProcess = Item.GetListItemFromDictionary(station.GetItemsToDeliverToThisStation(priority_Parameters.Inventory_Hauler));

                var priority = GeneratePriority(
                    priority_Parameters,
                    itemsToProcess,
                    _moreItemsDesired_Percent,
                    _lessDistanceDesired_Percent);
                
                if (priority <= highestPriority) continue;
                
                highestPriority = priority;
            }

            return highestPriority;
        }

        static float GeneratePriority(Priority_Parameters priority_Parameters, List<Item> allItems,
            Func<List<Item>, float, float, float> calculateItemPriority, Func<Vector3, Vector3, float, float, float> calculateDistancePriority)
        {
            return calculateItemPriority(
                       allItems, 
                       priority_Parameters.TotalItems, 
                       priority_Parameters.DefaultMaxPriority) 
                   + 
                   calculateDistancePriority(
                       priority_Parameters.Position_Source, 
                       priority_Parameters.Position_Destination,
                       priority_Parameters.TotalDistance,
                       priority_Parameters.DefaultMaxPriority);
        }
    }
}