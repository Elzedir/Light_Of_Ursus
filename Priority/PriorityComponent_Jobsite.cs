using System.Collections.Generic;
using System.Linq;
using Actors;
using Items;
using Jobs;
using Jobsite;
using Managers;
using Station;
using Tools;
using UnityEngine;

namespace Priority
{
    public class PriorityComponent_Jobsite : PriorityComponent
    {
        PriorityGenerator_Jobsite _priorityGenerator;
        PriorityGenerator_Jobsite PriorityGenerator => _priorityGenerator ??= new PriorityGenerator_Jobsite(); 
        public override void OnDataChanged(DataChanged                               dataChanged,
                                           Dictionary<PriorityParameterName, object> changedParameters)
        {
            return;
        }

        protected override List<uint> _canPeek(List<uint> priorityIDs)
        {
            var allowedPriorities = new List<uint>();
            
            foreach (var priorityID in priorityIDs)
            {
                if (priorityID is (uint)JobTaskName.Idle)
                {
                    Debug.LogError($"ActorActionName: {(ActorActionName)priorityID} not allowed in PeekHighestSpecificPriority.");
                    continue;
                }

                allowedPriorities.Add(priorityID);
            }   
            
            return allowedPriorities;
        }

        protected override void _regeneratePriority(uint priorityQueueID, uint priorityID)
        {
            if (!_priorityExists(priorityID, out var existingPriorityParameters)) return;

            var newPriorities = PriorityGenerator.GeneratePriority((JobTaskName)priorityID, existingPriorityParameters);

            if (newPriorities is null || newPriorities.Count is 0) return;

            AllPriorities[priorityQueueID].Update(priorityID, newPriorities);
        }

        protected override ObservableDictionary<uint, PriorityQueue> _getRelevantPriorityQueues(
            PriorityState priorityState)
        {
            return AllPriorities;
        }

        protected override bool _priorityExists(uint priorityID, out Dictionary<PriorityParameterName, object> existingPriorityParameters)
        {
            if (_priorityParameters.TryGetValue((JobName)priorityID, out var parameters))
            {
                existingPriorityParameters = parameters;
                return true;
            }
            
            Debug.LogError($"ActionName: {priorityID} not found in _existingParameters.");
            existingPriorityParameters = null;
            return false;
        }
        
        public PriorityComponent_Jobsite(uint jobsiteID) 
        {
            _jobsiteReferences                  =  new ComponentReference_Jobsite(jobsiteID);
        } 

        readonly ComponentReference_Jobsite _jobsiteReferences;

        public uint      JobsiteID => _jobsiteReferences.JobsiteID;
        JobsiteComponent _jobsite  => _jobsiteReferences.Jobsite;

        public (StationComponent Station, List<Item> Items) GetStationToFetchFrom(ActorComponent hauler)
        {
            // Reduce the redundancy of the constant GetItemsToFetch in relevant stations and here.
            
            var allRelevantStations = _jobsite.GetRelevantStations(ActorActionName.Fetch, hauler.ActorData.InventoryData);

            if (allRelevantStations.Count is 0)
            {
                //Debug.LogError("No stations to fetch from.");
                return (null, null);
            }

            var totalItemsToFetch = allRelevantStations.Sum(station => Item.GetItemListTotal_CountAllItems(station.GetInventoryItemsToFetch()));
            var totalDistance     = allRelevantStations.Sum(station => Vector3.Distance(hauler.transform.position, station.transform.position));

            foreach (var station in allRelevantStations)
            {
                var priorityParameters = _updateExistingPriorityParameters((uint)ActorActionName.Fetch, new Dictionary<PriorityParameterName, object>
                {
                    { PriorityParameterName.TotalItems, totalItemsToFetch },
                    { PriorityParameterName.TotalDistance, totalDistance },
                    { PriorityParameterName.InventoryHauler, hauler.ActorData.InventoryData },
                    { PriorityParameterName.InventoryTarget, station.StationData.InventoryData },
                });

                var newPriorities = PriorityGenerator.GeneratePriority(JobTaskName.Fetch_Items, priorityParameters);

                if (newPriorities is null || newPriorities.Count is 0) continue;

                AllPriorities[(uint)ActorActionName.Fetch].Update(station.StationID, newPriorities);
            }

            var peekedStation = Manager_Station.GetStation(AllPriorities[(uint)ActorActionName.Fetch].Peek().PriorityID);
        
            if (peekedStation is null) return (null, null);

            var allItemsToFetch = peekedStation.GetInventoryItemsToFetch();

            if (allItemsToFetch.Count is 0) return (null, null);

            var availableSpace = hauler.ActorData.StatsAndAbilities.Actor_Stats.AvailableCarryWeight;

            var itemsToFetch = new List<Item>();

            for (var i = 0; i < allItemsToFetch.Count; i++)
            {
                if (availableSpace <= 0) break;
                    
                var item       = new Item(allItemsToFetch[i]);
                
                // Find out how to remove items with 0 amount rather than skip them.
                if (item.ItemAmount <= 0) continue;
                
                var itemMaster = Items.Items.GetItem_Master(item.ItemID);
                var itemWeight = itemMaster.CommonStats_Item.ItemWeight;
                
                if (itemWeight > availableSpace) continue;
                
                var totalItemWeight =  itemWeight * item.ItemAmount;
                
                if (totalItemWeight < availableSpace)
                {
                    itemsToFetch.Add(item);
                    allItemsToFetch.Remove(item);
                    availableSpace -= totalItemWeight;
                    
                    continue;
                }

                var unitsAbleToCarry = (uint)(availableSpace / itemWeight);
                
                itemsToFetch.Add(item);
                item.ItemAmount -= unitsAbleToCarry;
                availableSpace  -= unitsAbleToCarry * itemWeight;
            }

            if (itemsToFetch.Count is 0) return (null, null);
            
            AllPriorities[(uint)ActorActionName.Fetch].Dequeue(peekedStation.StationID);

            return (peekedStation, itemsToFetch);
        }

