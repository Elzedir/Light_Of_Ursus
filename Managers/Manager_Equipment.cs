using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Manager_Equipment
{
    Dictionary<int, Equipment_Base> EquipmentSlots = new();

    public void InitialiseEquipment(Actor_Base actor)
    {
        _initialiseSlots(actor.transform);
        // Load(actor); Load the equipment saved on the actor. 
    }

    void _initialiseSlots(Transform actorTransform)
    {
        Debug.Log(actorTransform);
        for (int i = 0; i < 18; i++)
        {
            if (i < 5)
            {
                switch (i)
                {
                    case 0:
                        var headTransform = Manager_Game.FindTransformRecursively(actorTransform, $"{i}_Head");
                        if (headTransform == null) headTransform = _createEquipmentSlotTransform($"{i}_HeadTransform");
                        var headSlot = headTransform.GetComponentInChildren<Equipment_Base>();
                        headSlot.SetSlotID(i);
                        EquipmentSlots.Add(i, headSlot);
                        break;
                    case 1:
                        var neckSlot = Manager_Game.FindTransformRecursively(actorTransform, $"{i}_Neck").GetComponentInChildren<Equipment_Base>();
                        neckSlot.SetSlotID(i);
                        EquipmentSlots.Add(i, neckSlot);
                        break;
                    case 2:
                        var chestSlot = Manager_Game.FindTransformRecursively(actorTransform, $"{i}_Chest").GetComponentInChildren<Equipment_Base>();
                        chestSlot.SetSlotID(i);
                        EquipmentSlots.Add(i, chestSlot);
                        break;
                    case 3:
                        var leftHandSlot = Manager_Game.FindTransformRecursively(actorTransform, $"{i}_LeftHand").GetComponentInChildren<Equipment_LeftHand>();
                        leftHandSlot.SetSlotID(i);
                        EquipmentSlots.Add(i, leftHandSlot);
                        break;
                    case 4:
                        var rightHandSlot = Manager_Game.FindTransformRecursively(actorTransform, $"{i}_RightHand").GetComponentInChildren<Equipment_RightHand>();
                        rightHandSlot.SetSlotID(i);
                        EquipmentSlots.Add(i, rightHandSlot);
                        break;
                }
            }
            else if (i < 16)
            {
                var ringSlot = Manager_Game.FindTransformRecursively(actorTransform, $"{i}_Ring").GetComponentInChildren<Equipment_Base>();
                ringSlot.SetSlotID(i);
                EquipmentSlots.Add(i, ringSlot);
            }
            else
            {
                switch (i)
                {
                    case 16:
                        var waistSlot = Manager_Game.FindTransformRecursively(actorTransform, $"{i}_Waist").GetComponentInChildren<Equipment_Base>();
                        waistSlot.SetSlotID(i);
                        EquipmentSlots.Add(i, waistSlot);
                        break;
                    case 17:
                        var legsSlot = Manager_Game.FindTransformRecursively(actorTransform, $"{i}_Legs").GetComponentInChildren<Equipment_Base>();
                        legsSlot.SetSlotID(i);
                        EquipmentSlots.Add(i, legsSlot);
                        break;
                    case 18:
                        var feetSlot = Manager_Game.FindTransformRecursively(actorTransform, $"{i}_Feet").GetComponentInChildren<Equipment_Base>();
                        feetSlot.SetSlotID(i);
                        EquipmentSlots.Add(i, feetSlot);
                        break;
                    default:
                        Debug.Log("Somehow is beyond range");
                        break;
                }
            }
        }
    }

    public Transform _createEquipmentSlotTransform(string slotName, Equipment_Base slotScript)
    {
        Transform transform = null;
        transform.AddComponent<slotScript>();
        return transform;
    }

    public bool EquipItem(int slotID, int itemID)
    {
        Item item = Manager_Item.GetItem(itemID);

        if (item != null && EquipmentSlots.TryGetValue(slotID, out var equipment))
        {
            return equipment.EquipItem(item);
        }

        Debug.Log($"Either item: {item} is null, or equipmentSlotID: {slotID} doesn't exist in EquipmentSlots: {EquipmentSlots[slotID]}.");

        return false;
    }
}

public class Actor_Equipment : MonoBehaviour
{
    private Dictionary<int, Equipment_Base> EquipmentSlots = new Dictionary<int, Equipment_Base>();

    void _initialiseSlots(Transform actorTransform)
    {
        Debug.Log(actorTransform);
        for (int i = 0; i < 18; i++)
        {
            Equipment_Base slot = null;
            string slotName = $"{i}_Slot";

            switch (i)
            {
                case 0:
                    slot = InitializeSlot(actorTransform, slotName, typeof(Equipment_Base), i);
                    break;
                case 1:
                    slot = InitializeSlot(actorTransform, slotName, typeof(Equipment_Base), i);
                    break;
                case 2:
                    slot = InitializeSlot(actorTransform, slotName, typeof(Equipment_Base), i);
                    break;
                case 3:
                    slot = InitializeSlot(actorTransform, slotName, typeof(Equipment_LeftHand), i);
                    break;
                case 4:
                    slot = InitializeSlot(actorTransform, slotName, typeof(Equipment_RightHand), i);
                    break;
                case 16:
                    slot = InitializeSlot(actorTransform, slotName, typeof(Equipment_Base), i);
                    break;
                case 17:
                    slot = InitializeSlot(actorTransform, slotName, typeof(Equipment_Base), i);
                    break;
                case 18:
                    slot = InitializeSlot(actorTransform, slotName, typeof(Equipment_Base), i);
                    break;
                default:
                    if (i < 16)
                    {
                        slot = InitializeSlot(actorTransform, slotName, typeof(Equipment_Base), i);
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

    private Equipment_Base InitializeSlot(Transform actorTransform, string slotName, System.Type slotType, int slotID)
    {
        Transform slotTransform = Manager_Game.FindTransformRecursively(actorTransform, slotName);
        if (slotTransform == null)
        {
            slotTransform = CreateEquipmentSlotTransform(actorTransform, slotName, slotType);
        }

        return slotTransform.GetComponentInChildren(slotType) as Equipment_Base;
    }

    private Transform CreateEquipmentSlotTransform(Transform parent, string slotName, System.Type slotType)
    {
        GameObject slotObject = new GameObject(slotName);
        slotObject.transform.parent = parent;
        slotObject.transform.localPosition = Vector3.zero;
        slotObject.transform.localRotation = Quaternion.identity;
        slotObject.transform.localScale = Vector3.one;
        slotObject.AddComponent(slotType);

        return slotObject.transform;
    }
}

public static class Manager_Game
{
    public static Transform FindTransformRecursively(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
            {
                return child;
            }
            Transform result = FindTransformRecursively(child, name);
            if (result != null)
            {
                return result;
            }
        }
        return null;
    }
}
