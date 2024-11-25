using System;
using System.Collections.Generic;
using System.Linq;
using Actors;
using Debuggers;
using Items;
using Managers;
using Station;
using UnityEngine;

namespace Priority
{
    public abstract class PriorityGenerator
    {

        // Create a dictionary that contains the action name and a list of priority parameters. This will contiain the "existing priorities" that will be updated accordingly, so that we don't have to add and subract, instead just replace the existing priorities according to the conditions that are met or not met. That way we can simply add the actor distance to the slot and it will automatically update the priority by including it in its calculations.

        static float _defaultMaxPriority => 10;

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

        public static List<float> GeneratePriorities(ActorActionName actorActionName, Dictionary<PriorityParameterName, object> existingPriorityParameters)
        {
            if (existingPriorityParameters == null)
            {
                Debug.LogError($"ActionName: {actorActionName} not found in _actionPriorityParameters.");
                return null;
            }

            switch (actorActionName)
            {
                case ActorActionName.Fetch:
                    return _generateFetchPriority(existingPriorityParameters) ?? new List<float>();
                case ActorActionName.Deliver:
                    return _generateDeliverPriority(existingPriorityParameters) ?? new List<float>();
                default:
                    Debug.LogError($"ActionName: {actorActionName} not found.");
                    return null;
            }
        }

        static List<float> _generateFetchPriority(Dictionary<PriorityParameterName, object> existingPriorityParameters)
        {
            float         maxPriority      = existingPriorityParameters[PriorityParameterName.MaxPriority] as float?   ?? _defaultMaxPriority;
            float         totalDistance    = existingPriorityParameters[PriorityParameterName.TotalDistance] as float? ?? 0;
            float         totalItems       = existingPriorityParameters[PriorityParameterName.TotalItems] as float?    ?? 0;
            InventoryData inventory_Hauler = existingPriorityParameters[PriorityParameterName.InventoryHauler] as InventoryData;
            InventoryData inventory_Target = existingPriorityParameters[PriorityParameterName.InventoryTarget] as InventoryData;

            if (maxPriority == 0)
            {
                Debug.LogError("MaxPriority is 0. Default initialiser failed.");
                return null;
            }

            if (totalItems == 0 && totalDistance == 0)
            {
                Debug.LogError($"MaxItems and MaxDistance are 0.");
                return new List<float> { 0 };
            }

            if (inventory_Hauler == null || inventory_Target == null)
            {
                Debug.LogError($"Inventory_Hauler {inventory_Hauler} or Inventory_Target: {inventory_Target} is null.");
                return new List<float> { 0 };
            }

            var allItemsToFetch = inventory_Target.GetInventoryItemsToFetch();

            if (Item.GetItemListTotal_CountAllItems(allItemsToFetch) == 0)
            {
                Debug.Log("No items to fetch.");
                return new List<float> { 0 };
            }

            var haulerPosition = inventory_Hauler.Reference.GameObject.transform.position;
            var targetPosition = inventory_Target.Reference.GameObject.transform.position;

            var priority_ItemQuantity = allItemsToFetch.Count != 0
                ? _moreItemsDesired_Total(allItemsToFetch, totalItems, maxPriority) : 0;

            var priority_Distance = haulerPosition != Vector3.zero && targetPosition != Vector3.zero
                ? _lessDistanceDesired_Total(haulerPosition, targetPosition, totalDistance, maxPriority) : 0;

            DebugEntry_Data debugDataList = new DebugEntry_Data
            (
                new DebugEntryKey
                (
                    "Fetch",
                    $"{inventory_Target.ComponentType}",
                    inventory_Target.Reference.ComponentID
                ), 
                new List<DebugData_Data>
                { 
                    new DebugData_Data (DebugDataType.Priority_Item,     priority_ItemQuantity.ToString()),
                    new DebugData_Data (DebugDataType.Priority_Distance, priority_Distance.ToString()),
                    new DebugData_Data (DebugDataType.Priority_Total,    (priority_ItemQuantity + priority_Distance).ToString())
                }
            );
        
            DebugVisualiser.Instance.UpdateDebugEntry(DebugSectionType.Hauling, debugDataList);

            return new List<float>
            {
                priority_ItemQuantity + priority_Distance
            };
        }

        static List<float> _generateDeliverPriority(Dictionary<PriorityParameterName, object> existingPriorityParameters)
        {
            var         maxPriority      = existingPriorityParameters[PriorityParameterName.MaxPriority] as float?   ?? _defaultMaxPriority;
            var         totalDistance    = existingPriorityParameters[PriorityParameterName.TotalDistance] as float? ?? 0;
            var         totalItems       = existingPriorityParameters[PriorityParameterName.TotalItems] as float?    ?? 0;
            var inventory_Hauler = existingPriorityParameters[PriorityParameterName.InventoryHauler] as InventoryData;
            var inventory_Target = existingPriorityParameters[PriorityParameterName.InventoryTarget] as InventoryData;

            StationName currentStationType = existingPriorityParameters.TryGetValue(PriorityParameterName.CurrentStationType, out var stationType)
                ? stationType as StationName? ?? StationName.None
                : StationName.None;

            HashSet<StationName> allStationTypes = existingPriorityParameters.TryGetValue(PriorityParameterName.AllStationTypes, out var stationTypes)
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
                return new List<float> { 0 };
            }

            if (inventory_Hauler == null || inventory_Target == null)
            {
                Debug.LogError($"Inventory_Hauler {inventory_Hauler} or Inventory_Target: {inventory_Target} is null.");
                return new List<float> { 0 };
            }

            var allItemsToDeliver = inventory_Target.GetInventoryItemsToDeliver(inventory_Hauler);

            if (allItemsToDeliver.Count == 0)
            {
                Debug.Log("No items to fetch.");
                return new List<float> { 0 };
            }

            var haulerPosition = inventory_Hauler.Reference.GameObject.transform.position;
            var targetPosition = inventory_Target.Reference.GameObject.transform.position;

            var priority_ItemQuantity = allItemsToDeliver.Count != 0
                ? _moreItemsDesired_Total(allItemsToDeliver, totalItems, maxPriority, currentStationType, allStationTypes) : 0;

            var priority_Distance = haulerPosition != Vector3.zero && targetPosition != Vector3.zero
                ? _lessDistanceDesired_Total(haulerPosition, targetPosition, totalDistance, maxPriority) : 0;
        
            DebugEntry_Data debugDataList = new DebugEntry_Data
            (
                new DebugEntryKey
                (
                    "Deliver",
                    $"{inventory_Target.ComponentType}",
                    inventory_Target.Reference.ComponentID
                ), 
                new List<DebugData_Data>
                { 
                    new DebugData_Data (DebugDataType.Priority_Item,     priority_ItemQuantity.ToString()) ,
                    new DebugData_Data (DebugDataType.Priority_Distance, priority_Distance.ToString()) ,
                    new DebugData_Data (DebugDataType.Priority_Total,    (priority_ItemQuantity + priority_Distance).ToString()) 
                }
            );

            DebugVisualiser.Instance.UpdateDebugEntry(DebugSectionType.Hauling, debugDataList);

            return new List<float>
            {
                priority_ItemQuantity + priority_Distance
            };
        }
    }
}