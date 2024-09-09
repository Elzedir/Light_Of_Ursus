using System;
using System.Collections.Generic;

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
        // foreach (var actorID in AllFactionActorIDs)
        // {
        //     Manager_Actor.GetActorData(actorID).PrepareForInitialisation();
        // }
    }

    public void AddToFactionActorIDList(int actorID) => AllFactionActorIDs.Add(actorID);

    public void RemoveFromFactionActorIDList(int actorID) => AllFactionActorIDs.Remove(actorID);

    public void ClearActorData()
    {
        AllFactionActorIDs.Clear();
    }
}
