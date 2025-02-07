using System;
using System.Collections.Generic;
using ActorActions;
using Items;
using Tools;
using UnityEngine;

namespace Inventory
{
    [Serializable]
    public class InventoryData_Actor : InventoryData
    {
        public InventoryData_Actor(ulong actorID, ObservableDictionary<ulong, Item> allInventoryItems) : base(actorID, ComponentType.Actor)
        {
            SetInventory(allInventoryItems ?? new ObservableDictionary<ulong, Item>());
            AllInventoryItems.DictionaryChanged += OnInventoryChanged;
        }
        
        public InventoryData_Actor(InventoryData inventoryData_Actor) : base(inventoryData_Actor.Reference.ComponentID, ComponentType.Actor)
        {
            SetInventory(inventoryData_Actor.GetAllObservableInventoryItemsClone());
        }
        
        public override ComponentType       ComponentType      => ComponentType.Actor;
        public          InventoryData_Actor GetInventoryData() => this;

        public ComponentReference_Actor ActorReference => Reference as ComponentReference_Actor;

        float _availableCarryWeight;
        public float AvailableCarryWeight => _availableCarryWeight != 0 
            ? _availableCarryWeight 
            : ActorReference.Actor_Component.ActorData.StatsAndAbilities.Stats.AvailableCarryWeight;

        public override bool HasSpaceForItem(ulong itemID, ulong itemAmount) =>
            itemID != 0
                ? Item.GetItemWeight(new Item(itemID, itemAmount)) <= AvailableCarryWeight
                : AvailableCarryWeight > 0;

        public override Item GetUnaddedItem(ulong itemID, ulong itemAmount)
        {
            if (itemID is 0) return null;
            
            var item = new Item(itemID, itemAmount);

            var itemWeight = Item.GetItemWeight(item);
            
            if (itemWeight <= AvailableCarryWeight) 
                return null;
            
            var fitAmount = (ulong)(item.ItemAmount * (AvailableCarryWeight / itemWeight));

            return fitAmount == 0 
                ? item 
                : new Item(item.ItemID, item.ItemAmount - fitAmount);
        }

        public override List<ActorActionName> GetAllowedActions()
        {
            return new List<ActorActionName>();
        }

        public override Dictionary<ulong, ulong> GetItemsToFetchFromStation()
        {
            Debug.LogError("Not implemented yet.");
            return null;
        }
        
        public override Dictionary<ulong, Dictionary<ulong, ulong>> GetItemsToDeliverFromOtherStations(bool limitToAvailableInventoryCapacity = true)
        {
            Debug.LogError("Not implemented yet.");
            return null;
        }

        public override Dictionary<ulong, ulong> GetItemsToDeliverFromActor(InventoryData inventory_Actor, bool limitToAvailableInventoryCapacity = true)
        {
            Debug.LogError("Not implemented yet.");
            return null;
        }

        public override Dictionary<ulong, ulong> GetInventoryItemsToProcess(InventoryData inventory_Actor)
        {
            Debug.LogError("Not implemented yet.");
            return null;
        }
    }
}