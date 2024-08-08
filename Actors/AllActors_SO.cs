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

        foreach (ActorData actorData in AllActorData)
        {
            AllActorIDs.Add(actorData.ActorID);

            // Do a test here to see if they already exist as game objects in the world
            //Actor_Base actor = Manager_Actor.SpawnNewActorOnGO(actorData.GameObjectProperties.ActorPosition);
        }
    }

    public void AddToAllActorsList(ActorData actorData)
    {
        var existingActor = AllActorData.FirstOrDefault(a => a.ActorID == actorData.ActorID);

        if (existingActor == null) { AllActorData.Add(actorData); return; }

        var existingActorData = AllActorData[AllActorData.IndexOf(existingActor)];

        if (!existingActorData.OverwriteDataInActor) return;

        existingActorData = actorData;
        existingActorData.OverwriteDataInActor = false;
    }

    public ActorData GetActorDataFromID(int actorID)
    {
        List<ActorData> actorDataList = AllActorData.Where(a => a.ActorID == actorID).ToList();

        if (actorDataList.Count != 1) throw new ArgumentException($"ActorID: {actorID} has {actorDataList.Count} entries in AllActorData.");

        return actorDataList.FirstOrDefault();
    }

    public ActorData GetActorDataFromExistingActor(Actor_Base actor)
    {
        if (actor.ActorData != null) return AllActorData.FirstOrDefault(a => a.ActorID == actor.ActorData.ActorID);
        else
        {
            var actorData = Manager_Actor.GenerateNewActorData(actor);
            AddToAllActorsList(actorData); 
            return actorData;
        }
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
