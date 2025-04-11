using System.Collections;
using Actors;
using UnityEngine;
using Jobs;
using UnityEngine.Serialization;

namespace Jobs
{
    public class Jo_Component : MonoBehaviour
    {
        [FormerlySerializedAs("Job")] public Job_Old JobOld;
        
        public Job_DefaultValue Job_DefaultValues => Job_List.GetJob_DefaultValue(JobOld.Station.StationName, JobOld.JobID);
        public ulong JobID                 => JobOld.JobID;
        public Actor_Component CurrentWorker            => JobOld.Actor;
        public bool IsCurrentlyBeingOperated() => false; // If the actor is actually at the operating area, operating.
        
        BoxCollider          _JobCollider;
        public BoxCollider   JobCollider => _JobCollider ??= GetComponent<BoxCollider>();

        public void Initialise(Job_Old jobOld)
        {
            JobOld = jobOld;
            
            if (JobCollider.isTrigger) return;
            
            JobCollider.isTrigger = true;
        }

        public IEnumerator MoveWorkerToJob(Actor_Component actor, Vector3 position)
        {
            JobOld.IsWorkerMovingToJob = true;

            yield return actor.StartCoroutine(actor.BasicMove(position));

            if (actor.ActorData.SceneObject.ActorTransform.position != position)
                actor.ActorData.SceneObject.ActorTransform.position = position;

            JobOld.IsWorkerMovingToJob = false;
        }
    }
}
