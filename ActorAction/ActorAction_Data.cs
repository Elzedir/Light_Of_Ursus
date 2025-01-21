using System;
using System.Collections;
using System.Collections.Generic;
using Actor;
using Jobs;
using Priority;
using StateAndCondition;

namespace ActorAction
{
    [Serializable]
    public class ActorAction_Data
    {
        public readonly ActorActionName ActionName;
        public string ActionDescription;
        public readonly Dictionary<StateName, bool> RequiredStates;
        public readonly List<PriorityParameterName> RequiredParameters;
        public JobName PrimaryJob;
        public List<Func<Actor_Component, uint, IEnumerator>> ActionList;

        public ActorAction_Data(ActorActionName actionName, string actionDescription,
            Dictionary <StateName, bool> requiredStates, List<PriorityParameterName> requiredParameters, 
            JobName primaryJob, List<Func<Actor_Component, uint, IEnumerator>> actionList)
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