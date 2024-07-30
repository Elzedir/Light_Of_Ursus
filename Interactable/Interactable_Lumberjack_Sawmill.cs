using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable_Lumberjack_Sawmill : Interactable_Lumberjack, ICraftingStation
{
    public override IEnumerator Interact(Actor_Base actor)
    {
        yield return null;
    }

    public virtual CraftingStationName GetCraftingStationName()
    {
        return CraftingStationName.Sawmill;
    }

    public virtual Vector3 GetCraftingPosition()
    {
        return GameObject.GetComponent<Collider>().bounds.center;
    }

    public virtual IEnumerator CraftItem(Actor_Base actor)
    {
        yield return base.Interact(actor);
    }

    public virtual List<Item> GetCraftingYield(Actor_Base actor)
    {
        return new List<Item> { Manager_Item.GetItem(itemID: 1100, itemQuantity: 1) };
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
