using System;
using System.Collections.Generic;
using Equipment;
using Tools;

namespace Items
{
    [Serializable]
    public class Item_CommonStats : Data_Class
    {
        public uint                ItemID;
        public string              ItemName;
        public ItemType            ItemType;
        public List<EquipmentSlot> EquipmentSlots;
        public uint                MaxStackSize;
        public uint                ItemLevel;
        public ItemQualityName     ItemQuality;
        public uint                ItemValue;
        public float               ItemWeight;
        public bool                ItemEquippable;

        public Item_CommonStats(
            uint                itemID         = 0,
            string              itemName       = "",
            ItemType            itemType       = ItemType.Misc,
            List<EquipmentSlot> equipmentSlots = null,
            uint                maxStackSize   = 0,
            uint                itemLevel      = 0,
            ItemQualityName     itemQuality    = ItemQualityName.Junk,
            uint                itemValue      = 0,
            float               itemWeight     = 0,
            bool                itemEquippable = false
        )
        {
            ItemID         = itemID;
            ItemName       = itemName;
            ItemType       = itemType;
            EquipmentSlots = equipmentSlots;
            MaxStackSize   = maxStackSize;
            ItemLevel      = itemLevel;
            ItemQuality    = itemQuality;
            ItemValue      = itemValue;
            ItemWeight     = itemWeight;
            ItemEquippable = itemEquippable;
        }

        public Item_CommonStats(Item_CommonStats item)
        {
            ItemID         = item.ItemID;
            ItemName       = item.ItemName;
            ItemType       = item.ItemType;
            EquipmentSlots = item.EquipmentSlots != null ? new List<EquipmentSlot>(item.EquipmentSlots) : null;
            MaxStackSize   = item.MaxStackSize;
            ItemLevel      = item.ItemLevel;
            ItemQuality    = item.ItemQuality;
            ItemValue      = item.ItemValue;
            ItemWeight     = item.ItemWeight;
            ItemEquippable = item.ItemEquippable;
        }
        
        public override Dictionary<string, string> GetStringData()
        {
            return new Dictionary<string, string>
            {
                { "ItemID", $"{ItemID}" },
                { "ItemName", $"{ItemName}" },
                { "ItemType", $"{ItemType}" },
                { "MaxStackSize", $"{MaxStackSize}" },
                { "ItemLevel", $"{ItemLevel}" },
                { "ItemQuality", $"{ItemQuality}" },
                { "ItemValue", $"{ItemValue}" },
                { "ItemWeight", $"{ItemWeight}" },
                { "ItemEquippable", $"{ItemEquippable}" }
            };
        }
        
        public override DataToDisplay GetDataToDisplay(bool toggleMissingDataDebugs)
        {
            _updateDataDisplay(DataToDisplay,
                title: "Common Stats",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: GetStringData());

            return DataToDisplay;
        }
    }
}