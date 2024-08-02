using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Manager_Actor
{
    public static HashSet<Actor_Base> AllActors = new();
    public static HashSet<int> AllActorIDs = new();
    static int _lastUnusedID = 100000;

    public static void AddToAllActorList(Actor_Base actor)
    {
        AllActors.Add(actor);
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

    public static ActorName GetRandomActorName(Actor_Base actor)
    {
        // Get name based on culture, religion, species, etc.

        return new ActorName($"Test_{UnityEngine.Random.Range(0, _lastUnusedID)}", $"of Tester");
    }

    public static Actor_Base GetActor(int actorID)
    {
        return AllActors.FirstOrDefault(a => a.ActorData != null && a.ActorData.BasicIdentification.ActorID == actorID);
    }

    public static Actor_Base InitialiseNewActorOnGO(Vector3 spawnPoint)
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
        actorGO.AddComponent<MeshRenderer>().material = Resources.Load<Material>("Materials/Material_Red");
        actorGO.AddComponent<MeshFilter>().mesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
    
        return actorGO;
    }

    public static Actor_Data_SO GenerateNewActorData(Actor_Base actor) // ActorGenerationParameters parameters)
    {
        Actor_Data_SO actorData = ScriptableObject.CreateInstance<Actor_Data_SO>();

        actorData.InitialiseNewData(
            fullIdentification: new FullIdentification(
                actorID: GetRandomActorID(),
                actor: actor,
                actorName: GetRandomActorName(actor),
                actorFaction: GetRandomFaction()
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

        actor.Initialise(actorData);

        return actorData;
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
