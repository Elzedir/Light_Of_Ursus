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
    static Item_Master[] _allItems;
    public static Dictionary<uint, Item_Master> AllItems = new();
    public static Item_Master GetMasterItem(uint itemID)
    {
        if (!AllItems.TryGetValue(itemID, out Item_Master item))
        {
            Debug.LogError("Item with ID " + itemID + " not found in AllItems.");
            return null;
        }

        return item;
    }

    static uint _lastUnusedID = 100000;

    public static void Initialise()
    {
        List_Weapon.InitializeWeaponData();
        List_Armour.InitializeArmourData();
        List_Consumable.InitializeConsumableData();
        List_RawMaterial.InitializeRawMaterialData();
        List_ProcessedMaterial.InitializeProcessedMaterialData();
    }

    public static void AddToList(Item_Master itemData)
    {
        if (AllItems.ContainsKey(itemData.CommonStats_Item.ItemID))
        {
            uint alreadyUsedID = itemData.CommonStats_Item.ItemID;

            while (AllItems.ContainsKey(_lastUnusedID))
            {
                _lastUnusedID++;
            }

            itemData.CommonStats_Item.ItemID = _lastUnusedID;
            Debug.Log($"ItemName: {itemData.CommonStats_Item.ItemName} ItemID is now: {itemData.CommonStats_Item.ItemID} as old ItemID: {alreadyUsedID} is used by ItemName: {GetMasterItem(alreadyUsedID).CommonStats_Item.ItemName}");
            //throw new ArgumentException("Item ID " + item.CommonStats_Item.ItemID + " is already used");
        }

        AllItems.Add(itemData.CommonStats_Item.ItemID, itemData);
    }

    public void AttachWeaponScript(Item_Master item, Equipment_Base equipmentSlot)
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
    public uint ItemID;
    public string ItemName;
    public uint ItemAmount;
    public uint MaxStackSize;

    public Item(uint itemID, uint itemAmount)
    {
        var masterItem = Manager_Item.GetMasterItem(itemID);

        if (masterItem == null) 
        {
            Debug.LogError("MasterItem for itemID: " + itemID + " is null");
            return;
        }

        ItemID = itemID;
        ItemName = masterItem.CommonStats_Item.ItemName;
        ItemAmount = itemAmount;
        MaxStackSize = masterItem.CommonStats_Item.MaxStackSize;
    }

    public Item (Item item)
    {
        ItemID = item.ItemID;
        ItemName = item.ItemName;
        ItemAmount = item.ItemAmount;
        MaxStackSize = item.MaxStackSize;
    }

    public static uint GetItemListTotal_CountAllItems(List<Item> items) 
    => (uint)items.Sum(item => item.ItemAmount);
    public static uint GetItemListTotal_CountSpecificItem(List<Item> items, uint itemID) 
    => (uint)items.Where(item => item.ItemID == itemID).Sum(item => item.ItemAmount);

    public static float GetItemListTotal_Weight(List<Item> items)
    {
        float totalWeight = 0;

        foreach (Item item in items)
        {
            totalWeight += item.ItemAmount * Manager_Item.GetMasterItem(item.ItemID).CommonStats_Item.ItemWeight;
        }

        return totalWeight;
    }

}

public class Item_Master
{
    public CommonStats_Item CommonStats_Item;
    public VisualStats_Item VisualStats_Item;
    public WeaponStats_Item WeaponStats_Item;
    public ArmourStats_Item ArmourStats_Item;
    public FixedModifiers_Item FixedModifiers_Item;
    public PercentageModifiers_Item PercentageModifiers_Item;
    public PriorityStats_Item PriorityStats_Item;

