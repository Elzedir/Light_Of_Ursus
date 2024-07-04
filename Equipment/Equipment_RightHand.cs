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
}
