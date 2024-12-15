using System.Collections;
using System.Collections.Generic;
using Equipment;
using UnityEngine;

public class Equipment_RightHand : Equipment_Base
{
    public override void Initialise()
    {
        base.Initialise();

        EquipmentSlot = EquipmentSlot.RightHand;
    }
}
