using System;
using System.Collections;
using Managers;
using Priority;
using UnityEngine;

namespace Actors
{
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

        void Awake()
        {
            Manager_Initialisation.OnInitialiseActors += Initialise;
        }

        public void Initialise()
        {
            if (ActorData == null) throw new ArgumentException($"Actor: {name} doesn't have ActorData.");

            transform.parent.name = $"{ActorData.ActorName.Name}Body";
            transform.name        = $"{ActorData.ActorName.Name}";

            PersonalityComponent.SetPersonalityTraits(ActorData.SpeciesAndPersonality.ActorPersonality.GetPersonality());
            
            Manager_TickRate.RegisterTickable(_onTick, TickRate.OneSecond);

            _updateVisuals();
        }

        static void _onTick()
        {   
            
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
    }

    public enum ActionName
    {
        None,
        All,
        
        Wander,

        Deliver,
        Fetch,
        Scavenge,
    }
}