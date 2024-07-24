using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class Actor_Base : MonoBehaviour
{
    public Actor_Data_SO ActorData;
    public CharacterJobManager CharacterJobManager;
    public Rigidbody RigidBody { get; protected set; }
    public Collider Collider { get; protected set; }
    public Animator Animator { get; protected set; }
    public Animation Animation { get; protected set; }
    public CharacterEquipmentManager Manager_Equipment { get; protected set; }
    public GroundedCheckComponent GroundedObject { get; protected set; }

    void Awake()
    {
        RigidBody = GetComponentInParent<Rigidbody>();
        Collider = GetComponent<Collider>();
        Animator = GetComponent<Animator>();
        Animation = GetComponent<Animation>();
        Manager_Equipment = new CharacterEquipmentManager();
        Manager_Equipment.InitialiseEquipment(this);
    }

    void Start()
    {
        if (ActorData != null)
        {
            ActorData.Initialise(this);
            CharacterJobManager = new CharacterJobManager(this, ActorData.ActorCareer, Manager_Career.GetCareer(ActorData.ActorCareer).CareerJobs);
        }
    }

    public bool IsGrounded()
    {
        if (GroundedObject == null) GroundedObject = Manager_GroundCheck.AddGroundedObject(gameObject);

        return GroundedObject.IsGrounded();
    }
}
