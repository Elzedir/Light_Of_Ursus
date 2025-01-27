using System;
using System.Linq;
using Actor;
using Faction;
using Personality;
using UnityEngine;

namespace Relationships
{
    public class Manager_Relation
    {
        public static float GetRelation(Actor_Component a, Actor_Component b)
        {
            float relation = 0;

            relation += _compareFaction(a.ActorData.ActorFactionID, b.ActorData.ActorFactionID);
            relation += _comparePersonality(a.ActorData.SpeciesAndPersonality.ActorPersonality, b.ActorData.SpeciesAndPersonality.ActorPersonality);

            return relation;
        }

        static float _compareFaction(ulong a, ulong b)
        {
            Faction_Data factionDataA = Faction_Manager.GetFaction_Data(a);

            if (!factionDataA.AllFactionRelations.Any(r => r.FactionID_A == b)) return 0;

            return factionDataA.AllFactionRelations.FirstOrDefault(r => r.FactionID_A == b).FactionRelation;
        }

        static float _comparePersonality(ActorPersonality a, ActorPersonality b)
        {
            return Personality_Manager.GetPersonalityRelation(a.PersonalityTraits, b.PersonalityTraits);
        }
    }

    [Serializable]
    public class FactionRelationData
    {
        public ulong FactionID_A;
        public ulong FactionID_B;
        public int  FactionRelation;

        public FactionRelationData(ulong factionID_A, ulong factionID_B, int factionRelations)
        {
            FactionID_A     = factionID_A;
            FactionRelation = factionRelations;
            FactionID_B     = factionID_B;
        }
    }

    [Serializable]
    public class Relation
    {

    }
    
    [Serializable]
    public class FactionRelationship
    {
        public                   FactionName               FactionName;
        public                   FactionRelationshipStatus Relationship;
        [SerializeField] float                     _relationshipValue; public float RelationshipValue { get { return _relationshipValue; } set { _relationshipValue = value; RefreshRelationship(); } }

        public void RefreshRelationship()
        {
            if (_relationshipValue > 100)
            {
                _relationshipValue = 100;
            }
            else if (_relationshipValue < -100)
            {
                _relationshipValue = -100;
            }

            Relationship = _relationshipValue > 75
                ? FactionRelationshipStatus.Ally
                : _relationshipValue > 25
                    ? FactionRelationshipStatus.Friend
                    : _relationshipValue > -25
                        ? FactionRelationshipStatus.Neutral
                        : _relationshipValue > -75
                            ? FactionRelationshipStatus.Hostile
                            : FactionRelationshipStatus.Enemy;
        }
    }
    
    public enum FactionName
    {
        Passive,
        Player,
        Bandit,
        Demon,
        Human,
        Orc
    }

    public enum FactionRelationshipStatus
    {
        Neutral,
        Ally,
        Friend,
        Hostile,
        Enemy
    }
}