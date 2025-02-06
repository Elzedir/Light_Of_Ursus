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

        protected static float _moreItemsDesired_Total(List<Item> items, float total, float maxPriority,
            StationName currentStationType = StationName.None,
            HashSet<StationName> allStationTypes = null)
        {
            if (allStationTypes == null)
            {
                return _addPriorityIfAbovePercent(Item.GetItemListTotal_CountAllItems(items), total, 0, maxPriority);
            }

            var priority = 0f;

            foreach (var item in items)
            {
                var masterItem = Item_Manager.GetItem_Data(item.ItemID);

                var allStationTypesList = allStationTypes.ToList();

                if (!masterItem.ItemPriorityStats.IsHighestPriorityStation(currentStationType, allStationTypesList))
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

        protected static float _moreDistanceDesired_Total(Vector3 currentPosition, Vector3 targetPosition, float total,
            float maxPriority)
        {
            return _addPriorityIfAbovePercent(Vector3.Distance(currentPosition, targetPosition), total, 0, maxPriority);
        }

        protected static float _lessDistanceDesired_Total(Vector3 currentPosition, Vector3 targetPosition, float total,
            float maxPriority)
        {
            return _addPriorityIfBelowPercent(Vector3.Distance(currentPosition, targetPosition), total, 100,
                maxPriority);
        }

        protected static float _exactDistanceDesired_Total(Vector3 currentPosition, Vector3 targetPosition, float total,
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
            var inventory_HaulerItems = priority_Parameters.Inventory_Hauler?.GetItemsToDeliverFromActor(
                priority_Parameters.Inventory_Target) ?? new List<Item>();
            
            var allItems = priority_Parameters.Inventory_Target?.GetItemsToFetchFromStation() ?? new List<Item>();
            
            var allItems =
                priority_Parameters.Inventory_Target?.GetItemsToDeliverFromActor(priority_Parameters
                    .Inventory_Hauler) ?? new List<Item>();

            return GeneratePriority(
                priority_Parameters,
                allItems,
                (items, totalItems, maxPriority) => 
                    Item.GetItemListTotal_CountAllItems(items) != 0
                        ? _moreItemsDesired_Total(
                            items, totalItems, maxPriority, 
                            priority_Parameters.StationName_Source,
                            priority_Parameters.StationType_All)
                        : 0
            );

            return GeneratePriority(
                priority_Parameters,
                allItems,
                (items, totalItems, maxPriority) => Item.GetItemListTotal_CountAllItems(items) != 0
                    ? _moreItemsDesired_Total(items, totalItems, maxPriority)
                    : 0
            );
        }

        static float _generateChop_WoodPriority(Priority_Parameters priority_Parameters)
        {
            var allItems = priority_Parameters.Inventory_Source?.GetItemsToFetchFromStation() ?? new List<Item>();

            //* Decrease the priority of Chop_Wood by how many logs you have in your inventory. For now, we will do it as a one to one,
            //* But later, we can change it to be a percentage of the total storage space, or a previously stated percentage value.
            
            return GeneratePriority(priority_Parameters, allItems, _lessItemsDesired_Total);
        }

        static float _generateProcess_LogsPriority(Priority_Parameters priority_Parameters)
        {
            var allItems = priority_Parameters.Inventory_Source?
                .GetInventoryItemsToProcess(priority_Parameters.Inventory_Hauler) ?? new List<Item>();

            return GeneratePriority(priority_Parameters, allItems,
                (items, totalItems, maxPriority) => 
                    _moreItemsDesired_Total(items, totalItems, maxPriority)
            );
        }

        static float GeneratePriority(Priority_Parameters priority_Parameters, List<Item> allItems,
            Func<List<Item>, float, float, float> calculateItemPriority)
        {
            var priority_ItemQuantity = calculateItemPriority(allItems, priority_Parameters.TotalItems,
                priority_Parameters.DefaultMaxPriority);

            if (priority_Parameters.Inventory_Hauler is null) return priority_ItemQuantity;

            var priority_Distance = CalculateDistancePriority(priority_Parameters);

            return priority_ItemQuantity + priority_Distance;
        }

        static float CalculateDistancePriority(Priority_Parameters priority_Parameters)
        {
            var haulerPosition = priority_Parameters.Inventory_Hauler != null 
            ? priority_Parameters.Inventory_Hauler.Reference.GameObject.transform.position
            : Vector3.zero;

            var targetPosition = priority_Parameters.Inventory_Target != null
            ? priority_Parameters.Inventory_Target.Reference.GameObject.transform.position
            : Vector3.zero;

            return haulerPosition != Vector3.zero && targetPosition != Vector3.zero
                ? _lessDistanceDesired_Total(haulerPosition, targetPosition, priority_Parameters.TotalDistance,
                    priority_Parameters.DefaultMaxPriority)
                : 0;
        }
    }
}