using Managers;
using Priority;
using UnityEngine;

namespace Actor
{
    public class DecisionMakerComponent
    {
        readonly ComponentReference_Actor _actorReferences;

        uint           _actorID   => _actorReferences.ActorID;
        Actor_Component _actor     => _actorReferences.Actor;
        Actor_Data      _actorData => _actor.ActorData;

        public DecisionMakerComponent(uint actorID)
        {
            _actorReferences = new ComponentReference_Actor(actorID);
        }

        PriorityComponent_Actor _priorityComponent;

        public PriorityComponent_Actor PriorityComponent =>
            _priorityComponent ??= new PriorityComponent_Actor(_actorID);

        ActorAction _currentActorActionMaster => PriorityComponent.GetCurrentAction();

        public void MakeDecision()
        {
            // Change tick rate according to number of zones to player.
            // Local region is same zone.
            // Regional region is within 1 zone distance.
            // Distant region is 2+ zones.

            var priorityState = _getPriorityState();

            if (!_mustChangeCurrentAction(priorityState, out var nextHighestPriorityValue))
            {
                //Debug.Log("No need to change current action.");
                return;
            }

            PriorityComponent.SetCurrentAction(nextHighestPriorityValue.PriorityID);
        }

        ActorPriorityState _getPriorityState()
        {
            if (_actorData.StatesAndConditionsData.Actor_States.GetSubState(SubStateName.IsInCombat))
            {
                return ActorPriorityState.InCombat;
            }

            if (_actorData.CareerData.JobsActive && _actorData.CareerData.HasCurrentJob())
            {
                Debug.Log("Step 1: Has Job");
                return ActorPriorityState.HasJob;
            }

            if (_actorData.CareerData.GetNewCurrentJob())
            {
                return ActorPriorityState.HasJob;
            }

            return ActorPriorityState.None;
        }

        bool _mustChangeCurrentAction(ActorPriorityState  actorPriorityState,
                                      out PriorityElement nextHighestPriorityElement)
        {
            var currentAction = PriorityComponent.GetCurrentAction();
            nextHighestPriorityElement = PriorityComponent.PeekHighestPriority(actorPriorityState);

            if (nextHighestPriorityElement is null)
            {
                //Debug.LogWarning("There is no next highest priority.");
                return false;
            }

            if ((uint)currentAction.ActionName == nextHighestPriorityElement.PriorityID)
            {
                Debug.Log("Current action is the same as next highest priority.");
                return false;
            }

            var nextHighestPriority =
                ActorAction_Manager.GetNewActorAction((ActorActionName)nextHighestPriorityElement.PriorityID);

            Debug.Log($"Next Highest Priority: {nextHighestPriority} is higher than Current Action: {currentAction.ActionName}");
            
            return true;
        }
    }

    public enum ActorPriorityState
    {
        None,
            
        InCombat,
        HasJob,
    }
}
