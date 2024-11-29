using System;
using System.Collections.Generic;
using System.Linq;
using Actors;
using Items;
using Priority;
using UnityEngine;

namespace Inventory
{
    [Serializable]
    public class InventoryData_Station : InventoryData
    {
        public InventoryData_Station(uint stationID) : base(stationID, ComponentType.Station) { }
        public override ComponentType         ComponentType      => ComponentType.Station;
        public          InventoryData_Station GetInventoryData() => this;

        public ComponentReference_Station StationReference => Reference as ComponentReference_Station;
        
        public          uint    MaxInventorySpace = 10; // Implement a way to change the size depending on the station. Maybe StationComponent default value.
        List<uint>              _getDesiredItemIDs()                      => StationReference.Station.DesiredStoredItemIDs;
        protected override bool _priorityChangeNeeded(object dataChanged) => (PriorityUpdateTrigger)dataChanged == PriorityUpdateTrigger.ChangedInventory;
    
        public override bool HasSpaceForItems(List<Item> items)
        {
            if (Item.GetItemListTotal_CountAllItems(items) <= MaxInventorySpace) return true;
        
            //Debug.Log("Not enough space in inventory.");
            return false;
        }

        public override List<Item> GetInventoryItemsToFetch()
        {
            for (var i = 0; i < AllInventoryItems.Count; i++)
            {
                var item = AllInventoryItems.ElementAt(i);

                if (item.Value.ItemAmount >= 1) continue;
                
                //Debug.LogError($"Item in inventory - ID: {item.Value.ItemID} Qty: {item.Value.ItemAmount} is somehow less than 1.");
                AllInventoryItems.Remove(item.Key);
            }
            
            var itemsToFetch = GetAllInventoryItemsClone();

            foreach (var itemID in _getDesiredItemIDs()) itemsToFetch.Remove(itemID);

            if (itemsToFetch.Count     == 0) return new List<Item>();
            if (FetchItemsOnHold.Count == 0) return itemsToFetch.Values.ToList();

            foreach (var itemOnHold in FetchItemsOnHold.Values)
            {
                if (!itemsToFetch.TryGetValue(itemOnHold.ItemID, out var itemToFetch))
                {
                    Debug.LogError(
                        $"Item on hold - ID: {itemOnHold.ItemID} Qty: {itemOnHold.ItemAmount} not found in ItemToFetch list.");
                    continue;
                }

                if (itemOnHold.ItemAmount > itemToFetch.ItemAmount)
                {
                    Debug.LogError($"Item quantity on hold - ID: {itemOnHold.ItemID} Qty: {itemOnHold.ItemAmount} " +
                                   "is somehow greater than "                                                       +
                                   $"item quantity in inventory - ID: {itemToFetch.ItemID} Qty: {itemToFetch.ItemAmount}.");

                    itemsToFetch.Remove(itemToFetch.ItemID);
                    continue;
                }

                if (itemOnHold.ItemAmount == itemToFetch.ItemAmount)
                {
                    itemsToFetch.Remove(itemToFetch.ItemID);
                    continue;
                }

                itemToFetch.ItemAmount -= itemOnHold.ItemAmount;
            }

            return itemsToFetch.Values.ToList();
        }
        
        public override List<Item> GetInventoryItemsToDeliver(InventoryData inventory)
        {
            var itemsToDeliver = new List<Item>();

            foreach (var itemID in _getDesiredItemIDs()
                         .Where(itemID => inventory.AllInventoryItems.ContainsKey(itemID)))
            {
                var item = inventory.AllInventoryItems[itemID];

                itemsToDeliver.Add(new Item(item));
            }

            if (itemsToDeliver.Count == 0) return new List<Item>();

            for (var i = 0; i < itemsToDeliver.Count; i++)
                if (!HasSpaceForItems(new List<Item> { itemsToDeliver[i] }))
                    itemsToDeliver.RemoveAt(i);

            return itemsToDeliver;
        }
    }
}