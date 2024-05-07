using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Controller_Puzzle_Directional : Controller
{
    BoxCollider _collider;
    PuzzleSet _puzzleSet;
    PuzzleType _puzzleType;
    Transform Target;

    public float shieldRadius = 1.5f;

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

    void OnCollisionStay(Collision collision)
    {
        //if (Manager_Puzzle.Instance.Puzzle.PuzzleSet == PuzzleSet.Directional)
        //{
        //    int zAngle = Mathf.RoundToInt(collision.transform.localRotation.eulerAngles.z) % 360;
        //    if (zAngle < 0) zAngle += 360;

        //    if (zAngle == 0 && (Input.GetKey(KeyCode.UpArrow) || Input.GetKeyDown(KeyBindings.Keys[ActionKey.Move_Up])) &&
        //        !(Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.RightArrow)))
        //        collision.gameObject.GetComponent<Arrow>().DestroyArrow();
        //    else if (zAngle == 90 && (Input.GetKey(KeyCode.LeftArrow) || Input.GetKeyDown(KeyBindings.Keys[ActionKey.Move_Left])) &&
        //        !(Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.RightArrow)))
        //        collision.gameObject.GetComponent<Arrow>().DestroyArrow();
        //    else if (zAngle == 180 && (Input.GetKey(KeyCode.DownArrow) || Input.GetKeyDown(KeyBindings.Keys[ActionKey.Move_Down])) &&
        //        !(Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.RightArrow)))
        //        collision.gameObject.GetComponent<Arrow>().DestroyArrow();
        //    else if (zAngle == 270 && (Input.GetKey(KeyCode.RightArrow) || Input.GetKeyDown(KeyBindings.Keys[ActionKey.Move_Right])) &&
        //        !(Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.UpArrow)))
        //        collision.gameObject.GetComponent<Arrow>().DestroyArrow();
        //}

        if (Manager_Puzzle.Instance.Puzzle.PuzzleSet == PuzzleSet.AntiDirectional && collision.gameObject.name != "Focus") collision.gameObject.GetComponent<Arrow>().DestroyArrow();
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
