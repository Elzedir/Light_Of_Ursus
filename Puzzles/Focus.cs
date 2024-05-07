using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Focus : MonoBehaviour
{
    BoxCollider _collider;

    void Awake()
    {
        _collider = GetComponent<BoxCollider>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent<Arrow>(out Arrow arrow))
        {
            arrow.DestroyArrow();

            if (!Manager_Puzzle.Instance.Invulnerable) Manager_Puzzle.Instance.TakeDamage();
        }
    }
}
