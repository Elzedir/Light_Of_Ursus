using System;
using System.Collections;
using Actors;
using Jobs;
using Recipes;
using UnityEngine;

namespace WorkPosts
{
    [RequireComponent(typeof(BoxCollider))]
    public class WorkPost_Component : MonoBehaviour
    {
        public Job Job;
        public ulong WorkPostID                 => Job.WorkPostID;
        public Actor_Component CurrentWorker            => Job.Actor;
        public bool IsCurrentlyBeingOperated() => false; // If the actor is actually at the operating area, operating.
        
        BoxCollider          _workPostCollider;
        public BoxCollider   WorkPostCollider => _workPostCollider ??= GetComponent<BoxCollider>();

        public void Initialise(Job job)
        {
            Job = job;
            
            if (WorkPostCollider.isTrigger) return;
            
            WorkPostCollider.isTrigger = true;
        }

        public float Operate(float baseProgressRate, Recipe_Data recipe)
        {
            if (CurrentWorker.ActorID is 0 || Job.IsWorkerMovingToWorkPost) return 0;

            if (WorkPostCollider.bounds.Contains(Job.Actor.transform.position)) return _produce(baseProgressRate, recipe);
            
            StartCoroutine(_moveWorkerToWorkPost(Actor_Manager.GetActor_Component(actorID: CurrentWorker.ActorID), transform.position));

            return 0;
        }
        
        float _produce(float baseProgressRate, Recipe_Data recipe)
        {
            var productionRate = baseProgressRate;
            // Then modify production rate by any area modifiers (Land type, events, etc.)

            foreach (var vocation in recipe.RequiredVocations)
            {
                productionRate *= CurrentWorker.ActorData.Vocation.GetProgress(vocation);
            }

            return productionRate;
        }

        IEnumerator _moveWorkerToWorkPost(Actor_Component actor, Vector3 position)
        {
            if (Job.IsWorkerMovingToWorkPost) yield break;

            Job.IsWorkerMovingToWorkPost = true;

            yield return actor.StartCoroutine(actor.BasicMove(position));

            if (actor.ActorData.SceneObject.ActorTransform.position != position)
                actor.ActorData.SceneObject.ActorTransform.position = position;

            Job.IsWorkerMovingToWorkPost = false;
        }
    }
}
