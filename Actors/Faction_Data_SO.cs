using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FactionName
{
    Passive,
    Player,
    Bandit,
    Demon,
    Human,
    Orc
}

public enum Relationship
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
    public FactionName FactionName;
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
    public FactionName FactionName;
    public Relationship Relationship;
    [SerializeField] private float _relationshipValue; public float RelationshipValue { get { return _relationshipValue; } set { _relationshipValue = value; CheckRelationship(); } }

    public void CheckRelationship()
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
            ? Relationship.Ally
            : _relationshipValue > 25
            ? Relationship.Friend
            : _relationshipValue > -25
            ? Relationship.Neutral
            : _relationshipValue > -75
            ? Relationship.Hostile
            : Relationship.Enemy;
    }
}
