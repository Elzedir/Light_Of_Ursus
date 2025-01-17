using System.Collections;
using Equipment;
using Initialisation;
using Managers;
using Personality;
using TickRates;
using UnityEngine;

namespace Actor
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(BoxCollider))]
    public class Actor_Component : MonoBehaviour
    {
        public bool IsPlayer => GetComponent<Player>() is not null;
        public uint ActorID => ActorData.ActorID;
        public bool IsSpawned 
        { 
            get => ActorData.IsSpawned;
            set => ActorData.IsSpawned = value;
        }
        public Actor_Data ActorData;
        public void SetActorData(Actor_Data actorData) => ActorData = actorData;
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
        public GroundedCheckComponent GroundCheckComponent;
        DecisionMakerComponent _decisionMakerComponent;

        public DecisionMakerComponent DecisionMakerComponent =>
            _decisionMakerComponent ??= new DecisionMakerComponent(ActorID);

        void Awake()
        {
            Manager_Initialisation.OnInitialiseActors += _initialise_PreExisting;
        }

        bool _initialised;

        void _initialise_PreExisting()
        {
            var actorData = Actor_Manager.GetActor_DataFromComponent(this);
            
            if (actorData is null)
            {
                Debug.LogWarning($"Actor with name {name} not found in Actor_SO.");
                return;
            }
            
            SetActorData(actorData);
            
            Initialise();
        }

        public void Initialise()
        {
            if (ActorData == null)
            {
                Debug.LogError($"Actor: {name} doesn't have ActorData.");
                return;
            }

            transform.parent.name = $"{ActorData.ActorName}Body";
            transform.name        = $"{ActorData.ActorName}";

            _setTickRate(TickRateName.OneSecond, false);

            _updateVisuals();

            _initialised = true;
        }

        TickRateName _currentTickRateName;

        void _setTickRate(TickRateName tickRateName, bool unregister = true)
        {
            if (_currentTickRateName == tickRateName) return;

            if (unregister) Manager_TickRate.UnregisterTicker(TickerTypeName.Actor, _currentTickRateName, ActorID);
            Manager_TickRate.RegisterTicker(TickerTypeName.Actor, tickRateName, ActorID, _onTick);
            _currentTickRateName = tickRateName;
        }

        void _onTick()
        {
            if (!_initialised) return;

            DecisionMakerComponent.MakeDecision();
        }

        void _updateVisuals()
        {
            ActorMesh.mesh = ActorData.GameObjectData.ActorMesh ?? Resources.GetBuiltinResource<Mesh>("Cube.fbx");
            ActorMaterial.material = ActorData.GameObjectData.ActorMaterial ??
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