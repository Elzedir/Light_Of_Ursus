using System.Collections.Generic;
using Equipment;
using UnityEngine;

namespace Items
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
        static Dictionary<ulong, Item_Data> _defaultWeapons;
        public static Dictionary<ulong, Item_Data> DefaultWeapons => _defaultWeapons ??= _initialiseDefaultWeapons();

        static Dictionary<ulong, Item_Data> _initialiseDefaultWeapons()
        {
            var allWeapons = new Dictionary<ulong, Item_Data>();

            foreach (var weapon in _defaultShortBows())
            {
                allWeapons.Add(weapon.Key, weapon.Value);
            }

            foreach (var weapon in _defaultShortSwords())
            {
                allWeapons.Add(weapon.Key, weapon.Value);
            }

            foreach (var weapon in _defaultShields())
            {
                allWeapons.Add(weapon.Key, weapon.Value);
            }

            return allWeapons;
        }

        static Dictionary<ulong, Item_Data> _defaultShortBows()
        {
            return new Dictionary<ulong, Item_Data>
            {
                {
                    3,
                    new Item_Data(
                        new Item_CommonStats(
                            itemID: 3,
                            itemType: ItemType.Weapon,
                            itemName: "Wooden ShortBow",
                            equipmentSlots: new List<EquipmentSlot>() { EquipmentSlot.RightHand },
                            itemEquippable: true,
                            maxStackSize: 1,
                            itemValue: 15
                        ),

                        new Item_VisualStats(
                            itemIcon: null,
                            itemPosition: Vector3.zero,
                            itemRotation: Quaternion.identity,
                            itemScale: Vector3.one
                        ),

                        new Item_WeaponStats(
                            weaponType: new[] { WeaponType.TwoHandedRanged },
                            weaponClass: new[] { WeaponClass.ShortBow },
                            maxChargeTime: 2
                        ),

                        null,

                        new Item_FixedModifiers(
                            attackRange: 1
                        ),

                        new Item_PercentageModifiers(
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
        }

        static Dictionary<ulong, Item_Data> _defaultShortSwords()
        {
            return new Dictionary<ulong, Item_Data>
            {
                {
                    1,
                    new Item_Data(
                        new Item_CommonStats(
                            itemID: 1,
                            itemType: ItemType.Weapon,
                            itemName: "Wooden ShortSword",
                            equipmentSlots: new List<EquipmentSlot>()
                                { EquipmentSlot.RightHand, EquipmentSlot.LeftHand },
                            itemEquippable: true,
                            maxStackSize: 1,
                            itemValue: 15
                        ),

                        new Item_VisualStats(
                            itemIcon: null,
                            //itemMesh: GameObject.Find("TestSword").GetComponent<MeshFilter>().mesh, //Other thing for now
                            itemMaterial: Resources.Load<Material>("Materials/Material_Red"),
                            itemAnimatorController: Resources.Load<RuntimeAnimatorController>("Animators/Test_Weapon"),
                            itemCollider: new CapsuleCollider(),
                            itemPosition: new Vector3(0.5f, -0.2f, -0.2f),
                            itemRotation: Quaternion.Euler(215, 0, 0),
                            itemScale: new Vector3(0.1f, 0.6f, 0.1f)
                        ),

                        new Item_WeaponStats(
                            weaponType: new[] { WeaponType.OneHandedMelee },
                            weaponClass: new[] { WeaponClass.ShortSword },
                            maxChargeTime: 3
                        ),

                        null,
                        null,

                        new Item_PercentageModifiers(
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

        static Dictionary<ulong, Item_Data> _defaultShields()
        {
            return new Dictionary<ulong, Item_Data>
            {
                {
                    2,
                    new Item_Data(
                        new Item_CommonStats(
                            itemID: 2,
                            itemType: ItemType.Weapon,
                            itemName: "Test Shield",
                            equipmentSlots: new List<EquipmentSlot>() { EquipmentSlot.LeftHand },
                            itemEquippable: true,
                            maxStackSize: 1,
                            itemValue: 15
                        ),

                        new Item_VisualStats(
                            itemIcon: null,
                            itemMesh: Resources.GetBuiltinResource<Mesh>("Cube.fbx"),
                            itemMaterial: Resources.Load<Material>("Materials/Material_Red"),
                            itemAnimatorController: Resources.Load<RuntimeAnimatorController>("Animators/Test_Shield"),
                            itemPosition: new Vector3(-0.6f, 0f, 0f),
                            itemRotation: Quaternion.Euler(0, -90, 0),
                            itemScale: new Vector3(1f, 1f, 0.1f)
                        ),

                        new Item_WeaponStats(
                            weaponType: new[] { WeaponType.OneHandedShield },
                            weaponClass: new[] { WeaponClass.Shield },
                            maxChargeTime: 3
                        ),

                        null,

                        new Item_FixedModifiers(
                            physicalArmour: 5
                        ),

                        new Item_PercentageModifiers(
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
}