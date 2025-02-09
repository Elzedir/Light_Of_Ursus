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

        public float AvailableCarryWeight => ActorReference.Actor_Component.ActorData.StatsAndAbilities.Stats.AvailableCarryWeight;

        public override bool HasSpaceForAllItem(ulong itemID, ulong itemAmount) =>
            itemID != 0
                ? Item.GetItemWeight(new Item(itemID, itemAmount)) <= AvailableCarryWeight
                : AvailableCarryWeight > 0;

        public override (Item AddedItem, Item ReturnedItem) HasSpaceForItem(ulong itemID, ulong itemAmount)
        {
            if (itemID is 0 || itemAmount is 0)
            {
                Debug.LogError($"Item ID: {itemID} or Item Amount: {itemAmount} is 0.");
                return (null, null);
            }

            var item = new Item(itemID, itemAmount);
            var itemWeight = Item.GetItemWeight(item);

            if (itemWeight == 0) Debug.LogError($"Item weight: {itemWeight} is 0 for item ID: {itemID}.");
            
            if (itemWeight <= AvailableCarryWeight) return (new Item(itemID, itemAmount), null);
            
            var fitAmount = (ulong)(AvailableCarryWeight / (double)itemWeight);
            var amountToAdd = Math.Min(fitAmount, itemAmount);

            var returnedAmount = itemAmount - amountToAdd;
            
            return (amountToAdd > 0 
                    ? new Item(itemID, amountToAdd) 
                    : null, 
                returnedAmount > 0 
                    ? new Item(itemID, returnedAmount) 
                    : null);
        }

        public override List<ActorActionName> GetAllowedActions()
        {
            return new List<ActorActionName>();
        }

        public override Dictionary<ulong, ulong> GetItemsToFetchFromThisInventory()
        {
            Debug.LogError("Not implemented yet.");
            return null;
        }
        
        public override Dictionary<ulong, Dictionary<ulong, ulong>> GetItemsToDeliverToThisInventoryFromAllStations(bool limitToAvailableInventoryCapacity = true)
        {
            Debug.LogError("Not implemented yet.");
            return null;
        }

        public override Dictionary<ulong, ulong> GetItemsToDeliverToThisInventory(InventoryData otherInventory, bool limitToAvailableInventoryCapacity = true)
        {
            Debug.LogError("Not implemented yet.");
            return null;
        }
    }
}