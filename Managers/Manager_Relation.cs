using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Manager_Relation
{
    public static float GetRelation(Actor_Base a, Actor_Base b)
    {
        float relation = 0;

        relation += _compareFaction(a.ActorData.FullIdentification.ActorFaction, b.ActorData.FullIdentification.ActorFaction);
        relation += _comparePersonality(a.PersonalityComponent, b.PersonalityComponent);

        return relation;
    }

    static float _compareFaction(FactionName a, FactionName b)
    {
        Faction_Data_SO factionDataA = Manager_Faction.GetFaction(a);

        if (!factionDataA.FactionData.Any(f => f.FactionName == b)) return 0;

        return factionDataA.FactionData.FirstOrDefault(f => f.FactionName == b).RelationshipValue;
    }

    static float _comparePersonality(PersonalityComponent a, PersonalityComponent b)
    {
        float personalityRelation = 0;

        foreach (PersonalityTrait traitA in a.PersonalityTraits)
        {
            foreach(PersonalityTrait traitB in b.PersonalityTraits)
            {
                personalityRelation += Manager_Personality.ComparePersonalityRelations(traitA.TraitName, traitB.TraitName);
            }
        }

        return personalityRelation;
    }
}

public class Relation
{

}
