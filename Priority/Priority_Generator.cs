using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Actor;
using Debuggers;
using Inventory;
using Items;
using Jobs;
using Station;
using UnityEngine;

namespace Priority
{
    public abstract class Priority_Generator
    {
        protected static float _defaultMaxPriority => 10;

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
            => total == 0 ? maxPriority : Math.Clamp((targetPercentage / 100 - current / total) * maxPriority, 0, maxPriority);

        protected static float _addPriorityIfNotEqualPercent(float current, float total, float targetPercentage,
                                                             float maxPriority)
            => total == 0 ? 0 : Math.Clamp(Math.Abs(current / total - targetPercentage / 100) * maxPriority, 0, maxPriority);

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

        protected static float _moreItemsDesired_Total(List<Item>           items, float total, float maxPriority,
                                                       StationName          currentStationType = StationName.None,
                                                       HashSet<StationName> allStationTypes    = null)
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
                                                           float   target,          float   maxPriority)
        {
            return _addPriorityIfBelowTarget(Vector3.Distance(currentPosition, targetPosition), target, maxPriority);
        }

        protected static float _lessDistanceDesired_Target(Vector3 currentPosition, Vector3 targetPosition,
                                                           float   target,          float   maxPriority)
        {
            return _addPriorityIfAboveTarget(Vector3.Distance(currentPosition, targetPosition), target, maxPriority);
        }

        protected static float _exactDistanceDesired_Target(Vector3 currentPosition, Vector3 targetPosition,
                                                            float   target,          float   maxPriority)
        {
            return _addPriorityIfNotEqualTarget(Vector3.Distance(currentPosition, targetPosition), target, maxPriority);
        }

        protected static float _moreDistanceDesired_Total(Vector3 currentPosition, Vector3 targetPosition, float total,
                                                          float   maxPriority)
        {
            return _addPriorityIfAbovePercent(Vector3.Distance(currentPosition, targetPosition), total, 0, maxPriority);
        }

        protected static float _lessDistanceDesired_Total(Vector3 currentPosition, Vector3 targetPosition, float total,
                                                          float   maxPriority)
        {
            return _addPriorityIfBelowPercent(Vector3.Distance(currentPosition, targetPosition), total, 100,
                maxPriority);
        }

        protected static float _exactDistanceDesired_Total(Vector3 currentPosition, Vector3 targetPosition, float total,
                                                           float   maxPriority)
        {
            return _addPriorityIfNotEqualPercent(Vector3.Distance(currentPosition, targetPosition), total, 100,
                maxPriority);
        }

        public static float GeneratePriority(PriorityType                              priorityType,
                                             uint                                      priorityID,
                                             Dictionary<PriorityParameterName, object> existingPriorityParameters) =>
            _generatePriority(priorityType, priorityID, existingPriorityParameters?
                                                        .Select(x => x)
                                                        .ToDictionary(x =>
                                                            (uint)x.Key, x => x.Value));

        static float _generatePriority(PriorityType             priorityType, uint priorityID,
                                       Dictionary<uint, object> existingPriorityParameters)
        {
            if (existingPriorityParameters == null)
            {
                return 0;
                
                Debug.LogError("ExistingPriorityParameters is null.");

                switch (priorityType)
                {
                    case PriorityType.ActorAction:
                        Debug.Log($"ActorAction: {(ActorActionName)priorityID} existing parameters not found.");
                        break;
                    case PriorityType.JobTask:
                        Debug.Log($"JobTask: {(JobTaskName)priorityID} existing parameters not found.");
                        break;
                }

                return 0;
            }

            switch (priorityType)
            {
                case PriorityType.ActorAction:
                    return _generatePriority_Actor(priorityID, existingPriorityParameters);
                case PriorityType.JobTask:
                    return _generatePriority_Jobsite(priorityID, existingPriorityParameters);
                default:
                    Debug.LogError($"PriorityType: {priorityType} not found.");
                    return 0;
            }
        }

        static float _generatePriority_Actor(uint priorityID, Dictionary<uint, object> existingPriorityParameters)
        {
            switch (priorityID)
            {
                case (uint)ActorActionName.Perform_JobTask:
                    // Calculate priority based on actor personality, time worked, time of day, etc.
                    return 2;
                default:
                    Debug.LogError($"ActorAction: {(ActorActionName)priorityID} not found.");
                    return 0;
            }
        }

