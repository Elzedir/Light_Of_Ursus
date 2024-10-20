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
    public PriorityComponent PriorityComponent;
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

        PriorityComponent = new PriorityComponent(_actorID);
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
    TargetPosition,
    WeightOfItem,
    TimeToComplete,

}

public class PriorityComponent : ActorReferences
{
    public PriorityComponent(uint actorID) : base(actorID) { }

    public PriorityQueue ActionQueue;
    public Dictionary<PriorityImportance, List<Priority>> CachedActionQueue;
    bool _syncingCachedQueue = false;
    float _timeDeferment = 1f;

    public void OnFullIdentificationChange()
    {
        // Update all relevant actions
    }

    public void OnConditionChange(Dictionary<ActionName, Dictionary<PriorityParameter, object>> actionsToPrioritise)
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
                    AddToCachedActionQueue(new Priority((uint)action.Key, priorities), PriorityImportance.High);
                    break;
                case PriorityImportance.Medium:
                    AddToCachedActionQueue(new Priority((uint)action.Key, priorities), PriorityImportance.Medium);
                    break;
                case PriorityImportance.Low:
                    AddToCachedActionQueue(new Priority((uint)action.Key, priorities), PriorityImportance.Low);
                    break;
                default:
                    Debug.LogError($"PriorityImportance: {action.Value} not found.");
                    break;
            }
        }
    }

    // For an action Haul, the priorities would be:
    // 1. Distance to target
    // 2. Weight of item
    // 3. Time to complete

    void _initialiseActions()
    {
        ActionQueue = new PriorityQueue(100);
    }

    public void FullPriorityUpdate()
    {
        SyncCachedActionQueueHigh();
    }

    public void SyncCachedActionQueueHigh(bool syncing = false)
    {
        foreach (var priority in CachedActionQueue[PriorityImportance.Low])
        {
            if (!ActionQueue.Update(priority.PriorityID, priority.AllPriorities))
            {
                if (!ActionQueue.Enqueue(priority.PriorityID, priority.AllPriorities))
                {
                    Debug.LogError($"PriorityID: {priority.PriorityID} unable to be added to PriorityQueue.");
                }
            }
        }

        CachedActionQueue[PriorityImportance.Low].Clear();
        if (syncing) _syncingCachedQueue = false;
    }

    void _syncCachedActionQueueHigh_DeferredUpdate()
    {
        _syncingCachedQueue = true;
        Manager_DeferredActions.AddDeferredAction(() => SyncCachedActionQueueHigh(true), _timeDeferment);
    }

    public void AddToCachedActionQueue(Priority priority, PriorityImportance priorityImportance)
    {
        if (CachedActionQueue == null) CachedActionQueue = new Dictionary<PriorityImportance, List<Priority>>();

        if (!CachedActionQueue.ContainsKey(priorityImportance)) CachedActionQueue.Add(priorityImportance, new List<Priority>());

        CachedActionQueue[priorityImportance].Add(priority);

        if (!_syncingCachedQueue) _syncCachedActionQueueHigh_DeferredUpdate();
    }

    public void AddAction(ActionName actionName, List<float> priorities)
    {
        if (ActionQueue == null) _initialiseActions();

        if (!ActionQueue.Enqueue((uint)actionName, priorities))
        {
            Debug.LogError($"ActionName: {actionName} unable to be added to PriorityQueue.");
        }
    }

    public void UpdateAction(ActionName actionName, List<float> priorities)
    {
        if (ActionQueue == null) _initialiseActions();

        if (!ActionQueue.Update((uint)actionName, priorities))
        {
            Debug.LogError($"ActionName: {actionName} unable to be updated in PriorityQueue.");
        }
    }

    public void RemoveAction(ActionName actionName)
    {
        if (ActionQueue == null) _initialiseActions();

        if (!ActionQueue.Remove((uint)actionName))
        {
            Debug.LogError($"ActionName: {actionName} unable to be removed from PriorityQueue.");
        }
    }

    public Priority CheckNextAction()
    {
        if (ActionQueue == null) _initialiseActions();

        return ActionQueue.Peek();
    }

    public Priority CheckSpecificAction(ActionName actionName)
    {
        if (ActionQueue == null) _initialiseActions();

        return ActionQueue.Peek((uint)actionName);
    }

    public Priority PerformNextAction()
    {
        if (ActionQueue == null) _initialiseActions();

        return ActionQueue.Dequeue();
    }

    public Priority PerformSpecificAction(ActionName actionName)
    {
        if (ActionQueue == null) _initialiseActions();

        return ActionQueue.Dequeue((uint)actionName);
    }

    
}