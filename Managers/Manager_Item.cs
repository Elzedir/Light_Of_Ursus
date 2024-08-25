using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

public enum ItemType { 
    Weapon, Armour, Consumable, 
    Raw_Material, Processed_Material, 
    Misc 
}

public class Manager_Item
{
    public static Dictionary<int, Item> AllItems = new();

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
        if (AllItems.ContainsKey(item.CommonStats_Item.ItemID))
        {
            int alreadyUsedID = item.CommonStats_Item.ItemID;

            while (AllItems.ContainsKey(_lastUnusedID))
            {
                _lastUnusedID++;
            }

            item.CommonStats_Item.ItemID = _lastUnusedID;
            Debug.Log($"ItemName: {item.CommonStats_Item.ItemName} ItemID is now: {item.CommonStats_Item.ItemID} as old ItemID: {alreadyUsedID} is used by ItemName: {GetItem(alreadyUsedID).CommonStats_Item.ItemName}");
            //throw new ArgumentException("Item ID " + item.CommonStats_Item.ItemID + " is already used");
        }

        AllItems.Add(item.CommonStats_Item.ItemID, item);
    }

    public static Item GetItem(int itemID = -1, int itemQuantity = 1, bool returnItemIDFirst = false)
    {
        if (itemID == -1) throw new ArgumentException($"ItemID: {itemID} is invalid.");

        return new Item(AllItems[itemID], itemQuantity);
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

[Serializable]
public class Item
{
    public CommonStats_Item CommonStats_Item;
    public VisualStats_Item VisualStats_Item;
    public WeaponStats_Item WeaponStats_Item;
    public ArmourStats_Item ArmourStats_Item;
    public FixedModifiers_Item FixedModifiers_Item;
    public PercentageModifiers_Item PercentageModifiers_Item;

    public Item(CommonStats_Item commonStats_Item, VisualStats_Item visualStats_Item, WeaponStats_Item weaponStats_Item, ArmourStats_Item armourStats_Item, 
        FixedModifiers_Item fixedModifiers_Item, PercentageModifiers_Item percentageModifiers_Item)
    {
        CommonStats_Item = commonStats_Item ?? new CommonStats_Item();
        VisualStats_Item = visualStats_Item ?? new VisualStats_Item();
        WeaponStats_Item = weaponStats_Item ?? new WeaponStats_Item();
        ArmourStats_Item = armourStats_Item ?? new ArmourStats_Item();
        FixedModifiers_Item = fixedModifiers_Item ?? new FixedModifiers_Item();
        PercentageModifiers_Item = percentageModifiers_Item ?? new PercentageModifiers_Item();
    }

    public Item(Item item, int itemQuantity)
    {
        CommonStats_Item = new CommonStats_Item(item.CommonStats_Item);
        CommonStats_Item.CurrentStackSize = itemQuantity;
        VisualStats_Item = new VisualStats_Item(item.VisualStats_Item);
        WeaponStats_Item = new WeaponStats_Item(item.WeaponStats_Item);
        ArmourStats_Item = new ArmourStats_Item(item.ArmourStats_Item);
        FixedModifiers_Item = new FixedModifiers_Item(item.FixedModifiers_Item);
        PercentageModifiers_Item = new PercentageModifiers_Item(item.PercentageModifiers_Item);
    }
}

[CustomPropertyDrawer(typeof(Item))]
public class Item_Drawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var commonStatsNameProp = property.FindPropertyRelative("CommonStats_Item");
        var itemIDProp = commonStatsNameProp.FindPropertyRelative("ItemID");
        var itemNameProp = commonStatsNameProp.FindPropertyRelative("ItemName");
        var itemQuantityProp = commonStatsNameProp.FindPropertyRelative("CurrentStackSize");

        label.text = !string.IsNullOrEmpty(itemNameProp.ToString()) ? $"{itemIDProp.intValue}: {itemNameProp.stringValue} (Qty: {itemQuantityProp.intValue})" : "Unnamed Jobsite";

        EditorGUI.PropertyField(position, property, label, true);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true);

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
    public int ItemLevel;
    public ItemQualityName ItemQuality;
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
        int itemLevel = 0,
        ItemQualityName itemQuality = ItemQualityName.Junk,
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
        ItemLevel = itemLevel;
        ItemQuality = itemQuality;
        ItemValue = itemValue;
        ItemWeight = itemWeight;
        ItemEquippable = itemEquippable;
    }

    public CommonStats_Item(CommonStats_Item item)
    {
        ItemID = item.ItemID;
        ItemName = item.ItemName;
        ItemType = item.ItemType;
        EquipmentSlots = item.EquipmentSlots != null ? new List<EquipmentSlot>(item.EquipmentSlots) : null;
        MaxStackSize = item.MaxStackSize;
        CurrentStackSize = item.CurrentStackSize;
        ItemLevel = item.ItemLevel;
        ItemQuality = item.ItemQuality;
        ItemValue = item.ItemValue;
        ItemWeight = item.ItemWeight;
        ItemEquippable = item.ItemEquippable;
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

    public VisualStats_Item(VisualStats_Item other)
    {
        ItemIcon = other.ItemIcon;
        ItemMesh = other.ItemMesh;
        ItemMaterial = other.ItemMaterial;
        ItemCollider = other.ItemCollider;
        ItemAnimatorController = other.ItemAnimatorController;
        ItemPosition = other.ItemPosition;
        ItemRotation = other.ItemRotation;
        ItemScale = other.ItemScale;
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

    public PercentageModifiers_Item(PercentageModifiers_Item other)
    {
        CurrentHealth = other.CurrentHealth;
        CurrentMana = other.CurrentMana;
        CurrentStamina = other.CurrentStamina;
        MaxHealth = other.MaxHealth;
        MaxMana = other.MaxMana;
        MaxStamina = other.MaxStamina;
        PushRecovery = other.PushRecovery;

        AttackDamage = other.AttackDamage;
        AttackSpeed = other.AttackSpeed;
        AttackSwingTime = other.AttackSwingTime;
        AttackRange = other.AttackRange;
        AttackPushForce = other.AttackPushForce;
        AttackCooldown = other.AttackCooldown;

        PhysicalDefence = other.PhysicalDefence;
        MagicalDefence = other.MagicalDefence;

        MoveSpeed = other.MoveSpeed;
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

    public FixedModifiers_Item(FixedModifiers_Item other)
    {
        CurrentHealth = other.CurrentHealth;
        CurrentMana = other.CurrentMana;
        CurrentStamina = other.CurrentStamina;
        MaxHealth = other.MaxHealth;
        MaxMana = other.MaxMana;
        MaxStamina = other.MaxStamina;
        PushRecovery = other.PushRecovery;

        HealthRecovery = other.HealthRecovery;

        AttackDamage = other.AttackDamage != null ? new List<(float, DamageType)>(other.AttackDamage) : null;
        AttackSpeed = other.AttackSpeed;
        AttackSwingTime = other.AttackSwingTime;
        AttackRange = other.AttackRange;
        AttackPushForce = other.AttackPushForce;
        AttackCooldown = other.AttackCooldown;

        PhysicalArmour = other.PhysicalArmour;
        MagicArmour = other.MagicArmour;
        MoveSpeed = other.MoveSpeed;
        DodgeCooldownReduction = other.DodgeCooldownReduction;
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

    public WeaponStats_Item(WeaponStats_Item other)
    {
        WeaponTypeArray = other.WeaponTypeArray.ToArray();
        WeaponClassArray = other.WeaponClassArray.ToArray();
        MaxChargeTime = other.MaxChargeTime;
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

    public ArmourStats_Item(ArmourStats_Item other)
    {
        EquipmentSlot = other.EquipmentSlot;
        ItemCoverage = other.ItemCoverage;
    }
}