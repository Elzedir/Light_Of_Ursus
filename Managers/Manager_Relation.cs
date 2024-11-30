using System;
using System.Linq;
using Actor;
using Managers;

public class Manager_Relation
{
    public static float GetRelation(Actor_Component a, Actor_Component b)
    {
        float relation = 0;

        relation += _compareFaction(a.ActorData.ActorFactionID, b.ActorData.ActorFactionID);
        relation += _comparePersonality(a.PersonalityComponent, b.PersonalityComponent);

        return relation;
    }

    static float _compareFaction(uint a, uint b)
    {
        FactionData factionDataA = Manager_Faction.GetFaction(a);

        if (!factionDataA.AllFactionRelations.Any(r => r.FactionID == b)) return 0;

        return factionDataA.AllFactionRelations.FirstOrDefault(r => r.FactionID == b).FactionRelation;
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

[Serializable]
public class FactionRelationData
{
    public int FactionID;
    public string FactionName;
    public int FactionRelation;

    public FactionRelationData(int factionID, string factionName, int factionRelations)
    {
        FactionID = factionID;
        FactionName = factionName;
        FactionRelation = factionRelations;
    }
}

[Serializable]
public class Relation
{

}
