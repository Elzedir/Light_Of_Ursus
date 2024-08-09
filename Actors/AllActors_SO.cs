using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "AllActors_SO", menuName = "SOList/AllActors_SO")]
public class AllActors_SO : ScriptableObject
{
    public List<int> AllActorIDs; //Can change later to a hashset for efficiency but for now need display
    public int LastUnusedID = 0;

    public List<ActorData> AllActorData;

    // For now, save all data of every actor to this list, but later find a better way to save the info as thousands
    // or tens of thousands of actors would become too much and inefficient.

    public void PrepareToInitialise()
    {
        Manager_Initialisation.OnInitialiseAllActorSO += _initialise;
    }

    void _initialise()
    {
        // Clear the List, and load all the actor Data from JSON.

        _getAllActorData();

        _addAddAllEditorActorIDs();
    }

    void _getAllActorData()
    {
        // This will function as a temporary load function by loading the list before it's cleared since it will persist with the SO.
        foreach (var actor in FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None).OfType<Actor_Base>().ToList())
        {
            if (!AllActorData.Any(a => a.ActorID == actor.ActorData.ActorID))
            {
                Debug.Log($"Actor: {actor.ActorData.ActorName} with ID: {actor.ActorData.ActorID} was not in AllActorData");
                AllActorData.Add(actor.ActorData);
            }
        }

        for (int i = 0; i < AllActorData.Count; i++)
        {
            AllActorData[i].InitialiseActorData();
        }
    }

    void _addAddAllEditorActorIDs()
    {
        AllActorIDs.Clear();

        foreach (var actorData in AllActorData)
        {
            AllActorIDs.Add(actorData.ActorID);
        }
    }

    void _addAllRuntimeActorIDs()
    {

    }

    public void AddToOrUpdateAllActorsDataList(ActorData actorData)
    {
        var existingActor = AllActorData.FirstOrDefault(a => a.ActorID == actorData.ActorID);

        if (existingActor == null) AllActorData.Add(actorData);
        else AllActorData[AllActorData.IndexOf(existingActor)] = actorData;
    }

    public ActorData GetActorDataFromID(int actorID)
    {
        if (!AllActorData.Any(a => a.ActorID == actorID)) { Debug.Log($"AllActorData does not contain ActorID: {actorID}"); return null; }

        return AllActorData.FirstOrDefault(a => a.ActorID == actorID);
    }

    public int GetRandomActorID()
    {
        while (AllActorIDs.Contains(LastUnusedID))
        {
            LastUnusedID++;
        }

        AllActorIDs.Add(LastUnusedID);

        return LastUnusedID;
    }
}
