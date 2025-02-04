using System;
using System.Collections.Generic;
using System.Linq;
using Actors;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Personality
{
    // Personae score adds up the score of all decisions you have made, and gives you a personae title from it.
    
    // Maybe rename to persona, or personae.

    public abstract class Personality_Manager
    {
        public static List<PersonalityTraitName> GetRandomPersonalityTraits(
            List<PersonalityTraitName> existingPersonalityTraits, int numberOfTraitsDesired = 1, SpeciesName speciesName = SpeciesName.Default)
        {
            existingPersonalityTraits ??= new List<PersonalityTraitName>();
            
            for (var i = 0; i < numberOfTraitsDesired; i++)
            {
                var availablePersonalityTraits = Personality_List.PersonalityTraits.Keys.Where(traitName =>
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
    }

    [Serializable]
    public class PersonalityTrait
    {
        public PersonalityTraitName TraitName;
        public PersonalityAffixName            PersonalityAffixName;
        public string               TraitDescription;

        [SerializeField] bool  _traitDisplayed;
        [SerializeField] float _traitScore;

        public List<PersonalityEffect> PersonalityTraitEffects;

        public Sprite PersonalityIcon;

        public PersonalityTrait(PersonalityTraitName traitName, PersonalityAffixName personalityAffixName, string traitDescription, bool traitDisplayed, List<PersonalityEffect> personalityTraitEffects)
        {
            TraitName        = traitName;
            PersonalityAffixName = personalityAffixName;
            TraitDescription = traitDescription;
            _traitDisplayed  = traitDisplayed;

            _traitScore = 0;

            PersonalityTraitEffects = personalityTraitEffects;
        }

        public void AddToTraitScore(float score)
        {
            _traitScore += score;
        }

        public void DisplayTrait()
        {
            _traitDisplayed = true;
        }

        public void HideTrait()
        {
            _traitDisplayed = false;
        }
    }

    public class PersonalityAffixName
    {
        public string Prefix;
        public string Infix;
        public string Suffix;
        
        public PersonalityAffixName(string prefix = "", string infix = "", string suffix = "")
        {
            Prefix = prefix;
            Infix  = infix;
            Suffix = suffix;
        }
    }

    public class PersonalityEffect
    {
        public string Name;
        public string Description;
        
        public PersonalityEffect(string name, string description)
        {
            Name        = name;
            Description = description;
        }
    }

    public enum PersonalityTraitName
    {
        None,
        
        Arrogant, 
        Ambitious, 
        Brave, 
        Craven, 
        Deceitful, 
        Honest, 
        Humble, 
        Just, 
        Impatient, 
        Patient, 
        Sadistic, 
        Savage, 
        Vengeful
    }
    
    public enum UniquePersonalities
    {
        None,
        
        Bleeding_Heart
    }
}