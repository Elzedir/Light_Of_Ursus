using System.Collections;
using Actors;
using UnityEngine;

public interface IInteractable
{
    float InteractRange { get; }
    void SetInteractRange(float interactRange);
    IEnumerator Interact(ActorComponent actor);
    bool WithinInteractRange(ActorComponent actor);
}
