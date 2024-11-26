using System;
using System.Collections.Generic;
using System.Linq;
using Items;
using Station;
using UnityEngine;

namespace Priority
{
    public abstract class PriorityGenerator
    {
        protected static float _defaultMaxPriority => 10;

        protected float _addPriorityIfAboveTarget(float current, float target, float maxPriority)
            => Math.Clamp(current - target, 0, maxPriority);
        protected float _addPriorityIfBelowTarget(float current, float target, float maxPriority)
            => Math.Clamp(target - current, 0, maxPriority);
        protected float _addPriorityIfNotEqualTarget(float current, float target, float maxPriority)
            => Math.Clamp(Math.Abs(current - target), 0, maxPriority);

        protected float _addPriorityIfOutsideRange(float current, float min, float max, float maxPriority)
            => (current < min || current > max) ? Math.Clamp(Math.Min(Math.Abs(current - min), Math.Abs(current - max)), 0, maxPriority) : 0;
        protected float _addPriorityIfInsideRange(float current, float min, float max, float maxPriority)
            => (current > min || current < max) ? Math.Clamp(Math.Max(Math.Abs(current - min), Math.Abs(current - max)), 0, maxPriority) : 0;

        protected float _addPriorityIfAbovePercent(float current, float total, float targetPercentage, float maxPriority)
            => Math.Clamp((current / total - targetPercentage / 100) * maxPriority, 0, maxPriority);
        protected float _addPriorityIfBelowPercent(float current, float total, float targetPercentage, float maxPriority)
            => Math.Clamp((targetPercentage / 100 - current / total) * maxPriority, 0, maxPriority);
        protected float _addPriorityIfNotEqualPercent(float current, float total, float targetPercentage, float maxPriority)
            => Math.Clamp(Math.Abs(current / total - targetPercentage / 100) * maxPriority, 0, maxPriority);

        protected float _addPriorityIfOutsidePercentRange(float current, float total, float min, float max, float maxPriority)
        {
            current = current / total * 100;
            return (current < min || current > max)
                ? Math.Clamp(Math.Min(Math.Abs(current - min), Math.Abs(current - max)), 0, maxPriority)
                : 0;
        }
        protected float _addPriorityIfInsidePercentRange
            (float current, float total, float min, float max, float maxPriority)
        {
            current = current / total * 100;

            return (current >= min && current <= max)
                ? Math.Clamp(Math.Max(Math.Abs(current - min), Math.Abs(current - max)), 0, maxPriority)
                : 0;
        }

        protected float _moreItemsDesired_Target(List<Item> items, float target, float maxPriority)
        {
            return _addPriorityIfBelowTarget(Item.GetItemListTotal_CountAllItems(items), target, maxPriority);
        }

        protected float _lessItemsDesired_Target(List<Item> items, float target, float maxPriority)
        {
            return _addPriorityIfAboveTarget(Item.GetItemListTotal_CountAllItems(items), target, maxPriority);
        }

        protected float _exactItemsDesired_Target(List<Item> items, float target, float maxPriority)
        {
            return _addPriorityIfNotEqualTarget(Item.GetItemListTotal_CountAllItems(items), target, maxPriority);
        }

        protected float _moreItemsDesired_Total(List<Item> items, float total, float maxPriority, StationName currentStationType = StationName.None, HashSet<StationName> allStationTypes = null)
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

        protected float _lessItemsDesired_Total(List<Item> items, float total, float maxPriority)
        {
            return _addPriorityIfBelowPercent(Item.GetItemListTotal_CountAllItems(items), total, 100, maxPriority);
        }

        protected float _exactItemsDesired_Total(List<Item> items, float total, float maxPriority)
        {
            return _addPriorityIfNotEqualPercent(Item.GetItemListTotal_CountAllItems(items), total, 100, maxPriority);
        }

        protected float _moreDistanceDesired_Target(Vector3 currentPosition, Vector3 targetPosition, float target, float maxPriority)
        {
            return _addPriorityIfBelowTarget(Vector3.Distance(currentPosition, targetPosition), target, maxPriority);
        }

        protected float _lessDistanceDesired_Target(Vector3 currentPosition, Vector3 targetPosition, float target, float maxPriority)
        {
            return _addPriorityIfAboveTarget(Vector3.Distance(currentPosition, targetPosition), target, maxPriority);
        }

        protected float _exactDistanceDesired_Target(Vector3 currentPosition, Vector3 targetPosition, float target, float maxPriority)
        {
            return _addPriorityIfNotEqualTarget(Vector3.Distance(currentPosition, targetPosition), target, maxPriority);
        }

        protected float _moreDistanceDesired_Total(Vector3 currentPosition, Vector3 targetPosition, float total, float maxPriority)
        {
            return _addPriorityIfAbovePercent(Vector3.Distance(currentPosition, targetPosition), total, 0, maxPriority);
        }

        protected float _lessDistanceDesired_Total(Vector3 currentPosition, Vector3 targetPosition, float total, float maxPriority)
        {
            return _addPriorityIfBelowPercent(Vector3.Distance(currentPosition, targetPosition), total, 100, maxPriority);
        }

        protected float _exactDistanceDesired_Total(Vector3 currentPosition, Vector3 targetPosition, float total, float maxPriority)
        {
            return _addPriorityIfNotEqualPercent(Vector3.Distance(currentPosition, targetPosition), total, 100, maxPriority);
        }

        protected List<float> _generatePriorities(uint priorityID, Dictionary<uint, object> existingPriorityParameters)
        {
            if (existingPriorityParameters != null) return _generatePriority(priorityID, existingPriorityParameters);
            
            Debug.LogError($"ActionName: {priorityID} not found in _actionPriorityParameters.");
            return null;
        }

        protected abstract List<float> _generatePriority(uint priorityID,
                                                         Dictionary<uint, object>
                                                             existingPriorityParameters);
    }
}