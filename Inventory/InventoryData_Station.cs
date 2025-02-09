using System;
using System.Collections.Generic;
using ActorActions;
using Items;
using Station;
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
        public ulong MaxInventorySpace = 100; 
        public ulong AvailableInventorySpace => MaxInventorySpace - (ulong)AllInventoryItems.Values.Count;

        public override bool HasSpaceForAllItem(ulong itemID, ulong itemAmount) =>
            itemID != 0
                ? itemAmount <= AvailableInventorySpace
                : AvailableInventorySpace > 0;

        public override (Item AddedItem, Item ReturnedItem) HasSpaceForItem(ulong itemID, ulong itemAmount)
        {
            if (itemID == 0 || itemAmount == 0)
            {
                Debug.LogError($"Item ID: {itemID} or Item Amount: {itemAmount} is 0.");
                return (null, null);
            }
            
            if (itemAmount <= AvailableInventorySpace) return (new Item(itemID, itemAmount), null);
            
            var amountToAdd = Math.Min(AvailableInventorySpace, itemAmount);

            var returnedAmount = itemAmount - amountToAdd;
            
            return (amountToAdd > 0 
                    ? new Item(itemID, amountToAdd) 
                    : null, 
                returnedAmount > 0 
                    ? new Item(itemID, returnedAmount) 
                    : null);
        }

        public override Dictionary<ulong, ulong> GetItemsToFetchFromThisInventory()
        {
            var itemsToFetch = new Dictionary<ulong, ulong>();

            foreach (var item in AllInventoryItems.Values)
            {
                if (StationReference.Station.DesiredStoredItemIDs.Contains(item.ItemID)) continue;
                
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

        public override Dictionary<ulong, Dictionary<ulong, ulong>> GetItemsToDeliverToThisInventoryFromAllStations(bool limitToAvailableInventoryCapacity = true)
        {
            if (!HasSpaceForAllItemList())
            {
                Debug.LogError("No space in inventory.");
                return new Dictionary<ulong, Dictionary<ulong, ulong>>();
            }
            
            var stationsAndItemsToFetchFrom = new Dictionary<ulong, Dictionary<ulong, ulong>>();

            foreach (var stationToFetchFrom in StationReference.Station.JobSite.JobSite_Data.AllStations)
            {
                if (stationToFetchFrom.Key == StationReference.StationID) continue;
                
                var itemsToFetch = stationToFetchFrom.Value.Station_Data.InventoryData.GetItemsToFetchFromThisInventory();
                
                if (itemsToFetch.Count == 0) continue;
                
                stationsAndItemsToFetchFrom.TryAdd(stationToFetchFrom.Key, new Dictionary<ulong, ulong>());

                foreach (var desiredItemID in StationReference.Station.DesiredStoredItemIDs)
                {
                    itemsToFetch.TryGetValue(desiredItemID, out var amountToFetch);
                    
                    if (amountToFetch == 0) continue;
                    
                    if (!limitToAvailableInventoryCapacity)
                    {
                        if (!stationsAndItemsToFetchFrom[stationToFetchFrom.Key].TryAdd(desiredItemID, amountToFetch))
                            stationsAndItemsToFetchFrom[stationToFetchFrom.Key][desiredItemID] += amountToFetch;
                    
                        continue;
                    }
                
                    var addedItem = HasSpaceForItem(desiredItemID, amountToFetch).AddedItem;
                    
                    if (addedItem?.ItemID is null or 0 || addedItem.ItemAmount == 0) continue;
                    
                    if (!stationsAndItemsToFetchFrom[stationToFetchFrom.Key].TryAdd(addedItem.ItemID, addedItem.ItemAmount)) 
                        stationsAndItemsToFetchFrom[stationToFetchFrom.Key][addedItem.ItemID] += addedItem.ItemAmount;
                }
            }
            
            return stationsAndItemsToFetchFrom;
        }

        public override Dictionary<ulong, ulong> GetItemsToDeliverToThisInventory(InventoryData otherInventory, bool limitToAvailableInventoryCapacity = true)
        {
            if (!HasSpaceForAllItemList())
            {
                Debug.LogError("No space in inventory.");
                return new Dictionary<ulong, ulong>();
            }
            
            var itemsToDeliver = new Dictionary<ulong, ulong>();

            foreach (var desiredItemID in StationReference.Station.DesiredStoredItemIDs)
            {
                if (!otherInventory.AllInventoryItems.TryGetValue(desiredItemID, out var itemToFetch))
                    continue;

                if (!limitToAvailableInventoryCapacity)
                {
                    if (!itemsToDeliver.TryAdd(itemToFetch.ItemID, itemToFetch.ItemAmount))
                    {
                        Debug.LogError($"ItemID: {itemToFetch.ItemID} somehow already exists in itemsToDeliver.");
                    }
                    
                    continue;
                }
                
                var addedItem = HasSpaceForItem(itemToFetch.ItemID, itemToFetch.ItemAmount).AddedItem;
                
                if (addedItem?.ItemID is null or 0 || addedItem.ItemAmount == 0) continue;

                if (!itemsToDeliver.TryAdd(addedItem.ItemID, addedItem.ItemAmount))
                    Debug.LogError($"ItemID: {addedItem.ItemID} somehow already exists in itemsToDeliver.");
            }
            
            return itemsToDeliver;
        }
        
        public override List<ActorActionName> GetAllowedActions() => new();
    }
}