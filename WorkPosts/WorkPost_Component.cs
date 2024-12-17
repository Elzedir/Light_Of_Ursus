using System.Collections;
using Actor;
using Recipe;
using UnityEngine;

namespace WorkPosts
{
    [RequireComponent(typeof(BoxCollider))]
    
    public class WorkPost_Component : MonoBehaviour
    {
        public uint WorkPostID                 => WorkPostData.WorkPostID;
        public uint CurrentWorkerID            => WorkPostData.CurrentWorkerID;
        
        public bool HasWorker()                => WorkPostData.HasWorker();
        public bool IsCurrentlyBeingOperated() => false; // If the actor is actually at the operating area, operating.
        
        public WorkPost_Data WorkPostData;
        public void          SetWorkPostData(WorkPost_Data workPostData) => WorkPostData = workPostData;
        BoxCollider          _workPostCollider;
        public BoxCollider   WorkPostCollider => _workPostCollider ??= GetComponent<BoxCollider>();

        public void Initialise()
        {
            if (WorkPostCollider.isTrigger) return;
            
            Debug.LogError($"Set IsTrigger to true for {WorkPostID}: {name} BoxCollider");
            WorkPostCollider.isTrigger = true;
        }

        public float Operate(float baseProgressRate, Recipe_Data recipeData)
        {
            if (WorkPostData.CurrentWorkerID is 0 || WorkPostData.IsWorkerMovingToWorkPost) return 0;

            if (WorkPostData.CurrentWorker.transform.position != null && !WorkPostCollider.bounds.Contains(WorkPostData.CurrentWorker.transform.position))
            {
                StartCoroutine(_moveWorkerToWorkPost(Actor_Manager.GetActor_Component(actorID: WorkPostData.CurrentWorkerID), transform.position));

                return 0;
            }

            return produce();

            float produce()
            {
                float productionRate = baseProgressRate;
                // Then modify production rate by any area modifiers (Land type, events, etc.)

                foreach (var vocation in recipeData.RequiredVocations)
                {
                    productionRate *= WorkPostData.CurrentWorker.ActorData.VocationData.GetProgress(vocation);
                }

                return productionRate;
            }
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
