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
    public class InventoryDataPreset_Actor : Inventory_Data_Preset
    {
        public InventoryDataPreset_Actor(uint actorID, ObservableDictionary<uint, Item> allInventoryItems) : base(actorID, ComponentType.Actor)
        {
            AllInventoryItems = allInventoryItems;
        }
        
        public InventoryDataPreset_Actor(Inventory_Data_Preset inventoryDataPreset_Actor) : base(inventoryDataPreset_Actor.Reference.ComponentID, ComponentType.Actor)
        {
            AllInventoryItems = inventoryDataPreset_Actor.GetAllObservableInventoryItemsClone();
        }
        
        public override ComponentType       ComponentType      => ComponentType.Actor;
        public          InventoryDataPreset_Actor GetInventoryData() => this;

        public ComponentReference_Actor ActorReference => Reference as ComponentReference_Actor;

        protected override bool _priorityChangeNeeded(object dataChanged) => (PriorityUpdateTrigger)dataChanged == PriorityUpdateTrigger.ChangedInventory;

        float _availableCarryWeight;
        public float AvailableCarryWeight => _availableCarryWeight != 0 
            ? _availableCarryWeight 
            : ActorReference.Actor_Component.ActorData.StatsAndAbilitiesPreset.ActorStats.AvailableCarryWeight;
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

        public override List<Item> GetInventoryItemsToDeliverFromInventory(Inventory_Data_Preset inventory)
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