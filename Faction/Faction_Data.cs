using System;
using System.Collections.Generic;
using Managers;
using UnityEngine;

namespace Faction
{
    [Serializable]
    public class Faction_Data
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

            if (Manager_Game.FindTransformRecursively(factionsGO.transform, $"{FactionID}: {FactionName}") == null)
            {
                var factionGO = new GameObject($"{FactionID}: {FactionName}");
                factionGO.transform.SetParent(factionsGO.transform);
                factionGO.transform.position = Vector3.zero;
            }
        }

        public void AddToFactionActorIDList(uint actorID) => AllFactionActorIDs.Add(actorID);

        public void RemoveFromFactionActorIDList(uint actorID) => AllFactionActorIDs.Remove(actorID);

        public void ClearActorData()
        {
            AllFactionActorIDs.Clear();
        }
    }
}


