using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.Experimental.GraphView.GraphView;

public abstract class Controller : MonoBehaviour
{
    protected SpriteRenderer _spriteRenderer;
    protected Rigidbody _rigidBody;

    void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _rigidBody = GetComponentInParent<Rigidbody>();
    }

    #region Old Movement
    //public KeyBindings KeyBindings;

    //void Awake()
    //{
    //    KeyBindings = new KeyBindings();
    //    KeyBindings.LoadBindings();
    //    KeyBindings.InitialiseKeyActions(this);
    //}

    //protected virtual void Update()
    //{
    //    if (Manager_Game.Instance.CurrentState == GameState.Cinematic) return;

    //    foreach (var actionKey in KeyBindings.SinglePressKeyActions.Keys)
    //    {
    //        if (Input.GetKeyDown(KeyBindings.Keys[actionKey]))
    //        {
    //            KeyBindings.SinglePressKeyActions[actionKey]?.Invoke();
    //        }
    //    }

    //    foreach (var actionKey in KeyBindings.ContinuousPressKeyActions.Keys)
    //    {
    //        if (Input.GetKey(KeyBindings.Keys[actionKey]))
    //        {
    //            KeyBindings.ContinuousPressKeyActions[actionKey]?.Invoke();
    //        }
    //    }
    //}

    //public virtual void HandleWPressed()
    //{

    //}
    //public virtual void HandleSPressed()
    //{

    //}
    //public virtual void HandleAPressed()
    //{

    //}
    //public virtual void HandleDPressed()
    //{

    //}

    //public virtual void HandleSpacePressed()
    //{

    //}
    //public virtual void HandleUpPressed()
    //{

    //}
    //public virtual void HandleDownPressed()
    //{

    //}
    //public virtual void HandleLeftPressed()
    //{

    //}
    //public virtual void HandleRightPressed()
    //{

    //}

    //public void HandleEPressed()
    //{
    //    if (Manager_Game.Instance.Player.ClosestInteractableObject != null)
    //    {
    //        Manager_Game.Instance.Player.ClosestInteractableObject.Interact(Manager_Game.Instance.Player.gameObject);
    //    }
    //}

    //public virtual void HandleEscapePressed()
    //{

    //}
    #endregion

    [SerializeField] protected float _speed;

    protected virtual void FixedUpdate()
    {

    }
}
