using System;
using System.Collections.Generic;
using Actor;
using UnityEngine;

namespace Personality
{
    // Personae score adds up the score of all decisions you have made, and gives you a personae title from it.
    
    // Maybe rename to persona, or personae.

    public abstract class Personality_Manager
    {
        public static HashSet<PersonalityTraitName> GetRandomPersonalityTraits(
            HashSet<PersonalityTraitName> existingPersonalityTraits, int numberOfTraitsDesired = 1,
            SpeciesName                   speciesName = SpeciesName.Default)
        {
            return Personality_List.GetRandomPersonalityTraits(existingPersonalityTraits, numberOfTraitsDesired, speciesName);
        }
    }

    [Serializable]
    public class ActorPersonality
    {
        public string                        PersonalityTitle;
        public string                        PersonalityDescription;
        // HashSets can't be serialised, change. Maybe save list and load a hashset so they can't add multiple of the same trait
        // from the save files.
        List<PersonalityTraitName> _personalityTraits;
        public HashSet<PersonalityTraitName> PersonalityTraits;

        public ActorPersonality(HashSet<PersonalityTraitName> personalityTraits)
        {
            PersonalityTraits = personalityTraits ?? Personality_Manager.GetRandomPersonalityTraits(null, 3);
        }
        
        public ActorPersonality(ActorPersonality actorPersonality)
        {
            PersonalityTitle       = actorPersonality.PersonalityTitle;
            PersonalityDescription = actorPersonality.PersonalityDescription;
            PersonalityTraits      = actorPersonality.PersonalityTraits;
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
        Bleeding_Heart
    }
}