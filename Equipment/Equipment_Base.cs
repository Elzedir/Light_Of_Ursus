using System.Collections;
using System.Collections.Generic;
using Actors;
using UnityEditor.Animations;
using UnityEngine;

public enum EquipmentSlot { None, Head, Neck, Chest, LeftHand, RightHand, Ring, Waist, Legs, Feet }

public class Equipment_Base : MonoBehaviour
{
    public int SlotID { get; protected set; }
    public EquipmentSlot EquipmentSlot { get; protected set; }
    public ActorComponent Actor { get; private set; }
    public Item Item;
    public MeshFilter MeshFilter { get; protected set; }
    public MeshRenderer MeshRenderer { get; protected set; }
    public Animator Animator { get; protected set; }
    public Collider Collider { get; protected set; }

    public virtual void Initialise()
    {
        Actor = GetComponentInParent<ActorComponent>();
        MeshFilter = GetComponent<MeshFilter>();
        MeshRenderer = GetComponent<MeshRenderer>();
        Animator = GetComponent<Animator>();
        Collider = GetComponent<Collider>();
    }

    public void SetSlotID(int slotID)
    {
        SlotID = slotID;
    }

    public virtual bool EquipItem(Item item)
    {
        if (item == null)
        {
            Debug.Log("Item is null");
            return false;
        }

        var masterItem = Manager_Item.GetItem_Master(item.ItemID);

        if (masterItem.CommonStats_Item.EquipmentSlots.Contains(EquipmentSlot))
        {
            if (UnequipItem())
            {
                Item = item;
                MeshFilter.mesh = masterItem.VisualStats_Item.ItemMesh;
                MeshRenderer.material = masterItem.VisualStats_Item.ItemMaterial;
                //Animator.runtimeAnimatorController = item.VisualStats.ItemAnimatorController;

                transform.localPosition = masterItem.VisualStats_Item.ItemPosition;
                transform.localRotation = masterItem.VisualStats_Item.ItemRotation;
                transform.localScale = masterItem.VisualStats_Item.ItemScale;

                return true;
            }
        }

        return false;
    }

    public virtual bool UnequipItem()
    {
        // if (Return item to inventory.)
        Item = null;
        MeshFilter.mesh = null;
        MeshRenderer.material = null;
        //Animator.runtimeAnimatorController = null;

        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;
        return true;
    }
}
