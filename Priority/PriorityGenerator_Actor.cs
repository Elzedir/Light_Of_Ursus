using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Actors;
using Debuggers;
using Items;
using Managers;
using Station;
using UnityEngine;

namespace Priority
{
    public class PriorityGenerator_Actor : PriorityGenerator
    {
        public List<float> GeneratePriority(ActorActionName                     actorActionName,
                                            Dictionary<PriorityParameterName, object> existingPriorityParameters) =>
            _generatePriorities((uint)actorActionName, existingPriorityParameters
                                                       .Select(x => x)
                                                       .ToDictionary(x => 
                                                           (uint)x.Key, x => x.Value));

        protected override List<float> _generatePriority(uint priorityID,
                                                         Dictionary<uint, object>
                                                             existingPriorityParameters)
        {
            switch (priorityID)
            {
                case (uint)ActorActionName.Fetch:
                    return _generateFetchPriority(existingPriorityParameters) ?? new List<float>();
                case (uint)ActorActionName.Deliver:
                    return _generateDeliverPriority(existingPriorityParameters) ?? new List<float>();
                default:
                    Debug.LogError($"ActionName: {priorityID} not found.");
                    return null;
            }
        }

        List<float> _generateFetchPriority(Dictionary<uint, object> existingPriorityParameters)
        {
            float maxPriority = existingPriorityParameters[(uint)PriorityParameterName.MaxPriority] as float? ??
                                _defaultMaxPriority;
            float totalDistance = existingPriorityParameters[(uint)PriorityParameterName.TotalDistance] as float? ?? 0;
            float totalItems    = existingPriorityParameters[(uint)PriorityParameterName.TotalItems] as float?    ?? 0;
            InventoryData inventory_Hauler =
                existingPriorityParameters[(uint)PriorityParameterName.InventoryHauler] as InventoryData;
            InventoryData inventory_Target =
                existingPriorityParameters[(uint)PriorityParameterName.InventoryTarget] as InventoryData;

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
                ? _moreItemsDesired_Total(allItemsToFetch, totalItems, maxPriority)
                : 0;

            var priority_Distance = haulerPosition != Vector3.zero && targetPosition != Vector3.zero
                ? _lessDistanceDesired_Total(haulerPosition, targetPosition, totalDistance, maxPriority)
                : 0;

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
                    new DebugData_Data(DebugDataType.Priority_Item,
                        priority_ItemQuantity.ToString(CultureInfo.InvariantCulture)),
                    new DebugData_Data(DebugDataType.Priority_Distance,
                        priority_Distance.ToString(CultureInfo.InvariantCulture)),
                    new DebugData_Data(DebugDataType.Priority_Total,
                        (priority_ItemQuantity + priority_Distance).ToString(CultureInfo.InvariantCulture))
                }
            );

            DebugVisualiser.Instance.UpdateDebugEntry(DebugSectionType.Hauling, debugDataList);

            return new List<float>
            {
                priority_ItemQuantity + priority_Distance
            };
        }

        List<float> _generateDeliverPriority(Dictionary<uint, object> existingPriorityParameters)
        {
            var maxPriority = existingPriorityParameters[(uint)PriorityParameterName.MaxPriority] as float? ??
                              _defaultMaxPriority;
            var totalDistance = existingPriorityParameters[(uint)PriorityParameterName.TotalDistance] as float? ?? 0;
            var totalItems    = existingPriorityParameters[(uint)PriorityParameterName.TotalItems] as float?    ?? 0;
            var inventory_Hauler =
                existingPriorityParameters[(uint)PriorityParameterName.InventoryHauler] as InventoryData;
            var inventory_Target =
                existingPriorityParameters[(uint)PriorityParameterName.InventoryTarget] as InventoryData;

            StationName currentStationType =
                existingPriorityParameters.TryGetValue((uint)PriorityParameterName.CurrentStationType,
                    out var stationType)
                    ? stationType as StationName? ?? StationName.None
                    : StationName.None;

            HashSet<StationName> allStationTypes =
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
                ? _moreItemsDesired_Total(allItemsToDeliver, totalItems, maxPriority, currentStationType,
                    allStationTypes)
                : 0;

            var priority_Distance = haulerPosition != Vector3.zero && targetPosition != Vector3.zero
                ? _lessDistanceDesired_Total(haulerPosition, targetPosition, totalDistance, maxPriority)
                : 0;

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
                    new DebugData_Data(DebugDataType.Priority_Item,     priority_ItemQuantity.ToString()),
                    new DebugData_Data(DebugDataType.Priority_Distance, priority_Distance.ToString()),
                    new DebugData_Data(DebugDataType.Priority_Total,
                        (priority_ItemQuantity + priority_Distance).ToString())
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
