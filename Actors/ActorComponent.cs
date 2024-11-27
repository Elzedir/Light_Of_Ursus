using System.Collections;
using System.Collections.Generic;
using Managers;
using Priority;
using UnityEngine;

namespace Actors
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(BoxCollider))]
    public class ActorComponent : MonoBehaviour
    {
        public bool IsPlayer => GetComponent<Player>() is not null;
        public uint ActorID => ActorData.ActorID;
        public ActorData ActorData;
        public void SetActorData(ActorData actorData) => ActorData = actorData;
        Rigidbody _rigidBody;
        public Rigidbody RigidBody => _rigidBody ??= gameObject.GetComponent<Rigidbody>();
        Collider _collider;
        public Collider Collider => _collider ??= gameObject.GetComponent<BoxCollider>();
        MeshFilter _actorMesh;
        public MeshFilter ActorMesh => _actorMesh ??= gameObject.GetComponent<MeshFilter>();
        MeshRenderer _actorMaterial;
        public MeshRenderer ActorMaterial => _actorMaterial ??= gameObject.GetComponent<MeshRenderer>();
        Animator _actorAnimator;
        public Animator ActorAnimator => _actorAnimator ??= gameObject.GetComponent<Animator>();
        Animation _actorAnimation;
        public Animation ActorAnimation => _actorAnimation ??= gameObject.GetComponent<Animation>();
        EquipmentComponent _equipmentComponent;
        public EquipmentComponent EquipmentComponent => _equipmentComponent ??= new EquipmentComponent(this);
        PersonalityComponent _personalityComponent;
        public PersonalityComponent PersonalityComponent => _personalityComponent ??= new PersonalityComponent(ActorID);
        public GroundedCheckComponent GroundCheckComponent;
        DecisionMakerComponent _decisionMakerComponent;

        public DecisionMakerComponent DecisionMakerComponent =>
            _decisionMakerComponent ??= new DecisionMakerComponent(ActorID);

        void Awake()
        {
            Manager_Initialisation.OnInitialiseActors += Initialise;
        }

        bool _initialised;

        public void Initialise()
        {
            if (ActorData == null)
            {
                Debug.LogError($"Actor: {name} doesn't have ActorData.");
                return;
            }

            transform.parent.name = $"{ActorData.ActorName.Name}Body";
            transform.name        = $"{ActorData.ActorName.Name}";

            PersonalityComponent.SetPersonalityTraits(ActorData.SpeciesAndPersonality.ActorPersonality
                                                               .GetPersonality());

            _setTickRate(TickRate.OneSecond);

            _updateVisuals();

            _initialised = true;
        }

        TickRate _currentTickRate;

        void _setTickRate(TickRate tickRate)
        {
            if (_currentTickRate == tickRate) return;

            Manager_TickRate.UnregisterTicker(TickerType.Actor, _currentTickRate, ActorID);
            Manager_TickRate.RegisterTicker(TickerType.Actor, tickRate, ActorID, _onTick);
            _currentTickRate = tickRate;
        }

        void _onTick()
        {
            if (!_initialised) return;

            DecisionMakerComponent.MakeDecision();
        }

        void _updateVisuals()
        {
            ActorMesh.mesh = ActorData.GameObjectProperties.ActorMesh ?? Resources.GetBuiltinResource<Mesh>("Cube.fbx");
            ActorMaterial.material = ActorData.GameObjectProperties.ActorMaterial ??
                                     Resources.Load<Material>("Materials/Material_Red");
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
            transform.position       = targetPosition;
        }
    }
}