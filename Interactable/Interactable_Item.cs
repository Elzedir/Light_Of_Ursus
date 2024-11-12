using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable_Item : MonoBehaviour, IInteractable
{
    public Item Item;

    public float InteractRange { get; private set; }

    public void SetInteractRange(float interactRange)
    {
        InteractRange = interactRange;
    }

    public bool WithinInteractRange(ActorComponent interactor)
    {
        return Vector3.Distance(interactor.transform.position, transform.position) < InteractRange;
    }

    public IEnumerator Interact(ActorComponent actor)
    {
        if (Item == null) throw new ArgumentException("Item has not been initialised");

        // Change to be a pickup function which will call the add to inventory when successful.
        
        // if (!actor.ActorData.InventoryData.AddToInventory(new List<Item> { Item }))
        // {
        //     Debug.Log("Couldn't pick up item.");
        //
             yield break;
        // }

        //Destroy(this);
    }

    public static void CreateNewItem(Item item, Vector3 dropPosition)
    {
        GameObject itemContainer = new GameObject($"{item.ItemName}Container");
        itemContainer.transform.parent = GameObject.Find("InteractableItems").transform;
        itemContainer.transform.position = dropPosition;

        Rigidbody itemBody = itemContainer.AddComponent<Rigidbody>();

        GameObject itemGO = new GameObject(item.ItemName);
        itemGO.transform.localPosition = Vector3.zero;
        itemGO.transform.parent = itemContainer.transform;

        itemGO.AddComponent<Interactable_Item>().InitialiseInteractableItem(item);
    }

    public void InitialiseInteractableItem(Item item)
    {
        var masterItem = Manager_Item.GetMasterItem(item.ItemID);

        if (masterItem.VisualStats_Item != null)
        {
            masterItem.VisualStats_Item.DisplayVisuals(gameObject);
        }
    }
}
