using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

public class List_Weapon : Manager_Item
{
    public static List<Item> AllWeaponData = new();

    public static void InitializeWeaponData()
    {
        Melee();
        Ranged();
    }
    static void Melee()
    {
        ShortSwords();
    }
    static void Ranged()
    {
        ShortBows();
    }
    static void ShortBows()
    {
        CommonStats commonStats = new CommonStats(
            itemID: 2,
            itemType: ItemType.Weapon,
            itemName: "Wooden ShortBow",
            itemEquippable: true,
            maxStackSize: 1,
            itemValue: 15
            );

        VisualStats visualStats = new VisualStats(
            itemIcon: null,
            itemPosition: Vector3.zero,
            itemRotation: Vector3.zero,
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

        AllWeaponData.Add(new Item(commonStats: commonStats, visualStats: visualStats, weaponStats: weaponStats, fixedModifiers: fixedModifiers, percentageModifiers: percentageModifiers));
    }
    static void ShortSwords()
    {
        CommonStats commonStats = new CommonStats(
            itemID: 1,
            itemType: ItemType.Weapon,
            itemName: "Wooden ShortSword",
            itemEquippable: true,
            maxStackSize: 1,
            itemValue: 15
            );

        VisualStats visualStats = new VisualStats(
            itemIcon: null,
            itemMesh: Resources.GetBuiltinResource<Mesh>("Cylinder.fbx"),
            itemMaterial: Resources.Load<Material>("Meshes/Material_Red"),
            itemPosition: new Vector3(0.5f, -0.2f, -0.2f),
            itemRotation: new Vector3(215, 0, 0),
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

        AllWeaponData.Add(new Item(commonStats: commonStats, visualStats: visualStats, weaponStats: weaponStats, fixedModifiers: fixedModifiers, percentageModifiers: percentageModifiers));
    }
}
