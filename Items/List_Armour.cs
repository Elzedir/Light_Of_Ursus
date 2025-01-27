using System.Collections.Generic;
using Equipment;
using UnityEngine;

namespace Items
{
    public abstract class List_Armour
    {
        static Dictionary<ulong, Item_Data> _defaultArmour;
        public static Dictionary<ulong, Item_Data> DefaultArmour => _defaultArmour ??= _initialiseDefaultArmour();
        
        static Dictionary<ulong, Item_Data> _initialiseDefaultArmour()
        {
            var defaultArmour = new Dictionary<ulong, Item_Data>();

            foreach (var item in _heavy())
            {
                defaultArmour.Add(item.Key, item.Value);
            }

            return defaultArmour;
        }

        static Dictionary<ulong, Item_Data> _heavy()
        {
            return new Dictionary<ulong, Item_Data>
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
}
