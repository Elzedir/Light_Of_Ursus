using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Security.Cryptography;
using UnityEngine;

public class Actor_Base : MonoBehaviour, IInventoryActor
{
    [SerializeField] Actor_Data_SO _actorData;
    public Actor_Data_SO ActorData { get { return _actorData; } private set { _actorData = value; } }
    public GameObject GameObject { get; private set; }
    public Rigidbody ActorBody { get; protected set; }
    public Collider ActorCollider { get; protected set; }
    public Animator ActorAnimator { get; protected set; }
    public Animation ActorAnimation { get; protected set; }
    public JobComponent JobComponent { get; protected set; }
    public InventoryComponent InventoryComponent { get; protected set; }
    public CraftingComponent CraftingComponent { get; protected set; }
    public VocationComponent VocationComponent { get; protected set; }
    public PersonalityComponent PersonalityComponent { get; protected set; }
    public GatheringComponent GatheringComponent { get; protected set; }
    public CharacterEquipmentManager ActorEquipmentManager { get; protected set; }
    public GroundedCheckComponent GroundedObject { get; protected set; }

    void Awake()
    {
        Manager_Initialisation.OnInitialiseActors += Initialise;
    }

    public void Initialise(Actor_Data_SO actorData)
    {
        GameObject = gameObject;
        ActorBody = GetComponentInParent<Rigidbody>() ?? gameObject.AddComponent<Rigidbody>();
        ActorCollider = GetComponent<Collider>() ?? gameObject.AddComponent<BoxCollider>();
        ActorAnimator = GetComponent<Animator>() ?? gameObject.AddComponent<Animator>();
        ActorAnimation = GetComponent<Animation>() ?? gameObject.AddComponent<Animation>();
        ActorEquipmentManager = new CharacterEquipmentManager();
        ActorEquipmentManager.InitialiseEquipment(this);

        ActorData ??= actorData;

        if (ActorData != null)
        {
            ActorData.Initialise(this);
            JobComponent = new JobComponent(this, ActorData.ActorCareer, Manager_Career.GetCareer(ActorData.ActorCareer).CareerJobs);
            CraftingComponent = new CraftingComponent(this, new List<Recipe> { Manager_Crafting.GetRecipe(RecipeName.Plank) });
            VocationComponent = new VocationComponent(this, new());
            GatheringComponent = new GatheringComponent(this);
            PersonalityComponent = new PersonalityComponent(this, ActorData.ActorPersonality.GetPersonality());
        }
    }

    public void InitialiseInventoryComponent()
    {
        InventoryComponent = new InventoryComponent(this, new List<Item>());
    }

    public void UpdateInventoryDisplay()
    {
        ActorData.ActorInventory.UpdateDisplayInventory(this);
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
}
