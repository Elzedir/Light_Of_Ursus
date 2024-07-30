using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Interactable_Lumberjack_DropOffZone : Interactable_Lumberjack, IInventoryOwner
{
    public override IEnumerator Interact(Actor_Base actor)
    {
        yield return base.Interact(actor);

        // Use later for opening its inventory.
    }

    public virtual List<Item> GetItemsToDropOff(IInventoryActor actor)
    {
        return actor.InventoryComponent.Inventory.Where(i => i.CommonStats_Item.ItemID == 2300)
        .Select(i => Manager_Item.GetItem(i.CommonStats_Item.ItemID, i.CommonStats_Item.CurrentStackSize)).ToList();
    }

    public void UpdateInventoryDisplay()
    {
        
    }

    public void InitialiseInventoryComponent()
    {
        GameObject = gameObject;
        InventoryComponent = new InventoryComponent(this, new List<Item>());
    }
}
