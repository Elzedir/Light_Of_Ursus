using System.Collections;
using Actor;
using Recipe;
using UnityEngine;
using UnityEngine.Serialization;

namespace WorkPost
{
    public class WorkPost_Component : MonoBehaviour
    {
        public uint          WorkPostID => WorkPostData.WorkPostID;
        public WorkPost_Data WorkPostData;
        public void          SetWorkPostData(WorkPost_Data workPostData) => WorkPostData = workPostData;
        public BoxCollider   WorkPostCollider;

        public void Initialise()
        {
            WorkPostData = WorkPostData;

            WorkPostCollider = WorkPost;

            if (!WorkPostCollider.isTrigger)
            {
                Debug.Log($"Set IsTrigger to true for {name}");
                WorkPostCollider.isTrigger = true;
            }
        }
        
        public bool HasWorker()              => WorkPostData.CurrentWorkerID != 0;
        public bool IsCurrentlyBeingOperated() => false; // If the actor is actually at the operating area, operating.

        public float Operate(float baseProgressRate, Recipe_Data recipeData)
        {
            if (WorkPostData.CurrentWorkerID == 0 || WorkPostData.IsWorkerMovingToWorkPost) return 0;

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
