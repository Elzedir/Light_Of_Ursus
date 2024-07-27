using System;
using System.Collections;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public abstract class Interactable_Base : MonoBehaviour
{
    [SerializeField] protected float _interactRange = 3;
    
    public virtual void Interact(GameObject interactor)
    {
        throw new ArgumentException("Can't use base class.");
    }

    public virtual IEnumerator Interact(Actor_Base actor)
    {
        throw new ArgumentException("Can't use base class.");
    }

    protected bool WithinInteractRange(GameObject interactor)
    {
        return Vector3.Distance(transform.position, interactor.transform.position) <= _interactRange;
    }
}
