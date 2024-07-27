using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable_Crafting : Interactable_Base
{
    public InventoryComponent InventoryComponent;

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
}
