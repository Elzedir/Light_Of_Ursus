using System;
using System.Collections.Generic;
using System.Linq;
using ActorActions;
using Items;
using Tools;
using UnityEngine;

namespace Inventory
{
    [Serializable]
    public class InventoryData_Station : InventoryData
    {
        public InventoryData_Station(ulong stationID, ObservableDictionary<ulong, Item> allInventoryItems) : base(stationID, ComponentType.Station)
        {
            SetInventory(allInventoryItems ?? new ObservableDictionary<ulong, Item>());
            AllInventoryItems.DictionaryChanged += OnInventoryChanged;
        }

        public override ComponentType         ComponentType      => ComponentType.Station;
        public          InventoryData_Station GetInventoryData() => this;

        public ComponentReference_Station StationReference => Reference as ComponentReference_Station;

        //* Implement a way to change the size depending on the station. Maybe StationComponent default value.
        public ulong MaxInventorySpace = 10; 

        HashSet<ulong> _getDesiredItemIDs() => StationReference.Station.DesiredStoredItemIDs;

        public override bool HasSpaceForItem(ulong itemID, ulong itemAmount) =>
            itemID != 0
                ? itemAmount <= MaxInventorySpace
                : MaxInventorySpace > 0;

        public override Item GetUnaddedItem(ulong itemID, ulong itemAmount) =>
            itemID != 0
                ? itemAmount <= MaxInventorySpace
                    ? null
                    : new Item(itemID, itemAmount - MaxInventorySpace)
                : null;

        public override Dictionary<ulong, ulong> GetItemsToFetchFromStation()
        {
            var itemsToFetch = new Dictionary<ulong, ulong>();

            foreach (var item in AllInventoryItems.Values)
            {
                if (_getDesiredItemIDs().Contains(item.ItemID)) continue;
                
                if (item.ItemAmountOnHold > item.ItemAmount)
                {
                    Debug.LogError($"Item quantity on hold - ID: {item.ItemID} Qty: {item.ItemAmount} " +
                                   "is somehow greater than " +
                                   $"item quantity in inventory - ID: {item.ItemID} Qty: {item.ItemAmount}." +
                                   $"Setting item quantity on hold to item quantity in inventory.");

                    item.ItemAmountOnHold = item.ItemAmount;
                    continue;
                }

                if (item.ItemAmountOnHold == item.ItemAmount) continue;

                itemsToFetch.Add(item.ItemID, item.ItemAmount - item.ItemAmountOnHold);
            }

            return itemsToFetch;
        }

        public override Dictionary<ulong, Dictionary<ulong, ulong>> GetItemsToDeliverFromOtherStations(bool limitToAvailableInventoryCapacity = true)
        {
            if (!HasSpaceForItemList())
            {
                Debug.LogError("Not enough space in inventory.");
                return new Dictionary<ulong, Dictionary<ulong, ulong>>();
            }
            
            var stationsAndItemsToFetchFrom = new Dictionary<ulong, Dictionary<ulong, ulong>>();

            foreach (var stationToFetchFrom in StationReference.Station.JobSite.JobSite_Data.AllStations)
            {
                if (stationToFetchFrom.Key == StationReference.StationID) continue;

                stationsAndItemsToFetchFrom.TryAdd(stationToFetchFrom.Key, new Dictionary<ulong, ulong>());
                
                var itemsToFetch = stationToFetchFrom.Value.Station_Data.InventoryData.GetItemsToFetchFromStation();

                foreach (var desiredItemID in _getDesiredItemIDs())
                {
                    itemsToFetch.TryGetValue(desiredItemID, out var amountToFetch);
                    
                    if (amountToFetch == 0) continue;
                    
                    if (!stationsAndItemsToFetchFrom[stationToFetchFrom.Key].TryAdd(desiredItemID, amountToFetch)) 
                        stationsAndItemsToFetchFrom[stationToFetchFrom.Key][desiredItemID] += amountToFetch;
                }
            }

            if (stationsAndItemsToFetchFrom.Count == 0 || !limitToAvailableInventoryCapacity) return stationsAndItemsToFetchFrom;

            foreach (var stationToFetch in stationsAndItemsToFetchFrom.ToList())
            {
                foreach (var itemToFetch in stationToFetch.Value.ToList())
                {
                    if (HasSpaceForItem(itemToFetch.Key, itemToFetch.Value))
                        stationsAndItemsToFetchFrom[stationToFetch.Key].Remove(itemToFetch.Key);
                }
            }

            return stationsAndItemsToFetchFrom;
        }

        public override Dictionary<ulong, ulong> GetItemsToDeliverFromActor(InventoryData inventory_Actor, bool limitToAvailableInventoryCapacity = true)
        {
            if (!HasSpaceForItemList())
            {
                Debug.LogError("Not enough space in inventory.");
                return new Dictionary<ulong, ulong>();
            }
            
            var itemsToDeliver = new Dictionary<ulong, ulong>();

            foreach (var desiredItemID in _getDesiredItemIDs())
            {
                inventory_Actor.AllInventoryItems.TryGetValue(desiredItemID, out var itemToFetch);
                
                if (itemToFetch == null) continue;

                if (!itemsToDeliver.TryAdd(desiredItemID, itemToFetch.ItemAmount))
                {
                    var unaddedItem = GetUnaddedItem(itemToFetch.ItemID, itemToFetch.ItemAmount);
                    
                    .ItemAmount += itemToFetch.ItemAmount;   
                }
            }

            if (itemsToDeliver.Count == 0 || !limitToAvailableInventoryCapacity) return itemsToDeliver;

            foreach (var itemToFetch in itemsToDeliver.ToList())
            {
                if (HasSpaceForItem(itemToFetch.Key, itemToFetch.Value))
                    itemsToDeliver.Remove(itemToFetch.Key);
            }

            return itemsToDeliver;
        }

        public override Dictionary<ulong, ulong> GetInventoryItemsToProcess(InventoryData inventory_Actor)
        {
            var itemsToProcess = new Dictionary<ulong, ulong>();
            
            foreach (var itemID in _getDesiredItemIDs())
            {
                if (AllInventoryItems.ContainsKey(itemID))
                    itemsToProcess.Add(itemID, AllInventoryItems[itemID].ItemAmount);        
                
                if (inventory_Actor == null || !inventory_Actor.AllInventoryItems.ContainsKey(itemID)) continue;
                
                if (itemsToProcess.TryAdd(itemID, inventory_Actor.AllInventoryItems[itemID].ItemAmount)) 
                    itemsToProcess[itemID] += inventory_Actor.AllInventoryItems[itemID].ItemAmount;
            }

            return itemsToProcess;
        }
        
        public override List<ActorActionName> GetAllowedActions() => new();
    }
}