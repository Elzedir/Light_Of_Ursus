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
    
}

[Serializable]
public class InventoryData
{
    public uint ActorID;
    ActorComponent _actor;
    public ActorComponent Actor { get => _actor ??= Manager_Actor.GetActor(ActorID); }
    public int Gold = 0;
    public List<Item> AllInventoryItems;
    public float GetTotalInventoryWeight() => AllInventoryItems.Sum(i => i.ItemAmount * Manager_Item.GetMasterItem(i.ItemID).CommonStats_Item.ItemWeight);
    public InventoryData(uint actorID) => ActorID = actorID;
    public void SetInventory(List<Item> allInventoryItems) => AllInventoryItems = allInventoryItems;

    public Item GetItemFromInventory(uint itemID)
    {
        return AllInventoryItems.FirstOrDefault(i => i.ItemID == itemID);
    }

    public bool HasSpaceForItem(List<Item> items)
    {
        float totalWeight = 0;

        foreach(Item item in items)
        {
            var itemMaster = Manager_Item.GetMasterItem(item.ItemID);

            var itemWeight = itemMaster.CommonStats_Item.ItemWeight * item.ItemAmount;

            totalWeight += itemWeight;
        }

        if (totalWeight > Actor.ActorData.StatsAndAbilities.Actor_Stats.AvailableCarryWeight)
        {
            Debug.LogWarning("Not enough space in inventory.");
            return false;
        }

        return true;
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
            uint amountToAdd = item.ItemAmount;

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

        void addNewItems(uint amountToAdd, Item item)
        {
            while (amountToAdd > 0)
            {
                uint amountAdded = Math.Min(amountToAdd, item.MaxStackSize);

                var newItem = new Item(item.ItemID, amountAdded);

                AllInventoryItems.Add(newItem);

                amountToAdd -= amountAdded;
            }
        }

        void addToExistingItems(List<Item> existingItems, ref uint amountToAdd)
        {
            foreach (var stackItem in existingItems.OrderBy(i => i.ItemAmount))
            {
                if (amountToAdd <= 0) break;

                uint availableSpace = stackItem.MaxStackSize - stackItem.ItemAmount;

                if (availableSpace > 0)
                {
                    uint amountAdded = Math.Min(amountToAdd, availableSpace);
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
                Debug.Log($"Couldn't remove {itemToRemove.ItemName} from inventory.");
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

            if (!existingItems.Any())
            {
                Debug.Log($"No {item.ItemName} in inventory.");
                return false;
            }

            if (existingItems.Sum(i => i.ItemAmount) < item.ItemAmount)
            {
                Debug.Log($"Not enough {item.ItemName} in inventory.");
                return false;
            }

            uint amountToRemove = item.ItemAmount;

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
            var actor = Manager_Actor.GetActor(ActorID);

            DropItems(items, actor.transform.position, itemsNotInInventory: true);
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

    
    public List<Item> InventoryContainsReturnedItems(List<uint> itemIDs) => AllInventoryItems.Where(i => itemIDs.Contains(i.ItemID)).ToList();
    
    public List<Item> InventoryMissingItems(List<Item> items)
    {
        List<Item> missingItems = new();

        foreach (var item in items)
        {
            var existingItems = AllInventoryItems.Where(i => i.ItemID == item.ItemID).ToList();

            if (!existingItems.Any())
            {
                missingItems.Add(item);
            }
            else if (existingItems.Sum(i => i.ItemAmount) < item.ItemAmount)
            {
                missingItems.Add(new Item(item.ItemID, item.ItemAmount - (uint)existingItems.Sum(i => i.ItemAmount)));
            }
        }

        return missingItems;
    }

    public bool InventoryContainsAnyItems(List<uint> itemIDs) => AllInventoryItems.Any(i => itemIDs.Contains(i.ItemID));
    public bool InventoryContainsAllItems(List<Item> requiredItems)
    {
        foreach (var requiredItem in requiredItems)
        {
            var existingItems = AllInventoryItems.Where(i => i.ItemID == requiredItem.ItemID).ToList();
            
            if (!AllInventoryItems.Any(i => i.ItemID == requiredItem.ItemID && i.ItemAmount >= requiredItem.ItemAmount))
            {
                return false;
            }
        }

        return true;
    }
}
