using System;
using System.Collections.Generic;
using Equipment;
using Tools;

namespace Items
{
    [Serializable]
    public class Item_ArmourStats : Data_Class
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

        public override Dictionary<string, string> GetStringData()
        {
            return new Dictionary<string, string>
            {
                { "EquipmentSlot", $"{EquipmentSlot}" },
                { "ItemCoverage", $"{ItemCoverage}" }
            };
        }

        public override DataToDisplay GetDataToDisplay(bool toggleMissingDataDebugs)
        {
            _updateDataDisplay(DataToDisplay,
                title: "Armour Stats",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: GetStringData());

            return DataToDisplay;
        }
    }
}