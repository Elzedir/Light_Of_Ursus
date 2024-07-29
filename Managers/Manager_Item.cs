using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using UnityEditor.Animations;
using UnityEngine;
using Unity.VisualScripting;

public enum ItemType { 
    Weapon, Armour, Consumable, 
    Raw_Material, Processed_Material, 
    Misc 
}

public class Manager_Item
{
    public static List<Item> ItemList = new();

    static HashSet<int> _usedIDs = new();
    static int _lastUnusedID = 100000;

    public static void Initialise()
    {
        List_Weapon.InitializeWeaponData();
        List_Armour.InitializeArmourData();
        List_Consumable.InitializeConsumableData();
        List_RawMaterial.InitializeRawMaterialData();
        List_ProcessedMaterial.InitializeProcessedMaterialData();
    }

    public static void AddToList(Item item)
    {
        if (_usedIDs.Contains(item.CommonStats_Item.ItemID))
        {
            // For now, just give them a new one.
            int alreadyUsedID = item.CommonStats_Item.ItemID;

            while (_usedIDs.Contains(_lastUnusedID))
            {
                _lastUnusedID++;
            }

            item.CommonStats_Item.ItemID = _lastUnusedID;
            Debug.Log($"ItemName: {item.CommonStats_Item.ItemName} ItemID is now: {item.CommonStats_Item.ItemID} as old ItemID: {alreadyUsedID} is used by ItemName: {GetItem(alreadyUsedID).CommonStats_Item.ItemName}");
            //throw new ArgumentException("Item ID " + item.CommonStats_Item.ItemID + " is already used");
        }

        _usedIDs.Add(item.CommonStats_Item.ItemID);
        ItemList.Add(item);
    }

    public static Item GetItem(int itemID = -1, int itemQuantity = 1, string itemName = "",  bool returnItemIDFirst = false)
    {
        if (itemID == -1 && itemName == "") throw new ArgumentException($"Both ItemID: {itemID} and ItemName: {itemName} are invalid.");

        //Eventually implement a more efficient search based on ID ranges for weapons, armour, etc.

        Item foundItem = null;

        if (returnItemIDFirst || string.IsNullOrEmpty(itemName))
        {
            foundItem = ItemList.FirstOrDefault(i => i.CommonStats_Item.ItemID == itemID)
                        ?? ItemList.FirstOrDefault(i => i.CommonStats_Item.ItemName == itemName);
        }
        else
        {
            foundItem = ItemList.FirstOrDefault(i => i.CommonStats_Item.ItemName == itemName)
                        ?? ItemList.FirstOrDefault(i => i.CommonStats_Item.ItemID == itemID);
        }

        return foundItem?.Clone(itemQuantity);
    }

