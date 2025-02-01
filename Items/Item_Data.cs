using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tools;

namespace Items
{
    [Serializable]
    public class Item_Data : Data_Class
    {
        public ulong   ItemID   => ItemCommonStats.ItemID;
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

        public override DataToDisplay GetDataToDisplay(bool toggleMissingDataDebugs)
        {
            _updateDataDisplay(DataToDisplay,
                title: "Common Stats",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allSubData: ItemCommonStats.GetDataToDisplay(toggleMissingDataDebugs));
            
            _updateDataDisplay(DataToDisplay,
                title: "Visual Stats",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allSubData: ItemVisualStats.GetDataToDisplay(toggleMissingDataDebugs));
            
            _updateDataDisplay(DataToDisplay,
                title: "Weapon Stats",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allSubData: ItemWeaponStats.GetDataToDisplay(toggleMissingDataDebugs));
            
            _updateDataDisplay(DataToDisplay,
                title: "Armour Stats",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allSubData: ItemArmourStats.GetDataToDisplay(toggleMissingDataDebugs));
            
            _updateDataDisplay(DataToDisplay,
                title: "Fixed Modifiers",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allSubData: ItemFixedModifiers.GetDataToDisplay(toggleMissingDataDebugs));
            
            _updateDataDisplay(DataToDisplay,
                title: "Percentage Modifiers",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allSubData: ItemPercentageModifiers.GetDataToDisplay(toggleMissingDataDebugs));
            
            _updateDataDisplay(DataToDisplay,
                title: "Priority Stats",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allSubData: ItemPriorityStats.GetDataToDisplay(toggleMissingDataDebugs));

            return DataToDisplay;
        }

        public override Dictionary<string, DataToDisplay> GetInteractableData(bool toggleMissingDataDebugs)
        {
            return new Dictionary<string, DataToDisplay>
            {
                {
                    "Common Stats",
                    ItemCommonStats.GetDataToDisplay(toggleMissingDataDebugs)
                },
                {
                    "Visual Stats",
                    ItemVisualStats.GetDataToDisplay(toggleMissingDataDebugs)
                },
                {
                    "Weapon Stats",
                    ItemWeaponStats.GetDataToDisplay(toggleMissingDataDebugs)
                },
                {
                    "Armour Stats",
                    ItemArmourStats.GetDataToDisplay(toggleMissingDataDebugs)
                },
                {
                    "Fixed Modifiers",
                    ItemFixedModifiers.GetDataToDisplay(toggleMissingDataDebugs)
                },
                {
                    "Percentage Modifiers",
                    ItemPercentageModifiers.GetDataToDisplay(toggleMissingDataDebugs)
                },
                {
                    "Priority Stats",
                    ItemPriorityStats.GetDataToDisplay(toggleMissingDataDebugs)
                }
            };
        }
    }

    [Serializable]
    public class Item : Data_Class
    {
        public ulong   ItemID;
        public string ItemName => DataItem.ItemCommonStats.ItemName;
        public ulong   ItemAmount;
        public ulong   ItemAmountOnHold;
        public ulong   MaxStackSize => DataItem.ItemCommonStats.MaxStackSize;

        Item_Data        _dataItem;
        public Item_Data DataItem => _dataItem ??= Item_Manager.GetItem_Data(ItemID);

        public Item(ulong itemID, ulong itemAmount)
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

        public static ulong GetItemListTotal_CountAllItems(List<Item> items)
            => (ulong)items.Sum(item => (int)item.ItemAmount);

        public static ulong GetItemListTotal_CountSpecificItem(List<Item> items, ulong itemID)
            => (ulong)items.Where(item => item.ItemID == itemID).Sum(item => (int)item.ItemAmount);

        public static float GetItemListTotal_Weight(List<Item> items)
            => items.Sum(item => item.ItemAmount * item.DataItem.ItemCommonStats.ItemWeight);
        
        public static List<Item> MergeItemLists(List<Item> listA, List<Item> listB)
        {
            var mergedItemsDict = new ConcurrentDictionary<int, Item>();

            Parallel.Invoke(
                () => processList(listA),
                () => processList(listB)
            );

            return mergedItemsDict.Values.ToList();

            void processList(List<Item> list)
            {
                foreach (var item in list)
                {
                    mergedItemsDict.AddOrUpdate(
                        (int)item.ItemID,
                        new Item(item),
                        (_, existingItem) =>
                        {
                            existingItem.ItemAmount += item.ItemAmount;
                            return existingItem;
                        });
                }
            }
        }

        public DataToDisplay DataSO_Object_Data(bool toggleMissingDataDebugs) => DataItem.GetDataToDisplay(toggleMissingDataDebugs);

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

        public override DataToDisplay GetDataToDisplay(bool toggleMissingDataDebugs)
        {
            _updateDataDisplay(DataToDisplay,
                title: "Common Stats",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: DataItem.ItemCommonStats.GetStringData());

            return DataToDisplay;
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