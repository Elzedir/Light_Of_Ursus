using System.Collections;
using Actors;
using UnityEngine;
using Jobs;
using UnityEngine.Serialization;

namespace Jobs
{
    public class Job_Component : MonoBehaviour
    {
        public Job_Data Job_Data;
        
        public ulong JobID;
        public bool IsCurrentlyBeingOperated() => false; // If the actor is actually at the operating area, operating.
        
        BoxCollider          _jobCollider;
        public BoxCollider   JobCollider => _jobCollider ??= GetComponent<BoxCollider>();

        public void Initialise(Job_Data job_Data)
        {
            Job_Data = job_Data;
            
            if (JobCollider.isTrigger) return;
            
            JobCollider.isTrigger = true;
        }

        public IEnumerator MoveWorkerToJob(Actor_Component actor, Vector3 position)
        {
            Job_Data.IsWorkerMovingToJob = true;

            yield return actor.StartCoroutine(actor.BasicMove(position));

            if (actor.ActorData.SceneObject.ActorTransform.position != position)
                actor.ActorData.SceneObject.ActorTransform.position = position;

            Job_Data.IsWorkerMovingToJob = false;
        }
    }
}
