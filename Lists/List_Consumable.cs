using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class List_Consumable : Manager_Item
{
    public static void InitializeConsumableData()
    {
        Potions();
    }

    static void Potions()
    {
        CommonStats_Item commonStats_Item = new CommonStats_Item(
            itemID: 202,
            itemType: ItemType.Consumable,
            itemName: "Small Health Potion",
            itemEquippable: true,
            maxStackSize: 99,
            itemValue: 1
            );

        FixedModifiers_Item fixedModifiers = new FixedModifiers_Item(
            healthRecovery: 5
            );

        AddToList(new Item(commonStats_Item: commonStats_Item, null, null, null, fixedModifiers_Item: fixedModifiers, null));
    }
}
