using System;
using System.Collections.Generic;
using System.Linq;
using Actor;
using ActorActions;
using Inventory;
using Items;
using Managers;
using Priority;
using Tools;
using UnityEngine;

namespace Equipment
{
    public class Equipment_Manager
    {
    
    }

    public class EquipmentComponent
    {
        Dictionary<int, Equipment_Base> EquipmentSlots = new();
        Actor_Component _actor;
        Transform _equipmentParent;

        public EquipmentComponent(Actor_Component actor)
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

        public bool EquipItem(int slotID, Item item)
        {
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

    [Serializable]
    public class Equipment_Data : Priority_Class
    {
        public ComponentReference_Actor ActorReference => Reference as ComponentReference_Actor;

        public Item Head;
        public Item Neck;
        public Item Chest;
        public Item LeftHand;
        public Item RightHand;
        public Item[] Rings;
        public Item Waist;
        public Item Legs;
        public Item Feet;

        public Equipment_Data(uint actorID, Item head = null, Item neck = null, Item chest = null, Item leftHand = null,
            Item rightHand = null, Item[] rings = null, Item waist = null, Item legs = null,
            Item feet      = null) : base(actorID, ComponentType.Actor)
        {
            Head      = head;
            Neck      = neck;
            Chest     = chest;
            LeftHand  = leftHand;
            RightHand = rightHand;
            Rings     = rings;
            Waist     = waist;
            Legs      = legs;
            Feet      = feet;
        }

        public Equipment_Data(Equipment_Data equipmentData) : base (equipmentData.ActorReference.ActorID, ComponentType.Actor)
        {
            Head      = new Item(equipmentData.Head);
            Neck      = new Item(equipmentData.Neck);
            Chest     = new Item(equipmentData.Chest);
            LeftHand  = new Item(equipmentData.LeftHand);
            RightHand = new Item(equipmentData.RightHand);
            Rings     = equipmentData.Rings.Select(ring => new Item(ring)).ToArray();
            Waist     = new Item(equipmentData.Waist);
            Legs      = new Item(equipmentData.Legs);
            Feet      = new Item(equipmentData.Feet);
        }
        
        public override List<ActorActionName> GetAllowedActions()
        {
            return new List<ActorActionName>();
        }
    
        public override Dictionary<string, string> GetStringData()
        {
            return new Dictionary<string, string>
            {
                { "Equipment Data", "Equipment Data would be here." }
            };
        }
    
        public override DataToDisplay GetDataToDisplay(bool toggleMissingDataDebugs)
        {
            _updateDataDisplay(DataToDisplay,
                title: "Equipment Slots",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: GetStringData());

            return DataToDisplay;
        }

        public void SetEquipment(Item head, Item neck, Item chest, Item leftHand, Item rightHand, Item[] rings, Item waist, Item legs, Item feet)
        {
            Head = head;
            Neck = neck;
            Chest = chest;
            LeftHand = leftHand;
            RightHand = rightHand;
            Waist = waist;
            Legs = legs;
            Feet = feet;
            Rings = rings;
        }

        public void UpdateEquipment(Item item, int index)
        {
            switch (index)
            {
                case 0:
                    Head = item;
                    break;
                case 1:
                    Neck = item;
                    break;
                case 2:
                    Chest = item;
                    break;
                case 3:
                    LeftHand = item;
                    break;
                case 4:
                    RightHand = item;
                    break;
                case 16:
                    Waist = item;
                    break;
                case 17:
                    Legs = item;
                    break;
                case 18:
                    Feet = item;
                    break;
                default:
                    if (index < 16)
                    {
                        Rings[index - 5] = item;
                    }
                    else
                    {
                        Debug.LogWarning("Index out of expected range");
                    }
                    break;
            }
        }
    }
}