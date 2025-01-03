using System;
using System.Collections.Generic;
using System.Linq;
using Tools;
using UnityEngine;

namespace Items
{
    [Serializable]
    public class Item_Data : Data_Class
    {
        // sort out the double popups when selecting an item, make it a first item when opening
        public uint   ItemID   => ItemCommonStats.ItemID;
        public string ItemName => ItemCommonStats.ItemName;

        public Item_CommonStats         ItemCommonStats;
        public Item_VisualStats         ItemVisualStats;
        public Item_WeaponStats         ItemWeaponStats;
        public Item_ArmourStats         ItemArmourStats;
        public Item_FixedModifiers      ItemFixedModifiers;
        public Item_PercentageModifiers ItemPercentageModifiers;
        public Item_PriorityStats       ItemPriorityStats;

        public Item_Data(
            Item_CommonStats         itemCommonStats_,
            Item_VisualStats         itemVisualStats_,
            Item_WeaponStats         itemWeaponStats_,
            Item_ArmourStats         itemArmourStats_,
            Item_FixedModifiers      itemFixedModifiers_,
            Item_PercentageModifiers itemPercentageModifiers_,
            Item_PriorityStats       itemPriorityStats_)
        {
            ItemCommonStats         = itemCommonStats_         ?? new Item_CommonStats();
            ItemVisualStats         = itemVisualStats_         ?? new Item_VisualStats();
            ItemWeaponStats         = itemWeaponStats_         ?? new Item_WeaponStats();
            ItemArmourStats         = itemArmourStats_         ?? new Item_ArmourStats();
            ItemFixedModifiers      = itemFixedModifiers_      ?? new Item_FixedModifiers();
            ItemPercentageModifiers = itemPercentageModifiers_ ?? new Item_PercentageModifiers();
            ItemPriorityStats       = itemPriorityStats_       ?? new Item_PriorityStats();
        }

        public Item_Data(Item_Data item)
        {
            ItemCommonStats         = new Item_CommonStats(item.ItemCommonStats);
            ItemVisualStats         = new Item_VisualStats(item.ItemVisualStats);
            ItemWeaponStats         = new Item_WeaponStats(item.ItemWeaponStats);
            ItemArmourStats         = new Item_ArmourStats(item.ItemArmourStats);
            ItemFixedModifiers      = new Item_FixedModifiers(item.ItemFixedModifiers);
            ItemPercentageModifiers = new Item_PercentageModifiers(item.ItemPercentageModifiers);
            ItemPriorityStats       = new Item_PriorityStats(item.ItemPriorityStats);
        }

        protected override Data_Display _getDataSO_Object(bool toggleMissingDataDebugs, Data_Display dataSO_Object)
        {
            if (dataSO_Object.Data is null && dataSO_Object.SubData is null)
                dataSO_Object = new Data_Display(
                    title: "Item Data",
                    dataDisplayType: DataDisplayType.List_CheckBox,
                    existingDataSO_Object: null,
                    data: new Dictionary<string, string>(),
                    firstData: true);

            try
            {
                if (dataSO_Object.SubData.TryGetValue("Common Stats", out var commonStats))
                {
                    dataSO_Object.SubData["Common Stats"] = new Data_Display(
                        title: "Common Stats",
                        dataDisplayType: DataDisplayType.List_Item,
                        existingDataSO_Object: dataSO_Object,
                        data: new Dictionary<string, string>());
                }
                
                if (commonStats is not null)
                {
                    commonStats.Data = new Dictionary<string, string>
                    {
                        { "ItemID", $"{ItemCommonStats.ItemID}" },
                        { "ItemName", $"{ItemCommonStats.ItemName}" },
                        { "ItemType", $"{ItemCommonStats.ItemType}" },
                        { "MaxStackSize", $"{ItemCommonStats.MaxStackSize}" },
                        { "ItemLevel", $"{ItemCommonStats.ItemLevel}" },
                        { "ItemQuality", $"{ItemCommonStats.ItemQuality}" },
                        { "ItemValue", $"{ItemCommonStats.ItemValue}" },
                        { "ItemWeight", $"{ItemCommonStats.ItemWeight}" },
                        { "ItemEquippable", $"{ItemCommonStats.ItemEquippable}" }
                    };
                }
            }
            catch
            {
                Debug.LogError("Error in Common Stats");

                Debug.LogError(ItemCommonStats.ItemID);
                Debug.LogError(ItemCommonStats.ItemName);
                Debug.LogError(ItemCommonStats.ItemType);
                Debug.LogError(ItemCommonStats.EquipmentSlots);
                Debug.LogError(ItemCommonStats.MaxStackSize);
                Debug.LogError(ItemCommonStats.ItemLevel);
                Debug.LogError(ItemCommonStats.ItemQuality);
                Debug.LogError(ItemCommonStats.ItemValue);
                Debug.LogError(ItemCommonStats.ItemWeight);
                Debug.LogError(ItemCommonStats.ItemEquippable);
            }

            try
            {
                if (dataSO_Object.SubData.TryGetValue("Visual Stats", out var visualStats))
                {
                    dataSO_Object.SubData["Visual Stats"] = new Data_Display(
                        title: "Visual Stats",
                        dataDisplayType: DataDisplayType.List_Item,
                        existingDataSO_Object: dataSO_Object,
                        data: new Dictionary<string, string>());
                }
                
                if (visualStats is not null)
                {
                    visualStats.Data = new Dictionary<string, string>
                    {
                        { "ItemIcon", $"{ItemVisualStats.ItemIcon}" },
                        { "ItemMesh", $"{ItemVisualStats.ItemMesh}" },
                        { "ItemMaterial", $"{ItemVisualStats.ItemMaterial}" },
                        { "ItemCollider", $"{ItemVisualStats.ItemCollider}" },
                        { "ItemAnimatorController", $"{ItemVisualStats.ItemAnimatorController}" },
                        { "ItemPosition", $"{ItemVisualStats.ItemPosition}" },
                        { "ItemRotation", $"{ItemVisualStats.ItemRotation}" },
                        { "ItemScale", $"{ItemVisualStats.ItemScale}" }
                    };
                }
            }
            catch
            {
                Debug.LogError("Error in Visual Stats");
            }

            try
            {
                if (dataSO_Object.SubData.TryGetValue("Weapon Stats", out var weaponStats))
                {
                    dataSO_Object.SubData["Weapon Stats"] = new Data_Display(
                        title: "Weapon Stats",
                        dataDisplayType: DataDisplayType.List_Item,
                        existingDataSO_Object: dataSO_Object,
                        data: new Dictionary<string, string>());
                }
                
                if (weaponStats is not null)
                {
                    weaponStats.Data = new Dictionary<string, string>
                    {
                        { "WeaponTypeArray", $"{string.Join(", ",  ItemWeaponStats.WeaponTypeArray)}" },
                        { "WeaponClassArray", $"{string.Join(", ", ItemWeaponStats.WeaponClassArray)}" },
                        { "MaxChargeTime", $"{ItemWeaponStats.MaxChargeTime}" }
                    };
                }
            }
            catch
            {
                Debug.LogError("Error in Weapon Stats");
            }

            try
            {
                if (dataSO_Object.SubData.TryGetValue("Armour Stats", out var armourStats))
                {
                    dataSO_Object.SubData["Armour Stats"] = new Data_Display(
                        title: "Armour Stats",
                        dataDisplayType: DataDisplayType.List_Item,
                        existingDataSO_Object: dataSO_Object,
                        data: new Dictionary<string, string>());
                }
                
                if (armourStats is not null)
                {
                    armourStats.Data = new Dictionary<string, string>
                    {
                        { "EquipmentSlot", $"{ItemArmourStats.EquipmentSlot}" },
                        { "ItemCoverage", $"{ItemArmourStats.ItemCoverage}" }
                    };
                }
            }
            catch
            {
                Debug.LogError("Error in Armour Stats");
            }

            return dataSO_Object;
        }
    }

