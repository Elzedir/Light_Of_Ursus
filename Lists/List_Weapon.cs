using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

public enum WeaponType
{
    None,
    OneHandedMelee,
    TwoHandedMelee,
    OneHandedRanged,
    TwoHandedRanged,
    OneHandedMagic,
    TwoHandedMagic,
    OneHandedShield,
    TwoHandedShield
}

public enum WeaponClass
{
    None,
    Axe,
    Shield,
    ShortBow,
    ShortSword,
    Spear
}

public class List_Weapon : Manager_Item
{
    public static void InitializeWeaponData()
    {
        _melee();
        _ranged();
    }
    static void _melee()
    {
        _shortSwords();
        _shields();
    }
    static void _ranged()
    {
        _shortBows();
    }
    static void _shortBows()
    {
        CommonStats commonStats = new CommonStats(
            itemID: 3,
            itemType: ItemType.Weapon,
            itemName: "Wooden ShortBow",
            equipmentSlots: new List<EquipmentSlot>() { EquipmentSlot.RightHand },
            itemEquippable: true,
            maxStackSize: 1,
            itemValue: 15
            );

        VisualStats visualStats = new VisualStats(
            itemIcon: null,
            itemPosition: Vector3.zero,
            itemRotation: Quaternion.identity,
            itemScale: Vector3.one
            );

        WeaponStats weaponStats = new WeaponStats(
            weaponType: new WeaponType[] { WeaponType.TwoHandedRanged },
            weaponClass: new WeaponClass[] { WeaponClass.ShortBow },
            maxChargeTime: 2
            );

        FixedModifiers fixedModifiers = new FixedModifiers(
            attackRange: 1
            );

        PercentageModifiers percentageModifiers = new PercentageModifiers(
            attackDamage: 1.1f,
            attackSpeed: 1.5f,
            attackSwingTime: 3f,
            attackRange: 3f,
            attackPushForce: 1.1f
            );

        AddToList(new Item(commonStats: commonStats, visualStats: visualStats, weaponStats: weaponStats, fixedModifiers: fixedModifiers, percentageModifiers: percentageModifiers));
    }
    static void _shortSwords()
    {
        CommonStats commonStats = new CommonStats(
            itemID: 1,
            itemType: ItemType.Weapon,
            itemName: "Wooden ShortSword",
            equipmentSlots: new List<EquipmentSlot>() { EquipmentSlot.RightHand, EquipmentSlot.LeftHand },
            itemEquippable: true,
            maxStackSize: 1,
            itemValue: 15
            );

        VisualStats visualStats = new VisualStats(
            itemIcon: null,
            itemMesh: GameObject.Find("TestSword").GetComponent<MeshFilter>().mesh, //Other thing for now
            itemMaterial: Resources.Load<Material>("Materials/Material_Red"),
            itemAnimatorController: Resources.Load<RuntimeAnimatorController>("Animators/Test_Weapon"),
            itemCollider: new CapsuleCollider(),
            itemPosition: new Vector3(0.5f, -0.2f, -0.2f),
            itemRotation: Quaternion.Euler(215, 0, 0),
            itemScale: new Vector3(0.1f, 0.6f, 0.1f)
            );

        WeaponStats weaponStats = new WeaponStats(
            weaponType: new WeaponType[] { WeaponType.OneHandedMelee },
            weaponClass: new WeaponClass[] { WeaponClass.ShortSword },
            maxChargeTime: 3
            );

        FixedModifiers fixedModifiers = new FixedModifiers(
            );

        PercentageModifiers percentageModifiers = new PercentageModifiers(
            attackDamage: 1.2f,
            attackSpeed: 1.1f,
            attackSwingTime: 1.1f,
            attackPushForce: 1.1f
            );

        AddToList(new Item(commonStats: commonStats, visualStats: visualStats, weaponStats: weaponStats, fixedModifiers: fixedModifiers, percentageModifiers: percentageModifiers));
    }

    static void _shields()
    {
        CommonStats commonStats = new CommonStats(
            itemID: 2,
            itemType: ItemType.Weapon,
            itemName: "Test Shield",
            equipmentSlots: new List<EquipmentSlot>() { EquipmentSlot.LeftHand },
            itemEquippable: true,
            maxStackSize: 1,
            itemValue: 15
            );

        VisualStats visualStats = new VisualStats(
            itemIcon: null,
            itemMesh: Resources.GetBuiltinResource<Mesh>("Cube.fbx"),
            itemMaterial: Resources.Load<Material>("Materials/Material_Red"),
            itemAnimatorController: Resources.Load<RuntimeAnimatorController>("Animators/Test_Shield"),
            itemPosition: new Vector3(-0.6f, 0f, 0f),
            itemRotation: Quaternion.Euler(0, -90, 0),
            itemScale: new Vector3(1f, 1f, 0.1f)
            );

        WeaponStats weaponStats = new WeaponStats(
            weaponType: new WeaponType[] { WeaponType.OneHandedShield },
            weaponClass: new WeaponClass[] { WeaponClass.Shield },
            maxChargeTime: 3
            );

        FixedModifiers fixedModifiers = new FixedModifiers(
            physicalArmour: 5
            );

        PercentageModifiers percentageModifiers = new PercentageModifiers(
            attackDamage: 1.2f,
            attackSpeed: 1.1f,
            attackSwingTime: 1.1f,
            attackPushForce: 1.1f
            );

        AddToList(new Item(commonStats: commonStats, visualStats: visualStats, weaponStats: weaponStats, fixedModifiers: fixedModifiers, percentageModifiers: percentageModifiers));
    }
}
