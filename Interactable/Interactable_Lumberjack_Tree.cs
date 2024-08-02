using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable_Lumberjack_Tree : Interactable_Lumberjack, IResourceStation
{
    public override IEnumerator Interact(Actor_Base actor)
    {
        yield return null;
    }

    public ResourceStationName GetResourceStationName()
    {
        return ResourceStationName.Tree;
    }

    public virtual Vector3 GetGatheringPosition()
    {
        return GameObject.GetComponent<Collider>().bounds.center;
    }

    public IEnumerator GatherResource(Actor_Base actor)
    {
        base.Interact(actor);

        // Play interacting animation.
        // Check Actor resource stats to calculate time alongside resource requirements

        yield return new WaitForSeconds(0.1f);
    }

    public List<Item> GetResourceYield(Actor_Base actor)
    {
        return new List<Item> { Manager_Item.GetItem(itemID: 1100, itemQuantity: 7) };
    }

    public void UpdateInventoryDisplay()
    {
        
    }

    public void InitialiseInventoryComponent()
    {
        GameObject = gameObject;
        InventoryComponent = new InventoryComponent(this, new List<Item>());
        EmployeePositions = new() { EmployeePosition.Owner, EmployeePosition.Chief_Lumberjack, EmployeePosition.Logger, EmployeePosition.Assistant_Logger };
    }
}