    [Serializable]
    public class Item : Data_Class
    {
        public uint   ItemID;
        public string ItemName => DataItem.ItemCommonStats.ItemName;
        public uint   ItemAmount;
        public uint   ItemAmountOnHold;
        public uint   MaxStackSize => DataItem.ItemCommonStats.MaxStackSize;

        Item_Data        _dataItem;
        public Item_Data DataItem => _dataItem ??= Item_Manager.GetItem_Data(ItemID);

        public Item(uint itemID, uint itemAmount)
        {
            ItemID       = itemID;
            ItemAmount   = itemAmount;
        }

        public Item(Item item)
        {
            ItemID           = item.ItemID;
            ItemAmount       = item.ItemAmount;
            ItemAmountOnHold = item.ItemAmountOnHold;
        }

        public static uint GetItemListTotal_CountAllItems(List<Item> items)
            => (uint)items.Sum(item => item.ItemAmount);

        public static uint GetItemListTotal_CountSpecificItem(List<Item> items, uint itemID)
            => (uint)items.Where(item => item.ItemID == itemID).Sum(item => item.ItemAmount);

        public static float GetItemListTotal_Weight(List<Item> items)
            => items.Sum(item => item.ItemAmount * item.DataItem.ItemCommonStats.ItemWeight);

        public Data_Display DataSO_Object_Data(bool toggleMissingDataDebugs) => DataItem.GetDataSO_Object(toggleMissingDataDebugs);

        protected override Data_Display _getDataSO_Object(bool toggleMissingDataDebugs, Data_Display dataSO_Object)
        {
            if (dataSO_Object.Data is null && dataSO_Object.SubData is null)
                dataSO_Object = new Data_Display(
                    title: "Item",
                    dataDisplayType: DataDisplayType.List_CheckBox,
                    existingDataSO_Object: null,
                    subData: new Dictionary<string, Data_Display>(),
                    firstData: true);

            try
            {
                if (!dataSO_Object.SubData.TryGetValue("Common Stats", out var commonStats))
                {
                    dataSO_Object.SubData["Common Stats"] = new Data_Display(
                        title: "Common Stats",
                        dataDisplayType: DataDisplayType.List_Item,
                        existingDataSO_Object: dataSO_Object,
                        data: new Dictionary<string, string>());
                }
                
                if (commonStats is not null)
                {
                    commonStats.Data = new Dictionary<string, string>
                    {
                        { "ItemID", $"{ItemID}" },
                        { "ItemName", $"{ItemName}" },
                        { "ItemAmount", $"{ItemAmount}" },
                        { "MaxStackSize", $"{MaxStackSize}" }
                    };
                }
            }
            catch
            {
                Debug.LogError("Error in Common Stats");
            }

            return dataSO_Object;
        }
    }

    public enum ItemQualityName
    {
        Junk,
        Rusted,
        Poor,
        Common,
        Uncommon,
        Rare,
        Epic,
        Divine,
        Legendary,
        Mythic,
        Celestial,
        Primordial,

        Unique,
        Named,
    }
}