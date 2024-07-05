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
    [SerializeField] CommonStats _itemStats;
    public Item Item { get { return _item; } protected set { _item = value; _itemStats = value?.CommonStats; } }
    public CommonStats ItemStats { get { return _itemStats; } set { _itemStats = value; Actor.Manager_Equipment.EquipItem(SlotID, value.ItemID); } }
    public MeshFilter MeshFilter { get; protected set; }
    public MeshRenderer MeshRenderer { get; protected set; }
    public Collider Collider { get; protected set; }

    protected virtual void Awake()
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
}
