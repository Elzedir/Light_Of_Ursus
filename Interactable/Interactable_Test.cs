using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Interactable_Test : Interactable_Base
{
    public override void Interact(GameObject interactor)
    {
        if (!WithinInteractRange(interactor)) { Debug.Log("Not within interact range."); return; }

        if (transform.name == "Sword")
        {
            TestEquipSword();
        }
        else if (transform.name == "Shield")
        {
            TestEquipShield();
        }
    }

    bool _swordEquipped = false;
    bool _shieldEquipped = false;

    public void TestEquipSword()
    {
        if (!_swordEquipped)
        {
            if (Manager_Game.Instance.Player.transform.gameObject.GetComponent<Actor_Base>().Manager_Equipment.EquipItem(4, 1))
            {
                _swordEquipped = true;
            }
        }
        else
        {
            if (Manager_Game.Instance.Player.transform.gameObject.GetComponent<Actor_Base>().Manager_Equipment.UnequipItem(4))
            {
                _swordEquipped = false;
            }
        }

    }

    public void TestEquipShield()
    {
        if (!_shieldEquipped)
        {
            if (Manager_Game.Instance.Player.transform.gameObject.GetComponent<Actor_Base>().Manager_Equipment.EquipItem(3, 2))
            {
                _shieldEquipped = true;
            }
        }
        else
        {
            if (Manager_Game.Instance.Player.transform.gameObject.GetComponent<Actor_Base>().Manager_Equipment.UnequipItem(3))
            {
                _shieldEquipped = false;
            }
        }
    }
}
