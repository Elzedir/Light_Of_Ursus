using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Interactable_Lumberjack_DropOffZone : Interactable_Base, IInventoryOwner
{
    public GameObject GameObject {  get; protected set; }
    public InventoryComponent InventoryComponent { get; protected set; }

    public override IEnumerator Interact(Actor_Base actor)
    {
        yield break;

        // Use later for opening its inventory.
    }

    public void UpdateInventoryDisplay()
    {
        //InventoryData.ActorInventory.UpdateDisplayInventory(this);
    }

    public virtual void InitialiseInventoryComponent()
    {
        GameObject = gameObject;
        InventoryComponent = new InventoryComponent(this, new List<Item>());
    }

    public virtual List<Item> GetItemsToDropOff(IInventoryActor actor)
    {
        return actor.InventoryComponent.Inventory.Where(i => i.CommonStats_Item.ItemID == 2300)
        .Select(i => Manager_Item.GetItem(i.CommonStats_Item.ItemID, i.CommonStats_Item.CurrentStackSize)).ToList();
    }
}
