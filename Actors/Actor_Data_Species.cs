using System;
using System.Collections.Generic;
using ActorActions;
using Inventory;
using Pathfinding;
using Priorities;
using Species;
using Tools;

namespace Actors
{
    [Serializable]
    public class Actor_Data_Species : Priority_Class
    {
        public ComponentReference_Actor ActorReference => Reference as ComponentReference_Actor;

        public SpeciesName ActorSpecies;
        
        public Actor_Data_Species(ulong actorID, SpeciesName actorSpecies) : base(
            actorID, ComponentType.Actor)
        {
            ActorSpecies = actorSpecies;
        }

        public Actor_Data_Species(Actor_Data_Species actorDataSpecies) : base(
            actorDataSpecies.ActorReference.ActorID, ComponentType.Actor)
        {
            ActorSpecies = actorDataSpecies.ActorSpecies;
        }
        
        public override List<ActorActionName> GetAllowedActions()
        {
            //* Maybe add some species specific actions, like cats grooming themselves or lizards suntanning.
            return new List<ActorActionName>();
        }
        
        public List<MoverType> GetMoverTypes() => Species_List.GetSpeciesData(ActorSpecies).MoverTypes;
        
        public override DataToDisplay GetDataToDisplay(bool toggleMissingDataDebugs)
        {
            _updateDataDisplay(DataToDisplay,
                title: "Species",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: GetStringData());

            return DataToDisplay;
        }

        public override Dictionary<string, string> GetStringData()
        {
            return new Dictionary<string, string>
            {
                { "Actor Species", $"{ActorSpecies}" }
            };
        }
    }
}