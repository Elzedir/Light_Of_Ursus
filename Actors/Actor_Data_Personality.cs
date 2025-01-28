using System;
using System.Collections.Generic;
using System.Linq;
using ActorActions;
using Inventory;
using Personality;
using Priority;
using Tools;

namespace Actors
{
    [Serializable]
    public class Actor_Data_Personality : Priority_Class
    {
        public ComponentReference_Actor ActorReference => Reference as ComponentReference_Actor;
        
        public Actor_Data_Personality(ulong actorID, List<PersonalityTraitName> actorPersonality, SpeciesName actorSpecies) : base(
            actorID, ComponentType.Actor)
        {
            PersonalityTraits = actorPersonality ?? Personality_Manager.GetRandomPersonalityTraits(null, 3, actorSpecies);
        }

        public Actor_Data_Personality(Actor_Data_Personality actorDataPersonality) : base(
            actorDataPersonality.ActorReference.ActorID, ComponentType.Actor)
        {
            PersonalityTitle       = actorDataPersonality.PersonalityTitle;
            PersonalityDescription = actorDataPersonality.PersonalityDescription;
            PersonalityTraits      = actorDataPersonality.PersonalityTraits;
        }
        
        public string                        PersonalityTitle;
        public string                        PersonalityDescription;
        // HashSets can't be serialised, change. Maybe save list and load a hashset so they can't add multiple of the same trait
        // from the save files.
        List<PersonalityTraitName> _personalityTraits;
        public List<PersonalityTraitName> PersonalityTraits;

        public Dictionary<string, string> SubData => new()
        {
            { "Personality Title", $"{PersonalityTitle}" },
            { "PersonalityDescription", $"{PersonalityDescription}" }
        };
        
        public float ComparePersonality(Actor_Data_Personality otherDataPersonality)
        {
            //* Current system allows double adding since it's an additive system, rather than a replacement system. Make it a
            //* Dictionary with a list of every possible type of relationship value, and then we total it, to make sure it can't go past its maximum,
            //* double add, or go below its minimum.
            
            var relation = 0f;

            foreach (var traitA in PersonalityTraits)
            {
                foreach (var traitB in otherDataPersonality.PersonalityTraits)
                {
                    if (Personality_List.PersonalityRelations.TryGetValue(traitA, out var relationData) && relationData.traitName == traitB)
                    {
                        relation += relationData.relation;
                    }
                    
                    if (Personality_List.PersonalityRelations.TryGetValue(traitB, out relationData) && relationData.traitName == traitA)
                    {
                        relation += relationData.relation;
                    }
                }
            }

            return relation;
        }
        
        public override List<ActorActionName> GetAllowedActions()
        {
            //* Maybe add some personality specific actions, like lashing out if wrathful, or receding if depressive.
            //* or admiring yourself, or cleaning random items of trash.
            return new List<ActorActionName>();
        }
        
        public override DataToDisplay GetDataToDisplay(bool toggleMissingDataDebugs)
        {
            _updateDataDisplay(DataToDisplay,
                title: "Personality",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: GetStringData());
            
            _updateDataDisplay(DataToDisplay,
                title: "Faction Relations",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: PersonalityTraits.ToDictionary(
                    trait => $"Personality Trait: {trait}",
                    trait => ""));

            return DataToDisplay;
        }

        public override Dictionary<string, string> GetStringData()
        {
            return new Dictionary<string, string>
            {
                { "Personality Title", $"{PersonalityTitle}" },
                { "Personality Description", $"{PersonalityDescription}" },
                { "Personality Traits", $"{PersonalityTraits.Count}" }
            };
        }
    }
}