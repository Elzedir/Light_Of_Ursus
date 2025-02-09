using System;
using System.Collections.Generic;
using System.Linq;
using Actors;
using Interactable;
using Items;
using Priorities;
using Tools;
using UnityEngine;

namespace Inventory
{
    public enum ComponentType
    {
        None,
        
        Actor,
        Station
    }

    [Serializable]
    public abstract class InventoryData : Priority_Class
    {
        protected InventoryData(ulong componentID, ComponentType componentType) : base(componentID, componentType) { }

        public override Dictionary<string, string> GetStringData()
        {
            return AllInventoryItems.Values.ToDictionary(item => $"{item.ItemID}:",
                item => $"{item.ItemName} - Qty: {item.ItemAmount}");
        }

        public override DataToDisplay GetDataToDisplay(bool toggleMissingDataDebugs)
        {
            _updateDataDisplay(DataToDisplay,
                title: "Inventory Items",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: AllInventoryItems.Values.ToDictionary(item => $"{item.ItemID}:",
                    item => $"{item.ItemName} - Qty: {item.ItemAmount}"));

            return DataToDisplay;
        }

        public abstract ComponentType ComponentType { get; }

        public int Gold;
        bool _skipNextPriorityCheck;
        public void SkipNextPriorityCheck() => _skipNextPriorityCheck = true;
        public List<Item> AllInventoryItems_DataPersistence() => AllInventoryItems.Values.ToList();
        [SerializeField] ObservableDictionary<ulong, Item> _allInventoryItems;
        public ObservableDictionary<ulong, Item> AllInventoryItems => _allInventoryItems;

        protected void OnInventoryChanged(ulong componentID)
        {
            if (_skipNextPriorityCheck)
            {
                _skipNextPriorityCheck = false;
                return;
            }

            PriorityData.RegenerateAllPriorities(DataChangedName.ChangedInventory);
        }

        public Dictionary<ulong, Item> GetAllInventoryItemsClone() =>
            AllInventoryItems.ToDictionary(entry => entry.Key, entry => new Item(entry.Value));

        public ObservableDictionary<ulong, Item> GetAllObservableInventoryItemsClone()
        {
            var inventoryClone = GetAllInventoryItemsClone();
            var observableInventoryClone = new ObservableDictionary<ulong, Item>();
            
            foreach (var (key, value) in inventoryClone)
            {
                observableInventoryClone.Add(key, value);
            }
            
            return observableInventoryClone;
        }

        public void SetInventory(ObservableDictionary<ulong, Item> allInventoryItems, bool skipPriorityCheck = false)
        {
            if (skipPriorityCheck) SkipNextPriorityCheck();

            if (allInventoryItems is null)
            {
                Debug.LogWarning("All inventory items is null. Creating new inventory.");
                allInventoryItems = new ObservableDictionary<ulong, Item>();
            }
            
            _allInventoryItems ??= new ObservableDictionary<ulong, Item>();
            
            AllInventoryItems.Clear();

            foreach (var item in allInventoryItems.Values.Where(item => !AllInventoryItems.TryAdd(item.ItemID, item)))
                Debug.LogError($"Failed to add item {item.ItemID} to inventory.");
        }

        public bool HasSpaceForAllItemList(Dictionary<ulong, ulong> items = null)
            => items?.All(
                item => HasSpaceForAllItem(item.Key, item.Value)) ?? HasSpaceForAllItem(0, 0);
        public abstract bool HasSpaceForAllItem(ulong itemID, ulong itemAmount);

        public (List<Item> addedItemList, List<Item> returnedItemList) HasSpaceForItemList(
            Dictionary<ulong, ulong> items = null)
        {
            var addedItemList = new List<Item>();
            var returnedItemList = new List<Item>();

            if (items == null) return (addedItemList, returnedItemList);
            
            foreach (var item in items)
            {
                var (addedItem, returnedItem) = HasSpaceForItem(item.Key, item.Value);
                
                if (addedItem is null && returnedItem is null)
                {
                    Debug.LogError("Both added and returned items null.");
                    continue;
                }

                if (addedItem != null)
                    addedItemList.Add(addedItem);

                if (returnedItem != null)
                    returnedItemList.Add(returnedItem);
            }

            return (addedItemList, returnedItemList);
        }
        public abstract (Item AddedItem, Item ReturnedItem) HasSpaceForItem(ulong itemID, ulong itemAmount);

