using System;
using System.Collections.Generic;
using System.Linq;
using Relationships;
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

        public HashSet<ulong>             AllFactionActorIDs;
        public List<FactionRelationData> AllFactionRelations;

        public Faction_Data(ulong factionID, string factionName, HashSet<ulong> allFactionActorIDs, List<FactionRelationData> allFactionRelations)
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
                allStringData: AllFactionActorIDs.ToDictionary(actorID => $"{actorID}", actorID => $"{actorID}"));
            
            _updateDataDisplay(DataToDisplay,
                title: "Faction Relations",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: AllFactionRelations.ToDictionary(relation => $"{relation.FactionID_B}:", relation => $"{relation.FactionRelation}"));

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
    }
}


