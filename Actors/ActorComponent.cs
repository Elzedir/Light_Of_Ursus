using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoxCollider))]
public class ActorComponent : MonoBehaviour, IInventoryOwner
{
    uint _actorID { get { return ActorData.ActorID; } }
    public ActorData ActorData;
    public void SetActorData(ActorData actorData) => ActorData = actorData;
    Rigidbody _rigidBody;
    public Rigidbody RigidBody { get { return _rigidBody ??= gameObject.GetComponent<Rigidbody>(); } }
    Collider _collider;
    public Collider Collider { get { return _collider ??= gameObject.GetComponent<BoxCollider>(); } }
    public MeshFilter ActorMesh;
    public MeshRenderer ActorMaterial;
    public Animator ActorAnimator;
    public Animation ActorAnimation;
    public EquipmentComponent EquipmentComponent;
    public PersonalityComponent PersonalityComponent;
    public GroundedCheckComponent GroundCheckComponent;
    public TaskComponent TaskComponent;
    public Coroutine ActorHaulCoroutine;

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
        ActorAnimator = GetComponent<Animator>() ?? gameObject.AddComponent<Animator>();
        ActorAnimation = GetComponent<Animation>() ?? gameObject.AddComponent<Animation>();
        ActorMesh = GetComponent<MeshFilter>() ?? gameObject.AddComponent<MeshFilter>();
        ActorMaterial = GetComponent<MeshRenderer>() ?? gameObject.AddComponent<MeshRenderer>();
        EquipmentComponent = new EquipmentComponent(this);

        if (ActorData == null) throw new ArgumentException("ActorData doesn't exist");

        transform.parent.name = $"{ActorData.ActorName.Name}Body";
        transform.name = $"{ActorData.ActorName.Name}";

        TaskComponent = new TaskComponent(_actorID);
        PersonalityComponent = new PersonalityComponent(_actorID);
        PersonalityComponent.SetPersonalityTraits(ActorData.SpeciesAndPersonality.ActorPersonality.GetPersonality());

        UpdateVisuals();
    }


    public void UpdateVisuals()
    {
        ActorMesh.mesh = ActorData.GameObjectProperties.ActorMesh ?? Resources.GetBuiltinResource<Mesh>("Cube.fbx");
        ActorMaterial.material = ActorData.GameObjectProperties.ActorMaterial ?? Resources.Load<Material>("Materials/Material_Red");
    }

    public bool IsGrounded()
    {
        if (GroundCheckComponent == null) GroundCheckComponent = Manager_GroundCheck.AddGroundedObject(gameObject);

        return GroundCheckComponent.IsGrounded();
    }

    public IEnumerator BasicMove(Vector3 targetPosition, float speed = 1)
    {
        // while (Vector3.Distance(transform.parent.position, targetPosition) > 0.1f)
        // {
        //     Vector3 direction = (targetPosition - transform.parent.position).normalized;

        //     RigidBody.linearVelocity = direction * speed;

        //     yield return null;
        // }

        // yield return new WaitForSeconds(Vector3.Distance(transform.parent.position, targetPosition / speed));

        yield return new WaitForSeconds(0.5f);
        // For now, not waiting, just teleporting.
        RigidBody.linearVelocity = Vector3.zero;
        transform.parent.position = targetPosition;
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }

    public InventoryData GetInventoryData()
    {
        return ActorData.InventoryData;
    }
}

public enum ActionType
{
    Basic,
    Survival,
    Social,
    Maintenance,
    Production,
    Intellectual,
    Skill,
    Logistics,
    Management,
    Communication,
    Investigation,
}

public enum ActionName
{
    // Basic Actions
    Idle,
    Move,
    Haul,
    Interact,
    Work,
    Rest,

    // Survival Actions
    Eat,
    Drink,
    Sleep,

    // Social Actions
    Socialise,
    Play,
    Exercise,

    // Maintenance Actions
    Clean,
    Repair,
    Build,

    // Production Actions
    Plant,
    Harvest,
    Cook,
    Craft,

    // Intellectual Actions
    Research,
    Study,
    Teach,

    // Skill Actions
    Train,
    Heal,
    Perform,
    Entertain,
    Create,
    Destroy,

