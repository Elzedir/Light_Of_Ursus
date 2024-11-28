using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Actors;
using Debuggers;
using Inventory;
using Items;
using Jobs;
using Managers;
using Station;
using UnityEngine;

namespace Priority
{
    public class PriorityGenerator
    {
        protected static float _defaultMaxPriority => 10;

        protected static float _addPriorityIfAboveTarget(float current, float target, float maxPriority)
            => Math.Clamp(current - target, 0, maxPriority);
        protected static float _addPriorityIfBelowTarget(float current, float target, float maxPriority)
            => Math.Clamp(target - current, 0, maxPriority);
        protected static float _addPriorityIfNotEqualTarget(float current, float target, float maxPriority)
            => Math.Clamp(Math.Abs(current - target), 0, maxPriority);

        protected static float _addPriorityIfOutsideRange(float current, float min, float max, float maxPriority)
            => (current < min || current > max) ? Math.Clamp(Math.Min(Math.Abs(current - min), Math.Abs(current - max)), 0, maxPriority) : 0;
        protected static float _addPriorityIfInsideRange(float current, float min, float max, float maxPriority)
            => (current > min || current < max) ? Math.Clamp(Math.Max(Math.Abs(current - min), Math.Abs(current - max)), 0, maxPriority) : 0;

        protected static float _addPriorityIfAbovePercent(float current, float total, float targetPercentage, float maxPriority)
            => Math.Clamp((current / total - targetPercentage / 100) * maxPriority, 0, maxPriority);
        protected static float _addPriorityIfBelowPercent(float current, float total, float targetPercentage, float maxPriority)
            => Math.Clamp((targetPercentage / 100 - current / total) * maxPriority, 0, maxPriority);
        protected static float _addPriorityIfNotEqualPercent(float current, float total, float targetPercentage, float maxPriority)
            => Math.Clamp(Math.Abs(current / total - targetPercentage / 100) * maxPriority, 0, maxPriority);

        protected static float _addPriorityIfOutsidePercentRange(float current, float total, float min, float max, float maxPriority)
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

        protected static float _moreItemsDesired_Total(List<Item> items, float total, float maxPriority, StationName currentStationType = StationName.None, HashSet<StationName> allStationTypes = null)
        {        
            if (allStationTypes == null)
            {
                return _addPriorityIfAbovePercent(Item.GetItemListTotal_CountAllItems(items), total, 0, maxPriority);
            }

            var priority = 0f;

            foreach(var item in items)
            {
                var masterItem = Items.Items.GetItem_Master(item.ItemID);

                var allStationTypesList = allStationTypes.ToList();

                if (!masterItem.PriorityStats_Item.IsHighestPriorityStation(currentStationType, allStationTypesList))
                {
                    // Debug.Log($"StationType: {currentStationType} is not the highest priority station type for item: {item.ItemName}");
                    continue;
                }
            
                priority += _addPriorityIfAbovePercent(item.ItemAmount, total, 0, maxPriority);
            }

            return priority;
        }

        protected static float _lessItemsDesired_Total(List<Item> items, float total, float maxPriority)
        {
            return _addPriorityIfBelowPercent(Item.GetItemListTotal_CountAllItems(items), total, 100, maxPriority);
        }

        protected static float _exactItemsDesired_Total(List<Item> items, float total, float maxPriority)
        {
            return _addPriorityIfNotEqualPercent(Item.GetItemListTotal_CountAllItems(items), total, 100, maxPriority);
        }

        protected static float _moreDistanceDesired_Target(Vector3 currentPosition, Vector3 targetPosition, float target, float maxPriority)
        {
            return _addPriorityIfBelowTarget(Vector3.Distance(currentPosition, targetPosition), target, maxPriority);
        }

        protected static float _lessDistanceDesired_Target(Vector3 currentPosition, Vector3 targetPosition, float target, float maxPriority)
        {
            return _addPriorityIfAboveTarget(Vector3.Distance(currentPosition, targetPosition), target, maxPriority);
        }

        protected static float _exactDistanceDesired_Target(Vector3 currentPosition, Vector3 targetPosition, float target, float maxPriority)
        {
            return _addPriorityIfNotEqualTarget(Vector3.Distance(currentPosition, targetPosition), target, maxPriority);
        }

        protected static float _moreDistanceDesired_Total(Vector3 currentPosition, Vector3 targetPosition, float total, float maxPriority)
        {
            return _addPriorityIfAbovePercent(Vector3.Distance(currentPosition, targetPosition), total, 0, maxPriority);
        }

        protected static float _lessDistanceDesired_Total(Vector3 currentPosition, Vector3 targetPosition, float total, float maxPriority)
        {
            return _addPriorityIfBelowPercent(Vector3.Distance(currentPosition, targetPosition), total, 100, maxPriority);
        }

