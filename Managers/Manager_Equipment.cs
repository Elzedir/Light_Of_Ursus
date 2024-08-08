using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.Animations;
using UnityEngine;

public class Manager_Equipment
{
    
}

public class EquipmentComponent
{
    Dictionary<int, Equipment_Base> EquipmentSlots = new();
    Actor_Base _actor;
    Transform _equipmentParent;

    public EquipmentComponent(Actor_Base actor)
    {
        _actor = actor;
        _equipmentParent = _actor.transform.parent.GetChild(0);

        _initialiseSlots();
        // Load(actor); Load the equipment saved on the actor. 
    }

    void _initialiseSlots()
    {
        for (int i = 0; i < 18; i++)
        {
            Equipment_Base slot = null;
            string slotName = $"Slot_{i}";

            switch (i)
            {
                case 0:
                    slot = _createSlot(slotName, typeof(Equipment_Base), i);
                    slot.Initialise();
                    break;
                case 1:
                    slot = _createSlot(slotName, typeof(Equipment_Base), i);
                    slot.Initialise();
                    break;
                case 2:
                    slot = _createSlot(slotName, typeof(Equipment_Base), i);
                    slot.Initialise();
                    break;
                case 3:
                    slot = _createSlot(slotName, typeof(Equipment_LeftHand), i);
                    slot.Initialise();
                    break;
                case 4:
                    slot = _createSlot(slotName, typeof(Equipment_RightHand), i);
                    slot.Initialise();
                    break;
                case 16:
                    slot = _createSlot(slotName, typeof(Equipment_Base), i);
                    slot.Initialise();
                    break;
                case 17:
                    slot = _createSlot(slotName, typeof(Equipment_Base), i);
                    slot.Initialise();
                    break;
                case 18:
                    slot = _createSlot(slotName, typeof(Equipment_Base), i);
                    slot.Initialise();
                    break;
                default:
                    if (i < 16)
                    {
                        slot = _createSlot(slotName, typeof(Equipment_Base), i);
                        slot.Initialise();
                    }
                    else
                    {
                        Debug.LogWarning("Index out of expected range");
                    }
                    break;
            }

            if (slot != null)
            {
                slot.SetSlotID(i);
                EquipmentSlots.Add(i, slot);
            }
        }
    }

    Equipment_Base _createSlot(string slotName, System.Type slotType, int slotID)
    {
        Transform slotTransform = Manager_Game.FindTransformRecursively(_equipmentParent, slotName);

        if (slotTransform == null) slotTransform = _createSlotTransform(slotName, slotType);

        return slotTransform.GetComponentInChildren(slotType) as Equipment_Base;
    }

    Transform _createSlotTransform(string slotName, System.Type slotScript)
    {
        GameObject slotGO = new GameObject(slotName);
        slotGO.transform.parent = _equipmentParent;
        slotGO.transform.localPosition = Vector3.zero;
        slotGO.transform.localRotation = Quaternion.identity;
        slotGO.transform.localScale = Vector3.one;
        slotGO.AddComponent(slotScript);
        slotGO.AddComponent<MeshFilter>();
        slotGO.AddComponent<MeshRenderer>();
        slotGO.AddComponent<Animator>();

        // Find a way to see which collider to add. Maybe dependant on item rather than here.

        return slotGO.transform;
    }

    public bool EquipItem(int slotID, int itemID)
    {
        Item item = Manager_Item.GetItem(itemID);

        if (item != null && EquipmentSlots.TryGetValue(slotID, out var equipment))
        {
            return equipment.EquipItem(item);
        }

        Debug.Log($"Either item: {item} is null, or equipmentSlotID: {slotID} doesn't exist in EquipmentSlots: {EquipmentSlots[slotID]}, or equip failed.");

        return false;
    }

    public bool UnequipItem(int slotID)
    {
        if (EquipmentSlots.TryGetValue(slotID, out var equipment))
        {
            return equipment.UnequipItem();
        }

        Debug.Log($"Either equipmentSlotID: {slotID} doesn't exist in EquipmentSlots: {EquipmentSlots[slotID]}, or unequip failed.");

        return false;
    }
}

public class ActorEquipment
{
    public CommonStats_Item Head;
    public CommonStats_Item Neck;
    public CommonStats_Item Chest;
    public CommonStats_Item LeftHand;
    public CommonStats_Item RightHand;
    public CommonStats_Item[] Rings;
    public CommonStats_Item Waist;
    public CommonStats_Item Legs;
    public CommonStats_Item Feet;

    public ActorEquipment(Item head, Item neck, Item chest, Item leftHand, Item rightHand, Item[] rings, Item waist, Item legs, Item feet)
    {
        Head = head?.CommonStats_Item;
        Neck = neck?.CommonStats_Item;
        Chest = chest?.CommonStats_Item;
        LeftHand = leftHand?.CommonStats_Item;
        RightHand = rightHand?.CommonStats_Item;
        Waist = waist?.CommonStats_Item;
        Legs = legs?.CommonStats_Item;
        Feet = feet?.CommonStats_Item;
        Rings = new CommonStats_Item[10];

        if (rings != null)
        {
            for (int i = 0; i < rings.Length && i < Rings.Length; i++)
            {
                Rings[i] = rings[i]?.CommonStats_Item;
            }
        }
    }

    public void UpdateEquipment(Item item, int index)
    {
        switch (index)
        {
            case 0:
                Head = item?.CommonStats_Item;
                break;
            case 1:
                Neck = item?.CommonStats_Item;
                break;
            case 2:
                Chest = item?.CommonStats_Item;
                break;
            case 3:
                LeftHand = item?.CommonStats_Item;
                break;
            case 4:
                RightHand = item?.CommonStats_Item;
                break;
            case 16:
                Waist = item?.CommonStats_Item;
                break;
            case 17:
                Legs = item?.CommonStats_Item;
                break;
            case 18:
                Feet = item?.CommonStats_Item;
                break;
            default:
                if (index < 16)
                {
                    Rings[index - 5] = item?.CommonStats_Item;
                }
                else
                {
                    Debug.LogWarning("Index out of expected range");
                }
                break;
        }
    }
}

