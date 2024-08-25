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
        CommonStats_Item commonStats_Item = new CommonStats_Item(
            itemID: 3,
            itemType: ItemType.Weapon,
            itemName: "Wooden ShortBow",
            equipmentSlots: new List<EquipmentSlot>() { EquipmentSlot.RightHand },
            itemEquippable: true,
            maxStackSize: 1,
            itemValue: 15
            );

        VisualStats_Item visualStats = new VisualStats_Item(
            itemIcon: null,
            itemPosition: Vector3.zero,
            itemRotation: Quaternion.identity,
            itemScale: Vector3.one
            );

        WeaponStats_Item weaponStats = new WeaponStats_Item(
            weaponType: new WeaponType[] { WeaponType.TwoHandedRanged },
            weaponClass: new WeaponClass[] { WeaponClass.ShortBow },
            maxChargeTime: 2
            );

        FixedModifiers_Item fixedModifiers = new FixedModifiers_Item(
            attackRange: 1
            );

        PercentageModifiers_Item percentageModifiers = new PercentageModifiers_Item(
            attackDamage: 1.1f,
            attackSpeed: 1.5f,
            attackSwingTime: 3f,
            attackRange: 3f,
            attackPushForce: 1.1f
            );

        AddToList(new Item(commonStats_Item: commonStats_Item, visualStats_Item: visualStats, weaponStats_Item: weaponStats, null, fixedModifiers_Item: fixedModifiers, percentageModifiers_Item: percentageModifiers));
    }
    static void _shortSwords()
    {
        CommonStats_Item commonStats_Item = new CommonStats_Item(
            itemID: 1,
            itemType: ItemType.Weapon,
            itemName: "Wooden ShortSword",
            equipmentSlots: new List<EquipmentSlot>() { EquipmentSlot.RightHand, EquipmentSlot.LeftHand },
            itemEquippable: true,
            maxStackSize: 1,
            itemValue: 15
            );

        VisualStats_Item visualStats = new VisualStats_Item(
            itemIcon: null,
            itemMesh: GameObject.Find("TestSword").GetComponent<MeshFilter>().mesh, //Other thing for now
            itemMaterial: Resources.Load<Material>("Materials/Material_Red"),
            itemAnimatorController: Resources.Load<RuntimeAnimatorController>("Animators/Test_Weapon"),
            itemCollider: new CapsuleCollider(),
            itemPosition: new Vector3(0.5f, -0.2f, -0.2f),
            itemRotation: Quaternion.Euler(215, 0, 0),
            itemScale: new Vector3(0.1f, 0.6f, 0.1f)
            );

        WeaponStats_Item weaponStats = new WeaponStats_Item(
            weaponType: new WeaponType[] { WeaponType.OneHandedMelee },
            weaponClass: new WeaponClass[] { WeaponClass.ShortSword },
            maxChargeTime: 3
            );

        FixedModifiers_Item fixedModifiers = new FixedModifiers_Item(
            );

        PercentageModifiers_Item percentageModifiers = new PercentageModifiers_Item(
            attackDamage: 1.2f,
            attackSpeed: 1.1f,
            attackSwingTime: 1.1f,
            attackPushForce: 1.1f
            );

        AddToList(new Item(commonStats_Item: commonStats_Item, visualStats_Item: visualStats, weaponStats_Item: weaponStats, null, fixedModifiers_Item: fixedModifiers, percentageModifiers_Item: percentageModifiers));
    }

    static void _shields()
    {
        CommonStats_Item commonStats_Item = new CommonStats_Item(
            itemID: 2,
            itemType: ItemType.Weapon,
            itemName: "Test Shield",
            equipmentSlots: new List<EquipmentSlot>() { EquipmentSlot.LeftHand },
            itemEquippable: true,
            maxStackSize: 1,
            itemValue: 15
            );

        VisualStats_Item visualStats = new VisualStats_Item(
            itemIcon: null,
            itemMesh: Resources.GetBuiltinResource<Mesh>("Cube.fbx"),
            itemMaterial: Resources.Load<Material>("Materials/Material_Red"),
            itemAnimatorController: Resources.Load<RuntimeAnimatorController>("Animators/Test_Shield"),
            itemPosition: new Vector3(-0.6f, 0f, 0f),
            itemRotation: Quaternion.Euler(0, -90, 0),
            itemScale: new Vector3(1f, 1f, 0.1f)
            );

        WeaponStats_Item weaponStats = new WeaponStats_Item(
            weaponType: new WeaponType[] { WeaponType.OneHandedShield },
            weaponClass: new WeaponClass[] { WeaponClass.Shield },
            maxChargeTime: 3
            );

        FixedModifiers_Item fixedModifiers = new FixedModifiers_Item(
            physicalArmour: 5
            );

        PercentageModifiers_Item percentageModifiers = new PercentageModifiers_Item(
            attackDamage: 1.2f,
            attackSpeed: 1.1f,
            attackSwingTime: 1.1f,
            attackPushForce: 1.1f
            );

        AddToList(new Item(commonStats_Item: commonStats_Item, visualStats_Item: visualStats, weaponStats_Item: weaponStats, null, fixedModifiers_Item: fixedModifiers, percentageModifiers_Item: percentageModifiers));
    }
}
