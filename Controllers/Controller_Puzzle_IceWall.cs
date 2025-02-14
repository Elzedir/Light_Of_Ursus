using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;
using UnityEngine.InputSystem;
using z_Abandoned;

public enum IceWallDirection { None, Up, Down, Left, Right }


public class Controller_Puzzle_IceWall : Controller, PathfinderMover_3D_Deprecated
{
    (bool, IceWallDirection) Direction;
    Vector3 _centrePosition;

    Spawner_IceWall _spawner;
    int _playerStaminaCurrent;
    int _playerStaminaMax;
    float _playerExtraStamina;
    Cell_IceWall _lastCell;
    public Pathfinder_Base_3D_Deprecated Pathfinder { get; private set; }
    public Cell_IceWall CurrentCell { get; private set; }
    public bool CanGetNewPath { get; set; }
    public List<MoverType_Deprecated> MoverTypes { get; set; } = new();

    Vector2 _move;

    //void Start()
    //{
    //    //transform.position += new Vector3(0.5f, 0.5f, 0);

    //    KeyBindings.ContinuousPressKeyActions.Remove(ActionKey.Space);

    //    KeyBindings.SinglePressKeyActions.Add(ActionKey.Space, HandleSpacePressed);
    //}

    //public override void HandleWPressed()
    //{
    //    _lean(IceWallDirection.Up);
    //}

    //public override void HandleSPressed()
    //{
    //    _lean(IceWallDirection.Down);
    //}

    //public override void HandleAPressed()
    //{
    //    _lean(IceWallDirection.Left);
    //}

    //public override void HandleDPressed()
    //{
    //    _lean(IceWallDirection.Right);
    //}

    //public override void HandleUpPressed()
    //{
    //    _lean(IceWallDirection.Up);
    //}

    //public override void HandleDownPressed()
    //{
    //    _lean(IceWallDirection.Down);
    //}

    //public override void HandleLeftPressed()
    //{
    //    _lean(IceWallDirection.Left);
    //}

    //public override void HandleRightPressed()
    //{
    //    _lean(IceWallDirection.Right);
    //}
    //public override void HandleSpacePressed()
    //{
    //    if (!Direction.Item1) return;

    //    if (!_spawner.PlayerCanMove(Direction.Item2)) return;

    //    transform.position = _centrePosition + _leanDirection(Direction.Item2);
    //    Direction = (false, IceWallDirection.None);
    //}

    public void Initialise(Spawner_IceWall spawner, int playerExtraStamina)
    {
        _spawner = spawner;
        _playerExtraStamina = playerExtraStamina;
        Pathfinder = new Pathfinder_Base_3D_Deprecated();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        _playerMove();
    }

    void _playerMove()
    {
        if ((_move.x != 0 || _move.y != 0) && !Direction.Item1) _lean();

        else if ((_move.x == 0 && _move.y == 0) && Direction.Item1) _returnToCenter();
    }

    void _lean()
    {
        IceWallDirection direction = IceWallDirection.None;

        if (_move.x > 0) direction = IceWallDirection.Right;
        if (_move.x < 0) direction = IceWallDirection.Left;
        if (_move.y > 0) direction = IceWallDirection.Up;
        if (_move.y < 0) direction = IceWallDirection.Down;

        transform.position += _leanDirection(direction) * 0.3f;
        Direction = (true, direction);
    }

    public void OnInput(InputAction.CallbackContext context)
    {
        _move = context.ReadValue<Vector2>();
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (!Direction.Item1) return;

            if (!_spawner.PlayerCanMove(Direction.Item2)) return;

            transform.position = _centrePosition + _leanDirection(Direction.Item2);

            Direction = (false, IceWallDirection.None);
        }
    }

    void _returnToCenter()
    {
        Vector3 velocity = Vector3.one;
        transform.position = _centrePosition;
        Direction = (false, IceWallDirection.None);
    }

    Vector3 _leanDirection(IceWallDirection direction)
    {
        switch (direction)
        {
            case IceWallDirection.Up: return Vector3.forward;
            case IceWallDirection.Down: return Vector3.back;
            case IceWallDirection.Left: return Vector3.left;
            case IceWallDirection.Right: return Vector3.right;
            default: return Vector3.zero;
        }
    }

    public bool DecreaseStamina(Cell_IceWall cell)
    {
        if (_lastCell == cell) return true;

        _playerStaminaCurrent -= cell.CellHealth;
        Manager_Puzzle.Instance.UseStamina(_playerStaminaCurrent.ToString());
        _lastCell = cell;

        if (_playerStaminaCurrent <= 0) return false;
        else return true;
    }

    public void Fall()
    {
        _playerStaminaCurrent = _playerStaminaMax;
        Manager_Puzzle.Instance.UseStamina(_playerStaminaCurrent.ToString());
    }

    public void MoveTo(Voxel_Base_Deprecated target)
    {
        List<Vector3> path = Pathfinder.RetrievePath(VoxelGrid_Deprecated.GetVoxelAtPosition(CurrentCell.Position), target);

        foreach (Vector3 position in path)
        {
            _playerStaminaMax += (int)VoxelGrid_Deprecated.GetVoxelAtPosition(position).MovementCost;
        }

        _playerStaminaMax += (int)(_playerStaminaMax * (_playerExtraStamina / 100));
        _playerStaminaCurrent = _playerStaminaMax;
    }

    public List<Vector3> GetObstaclesInVision()
    {
        return new List<Vector3>();
    }

    public void SetCurrentCell(Cell_IceWall cell)
    {
        CurrentCell = cell;
        _centrePosition = new Vector3(CurrentCell.transform.position.x, CurrentCell.transform.position.y + 1, CurrentCell.transform.position.z);
        _returnToCenter();
    }

    public void StartPathfindingCoroutine(IEnumerator coroutine)
    {

    }

    public void StopPathfindingCoroutine()
    {

    }
}
