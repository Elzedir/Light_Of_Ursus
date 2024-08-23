using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Security.Cryptography;
using UnityEngine;

public class Actor_Base : MonoBehaviour, IInventoryOwner
{
    [SerializeField] ActorData _actorData;
    public ActorData ActorData { get { return _actorData; } private set { _actorData = value; } }
    public Rigidbody ActorBody { get; protected set; }
    public Collider ActorCollider { get; protected set; }
    public MeshFilter ActorMesh { get; protected set; }
    public MeshRenderer ActorMaterial { get; protected set; }
    public Animator ActorAnimator { get; protected set; }
    public Animation ActorAnimation { get; protected set; }
    public EquipmentComponent EquipmentComponent { get; protected set; }
    public PersonalityComponent PersonalityComponent { get; protected set; }
    public GroundedCheckComponent GroundedObject { get; protected set; }

    void Awake()
    {
        Manager_Initialisation.OnInitialiseActors += Initialise;
    }

    private void Update()
    {
        //if (gameObject.name.Contains("Tom"))
        //{
        //    if (JobComponent == null) return;

        //    if (JobComponent.AllCurrentJobs.Count == 0) Debug.Log("Has no jobs");
        //    else foreach (Job job in JobComponent.AllCurrentJobs)
        //        {
        //            Debug.Log(job.JobName);
        //        }
        //}
    }

    public void Initialise()
    {
        ActorBody = GetComponentInParent<Rigidbody>() ?? gameObject.AddComponent<Rigidbody>();
        ActorCollider = GetComponent<Collider>() ?? gameObject.AddComponent<BoxCollider>();
        ActorAnimator = GetComponent<Animator>() ?? gameObject.AddComponent<Animator>();
        ActorAnimation = GetComponent<Animation>() ?? gameObject.AddComponent<Animation>();
        ActorMesh = GetComponent<MeshFilter>() ?? gameObject.AddComponent<MeshFilter>();
        ActorMaterial = GetComponent<MeshRenderer>() ?? gameObject.AddComponent<MeshRenderer>();
        EquipmentComponent = new EquipmentComponent(this);

        if (ActorData == null) throw new ArgumentException("ActorData doesn't exist");

        transform.parent.name = $"{ActorData.ActorName.Name}Body";
        transform.name = $"{ActorData.ActorName.Name}";
        PersonalityComponent = new PersonalityComponent(this, ActorData.SpeciesAndPersonality.ActorPersonality.GetPersonality());

        UpdateVisuals();
    }

    public void SetActorData(ActorData actorData)
    {
        ActorData = actorData;
    }

    public void UpdateVisuals()
    {
        ActorMesh.mesh = ActorData.GameObjectProperties.ActorMesh ?? Resources.GetBuiltinResource<Mesh>("Cube.fbx");
        ActorMaterial.material = ActorData.GameObjectProperties.ActorMaterial ?? Resources.Load<Material>("Materials/Material_Red");
    }

    public bool IsGrounded()
    {
        if (GroundedObject == null) GroundedObject = Manager_GroundCheck.AddGroundedObject(gameObject);

        return GroundedObject.IsGrounded();
    }

    public IEnumerator BasicMove(Vector3 targetPosition, float speed = 10)
    {
        while (Vector3.Distance(transform.parent.position, targetPosition) > 0.1f)
        {
            Vector3 direction = (targetPosition - transform.parent.position).normalized;

            ActorBody.velocity = direction * speed;

            yield return null;
        }

        ActorBody.velocity = Vector3.zero;
        transform.parent.position = targetPosition;
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }

    public InventoryData GetInventoryData()
    {
        return ActorData.InventoryAndEquipment.InventoryData;
    }
}