    public Item_Master(
        CommonStats_Item commonStats_Item, 
        VisualStats_Item visualStats_Item, WeaponStats_Item weaponStats_Item, ArmourStats_Item armourStats_Item, 
        FixedModifiers_Item fixedModifiers_Item, PercentageModifiers_Item percentageModifiers_Item,
        PriorityStats_Item priorityStats_Item)
    {
        CommonStats_Item = commonStats_Item ?? new CommonStats_Item();
        VisualStats_Item = visualStats_Item ?? new VisualStats_Item();
        WeaponStats_Item = weaponStats_Item ?? new WeaponStats_Item();
        ArmourStats_Item = armourStats_Item ?? new ArmourStats_Item();
        FixedModifiers_Item = fixedModifiers_Item ?? new FixedModifiers_Item();
        PercentageModifiers_Item = percentageModifiers_Item ?? new PercentageModifiers_Item();
        PriorityStats_Item = priorityStats_Item ?? new PriorityStats_Item();
    }

    public Item_Master(Item_Master item)
    {
        CommonStats_Item = new CommonStats_Item(item.CommonStats_Item);
        VisualStats_Item = new VisualStats_Item(item.VisualStats_Item);
        WeaponStats_Item = new WeaponStats_Item(item.WeaponStats_Item);
        ArmourStats_Item = new ArmourStats_Item(item.ArmourStats_Item);
        FixedModifiers_Item = new FixedModifiers_Item(item.FixedModifiers_Item);
        PercentageModifiers_Item = new PercentageModifiers_Item(item.PercentageModifiers_Item);
        PriorityStats_Item = new PriorityStats_Item(item.PriorityStats_Item);
    }
}

[CustomPropertyDrawer(typeof(Item_Master))]
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
    public uint ItemID;
    public string ItemName;
    public ItemType ItemType;
    public List<EquipmentSlot> EquipmentSlots;
    public uint MaxStackSize;
    public uint ItemLevel;
    public ItemQualityName ItemQuality;
    public uint ItemValue;
    public float ItemWeight;
    public bool ItemEquippable;

    public CommonStats_Item(
        uint itemID = 0,
        string itemName = "",
        ItemType itemType = ItemType.Misc,
        List<EquipmentSlot> equipmentSlots = null,
        uint maxStackSize = 0,
        uint itemLevel = 0,
        ItemQualityName itemQuality = ItemQualityName.Junk,
        uint itemValue = 0,
        float itemWeight = 0,
        bool itemEquippable = false
        )
    {
        ItemID = itemID;
        ItemName = itemName;
        ItemType = itemType;
        EquipmentSlots = equipmentSlots;
        MaxStackSize = maxStackSize;
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
        WeaponTypeArray = weaponType != null ? weaponType : new WeaponType[] { WeaponType.None };
        WeaponClassArray = weaponClass != null ? weaponClass : new WeaponClass[] { WeaponClass.None };
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

public class PriorityStats_Item
{
    public Dictionary<PriorityImportance, List<StationName>> Priority_Station;

    public PriorityStats_Item (Dictionary<PriorityImportance, List<StationName>> priority_Station = null)
    {
        Priority_Station = priority_Station != null
            ? new Dictionary<PriorityImportance, List<StationName>>(priority_Station)
            : new Dictionary<PriorityImportance, List<StationName>>();
    }

    public PriorityStats_Item (PriorityStats_Item other)
    {
        Priority_Station = other.Priority_Station != null
            ? new Dictionary<PriorityImportance, List<StationName>>(other.Priority_Station)
            : new Dictionary<PriorityImportance, List<StationName>>();
    }

    

    public PriorityImportance GetHighestStationPriority(List<StationName> allStations)
    {
        foreach (var priority in Priority_Station.Keys)
        {
            foreach (var station in allStations)
            {
                if (Priority_Station[priority].Contains(station))
                {
                    return priority;
                }
            }

            Debug.Log($"Priority: {priority} not found in Priority_StationsForProduction");
        }

        Debug.LogError("No priority found for any station in Priority_StationsForProduction");

        return PriorityImportance.None;
    }
}