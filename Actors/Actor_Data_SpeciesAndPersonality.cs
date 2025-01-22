using System;
using System.Collections.Generic;
using System.Linq;
using ActorAction;
using Inventory;
using Personality;
using Priority;
using Tools;

namespace Actor
{
    [Serializable]
    public class Actor_Data_SpeciesAndPersonality : Priority_Updater
    {
        public Actor_Data_SpeciesAndPersonality(uint actorID, SpeciesName actorSpecies, ActorPersonality actorPersonality) : base(
            actorID, ComponentType.Actor)
        {
            ActorSpecies = actorSpecies;
            ActorPersonality = actorPersonality ??
                               new ActorPersonality(
                                   Personality_Manager.GetRandomPersonalityTraits(null, 3, ActorSpecies));
        }

        public Actor_Data_SpeciesAndPersonality(Actor_Data_SpeciesAndPersonality actorDataSpeciesAndPersonality) : base(
            actorDataSpeciesAndPersonality.ActorReference.ActorID, ComponentType.Actor)
        {
            ActorSpecies = actorDataSpeciesAndPersonality.ActorSpecies;
            ActorPersonality = new ActorPersonality(actorDataSpeciesAndPersonality.ActorPersonality);
        }
        
        public override List<ActorActionName> GetAllowedActions()
        {
            //* Maybe add some species specific actions, like cats grooming themselves or lizards suntanning.
            //* And personality actions, like admiring yourself, or cleaning random items of trash.
            return new List<ActorActionName>();
        }
        
        public override DataToDisplay GetDataToDisplay(bool toggleMissingDataDebugs)
        {
            _updateDataDisplay(DataToDisplay,
                title: "Species And Personality",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: GetStringData());

            return DataToDisplay;
        }

        public override Dictionary<string, string> GetStringData()
        {
            var speciesData = new Dictionary<string, string>
            {
                { "Actor Species", $"{ActorSpecies}" }
            };

            var personalityData = ActorPersonality.SubData;

            return speciesData.Concat(personalityData).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public ComponentReference_Actor ActorReference => Reference as ComponentReference_Actor;

        public SpeciesName ActorSpecies;
        public void SetSpecies(SpeciesName speciesName) => ActorSpecies = speciesName;
        public ActorPersonality ActorPersonality;
        public void SetPersonality(ActorPersonality actorPersonality) => ActorPersonality = actorPersonality;

        protected override bool _priorityChangeNeeded(object dataChanged)
        {
            return false;
        }

        protected override Dictionary<PriorityUpdateTrigger, Dictionary<PriorityParameterName, object>>
            _priorityParameterList { get; set; } = new();
    }
    
    public enum SpeciesName
    {
        Default,
        Demon,
        Human,
        Orc
    }
}