    // Logistics Actions
    Collect,
    Store,
    Retrieve,
    Deliver,
    Transport,
    Trade,
    Buy,
    Sell,

    // Management Actions
    Upgrade,
    Downgrade,
    Hire,
    Fire,
    Promote,
    Demote,
    Assign,
    Unassign,
    Schedule,
    Unscheduled,
    Plan,
    Execute,
    Cancel,
    Pause,
    Resume,
    Stop,
    Start,
    Finish,
    Continue,
    Restart,
    Repeat,
    Wait,

    // Communication Actions
    Listen,
    Speak,
    Read,
    Write,
    Watch,
    Observe,
    Record,
    Report,
    Document,
    Communicate,
    Negotiate,
    Mediate,
    Arbitrate,
    Adjudicate,
    Judge,
    Prosecute,
    Defend,
    Accuse,

    // Investigation Actions
    Investigate,
    Inspect,
    Search,
    Find,
    Discover,
    Explore,
    Map,
    Chart,
    Survey,
    Measure,
    Analyse,
    Evaluate,
    Assess,
    Test,
    Experiment,
    Hypothesise,
    Theorise,
    Prove,
    Disprove,
    Verify,
    Validate,
    Invalidate,
    Correct,
    Improve,
    Enhance,
    Develop,
    Innovate,
    Invent,
    Design,
    Prepare,
    Organise,
    Coordinate,
    Manage,
    Lead,
    Follow,
    Support,
    Assist,
    Help,
    Aid,
    Guide,
    Mentor,
    Coach,
    Learn,
}

public class TaskComponent
{
    public uint ActorID;
    public TaskComponent(uint actorID) => ActorID = actorID;

    ActorComponent _actor;
    public ActorComponent Actor { get => _actor ??= Manager_Actor.GetActor(ActorID); }

    public PriorityQueue PriorityQueue;
    public List<Priority> CachedPriorityQueue;
    bool _syncingCachedQueue = false;
    float _timeDeferment = 1f;

    void _initialiseTasks()
    {
        PriorityQueue = new PriorityQueue(100);
    }

    public void SyncCachedPriorityQueue()
    {
        foreach (var priority in CachedPriorityQueue)
        {
            if (!PriorityQueue.Update(priority.PriorityID, priority.AllPriorities))
            {
                PriorityQueue.Enqueue(priority.PriorityID, priority.AllPriorities);
            }
        }

        CachedPriorityQueue.Clear();
        _syncingCachedQueue = false;
    }
    void SyncCachedPriorityQueue_DeferredUpdate()
    {
        _syncingCachedQueue = true;
        Manager_DeferredActions.AddDeferredAction(SyncCachedPriorityQueue, _timeDeferment);
    }

    public void AddToCachedPriorityQueue(Priority priority)
    {
        CachedPriorityQueue.Add(priority);

        if (!_syncingCachedQueue) SyncCachedPriorityQueue_DeferredUpdate();
    }

    public void AddTask(TaskName taskName, List<double> priorities)
    {
        if (PriorityQueue == null) _initialiseTasks();

        PriorityQueue.Enqueue((uint)taskName, priorities);
    }

    public void UpdateTask(TaskName taskName, List<double> priorities)
    {
        if (PriorityQueue == null) _initialiseTasks();

        PriorityQueue.Update((uint)taskName, priorities);
    }

    public void RemoveTask(TaskName taskName)
    {
        if (PriorityQueue == null) _initialiseTasks();

        PriorityQueue.Remove((uint)taskName);
    }

    public Priority CheckNextTask()
    {
        if (PriorityQueue == null) _initialiseTasks();

        return PriorityQueue.Peek();
    }

    public Priority CheckSpecificTask(TaskName taskName)
    {
        if (PriorityQueue == null) _initialiseTasks();

        return PriorityQueue.Peek((uint)taskName);
    }

    public Priority PerformNextTask()
    {
        if (PriorityQueue == null) _initialiseTasks();

        return PriorityQueue.Dequeue();
    }

    public Priority PerformSpecificTask(TaskName taskName)
    {
        if (PriorityQueue == null) _initialiseTasks();

        return PriorityQueue.Dequeue((uint)taskName);
    }
}