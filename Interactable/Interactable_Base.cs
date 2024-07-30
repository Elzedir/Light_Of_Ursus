using System;
using System.Collections;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public abstract class Interactable_Base : MonoBehaviour
{
    [SerializeField] protected float _interactRange = 3;
    
    public virtual void Interact(GameObject interactor)
    {
        if (!WithinInteractRange(interactor)) { Debug.Log("Not within interact range"); return; }
    }

    public virtual IEnumerator Interact(Actor_Base actor)
    {
        if (!WithinInteractRange(actor.gameObject)) { Debug.Log("Not within interact range"); yield break; }
    }

    protected bool WithinInteractRange(GameObject interactor)
    {
        return Vector3.Distance(transform.position, interactor.transform.position) <= _interactRange;
    }
}
