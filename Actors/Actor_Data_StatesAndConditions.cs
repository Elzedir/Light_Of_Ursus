using System.Collections.Generic;
using ActorActions;
using Inventory;
using Pathfinding;
using Priorities;
using StateAndCondition;
using Tools;

namespace Actors
{
    public class Actor_Data_StatesAndConditions : Priority_Class
    {
        public ComponentReference_Actor ActorReference => Reference as ComponentReference_Actor;
        
        public Actor_Data_States     States;
        public Actor_Data_Conditions Conditions;
        
        public Actor_Data_StatesAndConditions(ulong actorID, Actor_Data_States states, Actor_Data_Conditions conditions) : base (actorID, ComponentType.Actor)
        {
            States     = states;
            Conditions = conditions;
        }
        
        public Actor_Data_StatesAndConditions(Actor_Data_StatesAndConditions actorDataStatesAndConditions) : base (actorDataStatesAndConditions.ActorReference.ActorID, ComponentType.Actor)
        {
            States     = actorDataStatesAndConditions.States;
            Conditions = actorDataStatesAndConditions.Conditions;
        }
        
        public void SetActorStatesAndConditions (Actor_Data_StatesAndConditions actorDataStatesAndConditions)
        {
            States = actorDataStatesAndConditions.States;
            Conditions = actorDataStatesAndConditions.Conditions;
        }
        
        public (List<MoverType> enabledMoverTypes, List<MoverType> DisabledMoverTypes) GetMoverTypes()
        {
            var enabledMoverTypes = new List<MoverType>();
            var disabledMoverTypes = new List<MoverType>();
            
            var (enabledStates, disabledStates) = States.GetMoverTypes();
            enabledMoverTypes.AddRange(enabledStates);
            disabledMoverTypes.AddRange(disabledStates);
            
            var (enabledConditions, disabledConditions) = Conditions.GetMoverTypes();
            enabledMoverTypes.AddRange(enabledConditions);
            disabledMoverTypes.AddRange(disabledConditions);

            return (enabledMoverTypes, disabledMoverTypes);
        }
        
        public override Dictionary<string, string> GetStringData()
        {
            return new Dictionary<string, string>
            {
                { States != null ? "Actor States" : "Actor States is null", "" },
                { Conditions != null ? "Actor Conditions" : "Conditions is null", "" }
            };
        }

        public override DataToDisplay GetDataToDisplay(bool toggleMissingDataDebugs)
        {
            _updateDataDisplay(DataToDisplay,
                title: "States And Conditions",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: GetStringData());
            
            _updateDataDisplay(DataToDisplay,
                title: "Conditions",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allSubData: Conditions.GetDataToDisplay(toggleMissingDataDebugs));
            
            _updateDataDisplay(DataToDisplay,
                title: "States",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allSubData: States.GetDataToDisplay(toggleMissingDataDebugs));

            return DataToDisplay;
        }

        public override List<ActorActionName> GetAllowedActions()
        {
            var allowedActions = new List<ActorActionName>();
            
            allowedActions.AddRange(States.GetAllowedActions());
            allowedActions.AddRange(Conditions.GetAllowedActions());

            return allowedActions;
        }
    }
}