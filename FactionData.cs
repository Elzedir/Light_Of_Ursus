using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class FactionData
{
    public int FactionID;
    public string FactionName;

    public List<ActorData> AllFactionActors;
    public List<FactionRelationData> AllFactionRelations;

    public FactionData(int factionID, string factionName, List<ActorData> allFactionActors, List<FactionRelationData> allFactionRelations)
    {
        FactionID = factionID;
        FactionName = factionName;
        AllFactionActors = allFactionActors;
        AllFactionRelations = allFactionRelations;
    }

    public void InitialiseFaction()
    {
        foreach (var actorData in AllFactionActors)
        {
            actorData.PrepareForInitialisation();
        }
    }

    public void AddToOrUpdateFactionActorsDataList(ActorData actorData)
    {
        var existingActor = AllFactionActors.FirstOrDefault(a => a.ActorID == actorData.ActorID);

        if (existingActor == null) AllFactionActors.Add(actorData);
        else AllFactionActors[AllFactionActors.IndexOf(existingActor)] = actorData;
    }

    public ActorData GetActorData(int actorID)
    {
        if (!AllFactionActors.Any(a => a.ActorID == actorID)) { Debug.Log($"AllActorData does not contain ActorID: {actorID}"); return null; }

        return AllFactionActors.FirstOrDefault(a => a.ActorID == actorID);
    }

    public void ClearActorData()
    {
        AllFactionActors.Clear();
    }
}
