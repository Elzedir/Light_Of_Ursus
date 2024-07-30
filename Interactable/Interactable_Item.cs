using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable_Item : Interactable_Base
{
    public Item Item;

    public static void CreateNewItem(Item item, Vector3 dropPosition)
    {
        GameObject itemContainer = new GameObject($"{item.CommonStats_Item.ItemName}Container");
        itemContainer.transform.parent = GameObject.Find("InteractableItems").transform;
        itemContainer.transform.position = dropPosition;

        Rigidbody itemBody = itemContainer.AddComponent<Rigidbody>();

        GameObject itemGO = new GameObject(item.CommonStats_Item.ItemName);
        itemGO.transform.localPosition = Vector3.zero;
        itemGO.transform.parent = itemContainer.transform;

        itemGO.AddComponent<Interactable_Item>().InitialiseInteractableItem(item);
    }

    public void InitialiseInteractableItem(Item item)
    {
        Item = Manager_Item.GetItem(item.CommonStats_Item.ItemID, item.CommonStats_Item.CurrentStackSize);

        if (Item.VisualStats_Item != null)
        {
            Item.VisualStats_Item.DisplayVisuals(gameObject);
        }
    }

    public override IEnumerator Interact(Actor_Base actor)
    {
        base.Interact(actor.gameObject);

        if (Item == null) throw new ArgumentException("Item has not been initialised");

        if (!actor.InventoryComponent.AddToInventory(new List<Item> { Item }))
        {
            Debug.Log("Couldn't pick up item.");

            yield break;
        }

        //Destroy(this);
    }
}
