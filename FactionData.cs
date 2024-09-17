using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class FactionData
{
    public int FactionID;
    public string FactionName;

    public HashSet<int> AllFactionActorIDs = new();
    public List<FactionRelationData> AllFactionRelations;

    public FactionData(int factionID, string factionName, HashSet<int> allFactionActorIDs, List<FactionRelationData> allFactionRelations)
    {
        FactionID = factionID;
        FactionName = factionName;
        AllFactionActorIDs = allFactionActorIDs;
        AllFactionRelations = allFactionRelations;
    }

    public void InitialiseFaction()
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

    public void AddToFactionActorIDList(int actorID) => AllFactionActorIDs.Add(actorID);

    public void RemoveFromFactionActorIDList(int actorID) => AllFactionActorIDs.Remove(actorID);

    public void ClearActorData()
    {
        AllFactionActorIDs.Clear();
    }
}


