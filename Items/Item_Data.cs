using System;
using System.Collections.Generic;
using System.Linq;
using Tools;
using UnityEngine;
using UnityEngine.Serialization;

namespace Item
{
    [Serializable]
    public class Item_Data : Data_Class
    {
        // sort out the double popups when selecting an item, make it a first item when opening
        public uint   ItemID   => ItemCommonStats_.ItemID;
        public string ItemName => ItemCommonStats_.ItemName;

        [FormerlySerializedAs("CommonStats_Item")] public Item_CommonStats         ItemCommonStats_;
        [FormerlySerializedAs("VisualStats_Item")]                                           public Item_VisualStats         ItemVisualStats_;
        public                                            WeaponStats_Item         WeaponStats_Item;
        public                                            ArmourStats_Item         ArmourStats_Item;
        public                                            FixedModifiers_Item      FixedModifiers_Item;
        public                                            PercentageModifiers_Item PercentageModifiers_Item;
        public                                            PriorityStats_Item       PriorityStats_Item;

        public Item_Data(
            Item_CommonStats         itemCommonStats_,
            Item_VisualStats         itemVisualStats_,
            WeaponStats_Item         weaponStats_Item,
            ArmourStats_Item         armourStats_Item,
            FixedModifiers_Item      fixedModifiers_Item,
            PercentageModifiers_Item percentageModifiers_Item,
            PriorityStats_Item       priorityStats_Item)
        {
            ItemCommonStats_         = itemCommonStats_         ?? new Item_CommonStats();
            ItemVisualStats_         = itemVisualStats_         ?? new Item_VisualStats();
            WeaponStats_Item         = weaponStats_Item         ?? new WeaponStats_Item();
            ArmourStats_Item         = armourStats_Item         ?? new ArmourStats_Item();
            FixedModifiers_Item      = fixedModifiers_Item      ?? new FixedModifiers_Item();
            PercentageModifiers_Item = percentageModifiers_Item ?? new PercentageModifiers_Item();
            PriorityStats_Item       = priorityStats_Item       ?? new PriorityStats_Item();
        }

        public Item_Data(Item_Data item)
        {
            ItemCommonStats_         = new Item_CommonStats(item.ItemCommonStats_);
            ItemVisualStats_         = new Item_VisualStats(item.ItemVisualStats_);
            WeaponStats_Item         = new WeaponStats_Item(item.WeaponStats_Item);
            ArmourStats_Item         = new ArmourStats_Item(item.ArmourStats_Item);
            FixedModifiers_Item      = new FixedModifiers_Item(item.FixedModifiers_Item);
            PercentageModifiers_Item = new PercentageModifiers_Item(item.PercentageModifiers_Item);
            PriorityStats_Item       = new PriorityStats_Item(item.PriorityStats_Item);
        }

