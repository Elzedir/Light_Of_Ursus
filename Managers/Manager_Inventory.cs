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
    GameObject GameObject { get; }
    InventoryComponent InventoryComponent { get; }
    void UpdateInventoryDisplay();
    void InitialiseInventoryComponent();
}

public interface IInventoryActor : IInventoryOwner
{
    Actor_Data_SO ActorData { get; }
}

public interface IResourceStation : IInventoryOwner
{
    ResourceStationName GetResourceStationName();
    Vector3 GetGatheringPosition();
    IEnumerator GatherResource(Actor_Base actor);
    List<Item> GetResourceYield(Actor_Base actor);

}

public interface ICraftingStation : IInventoryOwner
{
    CraftingStationName GetCraftingStationName();
    Vector3 GetCraftingPosition();
    IEnumerator CraftItem(Actor_Base actor);
    List<Item> GetCraftingYield(Actor_Base actor);
}

public interface ISellingStation : IInventoryOwner
{
    bool IsActive { get; }
    void IsStationActive(bool isSelling);
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
        return Manager_Item.GetItem(itemID, Inventory.Where(i => i.CommonStats_Item.ItemID == itemID).Sum(i => i.CommonStats_Item.CurrentStackSize));
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

                Inventory.Add(newItem);

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

    public bool TransferItemFromInventory(InventoryComponent target, List<Item> items)
    {
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
            DropItemsGroup(items, InventoryOwner.GameObject.transform.position, itemsNotInInventory: true);
            Debug.Log("Took items out of inventory and can't put them back");
        }

        return false;
    }

    public bool DropItemsGroup(List<Item> items, Vector3 dropPosition, bool itemsNotInInventory = false)
    {
        if (itemsNotInInventory)
        {
            if (!dropItemsGroup())
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

        if (!dropItemsGroup())
        {
            Debug.Log("Can't drop items");
            return false;
        }

        return true;

        bool dropItemsGroup()
        {
            foreach(Item item in items)
            {
                Interactable_Item.CreateNewItem(item, dropPosition);
            }

            return true;

            //Later will have things like having available space, etc.
        }
    }

    public bool DropItemsIndividual(List<Item> items, Vector3 dropPosition, bool itemsNotInInventory = false)
    {
        if (itemsNotInInventory)
        {
            if (!dropItemsIndividual())
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

        if (!dropItemsIndividual())
        {
            Debug.Log("Can't drop items");
            return false;
        }

        return true;

        bool dropItemsIndividual()
        {
            foreach (Item item in items)
            {
                for (int i = 0; i < item.CommonStats_Item.CurrentStackSize; i++)
                {
                    Interactable_Item.CreateNewItem(Manager_Item.GetItem(item.CommonStats_Item.ItemID), dropPosition);
                }
            }

            return true;

            //Later will have things like having available space, etc.
        }
    }
}

[Serializable]
public class ActorInventory
{
    public IInventoryOwner InventoryOwner;
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
