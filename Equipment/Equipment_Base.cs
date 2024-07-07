using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

public enum EquipmentSlot { None, Head, Neck, Chest, LeftHand, RightHand, Ring, Waist, Legs, Feet }

public class Equipment_Base : MonoBehaviour
{
    public int SlotID { get; protected set; }
    public EquipmentSlot EquipmentSlot { get; protected set; }
    public Actor_Base Actor { get; private set; }
    Item _item;
    public Item Item { get { return _item; } protected set { _item = value; ItemStats = value?.CommonStats; } }
    public CommonStats ItemStats;
    public MeshFilter MeshFilter { get; protected set; }
    public MeshRenderer MeshRenderer { get; protected set; }
    public Animator Animator { get; protected set; }
    public Collider Collider { get; protected set; }

    public virtual void Initialise()
    {
        Actor = GetComponentInParent<Actor_Base>();
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

        if (item.CommonStats.EquipmentSlots.Contains(EquipmentSlot))
        {
            if (UnequipItem())
            {
                Item = item;
                MeshFilter.mesh = item.VisualStats.ItemMesh;
                MeshRenderer.material = item.VisualStats.ItemMaterial;

                transform.localPosition = item.VisualStats.ItemPosition;
                transform.localRotation = item.VisualStats.ItemRotation;
                transform.localScale = item.VisualStats.ItemScale;
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
        Animator.runtimeAnimatorController = null;

        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;
        return true;
    }
}
