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
    public class InventoryData_Actor : InventoryData
    {
        public InventoryData_Actor(uint actorID, ObservableDictionary<uint, Item> allInventoryItems) : base(actorID, ComponentType.Actor)
        {
            AllInventoryItems = allInventoryItems;
        }
        
        public override ComponentType       ComponentType      => ComponentType.Actor;
        public          InventoryData_Actor GetInventoryData() => this;

        public ComponentReference_Actor ActorReference => Reference as ComponentReference_Actor;

        protected override bool _priorityChangeNeeded(object dataChanged) => (PriorityUpdateTrigger)dataChanged == PriorityUpdateTrigger.ChangedInventory;

        float _availableCarryWeight;
        public float AvailableCarryWeight => _availableCarryWeight != 0 
            ? _availableCarryWeight 
            : ActorReference.Actor.ActorData.StatsAndAbilities.ActorStats.AvailableCarryWeight;
        public override bool HasSpaceForItems(List<Item> items)
        {
            if (Item.GetItemListTotal_Weight(items) > AvailableCarryWeight)
            {
                Debug.Log("Too heavy for inventory.");
                return false;
            }

            return true;
        }

        public override List<Item> GetInventoryItemsToFetch()
        {
            Debug.LogError("Not implemented yet.");
            return null;
        }

        public override List<Item> GetInventoryItemsToDeliver(InventoryData inventory)
        {
            Debug.LogError("Not implemented yet.");
            return null;
        }
    }
}