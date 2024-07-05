using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class List_Consumable : Manager_Item
{
    public static List<Item> AllConsumableData = new();

    public static void InitializeConsumableData()
    {
        Potions();
    }

    static void Potions()
    {
        CommonStats commonStats = new CommonStats(
            itemID: 202,
            itemType: ItemType.Consumable,
            itemName: "Small Health Potion",
            itemEquippable: true,
            maxStackSize: 99,
            itemValue: 1
            );

        FixedModifiers fixedModifiers = new FixedModifiers(
            healthRecovery: 5
            );

        AllConsumableData.Add(new Item(commonStats: commonStats, fixedModifiers: fixedModifiers));
    }
}