        protected override DataSO_Object _getDataSO_Object()
        {
            var dataObjects = new List<DataSO_Object>();

            try
            {
                dataObjects.Add(new DataSO_Object(
                    title: "Common Stats",
                    dataDisplayType: DataDisplayType.Item,
                    data: new List<string>
                    {
                        $"ItemID: {ItemCommonStats_.ItemID}",
                        $"ItemName: {ItemCommonStats_.ItemName}",
                        $"ItemType: {ItemCommonStats_.ItemType}",
                        $"EquipmentSlots: {string.Join(", ", ItemCommonStats_.EquipmentSlots)}",
                        $"MaxStackSize: {ItemCommonStats_.MaxStackSize}",
                        $"ItemLevel: {ItemCommonStats_.ItemLevel}",
                        $"ItemQuality: {ItemCommonStats_.ItemQuality}",
                        $"ItemValue: {ItemCommonStats_.ItemValue}",
                        $"ItemWeight: {ItemCommonStats_.ItemWeight}",
                        $"ItemEquippable: {ItemCommonStats_.ItemEquippable}"
                    }));
            }
            catch
            {
                Debug.LogError("Error in Common Stats");
            }

            try
            {
                dataObjects.Add(new DataSO_Object(
                    title: "Visual Stats",
                    dataDisplayType: DataDisplayType.Item,
                    data: new List<string>
                    {
                        $"ItemIcon: {ItemVisualStats_.ItemIcon}",
                        $"ItemMesh: {ItemVisualStats_.ItemMesh}",
                        $"ItemMaterial: {ItemVisualStats_.ItemMaterial}",
                        $"ItemCollider: {ItemVisualStats_.ItemCollider}",
                        $"ItemAnimatorController: {ItemVisualStats_.ItemAnimatorController}",
                        $"ItemPosition: {ItemVisualStats_.ItemPosition}",
                        $"ItemRotation: {ItemVisualStats_.ItemRotation}",
                        $"ItemScale: {ItemVisualStats_.ItemScale}"
                    }));
            }
            catch
            {
                Debug.LogError("Error in Visual Stats");
            }

            try
            {
                dataObjects.Add(new DataSO_Object(
                    title: "Weapon Stats",
                    dataDisplayType: DataDisplayType.Item,
                    data: new List<string>
                    {
                        $"WeaponTypeArray: {string.Join(", ",  WeaponStats_Item.WeaponTypeArray)}",
                        $"WeaponClassArray: {string.Join(", ", WeaponStats_Item.WeaponClassArray)}",
                        $"MaxChargeTime: {WeaponStats_Item.MaxChargeTime}"
                    }));
            }
            catch
            {
                Debug.LogError("Error in Weapon Stats");
            }

            try
            {
                dataObjects.Add(new DataSO_Object(
                    title: "Armour Stats",
                    dataDisplayType: DataDisplayType.Item,
                    data: new List<string>
                    {
                        $"EquipmentSlot: {ArmourStats_Item.EquipmentSlot}",
                        $"ItemCoverage: {ArmourStats_Item.ItemCoverage}"
                    }));
            }
            catch
            {
                Debug.LogError("Error in Armour Stats");
            }

            return new DataSO_Object(
                title: $"{ItemID}: {ItemName}",
                dataDisplayType: DataDisplayType.List,
                subData: new List<DataSO_Object>(dataObjects));
        }
    }
    [Serializable]
    public class Item : Data_Class
    {
        public uint   ItemID;
        public string ItemName;
        public uint   ItemAmount;
        public uint   MaxStackSize;

        Item_Data        _dataItem;
        public Item_Data DataItem => _dataItem ??= Item_Manager.GetItem_Data(ItemID);

        public Item(uint itemID, uint itemAmount)
        {
            var item_Data = Item_Manager.GetItem_Data(itemID);

            if (item_Data == null)
            {
                Debug.LogError("MasterItem for itemID: " + itemID + " is null");
                return;
            }

            ItemID       = itemID;
            ItemName     = item_Data.ItemCommonStats_.ItemName;
            ItemAmount   = itemAmount;
            MaxStackSize = item_Data.ItemCommonStats_.MaxStackSize;
        }

        public Item(Item item)
        {
            ItemID       = item.ItemID;
            ItemName     = item.ItemName;
            ItemAmount   = item.ItemAmount;
            MaxStackSize = item.MaxStackSize;
        }

        public static uint GetItemListTotal_CountAllItems(List<Item> items)
            => (uint)items.Sum(item => item.ItemAmount);

        public static uint GetItemListTotal_CountSpecificItem(List<Item> items, uint itemID)
            => (uint)items.Where(item => item.ItemID == itemID).Sum(item => item.ItemAmount);

        public static float GetItemListTotal_Weight(List<Item> items)
            => items.Sum(item => item.ItemAmount * item.DataItem.ItemCommonStats_.ItemWeight);

        public DataSO_Object DataSO_Object_Data => DataItem.DataSO_Object;

        protected override DataSO_Object _getDataSO_Object()
        {
            var dataObjects = new List<DataSO_Object>();

            try
            {
                dataObjects.Add(new DataSO_Object(
                    title: "Common Stats",
                    dataDisplayType: DataDisplayType.Item,
                    data: new List<string>
                    {
                        $"ItemID: {ItemID}",
                        $"ItemName: {ItemName}",
                        $"ItemAmount: {ItemAmount}",
                        $"MaxStackSize: {MaxStackSize}",
                    }));
            }
            catch
            {
                Debug.LogError("Error in Common Stats");
            }

            return new DataSO_Object(
                title: $"{ItemID}: {ItemName}",
                dataDisplayType: DataDisplayType.List,
                subData: new List<DataSO_Object>(dataObjects));
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