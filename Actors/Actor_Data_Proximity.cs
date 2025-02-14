using System;
using System.Collections.Generic;
using System.Linq;
using Actor;
using ActorActions;
using Faction;
using Inventory;
using Priorities;
using Priority;
using Proximity;
using Tools;
using UnityEngine;

namespace Actors
{
    //* Maybe we can make this more efficent by using the availableActions for the actor, and only getting variables that would be
    //* relevant for the actor. So if he is not in combat, closest enemy would not be needed, and so on. If he doesn't have the medic job,
    //* closest neutral, friend or ally to heal would not be needed, reducing the amount of checks needed.
    public class Actor_Data_Proximity : Priority_Class
    {
        public ComponentReference_Actor ActorReference => Reference as ComponentReference_Actor;
        
        public Actor_Data_Proximity(ulong actorID) : base(actorID, ComponentType.Actor) { }
        
        public Actor_Data_Proximity(Actor_Data_Proximity actorDataProximity) : base(actorDataProximity.ActorReference.ActorID, ComponentType.Actor) { }
        
        Dictionary<ulong, Actor_Component> _proximityActors;
        public Dictionary<ulong, Actor_Component> ProximityActors => _proximityActors ??= _getOrderedProximityActors();
        
        public GameObject ClosestAlly;
        
        public GameObject ClosestEnemy;

        public void PopulateProximityData()
        {
            var encounteredFactions = new Dictionary<ulong, Faction_Data>();
            
            var closestAllyDistance = float.PositiveInfinity;
            var closestEnemyDistance = float.PositiveInfinity;
            
            foreach (var actor in ProximityActors)
            {
                if (actor.Value.ActorID == ActorReference.ActorID) continue;
                
                var distance = Vector3.Distance(ActorReference.Actor_Component.transform.position, actor.Value.transform.position);

                if (!encounteredFactions.ContainsKey(actor.Value.ActorData.ActorFactionID))
                {
                    var factionToAdd = Faction_Manager.GetFaction_Data(actor.Value.ActorData.ActorFactionID);

                    if (factionToAdd is null)
                    {
                        Debug.LogError($"Faction with ID {actor.Value.ActorData.ActorFactionID} not found in Faction_SO.");
                        continue;
                    }
                    
                    encounteredFactions.Add(actor.Value.ActorData.ActorFactionID, factionToAdd);    
                }
                
                var actorFaction = encounteredFactions[actor.Value.ActorData.ActorFactionID];
                
                switch(actorFaction.GetFactionRelationship_Name(actorFaction.FactionID))
                {
                    case FactionRelationshipName.Ally:
                        if (!(distance < closestAllyDistance)) continue;
                    
                        ClosestAlly = actor.Value.gameObject;
                        closestAllyDistance = distance;
                        break;
                    case FactionRelationshipName.Enemy:
                        if (!(distance < closestEnemyDistance)) continue;
                    
                        ClosestEnemy = actor.Value.gameObject;
                        closestEnemyDistance = distance;
                        break;
                    case FactionRelationshipName.Neutral:
                        break;
                    case FactionRelationshipName.Friend:
                        break;
                    case FactionRelationshipName.Hostile:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        Dictionary<ulong, Actor_Component> _getOrderedProximityActors() =>
            Proximity_Manager.S_Proximity_Actors
                .OrderBy(actor => Vector3.Distance(
                    ActorReference.Actor_Component.transform.position,
                    actor.Value.transform.position))
                .ToDictionary(
                    actor => actor.Key,
                    actor => actor.Value);

        
        //* I have to check what this fully does again
        public override List<ActorActionName> GetAllowedActions()
        {
            //* Maybe add some personality specific actions, like lashing out if wrathful, or receding if depressive.
            //* or admiring yourself, or cleaning random items of trash.
            return new List<ActorActionName>();
        }
        
        public override DataToDisplay GetDataToDisplay(bool toggleMissingDataDebugs)
        {
            _updateDataDisplay(DataToDisplay,
                title: "Proximity",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: GetStringData());

            return DataToDisplay;
        }

        public override Dictionary<string, string> GetStringData()
        {
            return new Dictionary<string, string>
            {
                { "Closest Ally", $"{ClosestAlly}" },
                { "Closest Enemy", $"{ClosestEnemy}" }
            };
        }
    }
}