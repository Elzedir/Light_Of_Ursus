using System.Collections;
using Actor;
using Recipes;
using UnityEngine;

namespace WorkPosts
{
    [RequireComponent(typeof(BoxCollider))]
    
    public class WorkPost_Component : MonoBehaviour
    {
        public uint WorkPostID                 => WorkPostData.WorkPostID;
        public uint CurrentWorkerID            => WorkPostData.CurrentWorker.ActorID;
        public bool IsCurrentlyBeingOperated() => false; // If the actor is actually at the operating area, operating.
        
        public WorkPost_Data WorkPostData;
        public void          SetWorkPostData(WorkPost_Data workPostData) => WorkPostData = workPostData;
        BoxCollider          _workPostCollider;
        public BoxCollider   WorkPostCollider => _workPostCollider ??= GetComponent<BoxCollider>();

        public void Initialise()
        {
            if (WorkPostCollider.isTrigger) return;
            
            //Debug.LogError($"Set IsTrigger to true for {WorkPostID}: {name} BoxCollider");
            WorkPostCollider.isTrigger = true;
        }

        void FixedUpdate()
        {
            Debug.Log($"WorkPost: {WorkPostID} Actor: {WorkPostData.CurrentWorker?.ActorID}");
        }

        public float Operate(float baseProgressRate, Recipe_Data recipe)
        {
            if (CurrentWorkerID is 0 || WorkPostData.IsWorkerMovingToWorkPost) return 0;

            if (WorkPostCollider.bounds.Contains(WorkPostData.CurrentWorker.transform.position)) return _produce(baseProgressRate, recipe);
            
            StartCoroutine(_moveWorkerToWorkPost(Actor_Manager.GetActor_Component(actorID: CurrentWorkerID), transform.position));

            return 0;
        }
        
        float _produce(float baseProgressRate, Recipe_Data recipe)
        {
            var productionRate = baseProgressRate;
            // Then modify production rate by any area modifiers (Land type, events, etc.)

            foreach (var vocation in recipe.RequiredVocations)
            {
                productionRate *= WorkPostData.CurrentWorker.ActorData.VocationData.GetProgress(vocation);
            }

            return productionRate;
        }

        IEnumerator _moveWorkerToWorkPost(Actor_Component actor, Vector3 position)
        {
            if (WorkPostData.IsWorkerMovingToWorkPost) yield break;

            WorkPostData.IsWorkerMovingToWorkPost = true;

            yield return actor.StartCoroutine(actor.BasicMove(position));

            if (actor.ActorData.GameObjectData.ActorTransform.position != position)
            {
                actor.ActorData.GameObjectData.ActorTransform.position = position;
            }

            WorkPostData.IsWorkerMovingToWorkPost = false;
        }
    }
}
