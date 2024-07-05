using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class List_Armour : Manager_Item
{
    public static List<Item> AllArmourData = new();

    public static void InitializeArmourData()
    {
        Heavy();
    }

    static void Heavy()
    {
        CommonStats commonStats = new CommonStats(
            itemID: 100,
            itemType: ItemType.Armour,
            itemName: "Bronze ChestPlate",
            itemEquippable: true,
            maxStackSize: 1,
            itemValue: 10
            );

        VisualStats visualStats = new VisualStats(
            itemIcon: null,
            itemPosition: new Vector3(-0.04f, -0.07f, 0f),
            itemRotation: new Vector3(180, 0, 0),
            itemScale: new Vector3(0.4f, 0.4f, 0.4f)
            );

        ArmourStats armourStats = new ArmourStats(
            armourType: EquipmentSlot.Chest,
            itemCoverage: 75
            );

        FixedModifiers fixedModifiers = new FixedModifiers(
            maxHealth: 5,
            maxMana: 5,
            maxStamina: 5,
            pushRecovery: 1,
            physicalDefence: 2,
            magicalDefence: 2,
            dodgeCooldownReduction: -1
            );

        PercentageModifiers percentageModifiers = new PercentageModifiers(
            attackSpeed: 0.92f
            );

        AllArmourData.Add(new Item(commonStats: commonStats, armourStats: armourStats, fixedModifiers: fixedModifiers, percentageModifiers: percentageModifiers));
    }
}
