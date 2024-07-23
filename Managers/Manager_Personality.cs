using System;
using System.Collections;
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

    public static void Initialise()
    {
        AllPersonalityTitles.Clear();
        PersonalityRelations.Clear();

        AllPersonalityTitles.Add(
            PersonalityTraitName.Brave,
            (
            "Brave",
            "brave",
            "bravado"
            )
            );

        
    }

    static void _initialisePersonalityRelations()
    {
        PersonalityRelations.Add((PersonalityTraitName.Arrogant, PersonalityTraitName.Humble), -15);
        PersonalityRelations.Add((PersonalityTraitName.Brave, PersonalityTraitName.Craven), -15);
        PersonalityRelations.Add((PersonalityTraitName.Honest, PersonalityTraitName.Deceitful), -10);
    }

    public static string GetPersonalityTitle()
    {
        return "";
    }

    public static float ComparePersonalityRelations(PersonalityTraitName a, PersonalityTraitName b)
    {
        if (PersonalityRelations.TryGetValue(a < b ? (a, b) : (b, a), out float personalityRelations)) return personalityRelations;

        return 0;
    }
}

public enum PersonalityTraitName { Arrogant, Ambitious, Brave, Craven, Deceitful, Honest, Humble, Just, Sadistic, Savage, Vengeful }

[Serializable]
public class Personality
{
    public string PersonalityTitle;
    public string PersonalityDescription;

    public HashSet<PersonalityTrait> PersonalityTraits = new();

    public Personality(string personalityTitle, string personalityDescription, HashSet<PersonalityTrait> personalityTraits)
    {
        PersonalityTitle = personalityTitle;
        PersonalityDescription = personalityDescription;
        PersonalityTraits = personalityTraits;
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
        if (!PersonalityTraits.Any(t => t.TraitName == traitName)) PersonalityTraits.Add(Manager_PersonalityTrait.GetTrait(traitName));

        return PersonalityTraits.First(t => t.TraitName == traitName);
    }
}

public class Manager_PersonalityTrait
{
    public static List<PersonalityTrait> AllPersonalityTraits = new();

    public static void Initialise()
    {
        AllPersonalityTraits.Clear();

        AllPersonalityTraits.Add(_brave());
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

    public static PersonalityTrait GetTrait(PersonalityTraitName traitName)
    {
        return AllPersonalityTraits.First(t => t.TraitName == traitName);
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
