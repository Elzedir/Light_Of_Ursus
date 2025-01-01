using System;
using System.Collections.Generic;
using System.Linq;
using Managers;
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
        
        protected override Data_Display _getDataSO_Object(bool toggleMissingDataDebugs, ref Data_Display dataSO_Object)
        {
            var dataObjects = dataSO_Object == null
                ? new Dictionary<string, Data_Display>()
                : new Dictionary<string, Data_Display>(dataSO_Object.SubData);
            
            try
            {
                dataObjects["Base Faction Data"] = new Data_Display(
                    title: "Base Faction Data",
                    dataDisplayType: DataDisplayType.Item,
                    dataSO_Object: dataSO_Object,
                    data: new Dictionary<string, string>
                    {
                        { "Faction ID", $"{FactionID}" },
                        { "Faction Name", FactionName }
                    });
            }
            catch
            {
                Debug.LogError("Error in Base Faction Data");
            }
            
            try
            {
                dataObjects["Faction Actors"] = new Data_Display(
                    title: "Faction Actors",
                    dataDisplayType: DataDisplayType.CheckBoxList,
                    dataSO_Object: dataSO_Object,
                    data: AllFactionActorIDs.ToDictionary(actorID => $"{actorID}", actorID => $"{actorID}"));
            }
            catch
            {
                Debug.LogError("Error in Faction Actors");
            }
            
            try
            {
                dataObjects["Faction Relations"] = new Data_Display(
                    title: "Faction Relations",
                    dataDisplayType: DataDisplayType.CheckBoxList,
                    dataSO_Object: dataSO_Object,
                    data: AllFactionRelations.ToDictionary(relation => $"{relation.FactionID_B}:", relation => $"{relation.FactionRelation}"));
            }
            catch
            {
                Debug.LogError("Error in Faction Actors");
            }

            return dataSO_Object = new Data_Display(
                title: "Faction Data",
                dataDisplayType: DataDisplayType.CheckBoxList,
                dataSO_Object: dataSO_Object,
                subData: dataObjects);
        }
    }
}


