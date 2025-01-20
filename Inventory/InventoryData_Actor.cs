using System;
using System.Collections.Generic;
using Actor;
using Items;
using Priority;
using Tools;
using UnityEngine;

namespace Inventory
{
    [Serializable]
    public class InventoryData_Actor : Inventory_Data
    {
        public InventoryData_Actor(uint actorID, ObservableDictionary<uint, Item> allInventoryItems) : base(actorID, ComponentType.Actor)
        {
            AllInventoryItems = allInventoryItems;
        }
        
        public InventoryData_Actor(Inventory_Data inventoryData_Actor) : base(inventoryData_Actor.Reference.ComponentID, ComponentType.Actor)
        {
            AllInventoryItems = inventoryData_Actor.GetAllObservableInventoryItemsClone();
        }
        
        public override ComponentType       ComponentType      => ComponentType.Actor;
        public          InventoryData_Actor GetInventoryData() => this;

        public ComponentReference_Actor ActorReference => Reference as ComponentReference_Actor;

        protected override bool _priorityChangeNeeded(object dataChanged) => (PriorityUpdateTrigger)dataChanged == PriorityUpdateTrigger.ChangedInventory;

        float _availableCarryWeight;
        public float AvailableCarryWeight => _availableCarryWeight != 0 
            ? _availableCarryWeight 
            : ActorReference.Actor_Component.ActorData.StatsAndAbilities.Stats.AvailableCarryWeight;
        public override bool HasSpaceForItems(List<Item> items)
        {
            if (Item.GetItemListTotal_Weight(items) > AvailableCarryWeight)
            {
                Debug.Log("Too heavy for inventory.");
                return false;
            }

            return true;
        }

        public override List<Item> GetInventoryItemsToFetchFromStation()
        {
            Debug.LogError("Not implemented yet.");
            return null;
        }

        public override List<Item> GetInventoryItemsToDeliverFromInventory(Inventory_Data inventory)
        {
            Debug.LogError("Not implemented yet.");
            return null;
        }
        
        public override List<Item> GetInventoryItemsToDeliverFromOtherStations()
        {
            Debug.LogError("Not implemented yet.");
            return null;
        }
    }
}