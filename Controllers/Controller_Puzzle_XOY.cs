using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller_Puzzle_XOY : Controller
{
    float _cooldownTimer;
    float _cooldown = 0.5f;

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        _cooldownTimer += Time.deltaTime;

        if (_cooldownTimer >= _cooldown)
        {
            PlayerMove();
            _cooldown = 0;
        }
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
