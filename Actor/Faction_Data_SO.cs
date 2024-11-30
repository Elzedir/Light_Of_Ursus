using System;
using System.Collections.Generic;
using UnityEngine;

namespace Actor
{
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

    [CreateAssetMenu(fileName = "New Faction", menuName = "Factions/New Faction Data")]

    public class Faction_Data_SO : ScriptableObject
    {
        // To delete all

        public FactionName               FactionName;
        public List<FactionRelationship> FactionData = new();

        public void SetRelationshipValue(Faction_Data_SO interactedFaction, float relationshipValue = 0)
        {
            FactionRelationship relationship = FactionData.Find(x => x.FactionName == interactedFaction.FactionName);

            if (relationship == null)
            {
                relationship = new FactionRelationship { FactionName = interactedFaction.FactionName };
                FactionData.Add(relationship);
            }

            relationship.RelationshipValue = relationshipValue;
        }

        public void AdjustRelationshipValue(Faction_Data_SO interactedFaction, float relationshipValue = 0)
        {
            FactionRelationship relationship = FactionData.Find(x => x.FactionName == interactedFaction.FactionName);
            if (relationship != null)
            {
                relationship.RelationshipValue += relationshipValue;
            }
        }

        public bool CanAttack(Faction_Data_SO interactedFaction)
        {
            FactionRelationship relationship = FactionData.Find(x => x.FactionName == interactedFaction.FactionName);

            if (relationship != null)
            {
                return relationship.RelationshipValue < -25;
            }

            return false;
        }
    }

    [Serializable]
    public class FactionRelationship
    {
        public                   FactionName               FactionName;
        public                   FactionRelationshipStatus Relationship;
        [SerializeField] private float                     _relationshipValue; public float RelationshipValue { get { return _relationshipValue; } set { _relationshipValue = value; RefreshRelationship(); } }

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
}