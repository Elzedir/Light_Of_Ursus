using System.Collections.Generic;
using System.Linq;
using Managers;
using UnityEngine;

namespace Lists
{
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

    public abstract class List_Weapon
    {
        public static Dictionary<uint, Item_Master> GetAllDefaultWeapons()
        {
            var allWeapons = new Dictionary<uint, Item_Master>();
            
            foreach (var weapon in _defaultShortBows)
            {
                allWeapons.Add(weapon.Key, weapon.Value);
            }

            foreach (var weapon in _defaultShortSwords)
            {
                allWeapons.Add(weapon.Key, weapon.Value);
            }

            foreach (var weapon in _defaultShields)
            {
                allWeapons.Add(weapon.Key, weapon.Value);
            }
            
            return allWeapons;
        }
        
        static readonly Dictionary<uint, Item_Master> _defaultShortBows = new()
        {
            {
                3,
                new Item_Master(
                    new CommonStats_Item(
                        itemID: 3,
                        itemType: ItemType.Weapon,
                        itemName: "Wooden ShortBow",
                        equipmentSlots: new List<EquipmentSlot>() { EquipmentSlot.RightHand },
                        itemEquippable: true,
                        maxStackSize: 1,
                        itemValue: 15
                    ),

                    new VisualStats_Item(
                        itemIcon: null,
                        itemPosition: Vector3.zero,
                        itemRotation: Quaternion.identity,
                        itemScale: Vector3.one
                    ),

                    new WeaponStats_Item(
                        weaponType: new[] { WeaponType.TwoHandedRanged },
                        weaponClass: new[] { WeaponClass.ShortBow },
                        maxChargeTime: 2
                    ),

                    null,

                    new FixedModifiers_Item(
                        attackRange: 1
                    ),

                    new PercentageModifiers_Item(
                        attackDamage: 1.1f,
                        attackSpeed: 1.5f,
                        attackSwingTime: 3f,
                        attackRange: 3f,
                        attackPushForce: 1.1f
                    ),

                    null
                )
            },
        };

        static readonly Dictionary<uint, Item_Master> _defaultShortSwords = new()
        {
            {
                1,
                new Item_Master(
                    new CommonStats_Item(
                        itemID: 1,
                        itemType: ItemType.Weapon,
                        itemName: "Wooden ShortSword",
                        equipmentSlots: new List<EquipmentSlot>()
                            { EquipmentSlot.RightHand, EquipmentSlot.LeftHand },
                        itemEquippable: true,
                        maxStackSize: 1,
                        itemValue: 15
                    ),

                    new VisualStats_Item(
                        itemIcon: null,
                        itemMesh: GameObject.Find("TestSword").GetComponent<MeshFilter>().mesh, //Other thing for now
                        itemMaterial: Resources.Load<Material>("Materials/Material_Red"),
                        itemAnimatorController: Resources.Load<RuntimeAnimatorController>("Animators/Test_Weapon"),
                        itemCollider: new CapsuleCollider(),
                        itemPosition: new Vector3(0.5f, -0.2f, -0.2f),
                        itemRotation: Quaternion.Euler(215, 0, 0),
                        itemScale: new Vector3(0.1f, 0.6f, 0.1f)
                    ),

                    new WeaponStats_Item(
                        weaponType: new[] { WeaponType.OneHandedMelee },
                        weaponClass: new[] { WeaponClass.ShortSword },
                        maxChargeTime: 3
                    ),

                    null,
                    null,

                    new PercentageModifiers_Item(
                        attackDamage: 1.2f,
                        attackSpeed: 1.1f,
                        attackSwingTime: 1.1f,
                        attackPushForce: 1.1f
                    ),

                    null
                )
            }
        };


        static readonly Dictionary<uint, Item_Master> _defaultShields = new()
        {
            {
                2,
                new Item_Master(
                    new CommonStats_Item(
                        itemID: 2,
                        itemType: ItemType.Weapon,
                        itemName: "Test Shield",
                        equipmentSlots: new List<EquipmentSlot>() { EquipmentSlot.LeftHand },
                        itemEquippable: true,
                        maxStackSize: 1,
                        itemValue: 15
                    ),

                    new VisualStats_Item(
                        itemIcon: null,
                        itemMesh: Resources.GetBuiltinResource<Mesh>("Cube.fbx"),
                        itemMaterial: Resources.Load<Material>("Materials/Material_Red"),
                        itemAnimatorController: Resources.Load<RuntimeAnimatorController>("Animators/Test_Shield"),
                        itemPosition: new Vector3(-0.6f, 0f, 0f),
                        itemRotation: Quaternion.Euler(0, -90, 0),
                        itemScale: new Vector3(1f, 1f, 0.1f)
                    ),

                    new WeaponStats_Item(
                        weaponType: new[] { WeaponType.OneHandedShield },
                        weaponClass: new[] { WeaponClass.Shield },
                        maxChargeTime: 3
                    ),

                    null,

                    new FixedModifiers_Item(
                        physicalArmour: 5
                    ),

                    new PercentageModifiers_Item(
                        attackDamage: 1.2f,
                        attackSpeed: 1.1f,
                        attackSwingTime: 1.1f,
                        attackPushForce: 1.1f
                    ),
                    
                    null
                )

            }
        };
    }
}