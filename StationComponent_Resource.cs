using System;
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
        throw new ArgumentException("Cannot use base class.");
    }

    protected virtual List<Item> _getResourceYield(Actor_Base actor)
    {
        throw new ArgumentException("Cannot use base class.");
    }
}
