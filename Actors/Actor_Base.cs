using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class Actor_Base : MonoBehaviour
{
    public Actor_Data_SO ActorData;
    public CharacterJobManager CharacterJobManager;
    public Rigidbody ActorBody { get; protected set; }
    public Collider ActorCollider { get; protected set; }
    public Animator ActorAnimator { get; protected set; }
    public Animation ActorAnimation { get; protected set; }
    public InventoryComponent InventoryComponent { get; protected set; }
    public CraftingComponent CraftingComponent { get; protected set; }
    public CharacterEquipmentManager ActorEquipmentManager { get; protected set; }
    public GroundedCheckComponent GroundedObject { get; protected set; }

    void Awake()
    {
        ActorBody = GetComponentInParent<Rigidbody>();
        ActorCollider = GetComponent<Collider>();
        ActorAnimator = GetComponent<Animator>();
        ActorAnimation = GetComponent<Animation>();
        ActorEquipmentManager = new CharacterEquipmentManager();
        ActorEquipmentManager.InitialiseEquipment(this);
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

    public IEnumerator BasicMove(Vector3 targetPosition, float speed = 1)
    {
        while (Vector3.Distance(transform.position, targetPosition) > 1)
        {
            Vector3 direction = (targetPosition - transform.position).normalized;
            ActorBody.velocity = direction * speed;

            yield return null;
        }

        ActorBody.velocity = Vector3.zero;
        transform.position = targetPosition;
    }
}
