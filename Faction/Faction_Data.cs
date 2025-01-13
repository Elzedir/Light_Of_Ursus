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
        public uint   FactionID;
        public string FactionName;

        public HashSet<uint>             AllFactionActorIDs;
        public List<FactionRelationData> AllFactionRelations;

        public Faction_Data(uint factionID, string factionName, HashSet<uint> allFactionActorIDs, List<FactionRelationData> allFactionRelations)
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
            var factionsGO =  GameObject.Find("Factions");

            if (factionsGO == null) 
            {
                Debug.LogError("No Factions GameObject found.");
                return;
            }
        }
        
        protected override Data_Display _getDataSO_Object(bool toggleMissingDataDebugs, Data_Display dataSO_Object)
        {
            if (dataSO_Object.Data is null && dataSO_Object.SubData is null)
                dataSO_Object = new Data_Display(
                    title: "Faction Data",
                    dataDisplayType: DataDisplayType.List_CheckBox,
                    data: new Dictionary<string, string>());
            
            try
            {
                if (!dataSO_Object.SubData.TryGetValue("Base Faction Data", out var baseFactionData))
                {
                    dataSO_Object.SubData["Base Faction Data"] = new Data_Display(
                        title: "Base Faction Data",
                        dataDisplayType: DataDisplayType.List_Item,
                        data: new Dictionary<string, string>());
                }
                
                if (baseFactionData is not null)
                {
                    baseFactionData.Data = new Dictionary<string, string>
                    {
                        { "Faction ID", $"{FactionID}" },
                        { "Faction Name", FactionName }
                    };
                }
            }
            catch
            {
                Debug.LogError("Error in Base Faction Data");
            }
            
            try
            {
                if (!dataSO_Object.SubData.TryGetValue("Faction Actors", out var factionActors))
                {
                    dataSO_Object.SubData["Faction Actors"] = new Data_Display(
                        title: "Faction Actors",
                        dataDisplayType: DataDisplayType.List_CheckBox,
                        data: new Dictionary<string, string>());
                }
                
                if (factionActors is not null)
                {
                    factionActors.Data = AllFactionActorIDs.ToDictionary(actorID => $"{actorID}", actorID => $"{actorID}");
                }
            }
            catch
            {
                Debug.LogError("Error in Faction Actors");
            }
            
            try
            {
                if (!dataSO_Object.SubData.TryGetValue("Faction Relations", out var factionRelations))
                {
                    dataSO_Object.SubData["Faction Relations"] = new Data_Display(
                        title: "Faction Relations",
                        dataDisplayType: DataDisplayType.List_CheckBox,
                        data: new Dictionary<string, string>());
                }
                
                if (factionRelations is not null)
                {
                    factionRelations.Data = AllFactionRelations.ToDictionary(relation => $"{relation.FactionID_B}:", relation => $"{relation.FactionRelation}");
                }
            }
            catch
            {
                Debug.LogError("Error in Faction Actors");
            }

            return dataSO_Object;
        }
    }
}


