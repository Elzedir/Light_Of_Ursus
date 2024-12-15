using System.Collections.Generic;
using Equipment;
using Items;
using UnityEngine;

namespace Items
{
    public abstract class List_Armour
    {
        public static Dictionary<uint, Item_Data> GetAllDefaultArmour()
        {
            var allArmour = new Dictionary<uint, Item_Data>();
            
            foreach (var armour in _heavy)
            {
                allArmour.Add(armour.Key, armour.Value);
            }
            
            return allArmour;
        }

        static readonly Dictionary<uint, Item_Data> _heavy = new()
        {
            {
                100,
                new Item_Data(
                    new Item_CommonStats(
                        itemID: 100,
                        itemType: ItemType.Armour,
                        itemName: "Bronze ChestPlate",
                        equipmentSlots: new List<EquipmentSlot>() { EquipmentSlot.Chest },
                        itemEquippable: true,
                        maxStackSize: 1,
                        itemValue: 10
                    ),

                    new Item_VisualStats(
                        itemIcon: null,
                        itemPosition: new Vector3(-0.04f, -0.07f, 0f),
                        itemRotation: Quaternion.Euler(180, 0, 0),
                        itemScale: new Vector3(0.4f, 0.4f, 0.4f)
                    ),

                    null,

                    new Item_ArmourStats(
                        armourType: EquipmentSlot.Chest,
                        itemCoverage: 75
                    ),

                    new Item_FixedModifiers(
                        maxHealth: 5,
                        maxMana: 5,
                        maxStamina: 5,
                        pushRecovery: 1,
                        physicalArmour: 2,
                        magicArmour: 2,
                        dodgeCooldownReduction: -1
                    ),

                    new Item_PercentageModifiers(
                        attackSpeed: 0.92f
                    ),

                    null
                )
            }
        };
    }
}
