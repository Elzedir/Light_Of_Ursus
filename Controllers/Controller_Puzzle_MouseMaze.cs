using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum MouseMazeColour { None, Blue, Green, Red }

public class Controller_Puzzle_MouseMaze : Controller
{
    public event Action OnBreakWall;
    public MouseMazeColour PlayerColour { get; private set; }
    float _playerSpeed = 3f;
    Vector2 _move;

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        if (!Spawner_Maze.Initialised) return;

        _playerMove();
    }

    public void OnInput(InputAction.CallbackContext context)
    {
        _move = context.ReadValue<Vector2>();
    }

    void _playerMove()
    {
        _rigidBody.MovePosition(_rigidBody.position + new Vector3(_move.x * _playerSpeed, 0, _move.y * _playerSpeed) * UnityEngine.Time.fixedDeltaTime);

        //transform.position += new Vector3(_move.x * _playerSpeed, 0 ,_move.y * _playerSpeed);
    }

    public void BreakWall(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            OnBreakWall?.Invoke();
        }
    }

    //void Start()
    //{
    //    KeyBindings.ContinuousPressKeyActions.Remove(ActionKey.Space);
    //    KeyBindings.SinglePressKeyActions.Add(ActionKey.Space, HandleSpacePressed);
    //}
    //public override void HandleWPressed()
    //{
    //    transform.position += (Vector3.up * 0.01f * _playerSpeed);
    //}

    //public override void HandleSPressed()
    //{
    //    transform.position += (Vector3.down * 0.01f * _playerSpeed);
    //}

    //public override void HandleAPressed()
    //{
    //    transform.position += (Vector3.left * 0.01f * _playerSpeed);
    //}

    //public override void HandleDPressed()
    //{
    //    transform.position += (Vector3.right * 0.01f * _playerSpeed);
    //}

    //public override void HandleSpacePressed()
    //{
    //    OnBreakWall?.Invoke();
    //}

    public void SetPlayerColour(MouseMazeColour playerColour, Color color)
    {
        PlayerColour = playerColour;
        _spriteRenderer.color = color;
    }
}
