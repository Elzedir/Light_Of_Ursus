using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Equipment_LeftHand : Equipment_Base
{
    protected override void Awake()
    {
        base.Awake();

        EquipmentSlot = EquipmentSlot.LeftHand;
    }
}
