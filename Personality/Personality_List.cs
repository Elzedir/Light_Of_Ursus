using System.Collections.Generic;
using System.Linq;
using Actor;
using UnityEngine;

namespace Personality
{
    public abstract class Personality_List
    {
        public static PersonalityTrait GetPersonalityTrait(PersonalityTraitName traitName)
        {
            if (_allPersonalityTitles.TryGetValue(traitName, out var trait)) return trait;
            
            Debug.LogWarning($"No PersonalityTrait found for {traitName}. Returning null.");
            
            return null;
        }

        public static HashSet<PersonalityTraitName> GetRandomPersonalityTraits(
            HashSet<PersonalityTraitName> existingPersonalityTraits, int numberOfTraitsDesired = 1, SpeciesName speciesName = SpeciesName.Default)
        {
            existingPersonalityTraits ??= new HashSet<PersonalityTraitName>();
            
            for (var i = 0; i < numberOfTraitsDesired; i++)
            {
                var availablePersonalityTraits = _allPersonalityTitles.Keys.Where(traitName =>
                    !existingPersonalityTraits.Contains(traitName)).ToList();

                if (availablePersonalityTraits.Count == 0)
                {
                    Debug.LogWarning("No available personality traits found. Returning existing traits.");
                    break;
                }

                var randomTrait =
                    availablePersonalityTraits[Random.Range(0, availablePersonalityTraits.Count)];

                existingPersonalityTraits.Add(randomTrait);
            }

            return existingPersonalityTraits;
        }

        static readonly Dictionary<PersonalityTraitName, PersonalityTrait> _allPersonalityTitles = new()
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

        public static float GetPersonalityRelation(HashSet<PersonalityTraitName> a, HashSet<PersonalityTraitName> b)
        {
            // Currently VERY open to double adding. Fix later.
            
            var relation = 0f;

            foreach (var traitA in a)
            {
                foreach (var traitB in b)
                {
                    if (_personalityRelations.TryGetValue(traitA, out var relationData) && relationData.traitName == traitB)
                    {
                        relation += relationData.relation;
                    }
                    
                    if (_personalityRelations.TryGetValue(traitB, out relationData) && relationData.traitName == traitA)
                    {
                        relation += relationData.relation;
                    }
                }
            }

            return relation;
        }

        static readonly Dictionary<PersonalityTraitName, (PersonalityTraitName traitName, float relation)> _personalityRelations = new()
        {
            {
                PersonalityTraitName.Arrogant, (PersonalityTraitName.Humble,  -15)    
            },
            {
                PersonalityTraitName.Brave, (PersonalityTraitName.Craven,     -15)
            },
            {
                PersonalityTraitName.Honest, (PersonalityTraitName.Deceitful, -10)
            }
        };
    }
}