        protected static float _exactDistanceDesired_Total(Vector3 currentPosition, Vector3 targetPosition, float total, float maxPriority)
        {
            return _addPriorityIfNotEqualPercent(Vector3.Distance(currentPosition, targetPosition), total, 100, maxPriority);
        }

        public static Dictionary<PriorityParameterName, float> GeneratePriority(
            PriorityType priorityType, uint priorityQueueID,
            Dictionary<PriorityParameterName, object>
                existingPriorityParameters) =>
            _generatePriority(priorityType, priorityQueueID, existingPriorityParameters?
                                                             .Select(x => x)
                                                             .ToDictionary(x =>
                                                                 (uint)x.Key, x => x.Value));

        static Dictionary<PriorityParameterName, float> _generatePriority(
            PriorityType priorityType, uint priorityID,
            Dictionary<uint, object>
                existingPriorityParameters)
        {
            if (existingPriorityParameters == null)
            {
                Debug.LogError("ExistingPriorityParameters is null.");
                return null;
            }
            
            switch (priorityType)
            {
                case PriorityType.ActorAction:
                    switch (priorityID)
                    {
                        case (uint)ActorActionName.Fetch:
                            return _generateFetchPriority(existingPriorityParameters) ??
                                   new Dictionary<PriorityParameterName, float>();
                        case (uint)ActorActionName.Deliver:
                            return _generateDeliverPriority(existingPriorityParameters) ??
                                   new Dictionary<PriorityParameterName, float>();
                        default:
                            Debug.LogError($"PriorityID: {priorityID} not found.");
                            return null;
                    }
                case PriorityType.JobTask:
                    switch (priorityID)
                    {
                        case (uint)JobTaskName.Fetch_Items:
                            return _generateStockpilePriority(existingPriorityParameters) ??
                                   new Dictionary<PriorityParameterName, float>();
                        default:
                            Debug.LogError($"PriorityID: {priorityID} not found.");
                            return null;
                    }
                default:
                    Debug.LogError($"PriorityType: {priorityType} not found.");
                    return null;
            }
        }

        static Dictionary<PriorityParameterName, float> _generateFetchPriority(Dictionary<uint, object> existingPriorityParameters)
        {
            var maxPriority = existingPriorityParameters[(uint)PriorityParameterName.DefaultPriority] as float? ??
                              _defaultMaxPriority;
            var totalDistance = existingPriorityParameters[(uint)PriorityParameterName.TotalDistance] as float? ?? 0;
            var totalItems    = existingPriorityParameters[(uint)PriorityParameterName.TotalItems] as float?    ?? 0;
            var inventory_Hauler =
                existingPriorityParameters[(uint)PriorityParameterName.InventoryHauler] as InventoryData;
            var inventory_Target =
                existingPriorityParameters[(uint)PriorityParameterName.InventoryTarget] as InventoryData;

            if (maxPriority == 0)
            {
                Debug.LogError("MaxPriority is 0. Default initialiser failed.");
                return null;
            }

            if (totalItems == 0 && totalDistance == 0)
            {
                Debug.LogError($"MaxItems and MaxDistance are 0.");
                return new Dictionary<PriorityParameterName, float>();
            }

            if (inventory_Hauler == null || inventory_Target == null)
            {
                Debug.LogError($"Inventory_Hauler {inventory_Hauler} or Inventory_Target: {inventory_Target} is null.");
                return new Dictionary<PriorityParameterName, float>();
            }

            var allItemsToFetch = inventory_Target.GetInventoryItemsToFetch();

            if (Item.GetItemListTotal_CountAllItems(allItemsToFetch) == 0)
            {
                Debug.Log("No items to fetch.");
                return new Dictionary<PriorityParameterName, float>();
            }

            var haulerPosition = inventory_Hauler.Reference.GameObject.transform.position;
            var targetPosition = inventory_Target.Reference.GameObject.transform.position;

            var priority_ItemQuantity = allItemsToFetch.Count != 0
                ? _moreItemsDesired_Total(allItemsToFetch, totalItems, maxPriority)
                : 0;

            var priority_Distance = haulerPosition != Vector3.zero && targetPosition != Vector3.zero
                ? _lessDistanceDesired_Total(haulerPosition, targetPosition, totalDistance, maxPriority)
                : 0;

            var debugDataList = new DebugEntry_Data
            (
                new DebugEntryKey
                (
                    "Fetch",
                    $"{inventory_Target.ComponentType}",
                    inventory_Target.Reference.ComponentID
                ),
                new List<DebugData_Data>
                {
                    new(DebugDataType.Priority_Item,
                        priority_ItemQuantity.ToString(CultureInfo.InvariantCulture)),
                    new(DebugDataType.Priority_Distance,
                        priority_Distance.ToString(CultureInfo.InvariantCulture)),
                    new(DebugDataType.Priority_Total,
                        (priority_ItemQuantity + priority_Distance).ToString(CultureInfo.InvariantCulture))
                }
            );

            DebugVisualiser.Instance.UpdateDebugEntry(DebugSectionType.Hauling, debugDataList);

            return new Dictionary<PriorityParameterName, float>
            {
                {
                    PriorityParameterName.TotalItems, priority_ItemQuantity
                },
                
                {
                    PriorityParameterName.TotalDistance, priority_Distance    
                }
            };
        }

