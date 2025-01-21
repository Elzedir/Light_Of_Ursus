using System;
using System.Collections.Generic;
using System.Linq;
using Actor;
using ActorAction;
using Items;
using Priority;
using UnityEngine;

namespace Inventory
{
    [Serializable]
    public class InventoryData_Station : Inventory_Data
    {
        public InventoryData_Station(uint stationID) : base(stationID, ComponentType.Station)
        {
        }

        public override ComponentType         ComponentType      => ComponentType.Station;
        public          InventoryData_Station GetInventoryData() => this;

        public ComponentReference_Station StationReference => Reference as ComponentReference_Station;

        public uint
            MaxInventorySpace =
                10; // Implement a way to change the size depending on the station. Maybe StationComponent default value.

        List<uint> _getDesiredItemIDs() => StationReference.Station.DesiredStoredItemIDs;

        protected override bool _priorityChangeNeeded(object dataChanged) =>
            (PriorityUpdateTrigger)dataChanged == PriorityUpdateTrigger.ChangedInventory;

        public override bool HasSpaceForItems(List<Item> items)
        {
            if (Item.GetItemListTotal_CountAllItems(items) <= MaxInventorySpace) return true;

            //Debug.Log("Not enough space in inventory.");
            return false;
        }

        public override List<Item> GetInventoryItemsToFetchFromStation()
        {
            for (var i = 0; i < AllInventoryItems.Count; i++)
            {
                var item = AllInventoryItems.ElementAt(i);

                if (item.Value.ItemAmount >= 1) continue; // Clear items with 0 quantity.

                AllInventoryItems.Remove(item.Key);
            }

            var itemsToFetch = GetAllInventoryItemsClone();

            foreach (var itemID in _getDesiredItemIDs()) itemsToFetch.Remove(itemID);

            if (itemsToFetch.Count == 0) return new List<Item>();

            foreach (var itemOnHold in itemsToFetch.Values)
            {
                if (itemOnHold.ItemAmountOnHold > itemOnHold.ItemAmount)
                {
                    Debug.LogError($"Item quantity on hold - ID: {itemOnHold.ItemID} Qty: {itemOnHold.ItemAmount} " +
                                   "is somehow greater than " +
                                   $"item quantity in inventory - ID: {itemOnHold.ItemID} Qty: {itemOnHold.ItemAmount}." +
                                   $"Setting item quantity on hold to item quantity in inventory.");

                    itemOnHold.ItemAmountOnHold = itemOnHold.ItemAmount;
                    itemsToFetch.Remove(itemOnHold.ItemID);
                    continue;
                }

                if (itemOnHold.ItemAmountOnHold == itemOnHold.ItemAmount)
                {
                    itemsToFetch.Remove(itemOnHold.ItemID);
                    continue;
                }

                itemOnHold.ItemAmount -= itemOnHold.ItemAmountOnHold;
            }

            return itemsToFetch.Values.ToList();
        }

        public override List<Item> GetInventoryItemsToDeliverFromInventory(Inventory_Data inventory)
        {
            var itemsToDeliver = new List<Item>();

            foreach (var itemID in _getDesiredItemIDs().Where(itemID => inventory.AllInventoryItems.ContainsKey(itemID)))
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

        public override List<Item> GetInventoryItemsToDeliverFromOtherStations()
        {
            var itemsToDeliver = new List<Item>();

            foreach (var station in StationReference.Station.JobSite.JobSiteData.AllStationComponents)
            {
                if (station.Key == Reference.ComponentID) continue;

                var stationInventory = station.Value.Station_Data.InventoryData;

                foreach (var itemID in _getDesiredItemIDs().Where(itemID => stationInventory.AllInventoryItems.ContainsKey(itemID)))
                {
                    var item = stationInventory.AllInventoryItems[itemID];

                    itemsToDeliver.Add(new Item(item));
                }
            }

            return itemsToDeliver;
        }
        
        public override List<ActorActionName> GetAllowedActions()
        {
            return new List<ActorActionName>();
        }
    }
}