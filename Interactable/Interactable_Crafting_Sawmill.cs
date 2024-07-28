using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable_Crafting_Sawmill : Interactable_Crafting
{
    public override CraftingStationName GetCraftingStationName()
    {
        return CraftingStationName.Sawmill;
    }

    public override IEnumerator Interact(Actor_Base actor)
    {
        
        // Play interacting animation.
        // Check Actor resource stats to calculate time alongside resource requirements

        yield return new WaitForSeconds(2); // Temporary
    }

    public override List<Item> GetCraftingYield(Actor_Base actor)
    {
        return new List<Item> { Manager_Item.GetItem(itemID: 1100, itemQuantity: 1) };
    }

    public override void InitialiseInventoryComponent()
    {
        InventoryComponent = new InventoryComponent(this, new List<Item>());
    }
}
