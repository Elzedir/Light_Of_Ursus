using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Manager_Actor : MonoBehaviour, IDataPersistence
{
    public static AllActors_SO AllActors;
    public static Dictionary<int, ActorData> AllActorData;
    public static Dictionary<int, Actor_Base> AllActorComponents = new();

    public void SaveData(SaveData data)
    {
        AllActorData.Values.ToList().ForEach(actorData => actorData.UpdateActorData());

        data.SavedActorData = new SavedActorData(AllActorData.Values.ToList());
        AllActors.AllActorData = AllActorData.Values.ToList();
    }

    public void LoadData(SaveData data) 
    {
        try
        {
            AllActorData = data.SavedActorData.AllActorData.ToDictionary(x => x.ActorID);
        }
        catch
        {
            AllActorData = new();
            Debug.Log("No Actor Data found in SaveData.");
        }

        AllActors.AllActorData = AllActorData.Values.ToList();
    } 

    public void OnSceneLoaded()
    {
        AllActors = Resources.Load<AllActors_SO>("ScriptableObjects/AllActors_SO");

        Manager_Initialisation.OnInitialiseManagerActor += _initialise;
    }

    void _initialise()
    {
        if(AllActorData == null) AllActorData = new();

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

            if (!AllActorData.ContainsKey(actor.ActorData.ActorID)) Debug.Log($"Actor: {actor.ActorData.ActorID} {actor.name} does not exist in AllActorData.");
        }

        foreach(var actor in AllActorData)
        {
            actor.Value.PrepareForInitialisation();
        }
    }

    static List<Actor_Base> _findAllActorComponents()
    {
        return FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .OfType<Actor_Base>()
            .ToList();
    }

    public static void AddToAllActorData(ActorData actorData)
    {
        if (AllActorData.ContainsKey(actorData.ActorID))
        {
            Debug.Log($"ActorData: {actorData.ActorID} already exists in AllActorData.");
            return;
        }

        AllActorData.Add(actorData.ActorID, actorData);
    }

    public static void UpdateAllActorData(ActorData actorData) => AllActorData[actorData.ActorID] = actorData;
    public static void RemoveFromAllActorData(int actorID) => AllActorData.Remove(actorID);

    public static ActorData GetActorData(int actorID) => AllActorData[actorID];

    public static Actor_Base GetActor(int actorID, bool generateActorIfNotFound = false)
    {
        if (AllActorComponents.ContainsKey(actorID))
        {
            return AllActorComponents[actorID];
        }
        else if (generateActorIfNotFound)
        {
            return AllActorComponents[actorID] = SpawnActor(GetActorData(actorID).GameObjectProperties.LastSavedActorPosition, actorID);
        }

        return null;
    }

    public static Actor_Base SpawnNewActor(Vector3 spawnPoint)
    {
        Actor_Base actor = _createNewActorGO(spawnPoint).AddComponent<Actor_Base>();

        actor.SetActorData(GenerateNewActorData(actor));
        actor.Initialise();

        AllActorComponents[actor.ActorData.ActorID] = actor;

        return actor;
    }

    // Maybe stagger the spawning so they don't all spawn immediately but either in batches or per seconds.
    public static Actor_Base SpawnActor(Vector3 spawnPoint, int actorID, bool despawnActorIfExists = false)
    {
        if (despawnActorIfExists) DespawnActor(actorID);
        else if (AllActorComponents.ContainsKey(actorID)) return AllActorComponents[actorID];

        Actor_Base actor = _createNewActorGO(spawnPoint).AddComponent<Actor_Base>();

        actor.SetActorData(GetActorData(actorID));
        actor.Initialise();

        AllActorComponents[actor.ActorData.ActorID] = actor;

        return actor;
    }

    static void _spawnAllActors(bool despawnAllActors = false)
    {
        if (despawnAllActors) _despawnAllActors();

        foreach (var actorData in AllActorData.Values)
        {
            SpawnActor(actorData.GameObjectProperties.LastSavedActorPosition, actorData.ActorID, true);
        }
    }

    public static void DespawnActor(int actorID)
    {
        if (AllActorComponents.ContainsKey(actorID))
        {
            Destroy(AllActorComponents[actorID].gameObject);
            AllActorComponents.Remove(actorID);
        }
    }

    static void _despawnAllActors()
    {
        foreach (var actor in AllActorComponents.Values)
        {
            Destroy(actor.gameObject);
        }

        AllActorComponents.Clear();
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

        // Temporary
        //actor.ActorData.ActorName = new ActorName($"Test_{actor.ActorData.ActorID}", "of Tester");

        actor.ActorData.CraftingData.AddRecipe(RecipeName.Log);
        actor.ActorData.CraftingData.AddRecipe(RecipeName.Plank);

        actor.ActorData.SpeciesAndPersonality.SetSpecies(GetRandomSpecies());
        actor.ActorData.SpeciesAndPersonality.SetPersonality(GetRandomPersonality());
        actor.ActorData.GameObjectProperties.SetGameObjectProperties(actor.transform);

        AddToAllActorData(actor.ActorData);

        return actor.ActorData;
    }

    public static int GetRandomActorID()
    {
        int actorID = 1;
        while (AllActorData.ContainsKey(actorID))
        {
            actorID++;
        }
        return actorID;
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