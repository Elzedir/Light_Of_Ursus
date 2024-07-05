using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Equipment_RightHand : Equipment_Base
{
    protected override void Awake()
    {
        base.Awake();

        EquipmentSlot = EquipmentSlot.RightHand;
    }

    public override bool EquipItem(Item item)
    {
        if (item == null)
        {
            Debug.Log("Item is null");
            return false;
        }

        if (item.CommonStats.EquipmentSlots.Contains(EquipmentSlot))
        {
            Item = item;
            return true;
        }

        return false;
    }
}
