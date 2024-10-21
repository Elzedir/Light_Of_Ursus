using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;

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
    public PriorityComponent_Actor PriorityComponent;
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

        PriorityComponent = new PriorityComponent_Actor(_actorID);
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

public enum ActionName
{
    // Basic Actions
    Idle,
    Move,
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
    Fetch,
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

public enum PriorityImportance
{
    Low,
    Medium,
    High,
    Critical,
}

public enum PriorityParameter
{
    PriorityImportance,
    MaxPriority,
    ItemsToFetch,
    ItemsToDeliver,
    ActorPosition,
    TargetPosition,
    WeightOfItem,
    TimeToComplete,

}

public class PriorityComponent
{
    public PriorityQueue PriorityQueue;
    public Dictionary<PriorityImportance, List<Priority>> CachedPriorityQueue;
    protected bool _syncingCachedQueue = false;
    protected float _timeDeferment = 1f;

    public void OnDataChanged(Dictionary<ActionName, Dictionary<PriorityParameter, object>> actionsToPrioritise)
    {
        foreach (var action in actionsToPrioritise)
        {
            if (action.Value == null) 
            {
                Debug.LogError($"Action: {action.Key} has no values.");
                continue;
            }

            PriorityImportance priorityImportance;

            if (!action.Value.TryGetValue(PriorityParameter.PriorityImportance, out object priorityImportanceObject))
            {
                Debug.LogError($"Action: {action.Key} has no PriorityImportance value.");
                priorityImportance = PriorityImportance.Low;
            }

            priorityImportance = (PriorityImportance)priorityImportanceObject;

            var priorities = PriorityGenerator.GeneratePriorities(action.Key, action.Value);

            switch (priorityImportance)
            {
                case PriorityImportance.Critical:
                    FullPriorityUpdate();
                    break;
                case PriorityImportance.High:
                    AddToCachedPriorityQueue(new Priority((uint)action.Key, priorities), PriorityImportance.High);
                    break;
                case PriorityImportance.Medium:
                    AddToCachedPriorityQueue(new Priority((uint)action.Key, priorities), PriorityImportance.Medium);
                    break;
                case PriorityImportance.Low:
                    AddToCachedPriorityQueue(new Priority((uint)action.Key, priorities), PriorityImportance.Low);
                    break;
                default:
                    Debug.LogError($"PriorityImportance: {action.Value} not found.");
                    break;
            }
        }
    }

    public void FullPriorityUpdate()
    {
        SyncCachedPriorityQueueHigh();
    }

    public void SyncCachedPriorityQueueHigh(bool syncing = false)
    {
        foreach (var priority in CachedPriorityQueue[PriorityImportance.Low])
        {
            if (!PriorityQueue.Update(priority.PriorityID, priority.AllPriorities))
            {
                if (!PriorityQueue.Enqueue(priority.PriorityID, priority.AllPriorities))
                {
                    Debug.LogError($"PriorityID: {priority.PriorityID} unable to be added to PriorityQueue.");
                }
            }
        }

        CachedPriorityQueue[PriorityImportance.Low].Clear();
        if (syncing) _syncingCachedQueue = false;
    }

    void _syncCachedPriorityQueueHigh_DeferredUpdate()
    {
        _syncingCachedQueue = true;
        Manager_DeferredActions.AddDeferredAction(() => SyncCachedPriorityQueueHigh(true), _timeDeferment);
    }

    public void AddToCachedPriorityQueue(Priority priority, PriorityImportance priorityImportance)
    {
        if (CachedPriorityQueue == null) CachedPriorityQueue = new Dictionary<PriorityImportance, List<Priority>>();

        if (!CachedPriorityQueue.ContainsKey(priorityImportance)) CachedPriorityQueue.Add(priorityImportance, new List<Priority>());

        CachedPriorityQueue[priorityImportance].Add(priority);

        if (!_syncingCachedQueue) _syncCachedPriorityQueueHigh_DeferredUpdate();
    }

    void _initialiseActions()
    {
        PriorityQueue = new PriorityQueue(100);
    }

    public void AddAction(ActionName actionName, List<float> priorities)
    {
        if (PriorityQueue == null) _initialiseActions();

        if (!PriorityQueue.Enqueue((uint)actionName, priorities))
        {
            Debug.LogError($"ActionName: {actionName} unable to be added to PriorityQueue.");
        }
    }

    public void UpdateAction(ActionName actionName, List<float> priorities)
    {
        if (PriorityQueue == null) _initialiseActions();

        if (!PriorityQueue.Update((uint)actionName, priorities))
        {
            Debug.LogError($"ActionName: {actionName} unable to be updated in PriorityQueue.");
        }
    }

    public void RemoveAction(ActionName actionName)
    {
        if (PriorityQueue == null) _initialiseActions();

        if (!PriorityQueue.Remove((uint)actionName))
        {
            Debug.LogError($"ActionName: {actionName} unable to be removed from PriorityQueue.");
        }
    }

    public Priority CheckNextAction()
    {
        if (PriorityQueue == null) _initialiseActions();

        return PriorityQueue.Peek();
    }

    public Priority CheckSpecificAction(ActionName actionName)
    {
        if (PriorityQueue == null) _initialiseActions();

        return PriorityQueue.Peek((uint)actionName);
    }

    public Priority PerformNextAction()
    {
        if (PriorityQueue == null) _initialiseActions();

        return PriorityQueue.Dequeue();
    }

    public Priority PerformSpecificAction(ActionName actionName)
    {
        if (PriorityQueue == null) _initialiseActions();

        return PriorityQueue.Dequeue((uint)actionName);
    }    
}

public class PriorityComponent_Actor : PriorityComponent
{
    readonly ActorReferences _actorReferences;

    public uint ActorID { get { return _actorReferences.ActorID; } }
    protected ActorComponent Actor { get { return _actorReferences.Actor; } }

    public PriorityComponent_Actor(uint actorID) => _actorReferences = new ActorReferences(actorID);
}