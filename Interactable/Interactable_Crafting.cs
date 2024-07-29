using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable_Crafting : Interactable_Base, IInventoryOwner
{
    public GameObject GameObject {  get; protected set; }
    public InventoryComponent InventoryComponent { get; protected set; }

    public virtual CraftingStationName GetCraftingStationName()
    {
        throw new ArgumentException("Can't use base class.");
    }

    public override IEnumerator Interact(Actor_Base actor)
    {
        throw new ArgumentException("Can't use base class.");
    }

    public virtual List<Item> GetCraftingYield(Actor_Base actor)
    {
        throw new ArgumentException("Can't use base class.");
    }

    public void UpdateInventoryDisplay()
    {
        //InventoryData.ActorInventory.UpdateDisplayInventory(this);
    }

    public virtual void InitialiseInventoryComponent()
    {
        throw new ArgumentException("Can't use base class.");
    }
}
