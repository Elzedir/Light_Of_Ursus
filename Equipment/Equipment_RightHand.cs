using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Equipment_RightHand : Equipment_Base
{
    public override void Initialise()
    {
        base.Initialise();

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
            if (UnequipItem())
            {
                Item = item;
                MeshFilter.mesh = item.VisualStats.ItemMesh;
                MeshRenderer.material = item.VisualStats.ItemMaterial;
                transform.localPosition = item.VisualStats.ItemPosition;
                transform.localRotation = item.VisualStats.ItemRotation;
                transform.localScale = item.VisualStats.ItemScale;
                return true;
            }
        }

        return false;
    }

    public override bool UnequipItem()
    {
        // if (Return item to inventory.)
        Item = null;
        return true;
    }
}