        static float _generatePriority_Jobsite(uint priorityID, Dictionary<uint, object> existingPriorityParameters)
        {
            switch (priorityID)
            {
                case (uint)JobTaskName.Idle:
                    return 1;
                case (uint)JobTaskName.Fetch_Items:
                    return _generateFetchPriority(existingPriorityParameters);
                case (uint)JobTaskName.Deliver_Items:
                    return _generateDeliverPriority(existingPriorityParameters);
                case (uint)JobTaskName.Chop_Wood:
                    return _generateChop_WoodPriority(existingPriorityParameters);
                default:
                    Debug.LogError($"JobTask: {(JobTaskName)priorityID} not found.");
                    return 0;
            }
        }

        static float _generateFetchPriority(Dictionary<uint, object> existingPriorityParameters)
        {
            if (!_parameterChecks(existingPriorityParameters, out var defaultMaxPriority, out var totalDistance,
                out var totalItems, out var inventory_Hauler, out var inventory_Target))
            {
                return 0;
            }

            var allItemsToFetch = inventory_Target.GetInventoryItemsToFetchFromStation();
            var priority_ItemQuantity = Item.GetItemListTotal_CountAllItems(allItemsToFetch) != 0
                ? _moreItemsDesired_Total(allItemsToFetch, totalItems, defaultMaxPriority)
                : 0;

            float priority_Distance = 0;

            if (inventory_Hauler is not null)
            {
                var haulerPosition = inventory_Hauler.Reference.GameObject.transform.position;
                var targetPosition = inventory_Target.Reference.GameObject.transform.position;

                priority_Distance = haulerPosition != Vector3.zero && targetPosition != Vector3.zero
                    ? _lessDistanceDesired_Total(haulerPosition, targetPosition, totalDistance, defaultMaxPriority)
                    : 0;
            }

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

            return priority_ItemQuantity + priority_Distance;
        }

