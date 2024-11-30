using System;
using System.Collections;
using System.Collections.Generic;
using Actor;
using UnityEngine;

public class Interactable_Dialogue : MonoBehaviour, IInteractable
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
        Manager_Dialogue.Instance.OpenDialogue(actor.gameObject, Manager_Dialogue.Instance.GetConversation(name));

        yield break;
    }
}
