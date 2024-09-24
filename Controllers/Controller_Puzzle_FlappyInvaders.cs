using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Controller_Puzzle_FlappyInvaders : Controller
{
    Vector2 _move;
    Transform _bulletParent;

    //void Start()
    //{
    //    KeyBindings.ContinuousPressKeyActions.Remove(ActionKey.Space);
    //    KeyBindings.SinglePressKeyActions.Add(ActionKey.Space, HandleSpacePressed);
    //    _bulletParent = GameObject.Find("BulletParent").transform;
    //}

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        PlayerMove();
    }

    void PlayerMove()
    {
        transform.position += new Vector3(_move.x * 0.1f, _move.y * 0.1f, 0);
    }

    public void OnInput(InputAction.CallbackContext context)
    {
        _move = context.ReadValue<Vector2>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<Mine>() || collision.gameObject.GetComponent<Bullet>()) Hit(collision);
    }

    //public override void HandleWPressed()
    //{
    //    transform.position += (Vector3.up * 0.01f);
    //}

    //public override void HandleSPressed()
    //{
    //    transform.position += (Vector3.down * 0.01f);
    //}

    //public override void HandleAPressed()
    //{
    //    transform.position += (Vector3.left * 0.01f);
    //}

    //public override void HandleDPressed()
    //{
    //    transform.position += (Vector3.right * 0.01f);
    //}

    //public override void HandleSpacePressed()
    //{
    //    Shoot();
    //}

    public void Shoot(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            GameObject bulletGO = new GameObject("Bullet");
            Bullet bullet = bulletGO.AddComponent<Bullet>();
            bulletGO.transform.parent = _bulletParent;
            bullet.Initialise(
                Resources.GetBuiltinResource<Mesh>("Cube.fbx"),
                Resources.Load<Material>("Materials/Material_Green"),
                Vector3.right,
                transform.position
                );
        }
    }

    void Hit(Collision collision)
    {
        Debug.Log("Hit");
    }
}