        public (StationComponent Station, List<Item> Items) GetStationToDeliverTo(ActorComponent hauler)
        {
            // Reduce the redundancy of the constant GetItemsToDeliver in relevant stations and here.
            var allRelevantStations = _jobsite.GetRelevantStations(ActorActionName.Deliver, hauler.ActorData.InventoryData);

            if (allRelevantStations.Count is 0)
            {
                //Debug.Log("No stations to deliver to.");
                return (null, null);
            }
        
            var totalItemsToDeliver = allRelevantStations.Sum(station => Item.GetItemListTotal_CountAllItems(station.GetInventoryItemsToDeliver(hauler.ActorData.InventoryData)));
            var totalDistance       = allRelevantStations.Sum(station => Vector3.Distance(hauler.transform.position, station.transform.position));
            var allStationTypes     = new HashSet<StationName>(allRelevantStations.Select(station => station.StationName));

            foreach (var station in allRelevantStations)
            {
                var priorityParameters = _updateExistingPriorityParameters((uint)ActorActionName.Deliver, new Dictionary<PriorityParameterName, object>
                {
                    { PriorityParameterName.TotalItems, totalItemsToDeliver },
                    { PriorityParameterName.TotalDistance, totalDistance },
                    { PriorityParameterName.InventoryHauler, hauler.ActorData.InventoryData },
                    { PriorityParameterName.InventoryTarget, station.StationData.InventoryData }, 
                    { PriorityParameterName.CurrentStationType, station.StationName },
                    { PriorityParameterName.AllStationTypes, allStationTypes },
                });

                var newPriorities = PriorityGenerator.GeneratePriority(JobTaskName.Fetch_Items, priorityParameters);

                if (newPriorities is null || newPriorities.Count is 0) continue;

                AllPriorities[(uint)ActorActionName.Deliver].Update(station.StationID, newPriorities);
            }

            var peek = AllPriorities[(uint)ActorActionName.Deliver].Peek();
            
            if (peek is null) return (null, null);

            var peekedStation = Manager_Station.GetStation(peek.PriorityID);
        
            if (peekedStation is null) return (null, null);

            var itemsToDeliver = peekedStation.GetInventoryItemsToDeliver(hauler.ActorData.InventoryData);

            if (itemsToDeliver.Count is 0) return (null, null);

            AllPriorities[(uint)ActorActionName.Deliver].Dequeue(peekedStation.StationID);

            return (peekedStation, itemsToDeliver);
        }
        
        static readonly Dictionary<JobName, Dictionary<PriorityParameterName, object>> _priorityParameters = new()
        {
            {
                JobName.Logger, new Dictionary<PriorityParameterName, object>
                {
                    { PriorityParameterName.TotalItems, null },
                    { PriorityParameterName.TotalDistance, null },
                    { PriorityParameterName.InventoryHauler, null },
                    { PriorityParameterName.InventoryTarget, null },
                    { PriorityParameterName.Jobsite, null },
                }
            }
        };

        static readonly Dictionary<DataChanged, List<JobTaskToChange>> _prioritiesToChange = new()
        {
            {
                DataChanged.ChangedInventory, new List<JobTaskToChange>
                {
                    new(JobTaskName.Fetch_Items, PriorityImportance.High),
                    new(JobTaskName.Deliver_Items, PriorityImportance.High),
                }
            },
        };
    }
}
