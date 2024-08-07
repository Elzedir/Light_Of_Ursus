using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class Manager_Actor
{
    public static HashSet<int> AllActorIDs = new();
    static int _lastUnusedID = 100000;
    public static AllActors_SO AllActors;

    public static void Initialise()
    {
        AllActors = Resources.Load<AllActors_SO>("ScriptableObjects/Actors/AllActors_SO");
        AllActors.PrepareToInitialise();
    }

    public static void AddToAllActorList(ActorData actorData)
    {
        AllActors.AddToAllActorsList(actorData);
    }

    public static int GetRandomActorID()
    {
        while (AllActorIDs.Contains(_lastUnusedID))
        {
            _lastUnusedID++;
        }

        AllActorIDs.Add(_lastUnusedID);

        return _lastUnusedID;
    }

    public static ActorData GetActorDataFromID(int actorID, bool actorExists = true)
    {
        return AllActors.GetActorDataFromID(actorID, actorExists);
    }

    public static ActorData GetActorDataFromExistingActor(Actor_Base actor)
    {
        return AllActors.GetActorDataFromExistingActor(actor);
    }

    public static Actor_Base SpawnNewActorOnGO(Vector3 spawnPoint)
    {
        GameObject actorGO = _createNewActorGO(spawnPoint);

        Actor_Base actor = actorGO.AddComponent<Actor_Base>();

        return actor;
    }

    static GameObject _createNewActorGO(Vector3 spawnPoint)
    {
        Debug.Log(spawnPoint);

        GameObject actorBody = new GameObject();
        actorBody.transform.parent = GameObject.Find("Characters").transform;
        actorBody.transform.position = spawnPoint;
        actorBody.AddComponent<Rigidbody>();

        GameObject actorGO = new GameObject();
        actorGO.transform.parent = actorBody.transform;
        actorGO.transform.localPosition = Vector3.zero;

        actorGO.AddComponent<BoxCollider>();
        actorGO.AddComponent<Animator>();
        actorGO.AddComponent<Animation>();
        actorGO.AddComponent<MeshRenderer>();
        actorGO.AddComponent<MeshFilter>();
    
        return actorGO;
    }

    public static ActorData GenerateNewActorData(Actor_Base actor) // ActorGenerationParameters parameters)
    {
        return new ActorData(
            fullIdentification: new FullIdentification(
                actorID: GetRandomActorID(),
                actor: actor,
                actorName: GetRandomActorName(actor),
                actorFaction: GetRandomFaction()
                ),
            gameObjectProperties: new GameObjectProperties(
                actor.transform.parent.position,
                actor.transform.parent.rotation,
                actor.transform.parent.localScale,
                actor.ActorMesh.mesh,
                actor.ActorMaterial.material
                ),
            actorQuests: null,
            attributesCareerAndPersonality: new AttributesCareerAndPersonality(
                actorSpecies: GetRandomSpecies(),
                actorCareer: CareerName.None, // Later set to none only if you don't have a preset career
                actorPersonality: GetRandomPersonality()
                ),
            inventoryAndEquipment: new InventoryAndEquipment(),
            statsAndAbilities: new StatsAndAbilities(),
            worldState: null
        );
    }

    public static ActorName GetRandomActorName(Actor_Base actor)
    {
        // Get name based on culture, religion, species, etc.

        return new ActorName($"Test_{UnityEngine.Random.Range(0, _lastUnusedID)}", $"of Tester");
    }

    private static SpeciesName GetRandomSpecies()
    {
        return SpeciesName.Human;
    }

    private static FactionName GetRandomFaction()
    {
        // Take race and things into account for faction

        return FactionName.Passive;
    }

    private static WorldState_Data_SO GetRandomWorldState()
    {
        // Don't have a worldstate automatically, only inherit one.

        return null;
    }

    static ActorPersonality GetRandomPersonality()
    {
        return new ActorPersonality(Manager_Personality.GetRandomPersonalityTraits(null, 3));
    }
}

public class ActorGenerationParameters
{
    

    public ActorGenerationParameters()
    {

    }
}