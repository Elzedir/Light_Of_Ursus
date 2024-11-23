using System.Collections.Generic;
using Items;
using Managers;
using UnityEngine;

namespace Lists
{
    public abstract class List_Armour
    {
        public static Dictionary<uint, Item_Master> GetAllDefaultArmour()
        {
            var allArmour = new Dictionary<uint, Item_Master>();
            
            foreach (var armour in _heavy)
            {
                allArmour.Add(armour.Key, armour.Value);
            }
            
            return allArmour;
        }

        static readonly Dictionary<uint, Item_Master> _heavy = new()
        {
            {
                100,
                new Item_Master(
                    new CommonStats_Item(
                        itemID: 100,
                        itemType: ItemType.Armour,
                        itemName: "Bronze ChestPlate",
                        equipmentSlots: new List<EquipmentSlot>() { EquipmentSlot.Chest },
                        itemEquippable: true,
                        maxStackSize: 1,
                        itemValue: 10
                    ),

                    new VisualStats_Item(
                        itemIcon: null,
                        itemPosition: new Vector3(-0.04f, -0.07f, 0f),
                        itemRotation: Quaternion.Euler(180, 0, 0),
                        itemScale: new Vector3(0.4f, 0.4f, 0.4f)
                    ),

                    null,

                    new ArmourStats_Item(
                        armourType: EquipmentSlot.Chest,
                        itemCoverage: 75
                    ),

                    new FixedModifiers_Item(
                        maxHealth: 5,
                        maxMana: 5,
                        maxStamina: 5,
                        pushRecovery: 1,
                        physicalArmour: 2,
                        magicArmour: 2,
                        dodgeCooldownReduction: -1
                    ),

                    new PercentageModifiers_Item(
                        attackSpeed: 0.92f
                    ),

                    null
                )
            }
        };
    }
}
