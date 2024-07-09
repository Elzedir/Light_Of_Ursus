using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class Actor_Base : MonoBehaviour
{
    public Rigidbody RigidBody { get; protected set; }
    public Collider Collider { get; protected set; }
    public Animator Animator { get; protected set; }
    public Animation Animation { get; protected set; }
    public Manager_Equipment Manager_Equipment { get; protected set; }
    public GroundedCheckComponent GroundedObject { get; protected set; }

    void Awake()
    {
        RigidBody = GetComponentInParent<Rigidbody>();
        Collider = GetComponent<Collider>();
        Animator = GetComponent<Animator>();
        Animation = GetComponent<Animation>();
        Manager_Equipment = new Manager_Equipment();
        Manager_Equipment.InitialiseEquipment(this);
    }

    public bool IsGrounded()
    {
        if (GroundedObject == null) GroundedObject = Manager_GroundCheck.AddGroundedObject(gameObject);

        return GroundedObject.IsGrounded();
    }
}
