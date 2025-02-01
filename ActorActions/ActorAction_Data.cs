using System;
using System.Collections;
using System.Collections.Generic;
using Actor;
using Jobs;
using Priorities;
using Priority;
using StateAndCondition;
using UnityEngine.Serialization;

namespace ActorActions
{
    [Serializable]
    public class ActorAction_Data
    {
        public ActorActionName ActionName;
        public string ActionDescription;
        public Dictionary<StateName, bool> RequiredStates;
        public JobName PrimaryJob;
        public List<Func<Priority_Parameters, IEnumerator>> ActionList;

        public ActorAction_Data(ActorActionName actionName, string actionDescription,
            Dictionary <StateName, bool> requiredStates, JobName primaryJob, 
            List<Func<Priority_Parameters, IEnumerator>> actionList = null)
        {
            ActionName = actionName;
            RequiredStates = requiredStates;
            ActionDescription = actionDescription;
            PrimaryJob = primaryJob;
            ActionList = actionList ?? new List<Func<Priority_Parameters, IEnumerator>>();
        }
    }
}