using System;
using System.Collections.Generic;
using System.Linq;
using Tools;
using UnityEngine;

namespace Faction
{
    [Serializable]
    public class Faction_Data : Data_Class
    {
        public ulong   FactionID;
        public string FactionName;
        
        Faction_Component _faction_Component;
        public Faction_Component Faction_Component =>
            _faction_Component ??= Faction_Manager.GetFaction_Component(FactionID);

        public List<ulong>             AllFactionActorIDs;
        public Dictionary<ulong, float> AllFactionRelations;

        public Faction_Data(ulong factionID, string factionName, List<ulong> allFactionActorIDs, Dictionary<ulong, float> allFactionRelations)
        {
            FactionID           = factionID;
            FactionName         = factionName;
            AllFactionActorIDs  = allFactionActorIDs;
            AllFactionRelations = allFactionRelations;
        }
        
        public Faction_Data(Faction_Data factionData)
        {
            FactionID           = factionData.FactionID;
            FactionName         = factionData.FactionName;
            AllFactionActorIDs  = factionData.AllFactionActorIDs;
            AllFactionRelations = factionData.AllFactionRelations;
        }

        public void InitialiseFactionData()
        {
            _faction_Component = Faction_Manager.GetFaction_Component(FactionID);

            if (_faction_Component is not null) return;
            
            Debug.LogWarning($"Faction with ID {FactionID} not found in Faction_SO.");
        }
        
        public override DataToDisplay GetDataToDisplay(bool toggleMissingDataDebugs)
        {
            _updateDataDisplay(DataToDisplay,
                title: "Base Faction Data",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: GetStringData());
            
            _updateDataDisplay(DataToDisplay,
                title: "Faction Actors",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: AllFactionActorIDs.ToDictionary(
                    actorID => $"{actorID}", 
                    actorID => $"{actorID}"));

            _updateDataDisplay(DataToDisplay,
                title: "Faction Relations",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: AllFactionRelations.ToDictionary(
                    faction => $"{faction.Key}:",
                    relation => $"{relation.Value}"));

            return DataToDisplay;
        }

        public override Dictionary<string, string> GetStringData()
        {
            return new Dictionary<string, string>
            {
                { "Faction ID", $"{FactionID}" },
                { "Faction Name", FactionName }
            };
        }
        
        public float GetFactionRelationship_Value(ulong factionID)
        {
            if (AllFactionRelations.TryGetValue(factionID, out var relationshipValue)) return relationshipValue;
            
            Debug.LogError($"Faction with ID {factionID} not found in AllFactionRelations for Faction {FactionName}.");
            return 0;
        }
        
        public FactionRelationshipName GetFactionRelationship_Name(ulong factionID)
        {
            if (factionID == FactionID) return FactionRelationshipName.Ally;

            if (AllFactionRelations.TryGetValue(factionID, out var relationshipValue))
                return relationshipValue switch
                {
                    > 75 => FactionRelationshipName.Ally,
                    > 25 => FactionRelationshipName.Friend,
                    > -25 => FactionRelationshipName.Neutral,
                    > -75 => FactionRelationshipName.Hostile,
                    _ => FactionRelationshipName.Enemy
                };
            
            Debug.LogError($"Faction with ID {factionID} not found in AllFactionRelations for Faction {FactionName}.");
            return FactionRelationshipName.Neutral;
        }
    }
}


