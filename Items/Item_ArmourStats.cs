using System;
using Equipment;

namespace Items
{
    [Serializable]
    public class Item_ArmourStats
    {
        public EquipmentSlot EquipmentSlot;
        public float         ItemCoverage;

        public Item_ArmourStats(
            EquipmentSlot armourType   = EquipmentSlot.None,
            float         itemCoverage = 0
        )
        {
            EquipmentSlot = armourType;
            ItemCoverage  = itemCoverage;
        }

        public Item_ArmourStats(Item_ArmourStats other)
        {
            EquipmentSlot = other.EquipmentSlot;
            ItemCoverage  = other.ItemCoverage;
        }
    }
}