using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType { None, Weapon, Armour, Consumable, Misc }
public enum WeaponType
{
    None,
    OneHandedMelee,
    TwoHandedMelee,
    OneHandedRanged,
    TwoHandedRanged,
    OneHandedMagic,
    TwoHandedMagic
}

public enum WeaponClass
{
    None,
    Axe,
    ShortBow,
    ShortSword,
    Spear
}

public class Manager_Item : MonoBehaviour
{
    public static List<Item> ItemList = new();

    static HashSet<int> _usedIDs = new();

    public static void AddToList(List<Item> list, Item item)
    {
        if (_usedIDs.Contains(item.ItemStats.CommonStats.ItemID))
        {
            throw new ArgumentException("Item ID " + item.ItemStats.CommonStats.ItemID + " is already used");
        }

        _usedIDs.Add(item.ItemStats.CommonStats.ItemID);
        list.Add(item);
    }

    public virtual void Start()
    {

    }

    public static Item GetItem(int itemID = -1, string itemName = "")
    {
        if (itemID == -1 && itemName == "") { Debug.Log($"Both itemID: {itemID} and itemName: {itemName} are invalid."); return null; }

        //Eventually implement a more efficient search based on ID ranges for weapons, armour, etc.

        foreach (Item item in ItemList)
        {
            if (itemID == item.ItemID || itemName == item.ItemName) return item;
        }

        return null;
    }

