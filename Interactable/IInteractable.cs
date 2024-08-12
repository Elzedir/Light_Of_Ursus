using System.Collections;
using UnityEngine;

public interface IInteractable
{
    float InteractRange { get; }
    void SetInteractRange(float interactRange);
    IEnumerator Interact(Actor_Base actor);
    bool WithinInteractRange(Actor_Base actor);
}
