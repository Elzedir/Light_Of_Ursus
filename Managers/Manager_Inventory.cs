using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Manager_Inventory : MonoBehaviour
{
    
}

public class InventoryComponent
{
    public Actor_Base Actor;
    public List<Item> Inventory;

    public InventoryComponent(Actor_Base actor, List<Item> inventory)
    {
        Actor = actor;
        Inventory = inventory;
    }

    public Item ItemInInventory(int itemID)
    {
        int totalStackSize = Inventory
        .Where(i => i.CommonStats_Item.ItemID == itemID)
        .Sum(i => i.CommonStats_Item.CurrentStackSize);

        Item itemToReturn = Manager_Item.GetItem(itemID);
        itemToReturn.CommonStats_Item.CurrentStackSize = totalStackSize;

        return itemToReturn;
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
            var existingItems = Inventory.Where(i => i.CommonStats_Item.ItemID == item.CommonStats_Item.ItemID).ToList();
            int amountToAdd = item.CommonStats_Item.CurrentStackSize;

            if (!existingItems.Any())
            {
                addNewItems(amountToAdd, item);
            }
            else
            {
                addToExistingItems(existingItems, amountToAdd);
                addNewItems(amountToAdd, item);
            }

            return true;
        }
        
        void addNewItems(int amountToAdd, Item item)
        {
            while (amountToAdd > 0)
            {
                int amountAdded = Math.Min(amountToAdd, item.CommonStats_Item.MaxStackSize);

                var newItem = Manager_Item.GetItem(item.CommonStats_Item.ItemID);
                newItem.CommonStats_Item.CurrentStackSize = amountAdded;

                Inventory.Add(newItem);

                amountToAdd -= amountAdded;
            }
        }

        void addToExistingItems(List<Item> existingItems, int amountToAdd)
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
            return false;
        }

        return true;

        bool removeItem(Item item)
        {
            var existingItems = Inventory.Where(i => i.CommonStats_Item.ItemID == item.CommonStats_Item.ItemID).ToList();

            if (!existingItems.Any()) return false;

            if (existingItems.Sum(i => i.CommonStats_Item.CurrentStackSize) < item.CommonStats_Item.CurrentStackSize) return false;

            int amountToRemove = item.CommonStats_Item.CurrentStackSize;

            foreach (var stackItem in existingItems.OrderBy(i => i.CommonStats_Item.CurrentStackSize))
            {
                if (amountToRemove <= 0) break;

                if (stackItem.CommonStats_Item.CurrentStackSize <= amountToRemove)
                {
                    amountToRemove -= stackItem.CommonStats_Item.CurrentStackSize;
                    Inventory.Remove(stackItem);
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
}

public class ActorInventory
{
    public int Gold = 0;
    public List<Item> Inventory = new();

    public void UpdateInventory(List<Item> inventory, int gold)
    {
        Inventory = inventory;
        Gold = gold;
    }
}
