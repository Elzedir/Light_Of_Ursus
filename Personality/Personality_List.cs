using System.Collections.Generic;
using System.Linq;
using Actor;
using UnityEngine;

namespace Personality
{
    public abstract class Personality_List
    {
        static Dictionary<PersonalityTraitName, PersonalityTrait> _personalityTraits;
        public static Dictionary<PersonalityTraitName, PersonalityTrait> PersonalityTraits => _personalityTraits ??= _initialisePersonalityTitles();
        
        public static PersonalityTrait GetPersonalityTrait(PersonalityTraitName traitName)
        {
            if (PersonalityTraits.TryGetValue(traitName, out var trait)) return trait;
            
            Debug.LogWarning($"No PersonalityTrait found for {traitName}. Returning null.");
            
            return null;
        }

        static Dictionary<PersonalityTraitName, PersonalityTrait> _initialisePersonalityTitles()
        {
            return new Dictionary<PersonalityTraitName, PersonalityTrait>
            {
                {
                    PersonalityTraitName.Brave, new PersonalityTrait(
                        traitName: PersonalityTraitName.Brave,
                        personalityAffixName: new PersonalityAffixName
                        (
                            prefix: "Brave",
                            infix: "brave",
                            suffix: "bravado"
                        ),
                        traitDescription: "Stupidity... Or?",
                        traitDisplayed: false,
                        personalityTraitEffects: new List<PersonalityEffect>())
                },
                {
                    PersonalityTraitName.Craven, new PersonalityTrait(
                        traitName: PersonalityTraitName.Craven,
                        personalityAffixName: new PersonalityAffixName
                        (
                            prefix: "Craven",
                            infix: "craven",
                            suffix: "cravenly"
                        ),
                        traitDescription: "Cowardly... Or?",
                        traitDisplayed: false,
                        personalityTraitEffects: new List<PersonalityEffect>())
                },
                {
                    PersonalityTraitName.Honest, new PersonalityTrait(
                        traitName: PersonalityTraitName.Honest,
                        personalityAffixName: new PersonalityAffixName
                        (
                            prefix: "Honest",
                            infix: "honest",
                            suffix: "honestly"
                        ),
                        traitDescription: "Honest... Or?",
                        traitDisplayed: false,
                        personalityTraitEffects: new List<PersonalityEffect>())
                },
                {
                    PersonalityTraitName.Deceitful, new PersonalityTrait(
                        traitName: PersonalityTraitName.Deceitful,
                        personalityAffixName: new PersonalityAffixName
                        (
                            prefix: "Deceitful",
                            infix: "deceitful",
                            suffix: "deceitfully"
                        ),
                        traitDescription: "Deceitful... Or?",
                        traitDisplayed: false,
                        personalityTraitEffects: new List<PersonalityEffect>())
                },
                {
                    PersonalityTraitName.Arrogant, new PersonalityTrait(
                        traitName: PersonalityTraitName.Arrogant,
                        personalityAffixName: new PersonalityAffixName
                        (
                            prefix: "Arrogant",
                            infix: "arrogant",
                            suffix: "arrogantly"
                        ),
                        traitDescription: "Arrogant... Or?",
                        traitDisplayed: false,
                        personalityTraitEffects: new List<PersonalityEffect>())
                },
                {
                    PersonalityTraitName.Humble, new PersonalityTrait(
                        traitName: PersonalityTraitName.Humble,
                        personalityAffixName: new PersonalityAffixName
                        (
                            prefix: "Humble",
                            infix: "humble",
                            suffix: "humbly"
                        ),
                        traitDescription: "Humble... Or?",
                        traitDisplayed: false,
                        personalityTraitEffects: new List<PersonalityEffect>())
                }
            };
        }

        static Dictionary<PersonalityTraitName, (PersonalityTraitName traitName, float relation)> _personalityRelations;

        public static Dictionary<PersonalityTraitName, (PersonalityTraitName traitName, float relation)>
            PersonalityRelations => _personalityRelations ??= _initialisePersonalityRelations();
        

        static Dictionary<PersonalityTraitName, (PersonalityTraitName traitName, float relation)> _initialisePersonalityRelations()
        {
            return new Dictionary<PersonalityTraitName, (PersonalityTraitName traitName, float relation)>
            {
                {
                    PersonalityTraitName.Arrogant, (PersonalityTraitName.Humble, -15)
                },
                {
                    PersonalityTraitName.Brave, (PersonalityTraitName.Craven, -15)
                },
                {
                    PersonalityTraitName.Honest, (PersonalityTraitName.Deceitful, -10)
                }
            };
        }
    }
}