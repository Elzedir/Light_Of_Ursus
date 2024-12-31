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
        
        protected override Data_Display _getDataSO_Object(bool toggleMissingDataDebugs)
        {
            var dataObjects = new List<Data_Display>();
            
            try
            {
                dataObjects.Add(new Data_Display(
                    title: "Base Faction Data",
                    dataDisplayType: DataDisplayType.Item,
                    data: new List<string>
                    {
                        $"Faction ID: {FactionID}",
                        $"Faction Name: {FactionName}",
                    }));
            }
            catch
            {
                Debug.LogError("Error in Base Faction Data");
            }
            
            try
            {
                dataObjects.Add(new Data_Display(
                    title: "Faction Actors",
                    dataDisplayType: DataDisplayType.CheckBoxList,
                    data: AllFactionActorIDs.Select(actorID => actorID.ToString()).ToList()));
            }
            catch
            {
                Debug.LogError("Error in Faction Actors");
            }
            
            try
            {
                dataObjects.Add(new Data_Display(
                    title: "Faction Relations",
                    dataDisplayType: DataDisplayType.CheckBoxList,
                    data: AllFactionRelations.Select(relation => $"{relation.FactionID_B}: {relation.FactionRelation}").ToList()));
            }
            catch
            {
                Debug.LogError("Error in Faction Actors");
            }

            return new Data_Display(
                title: "Faction Data",
                dataDisplayType: DataDisplayType.CheckBoxList,
                subData: new List<Data_Display>(dataObjects),
                selectedIndex: dataSO_Object?.SelectedIndex ?? -1,
                showData: dataSO_Object?.ShowData           ?? false);
        }
    }
}