        static float _generateDeliverPriority(Dictionary<uint, object> existingPriorityParameters)
        {
            if (!_parameterChecks(existingPriorityParameters, out var defaultMaxPriority, out var totalDistance,
                    out var totalItems,                       out var inventory_Hauler,   out var inventory_Target))
            {
                return 0;
            }

            var currentStationType =
                existingPriorityParameters.TryGetValue((uint)PriorityParameterName.CurrentStationType,
                    out var stationType)
                    ? stationType as StationName? ?? StationName.None
                    : StationName.None;

            var allStationTypes =
                existingPriorityParameters.TryGetValue((uint)PriorityParameterName.AllStationTypes,
                    out var stationTypes)
                    ? stationTypes as HashSet<StationName>
                    : null;

                var allItemsToDeliver = inventory_Target.GetInventoryItemsToDeliverFromInventory(inventory_Hauler);
                var priority_ItemQuantity = Item.GetItemListTotal_CountAllItems(allItemsToDeliver) != 0
                    ? _moreItemsDesired_Total(allItemsToDeliver, totalItems, defaultMaxPriority, currentStationType,
                        allStationTypes)
                    : 0;
                
                float priority_Distance = 0;
            
                if (inventory_Hauler is not null)
                {
                    var haulerPosition = inventory_Hauler.Reference.GameObject.transform.position;
                    var targetPosition = inventory_Target.Reference.GameObject.transform.position;

                    priority_Distance = haulerPosition != Vector3.zero && targetPosition != Vector3.zero
                        ? _lessDistanceDesired_Total(haulerPosition, targetPosition, totalDistance, defaultMaxPriority)
                        : 0;
                }

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
                        new(DebugDataType.Priority_Item, priority_ItemQuantity.ToString(CultureInfo.InvariantCulture)),
                        new(DebugDataType.Priority_Distance, priority_Distance.ToString(CultureInfo.InvariantCulture)),
                        new(DebugDataType.Priority_Total,
                            (priority_ItemQuantity + priority_Distance).ToString(CultureInfo.InvariantCulture))
                    }
                );

                DebugVisualiser.Instance.UpdateDebugEntry(DebugSectionType.Hauling, debugDataList);

                return priority_ItemQuantity + priority_Distance;
        }

        static float _generateChop_WoodPriority(Dictionary<uint, object> existingPriorityParameters)
        {
            if (!_parameterChecks(existingPriorityParameters, out var defaultMaxPriority, out var totalDistance,
                out var totalItems, out var inventory_Hauler, out var inventory_Target))
            {
                return 0;
            }
            
            var allItemsToFetch = inventory_Target.GetInventoryItemsToFetchFromStation();

            var priority_ItemQuantity = _lessItemsDesired_Total(allItemsToFetch, totalItems, defaultMaxPriority);
            
            float priority_Distance = 0;
            
            if (inventory_Hauler is not null)
            {
                var haulerPosition = inventory_Hauler.Reference.GameObject.transform.position;
                var targetPosition = inventory_Target.Reference.GameObject.transform.position;

                priority_Distance = haulerPosition != Vector3.zero && targetPosition != Vector3.zero
                    ? _lessDistanceDesired_Total(haulerPosition, targetPosition, totalDistance, defaultMaxPriority)
                    : 0;
            }
            
            var debugDataList = new DebugEntry_Data
            (
                new DebugEntryKey
                (
                    "Chop Wood",
                    $"{inventory_Target.ComponentType}",
                    inventory_Target.Reference.ComponentID
                ),
                new List<DebugData_Data>
                {
                    new(DebugDataType.Priority_Item, priority_ItemQuantity.ToString(CultureInfo.InvariantCulture)),
                    new(DebugDataType.Priority_Distance, priority_Distance.ToString(CultureInfo.InvariantCulture)),
                    new(DebugDataType.Priority_Total,
                        (priority_ItemQuantity + priority_Distance).ToString(CultureInfo.InvariantCulture))
                }
            );

            DebugVisualiser.Instance.UpdateDebugEntry(DebugSectionType.Hauling, debugDataList);

            return priority_ItemQuantity + priority_Distance;
        }
        
        static bool _parameterChecks(Dictionary<uint, object> existingPriorityParameters, out float defaultMaxPriority,
                                     out float totalDistance, out long totalItems, out Inventory_Data_Preset inventory_Hauler,
                                     out Inventory_Data_Preset inventory_Target)
        {
            defaultMaxPriority = 0;
            totalDistance = 0;
            totalItems = 0;
            inventory_Hauler = null;
            inventory_Target = null;
            
            if (existingPriorityParameters is null)
            {
                Debug.LogError("ExistingPriorityParameters is null.");
                return false;
            }

            if (!existingPriorityParameters.TryGetValue((uint)PriorityParameterName.DefaultMaxPriority,
                    out var defaultPriorityObject)
                || defaultPriorityObject is not float maxPriorityValue)
            {
                if (_defaultMaxPriority == 0)
                {
                    Debug.LogError(existingPriorityParameters.ContainsKey((uint)PriorityParameterName.DefaultMaxPriority)
                        ? "DefaultMaxPriority is not float and _defaultMaxPriority is 0."
                        : "DefaultMaxPriority not found and _defaultMaxPriority is 0.");
                    return false;    
                }
                
                maxPriorityValue = _defaultMaxPriority;
            }

            defaultMaxPriority = maxPriorityValue;
            
            if (defaultMaxPriority == 0)
            {
                if (_defaultMaxPriority == 0)
                {
                    Debug.LogError("DefaultMaxPriority is 0.");
                    return false;
                }

                defaultMaxPriority = _defaultMaxPriority;
            }

            if (existingPriorityParameters.TryGetValue((uint)PriorityParameterName.Total_Distance, out var totalDistanceObject))
            {
                if (totalDistanceObject is not float totalDistanceValue)
                {
                    Debug.LogError("TotalDistance not found");
                    return false;
                }
                
                totalDistance = totalDistanceValue;
            }

            if (existingPriorityParameters.TryGetValue((uint)PriorityParameterName.Total_Items,
                    out var totalItemsObject))
            {
                if (totalItemsObject is not long totalItemsValue)
                {
                    Debug.LogError("TotalItems not found");
                    return false;    
                }
             
                totalItems = totalItemsValue;
            }

            if (existingPriorityParameters.TryGetValue((uint)PriorityParameterName.Worker_Component,
                    out var inventory_HaulerObject))
            {
                if (inventory_HaulerObject is not Inventory_Data_Preset inventory_HaulerData)
                {
                    Debug.LogError("Inventory_Hauler not found");
                    return false;    
                }
             
                inventory_Hauler = inventory_HaulerData;
            }

            if (existingPriorityParameters.TryGetValue((uint)PriorityParameterName.Target_Component,
                    out var inventory_TargetObject))
            {
                if (inventory_TargetObject is not Inventory_Data_Preset inventory_TargetData)
                {
                    Debug.LogError(existingPriorityParameters.ContainsKey((uint)PriorityParameterName.Target_Component)
                        ? "Inventory_Target is not InventoryData"
                        : "Inventory_Target not found");
                    return false;   
                }
                
                inventory_Target = inventory_TargetData;
            }

            return true;
        }
    }

    public enum PriorityType
    {
        ActorAction,
        JobTask,
    }
}