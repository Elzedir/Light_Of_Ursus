using System;
using System.Collections.Generic;
using System.Linq;
using Actor;
using ActorActions;
using Items;
using Priority;
using UnityEngine;

namespace Inventory
{
    [Serializable]
    public class InventoryData_Station : InventoryData
    {
        public InventoryData_Station(ulong stationID) : base(stationID, ComponentType.Station)
        {
        }

        public override ComponentType         ComponentType      => ComponentType.Station;
        public          InventoryData_Station GetInventoryData() => this;

        public ComponentReference_Station StationReference => Reference as ComponentReference_Station;

        public ulong
            MaxInventorySpace =
                10; // Implement a way to change the size depending on the station. Maybe StationComponent default value.

        List<ulong> _getDesiredItemIDs() => StationReference.Station.DesiredStoredItemIDs;

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

        public override List<Item> GetInventoryItemsToDeliverFromInventory(InventoryData inventory_Actor)
        {
            var itemsToDeliver = inventory_Actor != null
                ? _getDesiredItemIDs()
                .Where(itemID => inventory_Actor.AllInventoryItems.ContainsKey(itemID))
                .Select(itemID => inventory_Actor.AllInventoryItems[itemID]).Select(item => new Item(item))
                .ToList()
                : new List<Item>();

            if (itemsToDeliver.Count == 0) return itemsToDeliver;

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

                itemsToDeliver.AddRange(_getDesiredItemIDs()
                    .Where(itemID => stationInventory.AllInventoryItems.ContainsKey(itemID))
                    .Select(itemID => stationInventory.AllInventoryItems[itemID]).Select(item => new Item(item)));
            }

            return itemsToDeliver;
        }

        public override List<Item> GetInventoryItemsToProcess(InventoryData inventory_Actor)
        {
            var itemsInStation = _getDesiredItemIDs()
                .Where(itemID => AllInventoryItems.ContainsKey(itemID))
                .Select(itemID => AllInventoryItems[itemID]).Select(item => new Item(item)).ToList();
            
            var itemsInActor = inventory_Actor != null 
                ? _getDesiredItemIDs()
                .Where(itemID => inventory_Actor.AllInventoryItems.ContainsKey(itemID))
                .Select(itemID => inventory_Actor.AllInventoryItems[itemID]).Select(item => new Item(item)).ToList()
                : new List<Item>();

            return Item.MergeItemLists(itemsInStation, itemsInActor);
        }
        
        public override List<ActorActionName> GetAllowedActions()
        {
            return new List<ActorActionName>();
        }
    }
}