using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
    public InventoryData(uint componentID, ComponentType componentType) : base(componentID, componentType) { }

    public abstract ComponentType ComponentType { get; }

    ComponentReference _reference => Reference as ComponentReference;

    public int Gold = 0;
    bool _skipNextPriorityCheck = false;
    public void SkipNextPriorityCheck() => _skipNextPriorityCheck = true;
    public List<Item> _allInventoryItems;
    public List<Item> AllInventoryItems
    {
        get { return _allInventoryItems ??= new(); }
        set 
        { 
            _allInventoryItems = value; 
            
            if (_skipNextPriorityCheck)
            {
                _skipNextPriorityCheck = false;
                return;
            } 
            
            _priorityChangeCheck(DataChanged.ChangedInventory, true); 
        }
    }

    public Dictionary<uint, Item> InventoryItemsOnHold = new();
    public void AddToInventoryItemsOnHold(List<Item> items)
    {
        if (items == null) return;

        foreach (var item in items)
        {
            if (InventoryItemsOnHold.ContainsKey(item.ItemID))
            {
                InventoryItemsOnHold[item.ItemID].ItemAmount += item.ItemAmount;
            }
            else
            {
                InventoryItemsOnHold.Add(item.ItemID, item);
            }
        }
    }

    public void RemoveFromInventoryItemsOnHold(List<Item> items)
    {
        if (items == null) return;

        foreach (var item in items)
        {
            if (InventoryItemsOnHold.ContainsKey(item.ItemID))
            {
                if (InventoryItemsOnHold[item.ItemID].ItemAmount <= item.ItemAmount)
                {
                    InventoryItemsOnHold.Remove(item.ItemID);
                }
                else
                {
                    InventoryItemsOnHold[item.ItemID].ItemAmount -= item.ItemAmount;
                }
            }
        }
    }

    public void SetInventory(List<Item> allInventoryItems, bool skipPriorityCheck = false)
    {
        if (skipPriorityCheck) SkipNextPriorityCheck();
        AllInventoryItems = allInventoryItems;
    }

    public Item GetItemFromInventory(uint itemID)
    {
        return AllInventoryItems.FirstOrDefault(i => i.ItemID == itemID);
    }
    public abstract bool HasSpaceForItems(List<Item> items);

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
            DropItems(items, _reference.GameObject.transform.position, itemsNotInInventory: true, dropAsGroup: true);
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

    protected override Dictionary<DataChanged, Dictionary<PriorityParameter, object>> _priorityParameterList { get; set; } = new()
    {
        { 
            DataChanged.ChangedInventory, 
            new()
            {

            }
        }
    };

    public abstract List<Item> GetInventoryItemsToFetch();
    public abstract List<Item> GetInventoryItemsToHold();
    public abstract List<Item> GetInventoryItemsToDeliver(InventoryData inventory);
}

public class InventoryData_Actor : InventoryData
{
    public InventoryData_Actor(uint actorID) : base(actorID, ComponentType.Actor) { }
    public override ComponentType ComponentType => ComponentType.Actor;
    public InventoryData_Actor GetInventoryData() => this;

    public ComponentReference_Actor ActorReference => Reference as ComponentReference_Actor;

    public override PriorityComponent PriorityComponent { get => _priorityComponent ??= ActorReference.Actor.PriorityComponent; }

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

    public override List<Item> GetInventoryItemsToHold()
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

public class InventoryData_Station : InventoryData
{
    public InventoryData_Station(uint stationID) : base(stationID, ComponentType.Station) { }
    public override ComponentType ComponentType => ComponentType.Station;
    public InventoryData_Station GetInventoryData() => this;

    public ComponentReference_Station StationReference => Reference as ComponentReference_Station;

    public override PriorityComponent PriorityComponent { get => _priorityComponent ??= StationReference.Station.PriorityComponent; }
    public uint MaxInventorySpace = 10; // Implement a way to change the size depending on the station. Maybe StationComponent default value.
    List<uint> _getDesiredItemIDs() => StationReference.Station.DesiredStoredItemIDs;
    protected override bool _priorityChangeNeeded(object dataChanged) => (DataChanged)dataChanged == DataChanged.ChangedInventory;
    public override bool HasSpaceForItems(List<Item> items)
    {
        if (Item.GetItemListTotal_CountAllItems(items) > MaxInventorySpace)
        {
            Debug.Log("Not enough space in inventory.");
            return false;
        }

        return true;
    }

    public override List<Item> GetInventoryItemsToFetch()
    {
        var itemsToFetch = AllInventoryItems.Where(i => !_getDesiredItemIDs().Contains(i.ItemID)).ToList();

        for (int i = 0; i < itemsToFetch.Count; i++)
        {
            if (!InventoryItemsOnHold.ContainsKey(itemsToFetch[i].ItemID)) continue;

            if (InventoryItemsOnHold[itemsToFetch[i].ItemID].ItemAmount > itemsToFetch[i].ItemAmount)
            {
                Debug.LogError("Item amount in inventory is less than the amount on hold.");
                itemsToFetch.RemoveAt(i);
                continue;
            }
            else if (InventoryItemsOnHold[itemsToFetch[i].ItemID].ItemAmount == itemsToFetch[i].ItemAmount)
            {
                Debug.Log("All items are on hold.");
                itemsToFetch.RemoveAt(i);
                continue;
            }
            else
            {
                itemsToFetch[i].ItemAmount -= InventoryItemsOnHold[itemsToFetch[i].ItemID].ItemAmount;
            }
        }

        return itemsToFetch;
    }

    public override List<Item> GetInventoryItemsToHold() => AllInventoryItems.Where(i => _getDesiredItemIDs().Contains(i.ItemID)).ToList();
    public override List<Item> GetInventoryItemsToDeliver(InventoryData inventory) => inventory.AllInventoryItems.Where(i => _getDesiredItemIDs().Contains(i.ItemID)).ToList();
}