using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Manager_Inventory : MonoBehaviour
{
    
}

public interface IInventoryOwner
{
    GameObject GameObject { get; }
    InventoryData InventoryData { get; }
    void InitialiseInventoryComponent();
}

public interface IStationInventory : IInventoryOwner
{
    StationName StationName { get; }
    Vector3 GetOperatingPosition();
    List<Item> GetStationYield(Actor_Base actor);
}

[Serializable]
public class InventoryData
{
    public IInventoryOwner InventoryOwner;
    public int Gold = 0;
    public List<Item> InventoryItems;

    public InventoryData(IInventoryOwner inventoryOwner, List<Item> inventory)
    {
        InventoryOwner = inventoryOwner;
        InventoryItems = inventory;
    }

    public Item ItemInInventory(int itemID)
    {
        return Manager_Item.GetItem(itemID, InventoryItems.Where(i => i.CommonStats_Item.ItemID == itemID).Sum(i => i.CommonStats_Item.CurrentStackSize));
    }

    public bool AddToInventory(List<Item> items)
    {
        bool addedAllItems = true;
        List<Item> tempAddedItems = new();

        foreach (Item itemToAdd in items)
        {
            if (addItem(itemToAdd))
            {
                tempAddedItems.Add(itemToAdd);
            }
            else
            {
                addedAllItems = false;
                break;
            }
        }

        if (!addedAllItems)
        {
            RemoveFromInventory(tempAddedItems);
            tempAddedItems.Clear();

            return false;
        }

        return true;

        bool addItem(Item item)
        {
            var existingItems = InventoryItems.Where(i => i.CommonStats_Item.ItemID == item.CommonStats_Item.ItemID).ToList();
            int amountToAdd = item.CommonStats_Item.CurrentStackSize;

            if (existingItems.Any())
            {
                addToExistingItems(existingItems, ref amountToAdd);
            }

            if (amountToAdd > 0)
            {
                addNewItems(amountToAdd, item);
            }

            return true;
        }

        void addNewItems(int amountToAdd, Item item)
        {
            while (amountToAdd > 0)
            {
                int amountAdded = Math.Min(amountToAdd, item.CommonStats_Item.MaxStackSize);

                var newItem = Manager_Item.GetItem(item.CommonStats_Item.ItemID, amountAdded);

                InventoryItems.Add(newItem);

                amountToAdd -= amountAdded;
            }
        }

        void addToExistingItems(List<Item> existingItems, ref int amountToAdd)
        {
            foreach (var stackItem in existingItems.OrderBy(i => i.CommonStats_Item.CurrentStackSize))
            {
                if (amountToAdd <= 0) break;

                int availableSpace = stackItem.CommonStats_Item.MaxStackSize - stackItem.CommonStats_Item.CurrentStackSize;

                if (availableSpace > 0)
                {
                    int amountAdded = Math.Min(amountToAdd, availableSpace);
                    stackItem.CommonStats_Item.CurrentStackSize += amountAdded;
                    amountToAdd -= amountAdded;
                }
            }
        }
    }

    public bool RemoveFromInventory(List<Item> items)
    {
        bool removedAllItems = true;
        List<Item> tempRemovedItems = new();

        foreach (Item itemToRemove in items)
        {
            if (removeItem(itemToRemove))
            {
                tempRemovedItems.Add(itemToRemove);
            }
            else
            {
                removedAllItems = false;
                break;
            }
        }

        if (!removedAllItems)
        {
            AddToInventory(tempRemovedItems);
            tempRemovedItems.Clear();

            Debug.Log("Couldn't remove all items.");

            return false;
        }

        return true;

        bool removeItem(Item item)
        {
            var existingItems = InventoryItems.Where(i => i.CommonStats_Item.ItemID == item.CommonStats_Item.ItemID).ToList();

            if (!existingItems.Any()) return false;

            if (existingItems.Sum(i => i.CommonStats_Item.CurrentStackSize) < item.CommonStats_Item.CurrentStackSize) return false;

            int amountToRemove = item.CommonStats_Item.CurrentStackSize;

            foreach (var stackItem in existingItems.OrderBy(i => i.CommonStats_Item.CurrentStackSize))
            {
                if (amountToRemove <= 0) break;

                if (stackItem.CommonStats_Item.CurrentStackSize <= amountToRemove)
                {
                    amountToRemove -= stackItem.CommonStats_Item.CurrentStackSize;
                    InventoryItems.Remove(stackItem);
                }
                else
                {
                    stackItem.CommonStats_Item.CurrentStackSize -= amountToRemove;
                    amountToRemove = 0;
                }
            }

            return true;
        }
    }

    public bool TransferItemFromInventory(InventoryData target, List<Item> items)
    {
        Debug.Log(string.Join(", ", items.Select(i => $"{i.CommonStats_Item.ItemName}: {i.CommonStats_Item.CurrentStackSize}")));
        Debug.Log(target);

        if (!RemoveFromInventory(items))
        {
            Debug.Log("Can't remove items from inventory to transfer.");

            return false;
        }

        if (target.AddToInventory(items))
        {
            return true;
        }

        Debug.Log("Can't add items to target inventory");

        if (!AddToInventory(items))
        {
            DropItems(items, InventoryOwner.GameObject.transform.position, itemsNotInInventory: true);
            Debug.Log("Took items out of inventory and can't put them back");
        }

        return false;
    }

    public bool DropItems(List<Item> items, Vector3 dropPosition, bool itemsNotInInventory = false, bool dropAsGroup = true)
    {
        if (itemsNotInInventory)
        {
            if (!dropItems())
            {
                Debug.Log("Can't drop items.");
                return false;
            }

            return true;
        }

        if (!RemoveFromInventory(items))
        {
            Debug.Log("Can't remove items from inventory to drop.");
            return false;
        }

        if (!dropItems())
        {
            Debug.Log("Can't drop items.");
            return false;
        }

        return true;

        bool dropItems()
        {
            foreach (Item item in items)
            {
                if (dropAsGroup)
                {
                    Interactable_Item.CreateNewItem(item, dropPosition);
                }
                else
                {
                    for (int i = 0; i < item.CommonStats_Item.CurrentStackSize; i++)
                    {
                        Interactable_Item.CreateNewItem(Manager_Item.GetItem(item.CommonStats_Item.ItemID), dropPosition);
                    }
                }
            }

            return true;

            // Later will have things like having available space, etc.
        }
    }

    public bool InventoryContainsAllItems(List<Item> items)
    {
        foreach(var item in items)
        {
            var existingItems = InventoryItems.Where(i => i.CommonStats_Item.ItemID == item.CommonStats_Item.ItemID).ToList();

            if (!existingItems.Any()) return false;

            if (existingItems.Sum(i => i.CommonStats_Item.CurrentStackSize) < item.CommonStats_Item.CurrentStackSize) return false;
        }

        return true;
    }
}
