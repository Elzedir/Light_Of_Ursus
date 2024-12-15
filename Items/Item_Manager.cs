using System;
using System.Collections.Generic;
using System.Linq;
using Priority;
using Station;
using UnityEngine;

namespace Item
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

        public static Item_Data GetItem_Data(uint itemID) => AllItems.GetItem_Master(itemID).DataObject;

        public static void PopulateAllItems()
        {
            AllItems.PopulateDefaultItems();
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

            foreach (var weaponType in item.WeaponStats_Item.WeaponTypeArray)
            {
                switch (weaponType)
                {
                    case WeaponType.OneHandedMelee:
                    case WeaponType.TwoHandedMelee:
                        foreach (var weaponClass in item.WeaponStats_Item.WeaponClassArray)
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
                        foreach (var weaponClass in item.WeaponStats_Item.WeaponClassArray)
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
    }

    [Serializable]
    public class PercentageModifiers_Item
    {
        public float CurrentHealth;
        public float CurrentMana;
        public float CurrentStamina;
        public float MaxHealth;
        public float MaxMana;
        public float MaxStamina;
        public float PushRecovery;

        public float AttackDamage;
        public float AttackSpeed;
        public float AttackSwingTime;
        public float AttackRange;
        public float AttackPushForce;
        public float AttackCooldown;

        public float PhysicalDefence;
        public float MagicalDefence;

        public float MoveSpeed;
        public float DodgeCooldownReduction;

        public PercentageModifiers_Item(
            float maxHealth    = 1,
            float maxMana      = 1,
            float maxStamina   = 1,
            float pushRecovery = 1,

            float attackDamage    = 1,
            float attackSpeed     = 1,
            float attackSwingTime = 1,
            float attackRange     = 1,
            float attackPushForce = 1,
            float attackCooldown  = 1,

            float physicalDefence = 1,
            float magicalDefence  = 1,

            float moveSpeed              = 1,
            float dodgeCooldownReduction = 1
        )
        {
            MaxHealth              = maxHealth;
            MaxMana                = maxMana;
            MaxStamina             = maxStamina;
            PushRecovery           = pushRecovery;
            AttackDamage           = attackDamage;
            AttackSpeed            = attackSpeed;
            AttackSwingTime        = attackSwingTime;
            AttackRange            = attackRange;
            AttackPushForce        = attackPushForce;
            AttackCooldown         = attackCooldown;
            PhysicalDefence        = physicalDefence;
            MagicalDefence         = magicalDefence;
            MoveSpeed              = moveSpeed;
            DodgeCooldownReduction = dodgeCooldownReduction;
        }

        public PercentageModifiers_Item(PercentageModifiers_Item other)
        {
            CurrentHealth  = other.CurrentHealth;
            CurrentMana    = other.CurrentMana;
            CurrentStamina = other.CurrentStamina;
            MaxHealth      = other.MaxHealth;
            MaxMana        = other.MaxMana;
            MaxStamina     = other.MaxStamina;
            PushRecovery   = other.PushRecovery;

            AttackDamage    = other.AttackDamage;
            AttackSpeed     = other.AttackSpeed;
            AttackSwingTime = other.AttackSwingTime;
            AttackRange     = other.AttackRange;
            AttackPushForce = other.AttackPushForce;
            AttackCooldown  = other.AttackCooldown;

            PhysicalDefence = other.PhysicalDefence;
            MagicalDefence  = other.MagicalDefence;

            MoveSpeed              = other.MoveSpeed;
            DodgeCooldownReduction = other.DodgeCooldownReduction;
        }
    }

    [Serializable]
    public class FixedModifiers_Item
    {
        public float CurrentHealth;
        public float CurrentMana;
        public float CurrentStamina;
        public float MaxHealth;
        public float MaxMana;
        public float MaxStamina;
        public float PushRecovery;

        public float HealthRecovery;

        public List<(float, DamageType)> AttackDamage;
        public float                     AttackSpeed;
        public float                     AttackSwingTime;
        public float                     AttackRange;
        public float                     AttackPushForce;
        public float                     AttackCooldown;

        public float PhysicalArmour;
        public float MagicArmour;

        public float MoveSpeed;
        public float DodgeCooldownReduction;

        // public float DamagePerAbilityLevel;
        // public float DamagePerItemLevel;
        // public SPECIAL SPECIAL;

        public FixedModifiers_Item(
            float maxHealth    = 0,
            float maxMana      = 0,
            float maxStamina   = 0,
            float pushRecovery = 0,

            float healthRecovery = 0,

            List<(float, DamageType)> attackDamage    = null,
            float                     attackSpeed     = 0,
            float                     attackSwingTime = 0,
            float                     attackRange     = 0,
            float                     attackPushForce = 0,
            float                     attackCooldown  = 0,

            float physicalArmour = 0,
            float magicArmour    = 0,

            float moveSpeed              = 0,
            float dodgeCooldownReduction = 0
        )
        {
            MaxHealth    = maxHealth;
            MaxMana      = maxMana;
            MaxStamina   = maxStamina;
            PushRecovery = pushRecovery;

            HealthRecovery = healthRecovery;

            AttackDamage    = attackDamage;
            AttackSpeed     = attackSpeed;
            AttackSwingTime = attackSwingTime;
            AttackRange     = attackRange;
            AttackPushForce = attackPushForce;
            AttackCooldown  = attackCooldown;

            PhysicalArmour         = physicalArmour;
            MagicArmour            = magicArmour;
            MoveSpeed              = moveSpeed;
            DodgeCooldownReduction = dodgeCooldownReduction;
        }

        public FixedModifiers_Item(FixedModifiers_Item other)
        {
            CurrentHealth  = other.CurrentHealth;
            CurrentMana    = other.CurrentMana;
            CurrentStamina = other.CurrentStamina;
            MaxHealth      = other.MaxHealth;
            MaxMana        = other.MaxMana;
            MaxStamina     = other.MaxStamina;
            PushRecovery   = other.PushRecovery;

            HealthRecovery = other.HealthRecovery;

            AttackDamage    = other.AttackDamage != null ? new List<(float, DamageType)>(other.AttackDamage) : null;
            AttackSpeed     = other.AttackSpeed;
            AttackSwingTime = other.AttackSwingTime;
            AttackRange     = other.AttackRange;
            AttackPushForce = other.AttackPushForce;
            AttackCooldown  = other.AttackCooldown;

            PhysicalArmour         = other.PhysicalArmour;
            MagicArmour            = other.MagicArmour;
            MoveSpeed              = other.MoveSpeed;
            DodgeCooldownReduction = other.DodgeCooldownReduction;
        }
    }

    [Serializable]
    public class WeaponStats_Item
    {
        public WeaponType[]  WeaponTypeArray;
        public WeaponClass[] WeaponClassArray;
        public float         MaxChargeTime;

        public WeaponStats_Item(
            WeaponType[]  weaponType    = null,
            WeaponClass[] weaponClass   = null,
            float         maxChargeTime = 0
        )
        {
            WeaponTypeArray  = weaponType  ?? new[] { WeaponType.None };
            WeaponClassArray = weaponClass ?? new[] { WeaponClass.None };
            MaxChargeTime    = maxChargeTime;
        }

        public WeaponStats_Item(WeaponStats_Item other)
        {
            WeaponTypeArray  = other.WeaponTypeArray.ToArray();
            WeaponClassArray = other.WeaponClassArray.ToArray();
            MaxChargeTime    = other.MaxChargeTime;
        }
    }

    [Serializable]
    public class ArmourStats_Item
    {
        public EquipmentSlot EquipmentSlot;
        public float         ItemCoverage;

        public ArmourStats_Item(
            EquipmentSlot armourType   = EquipmentSlot.None,
            float         itemCoverage = 0
        )
        {
            EquipmentSlot = armourType;
            ItemCoverage  = itemCoverage;
        }

        public ArmourStats_Item(ArmourStats_Item other)
        {
            EquipmentSlot = other.EquipmentSlot;
            ItemCoverage  = other.ItemCoverage;
        }
    }

    public class PriorityStats_Item
    {
        public readonly Dictionary<PriorityImportance, List<StationName>> Priority_Stations;

        public PriorityStats_Item(Dictionary<PriorityImportance, List<StationName>> priority_Station = null)
        {
            Priority_Stations = priority_Station != null
                ? new Dictionary<PriorityImportance, List<StationName>>(priority_Station)
                : new Dictionary<PriorityImportance, List<StationName>>();
        }

        public PriorityStats_Item(PriorityStats_Item other)
        {
            Priority_Stations = other.Priority_Stations != null
                ? new Dictionary<PriorityImportance, List<StationName>>(other.Priority_Stations)
                : new Dictionary<PriorityImportance, List<StationName>>();
        }

        public PriorityImportance GetHighestStationPriority(List<StationName> allStations)
        {
            foreach (var priority in Priority_Stations.Keys)
            {
                foreach (var station in allStations)
                {
                    if (Priority_Stations[priority].Contains(station))
                    {
                        return priority;
                    }
                }

                Debug.Log($"Priority: {priority} not found in Priority_StationsForProduction");
            }

            Debug.LogError("No priority found for any station in Priority_StationsForProduction");

            return PriorityImportance.None;
        }

        public bool IsHighestPriorityStation(StationName currentStation, List<StationName> allStations)
        {
            PriorityImportance highestPriority = GetHighestStationPriority(allStations);

            return Priority_Stations[highestPriority].Contains(currentStation);
        }
    }
}