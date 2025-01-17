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

        public override Dictionary<string, string> GetStringData()
        {
            return new Dictionary<string, string>
            {
                { "ItemID", $"{ItemID}" },
                { "ItemName", $"{ItemName}" }
            };
        }

        public override DataToDisplay GetSubData(bool toggleMissingDataDebugs)
        {
            _updateDataDisplay(ref _dataToDisplay,
                title: "Common Stats",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allSubData: ItemCommonStats.GetSubData(toggleMissingDataDebugs));
            
            _updateDataDisplay(ref _dataToDisplay,
                title: "Visual Stats",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allSubData: ItemVisualStats.GetSubData(toggleMissingDataDebugs));
            
            _updateDataDisplay(ref _dataToDisplay,
                title: "Weapon Stats",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allSubData: ItemWeaponStats.GetSubData(toggleMissingDataDebugs));
            
            _updateDataDisplay(ref _dataToDisplay,
                title: "Armour Stats",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allSubData: ItemArmourStats.GetSubData(toggleMissingDataDebugs));
            
            _updateDataDisplay(ref _dataToDisplay,
                title: "Fixed Modifiers",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allSubData: ItemFixedModifiers.GetSubData(toggleMissingDataDebugs));
            
            _updateDataDisplay(ref _dataToDisplay,
                title: "Percentage Modifiers",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allSubData: ItemPercentageModifiers.GetSubData(toggleMissingDataDebugs));
            
            _updateDataDisplay(ref _dataToDisplay,
                title: "Priority Stats",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allSubData: ItemPriorityStats.GetSubData(toggleMissingDataDebugs));

            return _dataToDisplay;
        }

        public override Dictionary<string, DataToDisplay> GetInteractableData(bool toggleMissingDataDebugs)
        {
            return new Dictionary<string, DataToDisplay>
            {
                {
                    "Common Stats",
                    ItemCommonStats.GetSubData(toggleMissingDataDebugs)
                },
                {
                    "Visual Stats",
                    ItemVisualStats.GetSubData(toggleMissingDataDebugs)
                },
                {
                    "Weapon Stats",
                    ItemWeaponStats.GetSubData(toggleMissingDataDebugs)
                },
                {
                    "Armour Stats",
                    ItemArmourStats.GetSubData(toggleMissingDataDebugs)
                },
                {
                    "Fixed Modifiers",
                    ItemFixedModifiers.GetSubData(toggleMissingDataDebugs)
                },
                {
                    "Percentage Modifiers",
                    ItemPercentageModifiers.GetSubData(toggleMissingDataDebugs)
                },
                {
                    "Priority Stats",
                    ItemPriorityStats.GetSubData(toggleMissingDataDebugs)
                }
            };
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

        public DataToDisplay DataSO_Object_Data(bool toggleMissingDataDebugs) => DataItem.GetData_Display(toggleMissingDataDebugs);

        public override Dictionary<string, string> GetStringData()
        {
            return new Dictionary<string, string>
            {
                { "ItemID", $"{ItemID}" },
                { "ItemName", $"{ItemName}" },
                { "ItemAmount", $"{ItemAmount}" },
                { "MaxStackSize", $"{MaxStackSize}" }
            };
        }

        public override DataToDisplay GetSubData(bool toggleMissingDataDebugs)
        {
            _updateDataDisplay(ref _dataToDisplay,
                title: "Common Stats",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: DataItem.ItemCommonStats.GetStringData());

            return _dataToDisplay;
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