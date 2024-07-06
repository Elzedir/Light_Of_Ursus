using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Manager_Equipment
{
    Dictionary<int, Equipment_Base> EquipmentSlots = new();
    Actor_Base _actor;
    Transform _equipmentParent;

    public void InitialiseEquipment(Actor_Base actor)
    {
        _actor = actor;
        _equipmentParent = _actor.transform.GetChild(1);

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
