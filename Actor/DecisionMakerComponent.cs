using Managers;
using Priority;
using UnityEngine;

namespace Actor
{
    public class DecisionMakerComponent
    {
        readonly ComponentReference_Actor _actorReferences;

        uint           _actorID   => _actorReferences.ActorID;
        Actor_Component _actor     => _actorReferences.Actor_Component;
        Actor_Data      _actorData => _actor.ActorData;

        public DecisionMakerComponent(uint actorID)
        {
            _actorReferences = new ComponentReference_Actor(actorID);
        }

        Priority_Data_Actor _priorityData;

        public Priority_Data_Actor PriorityData =>
            _priorityData ??= new Priority_Data_Actor(_actorID);

        ActorAction_Master _currentActorActionMaster => PriorityData.GetCurrentAction();

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

            PriorityData.SetCurrentAction(nextHighestPriorityValue.PriorityID);
        }

        ActorPriorityState _getPriorityState()
        {
            if (_actorData.StatesAndConditionsData.Actor_States.GetSubState(SubStateName.IsInCombat))
            {
                return ActorPriorityState.InCombat;
            }

            if (!_actorData.CareerDataPreset.JobsActive || !_actorData.CareerDataPreset.HasCurrentJob())
                return _actorData.CareerDataPreset.GetNewCurrentJob() ? ActorPriorityState.HasJob : ActorPriorityState.None;
            
            Debug.Log("Step 1: Has Job");
            return ActorPriorityState.HasJob;

        }

        bool _mustChangeCurrentAction(ActorPriorityState  actorPriorityState,
                                      out PriorityElement nextHighestPriorityElement)
        {
            var currentAction = PriorityData.GetCurrentAction();
            nextHighestPriorityElement = PriorityData.PeekHighestPriority(actorPriorityState);
            
            Debug.Log($"Current Action: {currentAction?.ActionName}, Next Highest Priority: {nextHighestPriorityElement?.PriorityID}");

            if (nextHighestPriorityElement is null)
            {
                Debug.LogWarning("There is no next highest priority.");
                return false;
            }

            if ((uint)currentAction.ActionName == nextHighestPriorityElement.PriorityID)
            {
                Debug.Log("Current action is the same as next highest priority.");
                return false;
            }

            var nextHighestPriority =
                ActorAction_Manager.GetActorAction_Master((ActorActionName)nextHighestPriorityElement.PriorityID);

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