    public void AttachWeaponScript(Item item, Equipment_Base equipmentSlot)
    {
        //GameManager.Destroy(equipmentSlot.GetComponent<Weapon>());

        foreach (WeaponType weaponType in item.WeaponStats_Item.WeaponTypeArray)
        {
            switch (weaponType)
            {
                case WeaponType.OneHandedMelee:
                case WeaponType.TwoHandedMelee:
                    foreach (WeaponClass weaponClass in item.WeaponStats_Item.WeaponClassArray)
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
                    foreach (WeaponClass weaponClass in item.WeaponStats_Item.WeaponClassArray)
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
public class DisplayItem
{
    public int ItemID;
    public string ItemName;
    public int ItemQuantity;

    public DisplayItem(int itemID, string itemName, int itemQuantity)
    {
        ItemID = itemID;
        ItemName = itemName;
        ItemQuantity = itemQuantity;
    }
}

[Serializable]
public class Item
{
    public CommonStats_Item CommonStats_Item { get; private set; }
    public VisualStats_Item VisualStats_Item { get; private set; }
    public WeaponStats_Item WeaponStats_Item { get; private set; }
    public ArmourStats_Item ArmourStats_Item { get; private set; }
    public FixedModifiers_Item FixedModifiers_Item { get; private set; }
    public PercentageModifiers_Item PercentageModifiers_Item { get; private set; }

    public Item(CommonStats_Item commonStats_Item = null, VisualStats_Item visualStats_Item = null, WeaponStats_Item weaponStats_Item = null, ArmourStats_Item armourStats_Item = null, 
        FixedModifiers_Item fixedModifiers_Item = null, PercentageModifiers_Item percentageModifiers_Item = null)
    {
        CommonStats_Item = commonStats_Item ?? new CommonStats_Item();
        VisualStats_Item = visualStats_Item ?? new VisualStats_Item();
        WeaponStats_Item = weaponStats_Item ?? new WeaponStats_Item();
        ArmourStats_Item = armourStats_Item ?? new ArmourStats_Item();
        FixedModifiers_Item = fixedModifiers_Item ?? new FixedModifiers_Item();
        PercentageModifiers_Item = percentageModifiers_Item ?? new PercentageModifiers_Item();
    }

    public Item Clone(int itemQuantity)
    {
        return new Item
        {
            CommonStats_Item = this.CommonStats_Item.Clone(itemQuantity),
            VisualStats_Item = this.VisualStats_Item.Clone(),
        };
    }
}

[Serializable]
public class CommonStats_Item
{
    public int ItemID;
    public string ItemName;
    public ItemType ItemType;
    public List<EquipmentSlot> EquipmentSlots;
    public int MaxStackSize;
    public int CurrentStackSize;
    public int ItemValue;
    public float ItemWeight;
    public bool ItemEquippable;

    public CommonStats_Item(
        int itemID = 0,
        string itemName = "",
        ItemType itemType = ItemType.Misc,
        List<EquipmentSlot> equipmentSlots = null,
        int maxStackSize = 0,
        int currentStackSize = 0,
        int itemValue = 0,
        float itemWeight = 0,
        bool itemEquippable = false
        )
    {
        ItemID = itemID;
        ItemName = itemName;
        ItemType = itemType;
        EquipmentSlots = equipmentSlots;
        MaxStackSize = maxStackSize;
        CurrentStackSize = currentStackSize;
        ItemValue = itemValue;
        ItemWeight = itemWeight;
        ItemEquippable = itemEquippable;
    }

    public CommonStats_Item Clone(int itemQuantity)
    {
        return new CommonStats_Item
        {
            ItemID = this.ItemID,
            ItemName = this.ItemName,
            ItemType = this.ItemType,
            EquipmentSlots = this.EquipmentSlots != null ? new List<EquipmentSlot>(this.EquipmentSlots) : new List<EquipmentSlot>(),
            MaxStackSize = this.MaxStackSize,
            CurrentStackSize = itemQuantity,
            ItemValue = this.ItemValue,
            ItemWeight = this.ItemWeight,
            ItemEquippable = this.ItemEquippable
        };
    }
}

[Serializable]
public class VisualStats_Item
{
    public Sprite ItemIcon;
    public Mesh ItemMesh;
    public Material ItemMaterial;
    public Collider ItemCollider;
    public RuntimeAnimatorController ItemAnimatorController;
    public Vector3 ItemPosition;
    public Quaternion ItemRotation;
    public Vector3 ItemScale;

    public VisualStats_Item(
        Sprite itemIcon = null,
        Mesh itemMesh = null,
        Material itemMaterial = null,
        Collider itemCollider = null,
        RuntimeAnimatorController itemAnimatorController = null,
        Vector3? itemPosition = null,
        Quaternion? itemRotation = null,
        Vector3? itemScale = null

        )
    {
        ItemIcon = itemIcon;
        ItemMesh = itemMesh;
        ItemMaterial = itemMaterial;
        ItemCollider = itemCollider;
        ItemAnimatorController = itemAnimatorController;
        ItemPosition = itemPosition ?? Vector3.zero;
        ItemRotation = itemRotation ?? Quaternion.identity;
        ItemScale = itemScale ?? Vector3.one;
    }

    public VisualStats_Item Clone()
    {
        return new VisualStats_Item
        {
            ItemIcon = this.ItemIcon,
            ItemMesh = this.ItemMesh,
            ItemMaterial = this.ItemMaterial,
            ItemCollider = this.ItemCollider,
            ItemAnimatorController = this.ItemAnimatorController,
            ItemPosition = this.ItemPosition,
            ItemRotation = this.ItemRotation,
            ItemScale = this.ItemScale
        };
    }

    public void DisplayVisuals(GameObject go)
    {
        _addColliderToGameObject(go);
        _addMeshToGameObject(go);

        go.transform.localPosition = ItemPosition;
        go.transform.localRotation = ItemRotation;
        go.transform.localScale = ItemScale;
    }

    void _addColliderToGameObject(GameObject itemGO)
    {
        if (ItemCollider is BoxCollider)
        {
            BoxCollider original = ItemCollider as BoxCollider;
            BoxCollider copy = itemGO.AddComponent<BoxCollider>();
            copy.center = original.center;
            copy.size = original.size;
        }
        else if (ItemCollider is SphereCollider)
        {
            SphereCollider original = ItemCollider as SphereCollider;
            SphereCollider copy = itemGO.AddComponent<SphereCollider>();
            copy.center = original.center;
            copy.radius = original.radius;
        }
        else if (ItemCollider is CapsuleCollider)
        {
            CapsuleCollider original = ItemCollider as CapsuleCollider;
            CapsuleCollider copy = itemGO.AddComponent<CapsuleCollider>();
            copy.center = original.center;
            copy.radius = original.radius;
            copy.height = original.height;
            copy.direction = original.direction;
        }
        else if (ItemCollider is MeshCollider)
        {
            MeshCollider original = ItemCollider as MeshCollider;
            MeshCollider copy = itemGO.AddComponent<MeshCollider>();
            copy.sharedMesh = original.sharedMesh;
            copy.convex = original.convex;
        }
        else if (ItemCollider == null)
        {
            BoxCollider copy = itemGO.AddComponent<BoxCollider>();
        }
        else
        {
            Debug.LogWarning("Collider type not supported: " + ItemCollider.GetType());
        }
    }

    void _addMeshToGameObject(GameObject go)
    {
        MeshRenderer itemRenderer = go.AddComponent<MeshRenderer>();
        MeshFilter itemFilter = go.AddComponent<MeshFilter>();

        itemRenderer.material = ItemMaterial;
        itemFilter.mesh = ItemMesh;
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
        MaxHealth = maxHealth;
        MaxMana = maxMana;
        MaxStamina = maxStamina;
        PushRecovery = pushRecovery;
        AttackDamage = attackDamage;
        AttackSpeed = attackSpeed;
        AttackSwingTime = attackSwingTime;
        AttackRange = attackRange;
        AttackPushForce = attackPushForce;
        AttackCooldown = attackCooldown;
        PhysicalDefence = physicalDefence;
        MagicalDefence = magicalDefence;
        MoveSpeed = moveSpeed;
        DodgeCooldownReduction = dodgeCooldownReduction;
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
    public float AttackSpeed;
    public float AttackSwingTime;
    public float AttackRange;
    public float AttackPushForce;
    public float AttackCooldown;

    public float PhysicalArmour;
    public float MagicArmour;

    public float MoveSpeed;
    public float DodgeCooldownReduction;

    // public float DamagePerAbilityLevel;
    // public float DamagePerItemLevel;
    // public SPECIAL SPECIAL;

    public FixedModifiers_Item(
        float maxHealth = 0,
        float maxMana = 0,
        float maxStamina = 0,
        float pushRecovery = 0,

        float healthRecovery = 0,

        List<(float, DamageType)> attackDamage = null,
        float attackSpeed = 0,
        float attackSwingTime = 0,
        float attackRange = 0,
        float attackPushForce = 0,
        float attackCooldown = 0,

        float physicalArmour = 0,
        float magicArmour = 0,

        float moveSpeed = 0,
        float dodgeCooldownReduction = 0
        )
    {
        MaxHealth = maxHealth;
        MaxMana = maxMana;
        MaxStamina = maxStamina;
        PushRecovery = pushRecovery;

        HealthRecovery = healthRecovery;

        AttackDamage = attackDamage;
        AttackSpeed = attackSpeed;
        AttackSwingTime = attackSwingTime;
        AttackRange = attackRange;
        AttackPushForce = attackPushForce;
        AttackCooldown = attackCooldown;

        PhysicalArmour = physicalArmour;
        MagicArmour = magicArmour;
        MoveSpeed = moveSpeed;
        DodgeCooldownReduction = dodgeCooldownReduction;
    }
}

[Serializable]
public class WeaponStats_Item
{
    public WeaponType[] WeaponTypeArray;
    public WeaponClass[] WeaponClassArray;
    public float MaxChargeTime;

    public WeaponStats_Item(
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

        WeaponTypeArray = weaponType;
        WeaponClassArray = weaponClass;
        MaxChargeTime = maxChargeTime;
    }
}

[Serializable]
public class ArmourStats_Item
{
    public EquipmentSlot EquipmentSlot;
    public float ItemCoverage;

    public ArmourStats_Item(
        EquipmentSlot armourType = EquipmentSlot.None,
        float itemCoverage = 0
        )
    {
        EquipmentSlot = armourType;
        ItemCoverage = itemCoverage;
    }
}