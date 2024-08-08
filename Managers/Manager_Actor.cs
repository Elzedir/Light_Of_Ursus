using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class Manager_Actor : MonoBehaviour
{
    public static AllActors_SO AllActors;
    public static Dictionary<int, Actor_Base> AllActorComponents = new();

    public void OnSceneLoaded()
    {
        AllActors = Resources.Load<AllActors_SO>("ScriptableObjects/AllActors_SO");
        AllActors.PrepareToInitialise();

        Manager_Initialisation.OnInitialiseManagerActor += _initialise;
    }

    void _initialise()
    {
        foreach (var actor in _findAllActorComponents())
        {
            if (actor.ActorData == null) continue;

            AllActorComponents.Add(actor.ActorData.ActorID, actor);
        }
    }

    static List<Actor_Base> _findAllActorComponents()
    {
        return FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .OfType<Actor_Base>()
            .ToList();
    }

    public static void AddToAllActorList(ActorData actorData)
    {
        AllActors.AddToAllActorsList(actorData);
    }

    public static ActorData GetActorDataFromID(int actorID)
    {
        return AllActors.GetActorDataFromID(actorID);
    }

    public static ActorData GetActorDataFromExistingActor(Actor_Base actor)
    {
        return AllActors.GetActorDataFromExistingActor(actor);
    }

    public static Actor_Base GetActor(int actorID)
    {
        return AllActorComponents[actorID];
    }

    public static Actor_Base SpawnNewActorOnGO(Vector3 spawnPoint)
    {
        GameObject actorGO = _createNewActorGO(spawnPoint);

        Actor_Base actor = actorGO.AddComponent<Actor_Base>();

        return actor;
    }

    static GameObject _createNewActorGO(Vector3 spawnPoint)
    {
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

    public static ActorData GenerateNewActorData(Actor_Base actor) // ActorGenerationParameters parameters, select things like minimum skill range, abilties, etc.)
    {
        actor.SetActorData(new ActorData(
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
                actor.ActorMesh?.mesh ?? Resources.GetBuiltinResource<Mesh>("Cube.fbx"), // Later will come from species
                actor.ActorMaterial?.material ?? Resources.Load<Material>("Materials/Material_Red") // Later will come from species
                ),
            worldState: null,
            careerAndJobs: new CareerAndJobs(
                actorCareer: CareerName.None, // Later set to none only if you don't have a preset career in parameters
                actorJobs: new List<Job>()
                ),
            speciesAndPersonality: new SpeciesAndPersonality(
                actorSpecies: GetRandomSpecies(),
                actorPersonality: GetRandomPersonality()
                ),
            inventoryAndEquipment: new InventoryAndEquipment(
                new ActorInventory(),
                new ActorEquipment(null, null, null, null, null, null, null, null, null)
                ),
            statsAndAbilities: new StatsAndAbilities(),
            actorQuests: null
        ));

        return actor.ActorData;
    }

    public static int GetRandomActorID()
    {
        return AllActors.GetRandomActorID();
    }

    public static ActorName GetRandomActorName(Actor_Base actor)
    {
        // Get name based on culture, religion, species, etc.

        return new ActorName($"Test_{UnityEngine.Random.Range(0, 50000)}", $"of Tester");
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