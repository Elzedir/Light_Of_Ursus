using System.Collections;
using Actor;
using Items;
using Managers;
using UnityEngine;

namespace Interactable
{
    public class Interactable_Test : MonoBehaviour, IInteractable
    {
        public float InteractRange { get; private set; }

        public void SetInteractRange(float interactRange)
        {
            InteractRange = interactRange;
        }

        public bool WithinInteractRange(Actor_Component interactor)
        {
            return Vector3.Distance(interactor.transform.position, transform.position) < InteractRange;
        }

        public IEnumerator Interact(Actor_Component actor)
        {
            if (transform.name == "Sword")
            {
                TestEquipSword();
            }
            else if (transform.name == "Shield")
            {
                TestEquipShield();
            }

            yield break;
        }

        bool _swordEquipped  = false;
        bool _shieldEquipped = false;

        public void TestEquipSword()
        {
            if (!_swordEquipped)
            {
                if (Manager_Game.Instance.Player.transform.gameObject.GetComponent<Actor_Component>().EquipmentComponent.EquipItem(4, new Item(1, 1)))
                {
                    _swordEquipped = true;
                }
            }
            else
            {
                if (Manager_Game.Instance.Player.transform.gameObject.GetComponent<Actor_Component>().EquipmentComponent.UnequipItem(4))
                {
                    _swordEquipped = false;
                }
            }

        }

        public void TestEquipShield()
        {
            if (!_shieldEquipped)
            {
                if (Manager_Game.Instance.Player.transform.gameObject.GetComponent<Actor_Component>().EquipmentComponent.EquipItem(3, new Item(2, 1)))
                {
                    _shieldEquipped = true;
                }
            }
            else
            {
                if (Manager_Game.Instance.Player.transform.gameObject.GetComponent<Actor_Component>().EquipmentComponent.UnequipItem(3))
                {
                    _shieldEquipped = false;
                }
            }
        }
    }
}
