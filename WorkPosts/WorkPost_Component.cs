using System;
using System.Collections;
using Actors;
using Jobs;
using Recipes;
using Station;
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
            if (CurrentWorker.ActorID is 0)
            {
                Debug.LogWarning("No worker assigned to work post.");
                return 0;
            }

            if (WorkPostCollider.bounds.Contains(Job.Actor.transform.position)) return Job.Station.Produce(this, baseProgressRate, recipe);

            if (!Job.IsWorkerMovingToWorkPost)
                StartCoroutine(_moveWorkerToWorkPost(Actor_Manager.GetActor_Component(actorID: CurrentWorker.ActorID),
                    transform.position));

            return 0;
        }

        IEnumerator _moveWorkerToWorkPost(Actor_Component actor, Vector3 position)
        {
            Job.IsWorkerMovingToWorkPost = true;

            yield return actor.StartCoroutine(actor.BasicMove(position));

            if (actor.ActorData.SceneObject.ActorTransform.position != position)
                actor.ActorData.SceneObject.ActorTransform.position = position;

            Job.IsWorkerMovingToWorkPost = false;
        }
    }
}
