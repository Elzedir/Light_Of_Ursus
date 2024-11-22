using System;
using System.Collections;
using System.Collections.Generic;
using Managers;
using Priority;
using UnityEngine;

namespace Actors
{
    public enum PriorityStatus
    {
        None = 0,
            
        InCombat,
        HasWork,
    }
    
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(BoxCollider))]
    public class ActorComponent : MonoBehaviour
    {
        uint                           _actorID => ActorData.ActorID;
        public ActorData               ActorData;
        public void                    SetActorData(ActorData actorData) => ActorData = actorData;
        Rigidbody                      _rigidBody;
        public Rigidbody               RigidBody => _rigidBody ??= gameObject.GetComponent<Rigidbody>();
        Collider                       _collider;
        public Collider                Collider => _collider ??= gameObject.GetComponent<BoxCollider>();
        MeshFilter                     _actorMesh;
        public MeshFilter              ActorMesh => _actorMesh ??= gameObject.GetComponent<MeshFilter>();
        MeshRenderer                   _actorMaterial;
        public MeshRenderer            ActorMaterial => _actorMaterial ??= gameObject.GetComponent<MeshRenderer>();
        Animator                       _actorAnimator;
        public Animator                ActorAnimator => _actorAnimator ??= gameObject.GetComponent<Animator>();
        Animation                      _actorAnimation;
        public Animation               ActorAnimation => _actorAnimation ??= gameObject.GetComponent<Animation>();
        EquipmentComponent             _equipmentComponent;
        public EquipmentComponent      EquipmentComponent => _equipmentComponent ??= new EquipmentComponent(this);
        PersonalityComponent           _personalityComponent;
        public PersonalityComponent    PersonalityComponent => _personalityComponent ??= new PersonalityComponent(_actorID);
        public GroundedCheckComponent  GroundCheckComponent;
        PriorityComponent_Actor        _priorityComponent;
        public PriorityComponent_Actor PriorityComponent => _priorityComponent ??= new PriorityComponent_Actor(_actorID);
        public Coroutine               ActorHaulCoroutine;

        ActorActionName _currentActorAction => PriorityComponent.GetCurrentAction();

        void Awake()
        {
            Manager_Initialisation.OnInitialiseActors += Initialise;
        }

        bool _initialised;
        public void Initialise()
        {
            if (ActorData == null) throw new ArgumentException($"Actor: {name} doesn't have ActorData.");

            transform.parent.name = $"{ActorData.ActorName.Name}Body";
            transform.name        = $"{ActorData.ActorName.Name}";

            PersonalityComponent.SetPersonalityTraits(ActorData.SpeciesAndPersonality.ActorPersonality.GetPersonality());
            
            Manager_TickRate.RegisterTickable(_onTick, TickRate.OneSecond);

            _updateVisuals();
            
            _initialised = true;
        }

        void _onTick()
        {
            if (!_initialised) return;
            
            _makeDecision();
        }

        void _makeDecision()
        {
            // Change tick rate according to number of zones to player.
            // Local region is same zone.
            // Regional region is within 1 zone distance.
            // Distant region is 2+ zones.

            var priorityStatus = _getPriorityStatus();

            if (!_mustChangeCurrentAction(priorityStatus))
            {
                Debug.Log("No need to change current action.");
                return;
            }

            var highestPriority = PriorityComponent.GetHighestPriority(priorityStatus);
        }
        
        PriorityStatus _getPriorityStatus()
        {
            if (ActorData.StatesAndConditions.Actor_States.GetSubState(SubStateName.IsInCombat))
            {
                return PriorityStatus.InCombat;
            }

            if (ActorData.CareerAndJobs.HasJob())
            {
                return PriorityStatus.HasWork;
            }

            return PriorityStatus.None;
        }

        bool _mustChangeCurrentAction(PriorityStatus priorityStatus)
        {
            var currentAction       = PriorityComponent.GetCurrentAction();
            var nextHighestPriorityValue = PriorityComponent.PeekHighestPriority(priorityStatus);

            if (nextHighestPriorityValue == null)
            {
                //Debug.LogWarning("Next highest priority is null.");
                return false;
            }
            
            var nextHighestPriority = (ActorActionName)nextHighestPriorityValue.PriorityID;
            
            Debug.Log($"Current Action: {currentAction}, Next Highest Priority: {nextHighestPriority}");

            if (nextHighestPriority != ActorActionName.Idle)
            {
                return _isHigherPriorityThan(nextHighestPriority, currentAction);
            }
            
            Debug.LogError("Next highest priority is None.");
            return false;
        }

        void _updateVisuals()
        {
            ActorMesh.mesh         = ActorData.GameObjectProperties.ActorMesh     ?? Resources.GetBuiltinResource<Mesh>("Cube.fbx");
            ActorMaterial.material = ActorData.GameObjectProperties.ActorMaterial ?? Resources.Load<Material>("Materials/Material_Red");
        }

        public bool IsGrounded()
        {
            GroundCheckComponent ??= Manager_GroundCheck.AddGroundedObject(gameObject);

            return GroundCheckComponent.IsGrounded();
        }

        public IEnumerator BasicMove(Vector3 targetPosition, float speed = 4)
        {
            while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
            {
                Vector3 direction = (targetPosition - transform.position).normalized;
                RigidBody.linearVelocity = direction * speed;
                yield return null;
            }

            RigidBody.linearVelocity = Vector3.zero;
            transform.position = targetPosition;
        }

        static readonly Dictionary<ActorActionName, PriorityImportance> _allPriorityPerAction = new()
        {
            {ActorActionName.Wander, PriorityImportance.Low},
            {ActorActionName.Deliver, PriorityImportance.Medium},
            {ActorActionName.Fetch, PriorityImportance.Medium},
            {ActorActionName.Scavenge, PriorityImportance.Medium},
        };
        
        static bool _isHigherPriorityThan(ActorActionName actorActionName, ActorActionName otherActorActionName)
        {
            return _allPriorityPerAction[actorActionName] < _allPriorityPerAction[otherActorActionName];
        }
    }
}