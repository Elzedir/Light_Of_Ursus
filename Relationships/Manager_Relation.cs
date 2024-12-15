using System;
using System.Linq;
using Actor;
using Faction;
using Personality;
using UnityEngine.Serialization;

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

        if (!factionDataA.AllFactionRelations.Any(r => r.FactionID_A == b)) return 0;

        return factionDataA.AllFactionRelations.FirstOrDefault(r => r.FactionID_A == b).FactionRelation;
    }

    static float _comparePersonality(ActorPersonality a, ActorPersonality b)
    {
        return Personality_List.GetPersonalityRelation(a.PersonalityTraits, b.PersonalityTraits);
    }
}

[Serializable]
public class FactionRelationData
{
    public uint FactionID_A;
    public uint FactionID_B;
    public int  FactionRelation;

    public FactionRelationData(uint factionID_A, uint factionID_B, int factionRelations)
    {
        FactionID_A      = factionID_A;
        FactionRelation  = factionRelations;
        FactionID_B = factionID_B;
    }
}

[Serializable]
public class Relation
{

}
