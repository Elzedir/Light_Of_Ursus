using Managers;
using Priority;
using UnityEngine;

namespace Actors
{
    public class DecisionMakerComponent
    {
        readonly ComponentReference_Actor _actorReferences;

        uint           _actorID   => _actorReferences.ActorID;
        ActorComponent _actor     => _actorReferences.Actor;
        ActorData      _actorData => _actor.ActorData;
        public DecisionMakerComponent(uint actorID)
        {
            _actorReferences = new ComponentReference_Actor(actorID);
        }
        
        PriorityComponent_Actor        _priorityComponent;
        public PriorityComponent_Actor PriorityComponent => _priorityComponent ??= new PriorityComponent_Actor(_actorID);
        
        ActorAction_Master _currentActorActionMaster => PriorityComponent.GetCurrentAction();
        
        public void MakeDecision()
        {
            // Change tick rate according to number of zones to player.
            // Local region is same zone.
            // Regional region is within 1 zone distance.
            // Distant region is 2+ zones.

            var priorityState = _getPriorityState();

            if (!_mustChangeCurrentAction(priorityState, out var nextHighestPriorityValue))
            {
                Debug.Log("No need to change current action.");
                return;
            }

            PriorityComponent.SetCurrentAction(nextHighestPriorityValue.PriorityID);
        }
        
        PriorityState _getPriorityState()
        {
            if (_actorData.StatesAndConditions.Actor_States.GetSubState(SubStateName.IsInCombat))
            {
                return PriorityState.InCombat;
            }

            if (_actorData.CareerData.JobsActive && _actorData.CareerData.HasCurrentJob())
            {
                Debug.Log("Step 1: Has Job");
                return PriorityState.HasJob;
            }
            
            if (_actorData.CareerData.GetNewCurrentJob())
            {
                return PriorityState.HasJob;
            }

            return PriorityState.None;
        }

        bool _mustChangeCurrentAction(PriorityState priorityState, out PriorityValue nextHighestPriorityValue)
        {
            var currentAction            = PriorityComponent.GetCurrentAction();
            nextHighestPriorityValue = PriorityComponent.PeekHighestPriority(priorityState);
            
            if (nextHighestPriorityValue is null)
            {
                Debug.LogWarning("There is no next highest priority.");
                return false;
            }

            var nextHighestPriority =
                Manager_ActorAction.GetActorAction((ActorActionName)nextHighestPriorityValue.PriorityID);

            Debug.Log($"Current Action: {currentAction.ActionName}, Next Highest Priority: {nextHighestPriority}");

           if (currentAction.ActionName == nextHighestPriority.ActionName)
           {
               Debug.Log("Current action is the same as next highest priority.");
               return false;
           };

           if (!Manager_ActorAction.IsHigherPriorityThan(nextHighestPriority.ActionName, currentAction.ActionName)) return false;
           
           Debug.Log("Next highest priority is higher than current action.");
           return true;
        }
    }
    
    public enum PriorityState
    {
        None,
            
        InCombat,
        HasJob,
    }
}
