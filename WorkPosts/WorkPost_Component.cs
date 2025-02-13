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
        
        public WorkPost_DefaultValue WorkPost_DefaultValues => WorkPost_List.GetWorkPost_DefaultValue(Job.Station.StationName, Job.WorkPostID);
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

        public IEnumerator MoveWorkerToWorkPost(Actor_Component actor, Vector3 position)
        {
            Job.IsWorkerMovingToWorkPost = true;

            yield return actor.StartCoroutine(actor.BasicMove(position));

            if (actor.ActorData.SceneObject.ActorTransform.position != position)
                actor.ActorData.SceneObject.ActorTransform.position = position;

            Job.IsWorkerMovingToWorkPost = false;
        }
    }
}