        static Dictionary<PriorityParameterName, float> _generateDeliverPriority(Dictionary<uint, object> existingPriorityParameters)
        {
            var maxPriority = existingPriorityParameters[(uint)PriorityParameterName.DefaultPriority] as float? ??
                              _defaultMaxPriority;
            var totalDistance = existingPriorityParameters[(uint)PriorityParameterName.TotalDistance] as float? ?? 0;
            var totalItems    = existingPriorityParameters[(uint)PriorityParameterName.TotalItems] as float?    ?? 0;
            var inventory_Hauler =
                existingPriorityParameters[(uint)PriorityParameterName.InventoryHauler] as InventoryData;
            var inventory_Target =
                existingPriorityParameters[(uint)PriorityParameterName.InventoryTarget] as InventoryData;

            var currentStationType =
                existingPriorityParameters.TryGetValue((uint)PriorityParameterName.CurrentStationType,
                    out var stationType)
                    ? stationType as StationName? ?? StationName.None
                    : StationName.None;

            var allStationTypes =
                existingPriorityParameters.TryGetValue((uint)PriorityParameterName.AllStationTypes, out var stationTypes)
                    ? stationTypes as HashSet<StationName>
                    : null;

            if (maxPriority == 0)
            {
                Debug.LogError("MaxPriority is 0. Default initialiser failed.");
                return null;
            }

            if (totalItems == 0 && totalDistance == 0)
            {
                Debug.LogError($"MaxItems and MaxDistance are 0.");
                return new Dictionary<PriorityParameterName, float>();
            }

            if (inventory_Hauler == null || inventory_Target == null)
            {
                Debug.LogError($"Inventory_Hauler {inventory_Hauler} or Inventory_Target: {inventory_Target} is null.");
                return new Dictionary<PriorityParameterName, float>();
            }

            var allItemsToDeliver = inventory_Target.GetInventoryItemsToDeliver(inventory_Hauler);

            if (allItemsToDeliver.Count == 0)
            {
                Debug.Log("No items to fetch.");
                return new Dictionary<PriorityParameterName, float>();
            }

            var haulerPosition = inventory_Hauler.Reference.GameObject.transform.position;
            var targetPosition = inventory_Target.Reference.GameObject.transform.position;

            var priority_ItemQuantity = allItemsToDeliver.Count != 0
                ? _moreItemsDesired_Total(allItemsToDeliver, totalItems, maxPriority, currentStationType,
                    allStationTypes)
                : 0;

            var priority_Distance = haulerPosition != Vector3.zero && targetPosition != Vector3.zero
                ? _lessDistanceDesired_Total(haulerPosition, targetPosition, totalDistance, maxPriority)
                : 0;

            var debugDataList = new DebugEntry_Data
            (
                new DebugEntryKey
                (
                    "Deliver",
                    $"{inventory_Target.ComponentType}",
                    inventory_Target.Reference.ComponentID
                ),
                new List<DebugData_Data>
                {
                    new(DebugDataType.Priority_Item,     priority_ItemQuantity.ToString(CultureInfo.InvariantCulture)),
                    new(DebugDataType.Priority_Distance, priority_Distance.ToString(CultureInfo.InvariantCulture)),
                    new(DebugDataType.Priority_Total,
                        (priority_ItemQuantity + priority_Distance).ToString(CultureInfo.InvariantCulture))
                }
            );

            DebugVisualiser.Instance.UpdateDebugEntry(DebugSectionType.Hauling, debugDataList);

            return new Dictionary<PriorityParameterName, float>
            {
                {
                    PriorityParameterName.TotalItems, priority_ItemQuantity
                },
                
                {
                    PriorityParameterName.TotalDistance, priority_Distance    
                }
            };
        }

        static Dictionary<PriorityParameterName, float> _generateStockpilePriority(Dictionary<uint, object> existingPriorityParameters)
        {
            return new Dictionary<PriorityParameterName, float>()
            {
                {
                    PriorityParameterName.DefaultPriority, 1
                }
            };
        }
    }
    
    public enum PriorityType
    {
        ActorAction,
        JobTask,
    }
}