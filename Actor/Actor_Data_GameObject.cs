using System;
using System.Collections.Generic;
using Inventory;
using Priority;
using Tools;
using UnityEngine;

namespace Actor
{
    [Serializable]
    public class Actor_Data_GameObject : Priority_Updater
    {
        public ComponentReference_Actor ActorReference => Reference as ComponentReference_Actor;

        public Actor_Data_GameObject(uint actorID, Transform actorTransform = null, Mesh actorMesh = null,
            Material actorMaterial = null) : base(actorID, ComponentType.Actor)
        {
            _actorTransform = actorTransform;
            ActorMesh = actorMesh ?? Resources.GetBuiltinResource<Mesh>("Cube.fbx");
            ActorMaterial = actorMaterial ?? Resources.Load<Material>("Materials/Material_Red");
        }

        public Actor_Data_GameObject(Actor_Data_GameObject actorDataGameObject) : base(actorDataGameObject.ActorReference.ActorID,
            ComponentType.Actor)
        {
            _actorTransform = actorDataGameObject.ActorTransform;
            ActorMesh = actorDataGameObject.ActorMesh;
            ActorMaterial = actorDataGameObject.ActorMaterial;
        }
        
        public override DataToDisplay GetDataToDisplay(bool toggleMissingDataDebugs)
        {
            _updateDataDisplay(DataToDisplay,
                title: "Game Object Properties",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: GetStringData());

            return DataToDisplay;
        }

        public override Dictionary<string, string> GetStringData() => new()
        {
            { "Actor Last Saved Position", $"{LastSavedActorPosition}" },
            { "Actor Last Saved Scale", $"{LastSavedActorScale}" },
            { "Actor Last Saved Rotation", $"{LastSavedActorRotation.eulerAngles}" },
            { "Actor Mesh", $"{ActorMesh}" },
            { "Actor Material", $"{ActorMaterial}" }
        };

        public void UpdateActorGOProperties()
        {
            SetActorTransformProperties();
        }

        [NonSerialized] Transform _actorTransform;

        public Transform ActorTransform
        {
            get { return _actorTransform ??= Actor_Manager.GetActor_Component(ActorReference.ActorID)?.transform; }
        }

        public void SetActorTransformProperties()
        {
            if (ActorTransform is null)
            {
                Debug.Log($"ActorTransform for actor {ActorReference.ActorID} is null.");
                return;
            }

            _setActorPosition(ActorTransform.position);
            _setActorRotation(ActorTransform.rotation);
            _setActorScale(ActorTransform.localScale);
        }

        public Vector3 LastSavedActorPosition;
        void _setActorPosition(Vector3 actorPosition) => LastSavedActorPosition = actorPosition;
        public Quaternion LastSavedActorRotation;
        void _setActorRotation(Quaternion actorRotation) => LastSavedActorRotation = actorRotation;
        public Vector3 LastSavedActorScale;
        void _setActorScale(Vector3 actorScale) => LastSavedActorScale = actorScale;

        public Mesh ActorMesh;
        public void SetActorMesh(Mesh actorMesh) => ActorMesh = actorMesh;
        public Material ActorMaterial;
        public void SetActorMaterial(Material actorMaterial) => ActorMaterial = actorMaterial;

        public void SetGameObjectProperties(Transform actorTransform)
        {
            _actorTransform = actorTransform;

            ActorMesh = ActorTransform.GetComponent<MeshFilter>().sharedMesh;
            ActorMaterial = ActorTransform.GetComponent<MeshRenderer>().sharedMaterial;

            // Temporary

            ActorMesh ??= Resources.GetBuiltinResource<Mesh>("Cube.fbx"); // Later will come from species
            ActorMaterial ??= Resources.Load<Material>("Materials/Material_Red"); // Later will come from species
        }

        protected override bool _priorityChangeNeeded(object dataChanged)
        {
            return false;
        }

        protected override Dictionary<PriorityUpdateTrigger, Dictionary<PriorityParameterName, object>>
            _priorityParameterList { get; set; } = new();
    }
}