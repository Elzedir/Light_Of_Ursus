using System;
using System.Collections.Generic;
using System.Linq;
using Actors;
using Items;
using Priority;
using Tools;
using UnityEngine;

namespace Managers
{
    public class Manager_Inventory : MonoBehaviour
    {
    
    }

    public enum ComponentType
    {
        Actor,
        Station
    }

    [Serializable]
    public abstract class InventoryData : PriorityData
    {
        protected InventoryData(uint componentID, ComponentType componentType) : base(componentID, componentType)
        {
            AllInventoryItems                   =  new ObservableDictionary<uint, Item>();
            AllInventoryItems.DictionaryChanged += OnInventoryChanged;
        }

        public abstract ComponentType ComponentType { get; }

        public int                       Gold;
        bool                             _skipNextPriorityCheck;
        public void                      SkipNextPriorityCheck()           => _skipNextPriorityCheck = true;
        public List<Item>                AllInventoryItems_DataPersistence() => AllInventoryItems.Values.ToList(); 
        public ObservableDictionary<uint, Item> AllInventoryItems;

        void OnInventoryChanged(uint componentID)
        {
            if (_skipNextPriorityCheck)
            {
                _skipNextPriorityCheck = false;
                return;
            }

            _priorityChangeCheck(DataChanged.ChangedInventory, true);
        }

        public Dictionary<uint, Item> GetAllInventoryItemsClone() => AllInventoryItems.ToDictionary(entry => entry.Key, entry => new Item(entry.Value));
        
        public Dictionary<uint, Item> FetchItemsOnHold   = new();
        public Dictionary<uint, Item> DeliverItemsOnHold = new();
        public void AddToInventoryItemsOnHold(List<Item> itemsToAdd)
        {
            if (itemsToAdd == null) return;

            foreach (var itemToAdd in itemsToAdd)
            {
                if (FetchItemsOnHold.TryGetValue(itemToAdd.ItemID, out var itemOnHold))
                {
                    itemOnHold.ItemAmount += itemToAdd.ItemAmount;
                }
                else
                {
                    FetchItemsOnHold.Add(itemToAdd.ItemID, new Item(itemToAdd));
                }
            }
        }

        public void RemoveFromFetchItemsOnHold(List<Item> itemsToRemove)
        {
            if (itemsToRemove is null) return;

            foreach (var itemToRemove in itemsToRemove)
            {
                if (!FetchItemsOnHold.TryGetValue(itemToRemove.ItemID, out var itemOnHold)) continue;

                if (itemOnHold.ItemAmount <= itemToRemove.ItemAmount)
                    FetchItemsOnHold.Remove(itemToRemove.ItemID);
                else
                    itemOnHold.ItemAmount -= itemToRemove.ItemAmount;
            }
        }

        public void RemoveFromDeliverItemsOnHold(List<Item> itemsToRemove)
        {
            if (itemsToRemove is null) return;
            
            foreach (var itemToRemove in itemsToRemove)
            {
                if (!DeliverItemsOnHold.TryGetValue(itemToRemove.ItemID, out var itemOnHold)) continue;

                if (itemOnHold.ItemAmount <= itemToRemove.ItemAmount)
                    DeliverItemsOnHold.Remove(itemToRemove.ItemID);
                else
                    itemOnHold.ItemAmount -= itemToRemove.ItemAmount;
            }
        }

        public void SetInventory(ObservableDictionary<uint, Item> allInventoryItems, bool skipPriorityCheck = false)
        {
            if (skipPriorityCheck) SkipNextPriorityCheck();
            AllInventoryItems = allInventoryItems;
        }

        public          Item GetItemFromInventory(uint   itemID) => new(AllInventoryItems.GetValueOrDefault(itemID));
        public abstract bool HasSpaceForItems(List<Item> items);

        public void AddToInventory(List<Item> items)
        {
            foreach (var _ in items.Where(itemToAdd => !_addItem(itemToAdd)))
            {
                break;
            }
        }
    
        bool _addItem(Item item)
        {
            if (item.ItemAmount == 0)
            {
                Debug.LogError("Trying to add item with 0 quantity.");
                return false;    
            }
            
            if (!AllInventoryItems.TryGetValue(item.ItemID, out var existingItem))
            {
                AllInventoryItems.Add(item.ItemID, item);
            
                return true;
            }
        
            existingItem.ItemAmount += item.ItemAmount;

            return true;
        }

        public void RemoveFromInventory(List<Item> items)
        {
            foreach (var _ in items.Where(itemToRemove => !_removeItem(itemToRemove)))
            {
                break;
            }
        }

        bool _removeItem(Item item)
        {
            if (!AllInventoryItems.TryGetValue(item.ItemID, out var existingItem))
            {
                //Debug.LogWarning($"Item {item.ItemID} - {item.ItemAmount} not found in inventory.");

                return false;
            }

            existingItem.ItemAmount -= item.ItemAmount;

            if (existingItem.ItemAmount <= 0) AllInventoryItems.Remove(item.ItemID);

            return true;
        }

