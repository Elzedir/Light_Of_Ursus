using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager_GroundCheck
{
    public static List<GroundedCheckComponent> AllGroundedObjects = new();
    public static LayerMask GroundMask;

    public static GroundedCheckComponent AddGroundedObject(GameObject groundedObjectGO)
    {
        if (GroundMask.value == 0) GroundMask = LayerMask.GetMask("Ground");

        var groundedObject = new GroundedCheckComponent(groundedObjectGO, GroundMask);

        AllGroundedObjects.Add(groundedObject);

        return groundedObject;
    }

    public static bool IsGrounded(GameObject GO = null)
    {
        if (GO is null)
        {
            foreach (var allGroundedObject in AllGroundedObjects)
            {
                allGroundedObject.IsGrounded();
            }

            return false;
        }

        var groundedObject = AllGroundedObjects.Find(gc => gc.GroundedGO == GO);

        if (groundedObject == null) AddGroundedObject(GO);

        return groundedObject.IsGrounded();
    }
}

public class GroundedCheckComponent
{
    public GameObject GroundedGO { get; private set; }
    public Collider GroundedCollider { get; private set; }
    public LayerMask GroundMask { get; private set; }

    public GroundedCheckComponent(GameObject groundedGO, LayerMask groundMask)
    {
        GroundedGO = groundedGO;
        GroundedCollider = GroundedGO.GetComponent<Collider>();
        GroundMask = groundMask;

        IsGrounded();
    }

    public bool IsGrounded()
    {
        return Physics.CheckSphere(GroundedCollider.gameObject.transform.position, -GroundedCollider.bounds.extents.y * 1.25f, GroundMask);
    }
}
