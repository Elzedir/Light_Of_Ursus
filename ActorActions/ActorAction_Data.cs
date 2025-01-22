using System;
using System.Collections;
using System.Collections.Generic;
using Actor;
using Jobs;
using Priority;
using StateAndCondition;

namespace ActorActions
{
    [Serializable]
    public class ActorAction_Data
    {
        public ActorActionName ActionName;
        public string ActionDescription;
        public Dictionary<StateName, bool> RequiredStates;
        public List<PriorityParameterName> RequiredParameters;
        public JobName PrimaryJob;
        public List<Func<ActorAction_Parameters, IEnumerator>> ActionList;

        public ActorAction_Data(ActorActionName actionName, string actionDescription,
            Dictionary <StateName, bool> requiredStates, List<PriorityParameterName> requiredParameters, 
            JobName primaryJob, List<Func<ActorAction_Parameters, IEnumerator>> actionList)
        {
            ActionName = actionName;
            ActionList = actionList;
            RequiredStates = requiredStates;
            RequiredParameters = requiredParameters;
            ActionDescription = actionDescription;
            PrimaryJob = primaryJob;
        }
    }
}