        public void TransferItemsToTarget(InventoryData target, List<Item> items)
        {
            RemoveFromInventory(items);

            target.AddToInventory(items);
        }

        public bool DropItems(List<Item> items, Vector3 dropPosition, bool itemsNotInInventory = false, bool dropAsGroup = true)
        {
            if (itemsNotInInventory)
            {
                if (_dropItems(items, dropPosition, dropAsGroup)) return true;
                
                Debug.Log("Can't drop items.");
                return false;

            }
        
            if (!_dropItems(items, dropPosition, dropAsGroup))
            {
                Debug.Log("Can't drop items.");
                return false;
            }
        
            RemoveFromInventory(items);

            return true;
        }
    
        bool _dropItems(List<Item> items, Vector3 dropPosition, bool dropAsGroup)
        {
            foreach (Item item in items)
            {
                if (dropAsGroup)
                {
                    Interactable_Item.CreateNewItem(new Item(item), dropPosition);
                }
                else
                {
                    for (int i = 0; i < item.ItemAmount; i++)
                    {
                        Interactable_Item.CreateNewItem(new Item(item.ItemID, 1), dropPosition);
                    }
                }
            }

            return true;

            // Later will have things like having available space, etc.
        }

        public List<Item> InventoryContainsReturnedItems(HashSet<uint> itemIDs)
        {
            List<Item> returnedItems = new();

            foreach (var itemID in itemIDs)
            {
                if (AllInventoryItems.TryGetValue(itemID, out var item))
                {
                    returnedItems.Add(new Item(item));
                }
            }

            return returnedItems;
        }
        
        public List<Item> InventoryContainsReturnedItems(List<Item> items)
        {
            List<Item> returnedItems = new();

            foreach (var item in items)
            {
                if (AllInventoryItems.TryGetValue(item.ItemID, out var existingItem) && existingItem.ItemAmount >= item.ItemAmount)
                {
                    returnedItems.Add(new Item(item));
                }
            }

            return returnedItems;
        }

        public List<Item> InventoryMissingReturnedItems(List<Item> itemsToCheck)
        {
            List<Item> missingItems = new();
        
            foreach (var itemToCheck in itemsToCheck)
            {
                if (!AllInventoryItems.TryGetValue(itemToCheck.ItemID, out var existingItem))
                {
                    missingItems.Add(new Item(itemToCheck));
                }
                else if (existingItem.ItemAmount < itemToCheck.ItemAmount)
                {
                    missingItems.Add(new Item(itemToCheck.ItemID, itemToCheck.ItemAmount - existingItem.ItemAmount));
                }
            }

            return missingItems;
        }

        public bool InventoryContainsAnyItems(List<uint> itemIDs)
        {  
            return itemIDs.Any(itemID => AllInventoryItems.ContainsKey(itemID));
        }
        
        public bool InventoryContainsAnyItems(List<Item> items)
        {
            return items.Any(item => AllInventoryItems.TryGetValue(item.ItemID, out var existingItem) && existingItem.ItemAmount >= item.ItemAmount);
        }
        public bool InventoryContainsAllItems(List<Item> requiredItems)
        {
            return requiredItems.All(requiredItem => 
                AllInventoryItems.TryGetValue(requiredItem.ItemID, out var existingItem) 
                && existingItem.ItemAmount >= requiredItem.ItemAmount);
        }

        protected override Dictionary<DataChanged, Dictionary<PriorityParameterName, object>> _priorityParameterList
        {
            get;
            set;
        } = new();

        public abstract List<Item> GetInventoryItemsToFetch();
        public abstract List<Item> GetInventoryItemsToDeliver(InventoryData inventory);
    }

    [Serializable]
    public class InventoryData_Actor : InventoryData
    {
        public InventoryData_Actor(uint actorID) : base(actorID, ComponentType.Actor) { }
        public override ComponentType       ComponentType      => ComponentType.Actor;
        public          InventoryData_Actor GetInventoryData() => this;

        public ComponentReference_Actor ActorReference => Reference as ComponentReference_Actor;

        protected override bool _priorityChangeNeeded(object dataChanged) => (DataChanged)dataChanged == DataChanged.ChangedInventory;

        float _availableCarryWeight;
        public float AvailableCarryWeight => _availableCarryWeight != 0 
            ? _availableCarryWeight 
            : ActorReference.Actor.ActorData.StatsAndAbilities.Actor_Stats.AvailableCarryWeight;
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

    [Serializable]
    public class InventoryData_Station : InventoryData
    {
        public InventoryData_Station(uint stationID) : base(stationID, ComponentType.Station) { }
        public override ComponentType         ComponentType      => ComponentType.Station;
        public          InventoryData_Station GetInventoryData() => this;

        public ComponentReference_Station StationReference => Reference as ComponentReference_Station;
        
        public          uint              MaxInventorySpace = 10; // Implement a way to change the size depending on the station. Maybe StationComponent default value.
        HashSet<uint>                     _getDesiredItemIDs()                      => StationReference.Station.DesiredStoredItemIDs;
        protected override bool           _priorityChangeNeeded(object dataChanged) => (DataChanged)dataChanged == DataChanged.ChangedInventory;
    
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