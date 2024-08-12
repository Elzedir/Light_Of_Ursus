using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StationComponent_Resource : StationComponent
{
    public IEnumerator GatherResource(Actor_Base actor)
    {
        // Add in a while loop to gather until a certain condition is fulfilled.

        yield return actor.StartCoroutine(_gather());

        if (!addedIngredientsToActor(_getResourceYield(actor)))
        {
            // Drop resources on floor
            Debug.Log("Couldn't add to inventory");
        }

        bool addedIngredientsToActor(List<Item> items)
        {
            return actor.ActorData.InventoryAndEquipment.Inventory.AddToInventory(items);
        }
    }

    protected virtual IEnumerator _gather()
    {
        yield return new WaitForSeconds(1);
    }

    protected virtual List<Item> _getResourceYield(Actor_Base actor)
    {
        return new List<Item> { Manager_Item.GetItem(1100, 3) }; // For now

        // Base resource yield on actor relevant skill
    }
}
