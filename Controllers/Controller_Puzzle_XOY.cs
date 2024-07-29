using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Controller_Puzzle_XOY : Controller
{
    [SerializeField] [Range(0, 1)] float _cooldownTimer;
    float _cooldown = 0.25f;
    Vector2 _move;

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        _cooldownTimer += UnityEngine.Time.deltaTime;

        if (_cooldownTimer >= _cooldown)
        {
            PlayerMove();
            _cooldownTimer = 0;
        }
    }

    public void OnInput(InputAction.CallbackContext context)
    {
        _move = context.ReadValue<Vector2>();
    }

    void PlayerMove()
    {
        transform.position += new Vector3((int)_move.x, (int)_move.y, 0);
    }

    //void Start()
    //{
    //    transform.position += new Vector3(0.5f, 0.5f, 0);

    //    KeyBindings.ContinuousPressKeyActions.Remove(ActionKey.Move_Up);
    //    KeyBindings.ContinuousPressKeyActions.Remove(ActionKey.Move_Down);
    //    KeyBindings.ContinuousPressKeyActions.Remove(ActionKey.Move_Left);
    //    KeyBindings.ContinuousPressKeyActions.Remove(ActionKey.Move_Right);

    //    KeyBindings.SinglePressKeyActions.Add(ActionKey.Move_Up, HandleWPressed);
    //    KeyBindings.SinglePressKeyActions.Add(ActionKey.Move_Down, HandleSPressed);
    //    KeyBindings.SinglePressKeyActions.Add(ActionKey.Move_Left, HandleAPressed);
    //    KeyBindings.SinglePressKeyActions.Add(ActionKey.Move_Right, HandleDPressed);
    //}

    //public override void HandleWPressed()
    //{
    //    transform.position += Vector3.up;
    //}

    //public override void HandleSPressed()
    //{
    //    transform.position += Vector3.down;
    //}

    //public override void HandleAPressed()
    //{
    //    transform.position += Vector3.left;
    //}

    //public override void HandleDPressed()
    //{
    //    transform.position += Vector3.right;
    //}
}
