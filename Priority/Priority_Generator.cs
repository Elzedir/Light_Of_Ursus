using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ActorActions;
using Debuggers;
using Inventory;
using Items;
using Station;
using UnityEngine;

namespace Priority
{
    public abstract class Priority_Generator
    {
        const float _defaultMaxPriority = 10;

        protected static float _addPriorityIfAboveTarget(float current, float target, float maxPriority)
            => Math.Clamp(current - target, 0, maxPriority);

        protected static float _addPriorityIfBelowTarget(float current, float target, float maxPriority)
            => Math.Clamp(target - current, 0, maxPriority);

        protected static float _addPriorityIfNotEqualTarget(float current, float target, float maxPriority)
            => Math.Clamp(Math.Abs(current - target), 0, maxPriority);

        protected static float _addPriorityIfOutsideRange(float current, float min, float max, float maxPriority)
            => (current < min || current > max)
                ? Math.Clamp(Math.Min(Math.Abs(current - min), Math.Abs(current - max)), 0, maxPriority)
                : 0;

        protected static float _addPriorityIfInsideRange(float current, float min, float max, float maxPriority)
            => (current > min || current < max)
                ? Math.Clamp(Math.Max(Math.Abs(current - min), Math.Abs(current - max)), 0, maxPriority)
                : 0;

        protected static float _addPriorityIfAbovePercent(float current, float total, float targetPercentage,
            float maxPriority)
            => total == 0 ? 0 : Math.Clamp((current / total - targetPercentage / 100) * maxPriority, 0, maxPriority);

        protected static float _addPriorityIfBelowPercent(float current, float total, float targetPercentage,
            float maxPriority)
            => total == 0
                ? maxPriority
                : Math.Clamp((targetPercentage / 100 - current / total) * maxPriority, 0, maxPriority);

        protected static float _addPriorityIfNotEqualPercent(float current, float total, float targetPercentage,
            float maxPriority)
            => total == 0
                ? 0
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

        public static float GeneratePriority(uint priorityID, Priority_Parameters priority_Parameters)
        {
            if (priority_Parameters == null)
            {
                return 0;

                // Debug.LogError("ExistingPriorityParameters is null.");
                //
                // switch (priorityType)
                // {
                //     case PriorityType.ActorAction:
                //         Debug.Log($"ActorAction: {(ActorActionName)priorityID} existing parameters not found.");
                //         break;
                //     case PriorityType.JobTask:
                //         Debug.Log($"JobTask: {(ActorActionName)priorityID} existing parameters not found.");
                //         break;
                // }
                //
                // return 0;
            }

            if (priority_Parameters.DefaultMaxPriority == 0)
                priority_Parameters.DefaultMaxPriority = _defaultMaxPriority;

            switch (priorityID)
            {
                case (uint)ActorActionName.Idle:
                    return 1;
                case (uint)ActorActionName.Fetch_Items:
                    return _generateFetchPriority(priority_Parameters);
                case (uint)ActorActionName.Deliver_Items:
                    return _generateDeliverPriority(priority_Parameters);
                case (uint)ActorActionName.Chop_Wood:
                    return _generateChop_WoodPriority(priority_Parameters);
                case (uint)ActorActionName.Process_Logs:
                    return _generateProcess_LogsPriority(priority_Parameters);
                default:
                    Debug.LogError($"ActorAction: {(ActorActionName)priorityID} not found.");
                    return 0;
            }
        }

        static float _generateFetchPriority(Priority_Parameters priority_Parameters)
        {
            if (priority_Parameters == null) return 0;

            var allItemsToFetch = priority_Parameters.Inventory_Target.GetInventoryItemsToFetchFromStation();
            var priority_ItemQuantity = Item.GetItemListTotal_CountAllItems(allItemsToFetch) != 0
                ? _moreItemsDesired_Total(allItemsToFetch, priority_Parameters.TotalItems, priority_Parameters.DefaultMaxPriority)
                : 0;

            if (priority_Parameters.Inventory_Hauler is null) return priority_ItemQuantity;

            var haulerPosition = priority_Parameters.Inventory_Hauler.Reference.GameObject.transform.position;
            var targetPosition = priority_Parameters.Inventory_Target.Reference.GameObject.transform.position;

            var priority_Distance = haulerPosition != Vector3.zero && targetPosition != Vector3.zero
                ? _lessDistanceDesired_Total(haulerPosition, targetPosition, priority_Parameters.TotalDistance, priority_Parameters.DefaultMaxPriority)
                : 0;

            return priority_ItemQuantity + priority_Distance;
        }

