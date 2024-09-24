using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.GraphicsBuffer;

public class Controller_Puzzle_Directional : Controller
{
    BoxCollider _collider;
    PuzzleSet _puzzleSet;
    PuzzleType _puzzleType;
    Transform Target;

    Vector2 _move;
    bool _upHeld = false;
    bool _downHeld = false;
    bool _leftHeld = false;
    bool _rightHeld = false;

    public float shieldRadius = 1.5f;

    int _score = 0;

    void Start()
    {
        _collider = GetComponent<BoxCollider>();
        _puzzleSet = Manager_Puzzle.Instance.Puzzle.PuzzleSet;
        _puzzleType = Manager_Puzzle.Instance.Puzzle.PuzzleData.PuzzleState.PuzzleType;
        Target = GameObject.Find("Focus").transform;
    }

    protected override void FixedUpdate()
    {
        if (_puzzleSet == PuzzleSet.AntiDirectional)
        {
            MoveShield();
        }
    }

    void OnTriggerStay(Collider collision)
    {
        if (Manager_Puzzle.Instance.Puzzle.PuzzleSet == PuzzleSet.Directional)
        {
            int zAngle = Mathf.RoundToInt(collision.transform.localRotation.eulerAngles.z) % 360;
            if (zAngle < 0) zAngle += 360;

            if (zAngle == 0 && _rightHeld) { collision.gameObject.GetComponent<Arrow>().DestroyArrow(); _addToScore(); }
            else if (zAngle == 90 && _upHeld) { collision.gameObject.GetComponent<Arrow>().DestroyArrow(); _addToScore(); }
            else if (zAngle == 180 && _leftHeld) { collision.gameObject.GetComponent<Arrow>().DestroyArrow(); _addToScore(); }
            else if (zAngle == 270 && _downHeld) { collision.gameObject.GetComponent<Arrow>().DestroyArrow(); _addToScore(); }
        }

        if (Manager_Puzzle.Instance.Puzzle.PuzzleSet == PuzzleSet.AntiDirectional && collision.gameObject.name != "Focus") collision.gameObject.GetComponent<Arrow>().DestroyArrow();
    }

    void _addToScore()
    {
        _score++;
        Debug.Log(_score.ToString());
        Manager_Puzzle.Instance.AddScore(_score.ToString());
    }

    public void Move(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _move = context.ReadValue<Vector2>();

            if ((_upHeld && _move.y <= 0) || (_downHeld && _move.y >= 0) || (_leftHeld && _move.x <= 0) || (_rightHeld && _move.x >= 0)) { resetMovement();  return; }

            if (_move.y > 0) _upHeld = true;
            else if (_move.y < 0) _downHeld = true;
            else if (_move.x > 0) _rightHeld = true;
            else if (_move.x < 0) _leftHeld = true;
        }
        if (context.canceled)
        {
            resetMovement();
        }

        void resetMovement()
        {
            _upHeld = false;
            _downHeld = false;
            _leftHeld = false;
            _rightHeld = false;
            _move = Vector2.zero;
        }
    }

    void MoveShield()
    {
        //Vector3 direction = (Camera.main.ScreenToWorldPoint(Input.mousePosition) - Target.position).normalized;
        //transform.position = Target.position + direction * shieldRadius;

        //float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        //transform.rotation = Quaternion.Euler(0, 0, angle);

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        Vector3 hitPoint = ray.origin + ray.direction * 30f;

        Vector3 direction = (hitPoint - Target.position).normalized;
        transform.position = Target.position + direction * shieldRadius;

        transform.rotation = Quaternion.LookRotation(direction);
    }

    //void ToUseEventually()
    //{
    //    You can use this to control things that your mouse pointer is on.

    //    if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit))
    //    {
    //        Debug.Log("");

    //        Vector3 direction = (hit.point - Target.position).normalized;
    //        transform.position = Target.position + direction * shieldRadius;
    //        transform.rotation = Quaternion.LookRotation(direction);
    //    }
    //}
}
