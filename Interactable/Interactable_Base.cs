using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public abstract class Interactable_Base : MonoBehaviour
{
    [SerializeField] protected float _interactRange = 3;
    
    public virtual void Interact(GameObject interactor)
    {
        
    }

    protected bool WithinInteractRange(GameObject interactor)
    {
        return Vector3.Distance(transform.position, interactor.transform.position) <= _interactRange;
    }
}
