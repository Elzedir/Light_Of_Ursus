using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable_Dialogue : MonoBehaviour, IInteractable
{
    public float InteractRange { get; private set; }

    public IEnumerator Interact(Actor_Base actor)
    {
        Manager_Dialogue.Instance.OpenDialogue(actor.gameObject, Manager_Dialogue.Instance.GetConversation(name));

        yield break;
    }

    public bool WithinInteractRange(Actor_Base interactor)
    {
        return Vector3.Distance(interactor.transform.position, transform.position) < InteractRange;
    }
}
