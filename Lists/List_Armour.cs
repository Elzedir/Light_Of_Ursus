using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class List_Armour : Manager_Item
{
    public static void InitializeArmourData()
    {
        Heavy();
    }

    static void Heavy()
    {
        CommonStats_Item commonStats_Item = new CommonStats_Item(
            itemID: 100,
            itemType: ItemType.Armour,
            itemName: "Bronze ChestPlate",
            equipmentSlots: new List<EquipmentSlot>() { EquipmentSlot.Chest },
            itemEquippable: true,
            maxStackSize: 1,
            itemValue: 10
            );

        VisualStats_Item visualStats = new VisualStats_Item(
            itemIcon: null,
            itemPosition: new Vector3(-0.04f, -0.07f, 0f),
            itemRotation: Quaternion.Euler(180, 0, 0),
            itemScale: new Vector3(0.4f, 0.4f, 0.4f)
            );

        ArmourStats_Item armourStats = new ArmourStats_Item(
            armourType: EquipmentSlot.Chest,
            itemCoverage: 75
            );

        FixedModifiers_Item fixedModifiers = new FixedModifiers_Item(
            maxHealth: 5,
            maxMana: 5,
            maxStamina: 5,
            pushRecovery: 1,
            physicalArmour: 2,
            magicArmour: 2,
            dodgeCooldownReduction: -1
            );

        PercentageModifiers_Item percentageModifiers = new PercentageModifiers_Item(
            attackSpeed: 0.92f
            );

        AddToList(new Item(commonStats_Item: commonStats_Item, armourStats_Item: armourStats, fixedModifiers_Item: fixedModifiers, percentageModifiers_Item: percentageModifiers));
    }
}
