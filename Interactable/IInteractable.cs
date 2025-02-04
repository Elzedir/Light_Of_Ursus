using System.Collections;
using Actor;
using Actors;
using UnityEngine;

public interface IInteractable
{
    float InteractRange { get; }
    void SetInteractRange(float interactRange);
    IEnumerator Interact(Actor_Component actor);
    bool WithinInteractRange(Actor_Component actor);
}
