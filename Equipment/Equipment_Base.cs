using System.Collections;
using System.Collections.Generic;
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
    public Collider Collider { get; protected set; }

    public virtual void Initialise()
    {
        Actor = GetComponentInParent<Actor_Base>();
        MeshFilter = GetComponent<MeshFilter>();
        MeshRenderer = GetComponent<MeshRenderer>();
        Collider = GetComponent<Collider>();
    }

    public void SetSlotID(int slotID)
    {
        SlotID = slotID;
    }

    public virtual bool EquipItem(Item item)
    {
        return false;
    }

    public virtual bool UnequipItem()
    {
        return false;
    }
}
