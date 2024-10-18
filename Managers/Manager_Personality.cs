using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Personae score adds up the score of all decisions you have made, and gives you a personae title from it.


public enum UniquePersonalities
{
    Bleeding_Heart
}

public class Manager_Personality
{
    public static Dictionary<PersonalityTraitName, (string Prefix, string Infix, string Suffix)> AllPersonalityTitles = new();
    public static Dictionary<(PersonalityTraitName, PersonalityTraitName), float> PersonalityRelations = new();
    public static HashSet<PersonalityTrait> AllPersonalityTraits = new();

    public static void Initialise()
    {
        _initialisePersonalityTraits();

        _initialisePersonalityTitles();

        _initialisePersonalityRelations();
    }

    static void _initialisePersonalityTraits()
    {
        AllPersonalityTraits.Add(_brave());
    }

    static void _initialisePersonalityTitles()
    {
        AllPersonalityTitles.Add(
            PersonalityTraitName.Brave,
            (
            "Brave",
            "brave",
            "bravado"
            )
            );
    }

    static PersonalityTrait _brave()
    {
        return new PersonalityTrait(
            traitName: PersonalityTraitName.Brave,
            traitDescription: "Brave",
            traitDisplayed: false,
            traitEffects: new List<Effect>
            {

            }
            );
    }

    static void _initialisePersonalityRelations()
    {
        PersonalityRelations.Add((PersonalityTraitName.Arrogant, PersonalityTraitName.Humble), -15);
        PersonalityRelations.Add((PersonalityTraitName.Brave, PersonalityTraitName.Craven), -15);
        PersonalityRelations.Add((PersonalityTraitName.Honest, PersonalityTraitName.Deceitful), -10);
    }

    public static PersonalityTrait GetTrait(PersonalityTraitName traitName)
    {
        return AllPersonalityTraits.FirstOrDefault(t => t.TraitName == traitName);
    }

    public static (string Title, string Description) GetPersonalityTitleAndDescription(PersonalityComponent personality)
    {
        return ("Test personality title", "This is a description of a test personality title");
    }

    public static float ComparePersonalityRelations(PersonalityTraitName a, PersonalityTraitName b)
    {
        if (PersonalityRelations.TryGetValue(a < b ? (a, b) : (b, a), out float personalityRelations)) return personalityRelations;

        return 0;
    }

    public static HashSet<PersonalityTraitName> GetRandomPersonalityTraits(HashSet<PersonalityTraitName> existingPersonalityTraits, int numberOfTraits = 1)
    {
        existingPersonalityTraits ??= new HashSet<PersonalityTraitName>();
        HashSet<PersonalityTraitName> selectedPersonalityTraits = new HashSet<PersonalityTraitName>();

        for (int i = 0; i < numberOfTraits; i++)
        {
            var availablePersonalityTraits = AllPersonalityTraits.Where(p => !existingPersonalityTraits.Contains(p.TraitName) && !selectedPersonalityTraits.Contains(p.TraitName)).ToList();

            if (availablePersonalityTraits.Count == 0) break; 

            var randomTrait = availablePersonalityTraits[UnityEngine.Random.Range(0, availablePersonalityTraits.Count)];
            selectedPersonalityTraits.Add(randomTrait.TraitName);
        }

        return selectedPersonalityTraits;
    }
}

public enum PersonalityTraitName { Arrogant, Ambitious, Brave, Craven, Deceitful, Honest, Humble, Just, Sadistic, Savage, Vengeful }

public class PersonalityComponent
{
    public uint ActorID;
    ActorComponent _actor;
    public ActorComponent Actor { get => _actor ??= Manager_Actor.GetActor(ActorID); }
    public string PersonalityTitle;
    public string PersonalityDescription;

    public HashSet<PersonalityTrait> PersonalityTraits = new();

    public PersonalityComponent(uint actorID) => ActorID = actorID;

    public void SetPersonalityTraits(HashSet<PersonalityTrait> personalityTraits)
    {
        PersonalityTraits = personalityTraits;

        _setPersonalityTitle();
    }

    void _setPersonalityTitle()
    {
        (PersonalityTitle, PersonalityDescription) = Manager_Personality.GetPersonalityTitleAndDescription(this);

        Actor.ActorData.SpeciesAndPersonality.ActorPersonality.SetPersonalityTitle(PersonalityTitle, PersonalityDescription);
    }

    public void AddToPersonalityScore(PersonalityTraitName traitName, float score)
    {
        _traitCheck(traitName).AddToTraitScore(score);
    }

    public void DisplayTrait(PersonalityTraitName traitName)
    {
        _traitCheck(traitName).DisplayTrait();
    }

    public void HideTrait(PersonalityTraitName traitName)
    {
        _traitCheck(traitName).HideTrait();
    }

    PersonalityTrait _traitCheck(PersonalityTraitName traitName)
    {
        if (!PersonalityTraits.Any(t => t.TraitName == traitName)) PersonalityTraits.Add(Manager_Personality.GetTrait(traitName));

        return PersonalityTraits.First(t => t.TraitName == traitName);
    }
}

[Serializable]
public class ActorPersonality
{
    public string PersonalityTitle;
    public string PersonalityDescription;
    public HashSet<PersonalityTraitName> PersonalityTraits = new();

    public ActorPersonality(HashSet<PersonalityTraitName> personalityTraits)
    {
        PersonalityTraits = personalityTraits ?? new HashSet<PersonalityTraitName>();
    }

    public HashSet<PersonalityTrait> GetPersonality()
    {
        if (PersonalityTraits == null) return null;

        HashSet<PersonalityTrait> personality = new();

        foreach(PersonalityTraitName traitName in PersonalityTraits)
        {
            personality.Add(Manager_Personality.GetTrait(traitName));
        }

        return personality;
    }

    public void SetPersonalityTitle(string personalityTitle, string personalityDescription)
    {
        PersonalityTitle = personalityTitle;
        PersonalityDescription = personalityDescription;
    }
}

[Serializable]
public class PersonalityTrait
{
    public PersonalityTraitName TraitName;
    public string TraitDescription;

    [SerializeField] bool _traitDisplayed;
    [SerializeField] float _traitScore;

    public List<Effect> TraitEffects = new();

    public Sprite PersonalityIcon;

    public PersonalityTrait(PersonalityTraitName traitName, string traitDescription, bool traitDisplayed, List<Effect> traitEffects)
    {
        TraitName = traitName;
        TraitDescription = traitDescription;
        _traitDisplayed = traitDisplayed;

        _traitScore = 0;

        TraitEffects = traitEffects;
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

public class Effect
{
    public string Name;
    public string Description;
}
