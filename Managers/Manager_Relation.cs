using System;
using System.Linq;
using Actor;
using Faction;
using Personality;

public class Manager_Relation
{
    public static float GetRelation(Actor_Component a, Actor_Component b)
    {
        float relation = 0;

        relation += _compareFaction(a.ActorData.ActorFactionID, b.ActorData.ActorFactionID);
        relation += _comparePersonality(a.ActorData.SpeciesAndPersonality.ActorPersonality, b.ActorData.SpeciesAndPersonality.ActorPersonality);

        return relation;
    }

    static float _compareFaction(uint a, uint b)
    {
        Faction_Data factionDataA = Manager_Faction.GetFaction_Data(a);

        if (!factionDataA.AllFactionRelations.Any(r => r.FactionID == b)) return 0;

        return factionDataA.AllFactionRelations.FirstOrDefault(r => r.FactionID == b).FactionRelation;
    }

    static float _comparePersonality(ActorPersonality a, ActorPersonality b)
    {
        return Personality_List.GetPersonalityRelation(a.PersonalityTraits, b.PersonalityTraits);
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