        //* Later, allow overburdening, to a percentage.
        public List<Item> AddToInventory(Dictionary<ulong, ulong> items) => 
            items.Select(item => _addItem(item.Key, item.Value)).ToList();

        Item _addItem(ulong itemID, ulong itemAmount)
        {
            if (itemID == 0 || itemAmount == 0)
            {
                Debug.LogError($"ItemID: {itemID} or ItemAmount: {itemAmount} is 0.");
                return new Item(itemID, itemAmount);
            }

            var itemToAdd = HasSpaceForItem(itemID, itemAmount);
            
            if (itemToAdd.AddedItem is null)
            {
                Debug.LogError("Added items are null.");
                return new Item(itemID, itemAmount);
            }

            if (!AllInventoryItems.TryGetValue(itemID, out var existingItem))
            {
                AllInventoryItems.Add(itemToAdd.AddedItem.ItemID, itemToAdd.AddedItem);
                return itemToAdd.ReturnedItem;
            }

            existingItem.ItemAmount += itemToAdd.AddedItem.ItemAmount;
            return itemToAdd.ReturnedItem;
        }

        public void RemoveFromInventory(Dictionary<ulong, ulong> items)
        {
            foreach (var _ in items.Where(itemToRemove => !_removeItem(itemToRemove.Key, itemToRemove.Value)))
            {
                break;
            }
        }

        bool _removeItem(ulong itemID, ulong itemAmount)
        {
            if (!AllInventoryItems.TryGetValue(itemID, out var existingItem))
            {
                //Debug.LogWarning($"Item {item.ItemID} - {item.ItemAmount} not found in inventory.");

                return false;
            }

            existingItem.ItemAmount -= itemAmount;

            if (existingItem.ItemAmount <= 0) AllInventoryItems.Remove(itemID);

            return true;
        }

        public List<Item> TransferItemsToTarget(InventoryData target, Dictionary<ulong, ulong> items)
        {
            RemoveFromInventory(items);

            return target.AddToInventory(items);
        }

        public bool DropItems(Dictionary<ulong, ulong> items, Vector3 dropPosition, bool itemsNotInInventory = false,
                              bool       dropAsGroup = true)
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

        bool _dropItems(Dictionary<ulong, ulong> items, Vector3 dropPosition, bool dropAsGroup)
        {
            foreach (var item in items)
            {
                if (dropAsGroup)
                {
                    Interactable_Item.CreateNewItem(new Item(item.Key, item.Value), dropPosition);
                }
                else
                {
                    for (ulong i = 0; i < item.Value; i++)
                    {
                        Interactable_Item.CreateNewItem(new Item(item.Key, 1), dropPosition);
                    }
                }
            }

            return true;

            // Later will have things like having available space, etc.
        }

        public List<Item> InventoryContainsReturnedItems(HashSet<ulong> itemIDs)
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
                if (AllInventoryItems.TryGetValue(item.ItemID, out var existingItem) &&
                    existingItem.ItemAmount >= item.ItemAmount)
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

        public bool InventoryContainsAnyItemIDs(List<ulong> itemIDs) => 
            itemIDs.Any(
                itemID => AllInventoryItems.ContainsKey(itemID));

        public bool InventoryContainsAnyItems(Dictionary<ulong, ulong> items) =>
            items.Any(
                item => AllInventoryItems.TryGetValue(item.Key, out var existingItem) 
                        && existingItem.ItemAmount >= item.Value);

        public bool InventoryContainsAllItems(Dictionary<ulong, ulong> items) =>
            items.All(
                item => AllInventoryItems.TryGetValue(item.Key, out var existingItem)
                                && existingItem.ItemAmount >= item.Value);

        public abstract Dictionary<ulong, ulong> GetItemsToFetchFromThisInventory();
        public abstract Dictionary<ulong, Dictionary<ulong, ulong>> GetItemsToDeliverToThisInventoryFromAllStations(bool limitToAvailableInventoryCapacity = true);
        public abstract Dictionary<ulong, ulong> GetItemsToDeliverToThisInventory(InventoryData otherInventory, bool limitToAvailableInventoryCapacity = true);
    }
}