        static float _generateDeliverPriority(Priority_Parameters priority_Parameters)
        {
            var stationType_Source = priority_Parameters.StationType_Source;
            var stationType_All = priority_Parameters.StationType_All;

            var allItemsToDeliver = priority_Parameters.Inventory_Target.GetInventoryItemsToDeliverFromInventory(priority_Parameters.Inventory_Hauler);
            var priority_ItemQuantity = Item.GetItemListTotal_CountAllItems(allItemsToDeliver) != 0
                ? _moreItemsDesired_Total(
                    allItemsToDeliver, priority_Parameters.TotalItems, priority_Parameters.DefaultMaxPriority, stationType_Source, stationType_All)
                : 0;

            if (priority_Parameters.Inventory_Hauler is null) return priority_ItemQuantity;

            var haulerPosition = priority_Parameters.Inventory_Hauler.Reference.GameObject.transform.position;
            var targetPosition = priority_Parameters.Inventory_Target.Reference.GameObject.transform.position;

            var priority_Distance = haulerPosition != Vector3.zero && targetPosition != Vector3.zero
                ? _lessDistanceDesired_Total(haulerPosition, targetPosition, priority_Parameters.TotalDistance, priority_Parameters.DefaultMaxPriority)
                : 0;

            return priority_ItemQuantity + priority_Distance;
        }

        static float _generateChop_WoodPriority(Priority_Parameters priority_Parameters)
        {
            var allItemsToFetch = priority_Parameters.Inventory_Target.GetInventoryItemsToFetchFromStation();

            var priority_ItemQuantity = _lessItemsDesired_Total(allItemsToFetch, priority_Parameters.TotalItems, priority_Parameters.DefaultMaxPriority);

            if (priority_Parameters.Inventory_Hauler is null) return priority_ItemQuantity;
            
            float priority_Distance = 0;
            
            var haulerPosition = priority_Parameters.Inventory_Hauler.Reference.GameObject.transform.position;
            var targetPosition = priority_Parameters.Inventory_Target.Reference.GameObject.transform.position;

            priority_Distance = haulerPosition != Vector3.zero && targetPosition != Vector3.zero
                ? _lessDistanceDesired_Total(haulerPosition, targetPosition, priority_Parameters.TotalDistance, priority_Parameters.DefaultMaxPriority)
                : 0;

            return priority_ItemQuantity + priority_Distance;
        }

        static float _generateProcess_LogsPriority(Priority_Parameters priority_Parameters)
        {
            var actor = priority_Parameters.Inventory_Hauler;
            var allItemsToProcess = priority_Parameters.Station_Component_Destination.Station_Data.InventoryData.GetInventoryItemsToProcess(actor);

            var priority_ItemQuantity = _moreItemsDesired_Total(allItemsToProcess, priority_Parameters.TotalItems, priority_Parameters.DefaultMaxPriority);

            if (priority_Parameters.Inventory_Hauler is null) return priority_ItemQuantity;

            var haulerPosition = priority_Parameters.Inventory_Hauler.Reference.GameObject.transform.position;
            var targetPosition = priority_Parameters.Inventory_Target.Reference.GameObject.transform.position;

            var priority_Distance = haulerPosition != Vector3.zero && targetPosition != Vector3.zero
                ? _lessDistanceDesired_Total(haulerPosition, targetPosition, priority_Parameters.TotalDistance, priority_Parameters.DefaultMaxPriority)
                : 0;

            return priority_ItemQuantity + priority_Distance;
        }
    }
}