using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using static UnityEngine.UI.GridLayoutGroup;

public class Manager_Inventory : MonoBehaviour
{
    List<IInventoryOwner> _inventoryOwners;

    public void OnSceneLoaded()
    {
        _inventoryOwners = _findAllInventoryOwners();
        foreach (IInventoryOwner owner in _inventoryOwners) owner.InitialiseInventoryComponent();
    }

    List<IInventoryOwner> _findAllInventoryOwners()
    {
        return new List<IInventoryOwner>(FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None).OfType<IInventoryOwner>());
    }
}

public interface IInventoryOwner
{
    InventoryComponent InventoryComponent { get; }
    void UpdateInventoryDisplay();
    void InitialiseInventoryComponent();
}

public interface IInventoryActor : IInventoryOwner
{
    Actor_Data_SO ActorData { get; }
}

public interface IInventoryCrafting : IInventoryOwner
{
    
}

public class InventoryComponent
{
    public IInventoryOwner InventoryOwner;
    public int Gold = 50;
    public List<Item> Inventory;

    public InventoryComponent(IInventoryOwner inventoryOwner, List<Item> inventory)
    {
        InventoryOwner = inventoryOwner;
        Inventory = inventory;
    }

    public Item ItemInInventory(int itemID)
    {
        int totalStackSize = Inventory
        .Where(i => i.CommonStats_Item.ItemID == itemID)
        .Sum(i => i.CommonStats_Item.CurrentStackSize);

        Item itemToReturn = Manager_Item.GetItem(itemID);
        itemToReturn.CommonStats_Item.CurrentStackSize = totalStackSize;

        InventoryOwner.UpdateInventoryDisplay();

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

            InventoryOwner.UpdateInventoryDisplay();

            return false;
        }

        InventoryOwner.UpdateInventoryDisplay();

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
        foreach (Item item in Inventory)
        {
            Debug.Log($"ItemName: {item.CommonStats_Item.ItemName} Quantity: {item.CommonStats_Item.CurrentStackSize}");

        }
        foreach (Item item in items)
        {
            Debug.Log($"ItemName: {item.CommonStats_Item.ItemName} Quantity: {item.CommonStats_Item.CurrentStackSize}");
        }

        bool removedAllItems = true;
        List<Item> tempRemovedItems = new();

        foreach (Item itemToRemove in items)
        {
            if (removeItem(itemToRemove))
            {
                Debug.Log($"ItemName: {itemToRemove.CommonStats_Item.ItemName} of quantity: {itemToRemove.CommonStats_Item.CurrentStackSize} removed.");
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

            InventoryOwner.UpdateInventoryDisplay();

            Debug.Log("Couldn't remove all items.");

            return false;
        }

        InventoryOwner.UpdateInventoryDisplay();

        return true;

        bool removeItem(Item item)
        {
            var existingItems = Inventory.Where(i => i.CommonStats_Item.ItemID == item.CommonStats_Item.ItemID).ToList();

            if (!existingItems.Any()) return false;

            if (existingItems.Sum(i => i.CommonStats_Item.CurrentStackSize) < item.CommonStats_Item.CurrentStackSize) return false;

            int amountToRemove = item.CommonStats_Item.CurrentStackSize;

            Debug.Log($"AmountToRemove: {amountToRemove}");

            foreach (var stackItem in existingItems.OrderBy(i => i.CommonStats_Item.CurrentStackSize))
            {
                if (amountToRemove <= 0) break;

                if (stackItem.CommonStats_Item.CurrentStackSize <= amountToRemove)
                {
                    Debug.Log($"Removed everything since stackSize: {stackItem.CommonStats_Item.CurrentStackSize} is less than amount to remove: {amountToRemove}");
                    amountToRemove -= stackItem.CommonStats_Item.CurrentStackSize;
                    Inventory.Remove(stackItem);
                }
                else
                {
                    Debug.Log($"Removing part of stackItem since: {stackItem.CommonStats_Item.CurrentStackSize} is more than amount to remove: {amountToRemove}");
                    stackItem.CommonStats_Item.CurrentStackSize -= amountToRemove;
                    amountToRemove = 0;
                }
            }

            return true;
        }
    }
}

[Serializable   ]
public class DisplayInventory
{
    public Actor_Base Actor;
    public int Gold = 0;
    public List<DisplayItem> Inventory = new();

    public void UpdateDisplayInventory(Actor_Base actor)
    {
        if (actor.InventoryComponent == null) return;

        Gold = actor.InventoryComponent.Gold;
        
        Inventory.Clear();

        foreach(Item item in actor.InventoryComponent.Inventory)
        {
            Inventory.Add(new DisplayItem(
                itemID: item.CommonStats_Item.ItemID,
                itemName: item.CommonStats_Item.ItemName,
                itemQuantity: item.CommonStats_Item.CurrentStackSize
                ));
        }
    }
}
