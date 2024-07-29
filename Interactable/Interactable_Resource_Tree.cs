using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable_Resource_Tree : Interactable_Resource
{
    public override ResourceName GetResourceName()
    {
        return ResourceName.Tree;
    }

    public override IEnumerator Interact(Actor_Base actor)
    {
        // Play interacting animation.
        // Check Actor resource stats to calculate time alongside resource requirements

        yield return new WaitForSeconds(0.1f); // Temporary
    }

    public override List<Item> GetResourceYield(Actor_Base actor)
    {
        return new List<Item> { Manager_Item.GetItem(itemID: 1100, itemQuantity: 7) };
    }
}
