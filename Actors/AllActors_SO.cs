using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "AllActors_SO", menuName = "Actors/AllActors_SO")]
public class AllActors_SO : ScriptableObject
{
    public List<ActorData> AllActorData;

    // For now, save all data of every actor to this list, but later find a better way to save the info as thousands
    // or tens of thousands of actors would become too much and inefficient.

    public void PrepareToInitialise()
    {
        Manager_Initialisation.OnInitialiseActors += _initialise;
    }

    void _initialise()
    {
        // Clear the List, and load all the actor Data from JSON.

        foreach (ActorData actorData in AllActorData)
        {
            // Do a test here to see if they already exist as game objects in the world
            //Actor_Base actor = Manager_Actor.SpawnNewActorOnGO(actorData.GameObjectProperties.ActorPosition);
        }
    }

    public void AddToAllActorsList(ActorData actorData)
    {
        var existingActor = AllActorData.FirstOrDefault(a => a.ActorID == actorData.ActorID);

        Debug.Log(existingActor);

        if (existingActor == null) AllActorData.Add(actorData);
        else AllActorData[AllActorData.IndexOf(existingActor)] = actorData;//Debug.Log("Actor already exists in AllActors");//AllActorData[AllActorData.IndexOf(existingActor)] = actorData;
    }

    public ActorData GetActorDataFromID(int actorID, bool actorExists)
    {
        var actorData = AllActorData.FirstOrDefault(a => a.ActorID == actorID);
        if (actorData == null && !actorExists) throw new ArgumentException($"ActorID: {actorID} does not exist in AllActors for nonexistent actor.");
        else if (actorData == null && actorExists) throw new ArgumentException($"ActorID: {actorID} does not exist in AllActors for nonexistent actor.");

        return actorData;
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
}
