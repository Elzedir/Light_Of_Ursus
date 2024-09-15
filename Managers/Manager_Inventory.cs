using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Manager_Inventory : MonoBehaviour
{
    
}

public interface IInventoryOwner
{
    GameObject GetGameObject();
    InventoryData GetInventoryData();
}

public interface IStationInventory : IInventoryOwner
{
    Vector3 GetOperatingPosition();
    List<Item> GetStationYield(Actor_Base actor);
}

[Serializable]
public class InventoryData
{
    public IInventoryOwner InventoryOwner;
    public int Gold = 0;
    public List<Item> AllInventoryItems;

    public InventoryData(IInventoryOwner inventoryOwner, List<Item> allInventoryItems)
    {
        InventoryOwner = inventoryOwner;
        AllInventoryItems = allInventoryItems;
    }

    public Item GetItemFromInventory(int itemID)
    {
        return AllInventoryItems.FirstOrDefault(i => i.ItemID == itemID);
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
            var existingItems = AllInventoryItems.Where(i => i.ItemID == item.ItemID).ToList();
            int amountToAdd = item.ItemAmount;

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
                int amountAdded = Math.Min(amountToAdd, item.MaxStackSize);

                var newItem = new Item(item.ItemID, amountAdded);

                AllInventoryItems.Add(newItem);

                amountToAdd -= amountAdded;
            }
        }

        void addToExistingItems(List<Item> existingItems, ref int amountToAdd)
        {
            foreach (var stackItem in existingItems.OrderBy(i => i.ItemAmount))
            {
                if (amountToAdd <= 0) break;

                int availableSpace = stackItem.MaxStackSize - stackItem.ItemAmount;

                if (availableSpace > 0)
                {
                    int amountAdded = Math.Min(amountToAdd, availableSpace);
                    stackItem.ItemAmount += amountAdded;
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
            var existingItems = AllInventoryItems.Where(i => i.ItemID == item.ItemID).ToList();

            if (!existingItems.Any()) return false;

            if (existingItems.Sum(i => i.ItemAmount) < item.ItemAmount) return false;

            int amountToRemove = item.ItemAmount;

            foreach (var stackItem in existingItems.OrderBy(i => i.ItemAmount))
            {
                if (amountToRemove <= 0) break;

                if (stackItem.ItemAmount <= amountToRemove)
                {
                    amountToRemove -= stackItem.ItemAmount;
                    AllInventoryItems.Remove(stackItem);
                }
                else
                {
                    stackItem.ItemAmount -= amountToRemove;
                    amountToRemove = 0;
                }
            }

            return true;
        }
    }

    public bool TransferItemFromInventory(InventoryData target, List<Item> items)
    {
        Debug.Log(string.Join(", ", items.Select(i => $"{i.ItemName}: {i.ItemAmount}")));
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
            DropItems(items, InventoryOwner.GetGameObject().transform.position, itemsNotInInventory: true);
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
                    for (int i = 0; i < item.ItemAmount; i++)
                    {
                        Interactable_Item.CreateNewItem(new Item(item.ItemID, 1), dropPosition);
                    }
                }
            }

            return true;

            // Later will have things like having available space, etc.
        }
    }

    public List<Item> InventoryMissingAnyItems(List<Item> items)
    {
        List<Item> missingItems = new();

        foreach (var item in items)
        {
            var existingItems = AllInventoryItems.Where(i => i.ItemID == item.ItemID).ToList();

            if (!existingItems.Any() || existingItems.Sum(i => i.ItemAmount) < item.ItemAmount)
            {
                missingItems.Add(item);
            }
        }

        return missingItems;
    }

    public List<Item> InventoryContainsAnyItems(List<int> itemIDs) => AllInventoryItems.Where(i => itemIDs.Contains(i.ItemID)).ToList();

    public bool InventoryContainsAllItems(List<Item> items)
    {
        foreach (var item in items)
        {
            if (!AllInventoryItems.Any(i => i.ItemID == item.ItemID && i.ItemAmount >= item.ItemAmount))
            {
                return false;
            }
        }

        return true;
    }
}
