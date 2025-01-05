using System;
using System.Collections.Generic;
using System.Linq;
using Equipment;
using Priority;
using Station;
using UnityEngine;

namespace Items
{
    public enum ItemType
    {
        Weapon,
        Armour,
        Consumable,
        Raw_Material,
        Processed_Material,
        Misc
    }

    public abstract class Item_Manager
    {
        const string _item_SOPath = "ScriptableObjects/Item_SO";

        static Item_SO _allItems;
        static Item_SO AllItems => _allItems ??= _getItem_SO();

        public static Item_Data GetItem_Data(uint itemID) => AllItems.GetItem_Master(itemID).Data_Object;

        public static void PopulateAllItems()
        {
            AllItems.PopulateSceneData();
            // Then populate custom items.
        }

        static Item_SO _getItem_SO()
        {
            var item_SO = Resources.Load<Item_SO>(_item_SOPath);

            if (item_SO is not null) return item_SO;

            Debug.LogError("Item_SO not found. Creating temporary Item_SO.");
            item_SO = ScriptableObject.CreateInstance<Item_SO>();

            return item_SO;
        }

        public void AttachWeaponScript(Item_Data item, Equipment_Base equipmentSlot)
        {
            //GameManager.Destroy(equipmentSlot.GetComponent<Weapon>());

            foreach (var weaponType in item.ItemWeaponStats.WeaponTypeArray)
            {
                switch (weaponType)
                {
                    case WeaponType.OneHandedMelee:
                    case WeaponType.TwoHandedMelee:
                        foreach (var weaponClass in item.ItemWeaponStats.WeaponClassArray)
                        {
                            switch (weaponClass)
                            {
                                case WeaponClass.Axe:
                                    //equipmentSlot.AddComponent<Weapon_Axe>();
                                    break;
                                case WeaponClass.ShortSword:
                                    //equipmentSlot.AddComponent<Weapon_ShortSword>();
                                    break;
                                // Add more cases here
                            }
                        }

                        break;
                    case WeaponType.OneHandedRanged:
                    case WeaponType.TwoHandedRanged:
                        //equipmentSlot.AddComponent<Weapon_Bow>();
                        break;
                    case WeaponType.OneHandedMagic:
                    case WeaponType.TwoHandedMagic:
                        foreach (var weaponClass in item.ItemWeaponStats.WeaponClassArray)
                        {
                            //switch (weaponClass)
                            //{
                            //    case WeaponClass.Staff:
                            //        equipmentSlot.AddComponent<Weapon_Staff>();
                            //        break;
                            //    case WeaponClass.Wand:
                            //        equipmentSlot.AddComponent<Weapon_Wand>();
                            //        break;
                            //         Add more cases here
                            //}
                        }

                        break;
                }
            }
        }
        
        public static void ClearSOData()
        {
            AllItems.ClearSOData();
        }
    }
}