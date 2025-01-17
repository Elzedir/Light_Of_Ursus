using System;
using System.Collections.Generic;
using System.Linq;
using Actor;
using Interactable;
using Items;
using Priority;
using Tools;
using UnityEngine;

namespace Inventory
{
    public enum ComponentType
    {
        Actor,
        Station
    }

    [Serializable]
    public abstract class Inventory_Data : Priority_Updater
    {
        protected Inventory_Data(uint componentID, ComponentType componentType) : base(componentID, componentType)
        {
            AllInventoryItems                   =  new ObservableDictionary<uint, Item>();
            AllInventoryItems.DictionaryChanged += OnInventoryChanged;
        }

        public override Dictionary<string, string> GetStringData()
        {
            return AllInventoryItems.Values.ToDictionary(item => $"{item.ItemID}:",
                item => $"{item.ItemName} - Qty: {item.ItemAmount}");
        }

        public override DataToDisplay GetSubData(bool toggleMissingDataDebugs)
        {
            _updateDataDisplay(ref _dataToDisplay,
                title: "Inventory Items",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: AllInventoryItems.Values.ToDictionary(item => $"{item.ItemID}:",
                    item => $"{item.ItemName} - Qty: {item.ItemAmount}"));

            return _dataToDisplay;
        }

        public abstract ComponentType ComponentType { get; }

        public int Gold;
        bool _skipNextPriorityCheck;
        public void SkipNextPriorityCheck() => _skipNextPriorityCheck = true;
        public List<Item> AllInventoryItems_DataPersistence() => AllInventoryItems.Values.ToList();
        public ObservableDictionary<uint, Item> AllInventoryItems;

        void OnInventoryChanged(uint componentID)
        {
            if (_skipNextPriorityCheck)
            {
                _skipNextPriorityCheck = false;
                return;
            }

            _priorityChangeCheck(PriorityUpdateTrigger.ChangedInventory, true);
        }

        public Dictionary<uint, Item> GetAllInventoryItemsClone() =>
            AllInventoryItems.ToDictionary(entry => entry.Key, entry => new Item(entry.Value));

        public ObservableDictionary<uint, Item> GetAllObservableInventoryItemsClone()
        {
            var inventoryClone = GetAllInventoryItemsClone();
            var observableInventoryClone = new ObservableDictionary<uint, Item>();
            
            foreach (var (key, value) in inventoryClone)
            {
                observableInventoryClone.Add(key, value);
            }
            
            return observableInventoryClone;
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

        public void TransferItemsToTarget(Inventory_Data target, List<Item> items)
        {
            RemoveFromInventory(items);

            target.AddToInventory(items);
        }

        public bool DropItems(List<Item> items, Vector3 dropPosition, bool itemsNotInInventory = false,
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

        public bool InventoryContainsAnyItems(List<uint> itemIDs)
        {
            return itemIDs.Any(itemID => AllInventoryItems.ContainsKey(itemID));
        }

        public bool InventoryContainsAnyItems(List<Item> items)
        {
            return items.Any(item =>
                AllInventoryItems.TryGetValue(item.ItemID, out var existingItem) &&
                existingItem.ItemAmount >= item.ItemAmount);
        }

        public bool InventoryContainsAllItems(List<Item> requiredItems)
        {
            return requiredItems.All(requiredItem =>
                AllInventoryItems.TryGetValue(requiredItem.ItemID, out var existingItem)
                && existingItem.ItemAmount >= requiredItem.ItemAmount);
        }

        protected override Dictionary<PriorityUpdateTrigger, Dictionary<PriorityParameterName, object>>
            _priorityParameterList { get; set; } = new();

        public abstract List<Item> GetInventoryItemsToFetchFromStation();
        public abstract List<Item> GetInventoryItemsToDeliverFromInventory(Inventory_Data inventory);
        public abstract List<Item> GetInventoryItemsToDeliverFromOtherStations();
    }
}