    public void AttachWeaponScript(Item item, Equipment_Base equipmentSlot)
    {
        //GameManager.Destroy(equipmentSlot.GetComponent<Weapon>());

        foreach (WeaponType weaponType in item.ItemStats.WeaponStats.WeaponTypeArray)
        {
            switch (weaponType)
            {
                case WeaponType.OneHandedMelee:
                case WeaponType.TwoHandedMelee:
                    foreach (WeaponClass weaponClass in item.ItemStats.WeaponStats.WeaponClassArray)
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
                    foreach (WeaponClass weaponClass in item.ItemStats.WeaponStats.WeaponClassArray)
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

public class Item
{

    #region General
    public int ItemID { get; private set; }
    public string ItemName { get; private set; }
    public Sprite ItemIcon { get; private set; }
    public ItemType ItemType { get; private set; }
    public List<EquipmentSlot> EquipmentSlots { get; private set; }
    public int MaxStackSize { get; private set; }
    public int ItemValue { get; private set; }
    public Vector3 ItemScale { get; private set; }
    public Vector3 ItemPosition { get; private set; }
    public Vector3 ItemRotation { get; private set; }
    #endregion

    public ItemStats ItemStats { get; private set; }

    public Item(int itemID, string itemName, Sprite itemIcon, ItemType itemType, List<EquipmentSlot> equipmentSlots, int maxStackSize, int itemValue, Vector3 itemScale, Vector3 itemPosition, Vector3 itemRotation, 
        ItemStats itemStats)
    {
        ItemID = itemID;
        ItemName = itemName;
        ItemIcon = itemIcon;
        ItemType = itemType;
        EquipmentSlots = equipmentSlots;
        MaxStackSize = maxStackSize;
        ItemValue = itemValue;
        ItemScale = itemScale;
        ItemPosition = itemPosition;
        ItemRotation = itemRotation;
        ItemStats = itemStats;
    }
}

[Serializable]
public class ItemStats
{
    public CommonStats CommonStats { get; private set; }
    public WeaponStats WeaponStats { get; private set; }
    public ArmourStats ArmourStats { get; private set; }
    public FixedModifiers StatModifiersFixed { get; private set; }
    public PercentageModifiers StatModifiersPercentage { get; private set; }

    public ItemStats(
        CommonStats commonStats = null,
        WeaponStats weaponStats = null,
        ArmourStats armourStats = null,
        FixedModifiers fixedModifiers = null,
        PercentageModifiers percentageModifiers = null
        )
    {
        CommonStats = commonStats ?? new CommonStats();
        WeaponStats = weaponStats ?? new WeaponStats();
        ArmourStats = armourStats ?? new ArmourStats();
        StatModifiersFixed = fixedModifiers ?? new FixedModifiers();
        StatModifiersPercentage = percentageModifiers ?? new PercentageModifiers();
    }
}

[Serializable]
public class CommonStats
{
    public int ItemID;
    public ItemType ItemType;
    public string ItemName;
    public Sprite ItemIcon;
    public int MaxStackSize;
    public int CurrentStackSize;
    public int ItemValue;
    public float ItemWeight;
    public bool ItemEquippable;
    public Vector3 ItemPosition;
    public Vector3 ItemRotation;
    public Vector3 ItemScale;

    public CommonStats(
        int itemID,
        string itemName,
        ItemType itemType = ItemType.None,
        Sprite itemIcon = null,
        int maxStackSize = 0,
        int currentStackSize = 0,
        int itemValue = 0,
        float itemWeight = 0,
        bool itemEquippable = false,
        Vector3? itemPosition = null,
        Vector3? itemRotation = null,
        Vector3? itemScale = null
        )
    {
        this.ItemID = itemID;
        this.ItemName = itemName;
        this.ItemType = itemType;
        this.ItemIcon = itemIcon;
        this.MaxStackSize = maxStackSize;
        this.CurrentStackSize = currentStackSize;
        this.ItemValue = itemValue;
        this.ItemWeight = itemWeight;
        this.ItemEquippable = itemEquippable;
        this.ItemPosition = itemPosition ?? Vector3.zero;
        this.ItemRotation = itemRotation ?? Vector3.zero;
        this.ItemScale = itemScale ?? new Vector3(1, 1, 1);
    }
}

[Serializable]
public class PercentageModifiers
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

    public PercentageModifiers(
        float maxHealth = 1,
        float maxMana = 1,
        float maxStamina = 1,
        float pushRecovery = 1,

        float attackDamage = 1,
        float attackSpeed = 1,
        float attackSwingTime = 1,
        float attackRange = 1,
        float attackPushForce = 1,
        float attackCooldown = 1,

        float physicalDefence = 1,
        float magicalDefence = 1,

        float moveSpeed = 1,
        float dodgeCooldownReduction = 1
        )
    {
        this.MaxHealth = maxHealth;
        this.MaxMana = maxMana;
        this.MaxStamina = maxStamina;
        this.PushRecovery = pushRecovery;
        this.AttackDamage = attackDamage;
        this.AttackSpeed = attackSpeed;
        this.AttackSwingTime = attackSwingTime;
        this.AttackRange = attackRange;
        this.AttackPushForce = attackPushForce;
        this.AttackCooldown = attackCooldown;
        this.PhysicalDefence = physicalDefence;
        this.MagicalDefence = magicalDefence;
        this.MoveSpeed = moveSpeed;
        this.DodgeCooldownReduction = dodgeCooldownReduction;
    }
}

[Serializable]
public class FixedModifiers
{
    public float CurrentHealth;
    public float CurrentMana;
    public float CurrentStamina;
    public float MaxHealth;
    public float MaxMana;
    public float MaxStamina;
    public float PushRecovery;

    public List<(float, DamageType)> BaseDamage;
    public float AttackSpeed;
    public float AttackSwingTime;
    public float AttackRange;
    public float AttackPushForce;
    public float AttackCooldown;

    public float PhysicalDefence;
    public float MagicalDefence;

    public float MoveSpeed;
    public float DodgeCooldownReduction;

    // public float DamagePerAbilityLevel;
    // public float DamagePerItemLevel;
    // public SPECIAL SPECIAL;

    public FixedModifiers(
        float maxHealth = 0,
        float maxMana = 0,
        float maxStamina = 0,
        float pushRecovery = 0,

        float attackDamage = 0,
        float attackSpeed = 0,
        float attackSwingTime = 0,
        float attackRange = 0,
        float attackPushForce = 0,
        float attackCooldown = 0,

        float physicalDefence = 0,
        float magicalDefence = 0,

        float moveSpeed = 0,
        float dodgeCooldownReduction = 0
        )
    {
        this.MaxHealth = maxHealth;
        this.MaxMana = maxMana;
        this.MaxStamina = maxStamina;
        this.PushRecovery = pushRecovery;
        this.AttackDamage = attackDamage;
        this.AttackSpeed = attackSpeed;
        this.AttackSwingTime = attackSwingTime;
        this.AttackRange = attackRange;
        this.AttackPushForce = attackPushForce;
        this.AttackCooldown = attackCooldown;
        this.PhysicalDefence = physicalDefence;
        this.MagicalDefence = magicalDefence;
        this.MoveSpeed = moveSpeed;
        this.DodgeCooldownReduction = dodgeCooldownReduction;
    }
}

[Serializable]
public class WeaponStats
{
    public WeaponType[] WeaponTypeArray;
    public WeaponClass[] WeaponClassArray;
    public float MaxChargeTime;

    public WeaponStats(
        WeaponType[] weaponType = null,
        WeaponClass[] weaponClass = null,
        float maxChargeTime = 0
        )
    {
        if (weaponType == null)
        {
            weaponType = new WeaponType[] { WeaponType.None };
        }
        if (weaponClass == null)
        {
            weaponClass = new WeaponClass[] { WeaponClass.None };
        }

        this.WeaponTypeArray = weaponType;
        this.WeaponClassArray = weaponClass;
        this.MaxChargeTime = maxChargeTime;
    }
}

[Serializable]
public class ArmourStats
{
    public EquipmentSlot EquipmentSlot;
    public float ItemCoverage;

    public ArmourStats(
        EquipmentSlot armourType = EquipmentSlot.None,
        float itemCoverage = 0
        )
    {
        this.EquipmentSlot = armourType;
        this.ItemCoverage = itemCoverage;
    }
}