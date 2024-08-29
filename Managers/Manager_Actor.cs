using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Manager_Actor : MonoBehaviour, IDataPersistence
{
    public static AllFactions_SO AllFactions;
    public static Dictionary<int, Actor_Base> AllActorComponents = new();

    public void SaveData(SaveData data)
    {
        data.SavedFactionData = AllFactions.GetSavedFactionData();
    }

    public void LoadData(SaveData data)
    {
        AllFactions.SetSavedFactionData(data.SavedFactionData);
    }

    public void OnSceneLoaded()
    {
        AllFactions = Resources.Load<AllFactions_SO>("ScriptableObjects/AllFactions_SO");

        Manager_Initialisation.OnInitialiseManagerActor += _initialise;
    }

    void _initialise()
    {
        foreach (var actor in _findAllActorComponents())
        {
            if (actor.ActorData == null) { Debug.Log($"Actor: {actor.name} does not have ActorData."); continue; }

            if (!AllActorComponents.ContainsKey(actor.ActorData.ActorID)) AllActorComponents.Add(actor.ActorData.ActorID, actor);
            else
            {
                if (AllActorComponents[actor.ActorData.ActorID].gameObject == actor.gameObject) continue;
                else
                {
                    Debug.LogError($"ActorID {actor.ActorData.ActorID} and name {actor.name} already exists for actor {AllActorComponents[actor.ActorData.ActorID].name}");
                    actor.ActorData.ActorID = GetRandomActorID();
                }
            }
        }
    }

    static List<Actor_Base> _findAllActorComponents()
    {
        return FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .OfType<Actor_Base>()
            .ToList();
    }

    public static void AddToOrUpdateAllActorList(ActorData actorData, Actor_Base actor = null)
    {
        if (!AllActorComponents.ContainsKey(actorData.ActorID)) AllActorComponents.Add(actorData.ActorID, actor);

        AllFactions.AddToOrUpdateFactionActorsDataList(actorData.FullIdentification.ActorFactionID, actorData);
    }

    public static ActorData GetActorData(int actorID, out ActorData actorData, int factionID = -1)
    {
        return actorData = AllFactions.GetActorData(actorID, factionID);
    }

    public static Actor_Base GetActor(int actorID, out Actor_Base actor, int factionID = -1, bool generateActorIfNotFound = false)
    {
        if (AllActorComponents.ContainsKey(actorID))
        {
            if (AllActorComponents[actorID] != null) return actor = AllActorComponents[actorID];
            else
            {
                GetActorData(actorID, out var actorData, factionID);
                return AllActorComponents[actorID] = actor = SpawnActor(actorData.GameObjectProperties.LastSavedActorPosition, actorID);
            }
        }
        else if (GetActorData(actorID, out ActorData actorData, factionID) != null && generateActorIfNotFound)
        {
            return actor = SpawnActor(actorData.GameObjectProperties.LastSavedActorPosition, actorData.ActorID);
        }

        return actor = null;
    }

    // Maybe stagger the spawning so they don't all spawn immediately but either in batches or per seconds.
    public static Actor_Base SpawnActor(Vector3 spawnPoint, int actorID = -1, int factionID = -1)
    {
        if (actorID == -1) Debug.Log($"Spawning new actor at point: {spawnPoint}.");
        else Debug.Log($"Spawning actor with ID: {actorID} at point: {spawnPoint}.");

        Actor_Base actor = _createNewActorGO(spawnPoint).AddComponent<Actor_Base>();

        if (actorID != -1 && GetActorData(actorID, out ActorData actorData, factionID) != null)
        {
            actor.SetActorData(actorData);
            actor.Initialise();
        }
        else GenerateNewActorData(actor);

        AllActorComponents[actor.ActorData.ActorID] = actor;

        return actor;
    }

    static GameObject _createNewActorGO(Vector3 spawnPoint)
    {
        GameObject actorBody = new GameObject();
        actorBody.transform.parent = GameObject.Find("Characters").transform;
        actorBody.transform.position = spawnPoint;
        Rigidbody actorRb = actorBody.AddComponent<Rigidbody>();

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
        actor.SetActorData(new ActorData(new FullIdentification(
                actorID: GetRandomActorID(),
                actorName: GetRandomActorName(actor),
                actorFactionID: GetRandomFaction(),
                actorCityID: 0 // Placeholder, usually will pass through city in parameters, or -1 for not a citizen
                )
        ));

        actor.ActorData.CraftingData.AddRecipe(RecipeName.Log);
        actor.ActorData.CraftingData.AddRecipe(RecipeName.Plank);

        actor.ActorData.SpeciesAndPersonality.SetSpecies(GetRandomSpecies());
        actor.ActorData.SpeciesAndPersonality.SetPersonality(GetRandomPersonality());

        actor.Initialise();

        AddToOrUpdateAllActorList(actor.ActorData, actor);

        return actor.ActorData;
    }

    public static int GetRandomActorID()
    {
        return AllFactions.GetRandomActorID();
    }

    public static ActorName GetRandomActorName(Actor_Base actor)
    {
        // Get name based on culture, religion, species, etc.

        return new ActorName($"Test_{UnityEngine.Random.Range(0, 50000)}", $"of Tester");
    }

    private static int GetRandomFaction()
    {
        // Take race and things into account for faction

        return 0;
    }

    private static WorldState_Data_SO GetRandomWorldState()
    {
        // Don't have a worldstate automatically, only inherit one.

        return null;
    }

    static SpeciesName GetRandomSpecies()
    {
        return SpeciesName.Human;
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