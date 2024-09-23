using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Manager_Actor : MonoBehaviour, IDataPersistence
{
    public static AllActors_SO AllActors;
    public static Dictionary<int, ActorData> AllActorData = new();
    static int _lastUnusedActorID = 1;
    public static Dictionary<int, ActorComponent> AllActorComponents = new();

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

        foreach (var actor in AllActorData)
        {
            actor.Value.PrepareForInitialisation();
        }
    }

    static List<ActorComponent> _findAllActorComponents()
    {
        return FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .OfType<ActorComponent>()
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

    public static void UpdateAllActorData(ActorData actorData)
    {
        if (!AllActorData.ContainsKey(actorData.ActorID))
        {
            Debug.Log($"ActorData: {actorData.ActorID} does not exist in AllActorData.");
            return;
        }

        AllActorData[actorData.ActorID] = actorData;
    }
    public static void RemoveFromAllActorData(int actorID)
    {
        if (!AllActorData.ContainsKey(actorID))
        {
            Debug.Log($"ActorData: {actorID} does not exist in AllActorData.");
            return;
        }

        AllActorData.Remove(actorID);
    }

    public static ActorData GetActorData(int actorID)
    {
        if (!AllActorData.ContainsKey(actorID))
        {
            Debug.Log($"ActorData: {actorID} does not exist in AllActorData.");
            return null;
        }

        return AllActorData[actorID];
    }

    public static ActorComponent GetActor(int actorID, bool generateActorIfNotFound = false)
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

    public static ActorComponent SpawnNewActor(Vector3 spawnPoint, ActorGenerationParameters actorGenerationParameters)
    {
        ActorComponent actor = _createNewActorGO(spawnPoint).AddComponent<ActorComponent>();

        actor.SetActorData(GenerateNewActorData(actor, actorGenerationParameters));
        actor.Initialise();

        AllActorComponents[actor.ActorData.ActorID] = actor;

        Manager_Faction.AllocateActorToFactionGO(actor, actor.ActorData.ActorFactionID);

        return actor;
    }

    // Maybe stagger the spawning so they don't all spawn immediately but either in batches or per seconds.
    public static ActorComponent SpawnActor(Vector3 spawnPoint, int actorID, bool despawnActorIfExists = false)
    {
        if (despawnActorIfExists) DespawnActor(actorID);
        else if (AllActorComponents.ContainsKey(actorID)) return AllActorComponents[actorID];

        ActorComponent actor = _createNewActorGO(spawnPoint).AddComponent<ActorComponent>();

        actor.SetActorData(GetActorData(actorID));
        actor.Initialise();

        Manager_Faction.AllocateActorToFactionGO(actor, actor.ActorData.ActorFactionID);

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
        actorBody.transform.position = spawnPoint;
        Rigidbody actorRb = actorBody.AddComponent<Rigidbody>();
        actorRb.useGravity = false;

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

    public static ActorData GenerateNewActorData(ActorComponent actor, ActorGenerationParameters actorGenerationParameters)
    {
        var fullIdentification = new FullIdentification(
        actorID: actorGenerationParameters.ActorID != 0 ? actorGenerationParameters.ActorID : GetRandomActorID(),
        actorName: actorGenerationParameters.ActorName ?? GetRandomActorName(actor),
        actorFactionID: actorGenerationParameters.FactionID != 0 ? actorGenerationParameters.FactionID : GetRandomFaction(),
        actorCityID: actorGenerationParameters.CityID
    );

        actor.SetActorData(new ActorData(fullIdentification));

        foreach (var recipe in actorGenerationParameters.InitialRecipes)
        {
            Debug.Log($"Adding recipe: {recipe}");
            actor.ActorData.CraftingData.AddRecipe(recipe);
        }

        // Add initial vocations
        foreach (var vocation in actorGenerationParameters.InitialVocations)
        {
            Debug.Log($"Adding vocation: {vocation.VocationName} with experience: {vocation.VocationExperience}");
            actor.ActorData.VocationData.AddVocation(vocation.VocationName, vocation.VocationExperience);
        }

        actor.ActorData.CraftingData.AddRecipe(RecipeName.Log);
        actor.ActorData.CraftingData.AddRecipe(RecipeName.Plank);
        
        //Find a better way to put into groups.
        actor.ActorData.InventoryAndEquipment.InventoryData = new InventoryData(fullIdentification.ActorID, new List<Item>());

        actor.ActorData.SpeciesAndPersonality.SetSpecies(GetRandomSpecies());
        actor.ActorData.SpeciesAndPersonality.SetPersonality(GetRandomPersonality());
        actor.ActorData.GameObjectProperties.SetGameObjectProperties(actor.transform);

        AddToAllActorData(actor.ActorData);

        return actor.ActorData;
    }

    public static int GetRandomActorID()
    {
        while (AllActorData.ContainsKey(_lastUnusedActorID))
        {
            _lastUnusedActorID++;
        }

        return _lastUnusedActorID;
    }

    public static ActorName GetRandomActorName(ActorComponent actor)
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

[Serializable]
public class ActorGenerationParameters
{
    public int ActorID { get; private set; } = 0;
    public ActorName ActorName{ get; private set; }
    public int FactionID { get; private set; } = 0;
    public int CityID { get; private set; } = 0;
    public List<RecipeName> InitialRecipes { get; private set; } = new List<RecipeName>();
    public List<ActorVocation> InitialVocations { get; private set; } = new();

    public void SetActorID(int actorID) => ActorID = actorID;
    public void SetActorName(ActorName actorName) => ActorName = actorName;
    public void SetFactionID(int factionID) => FactionID = factionID;
    public void SetCityID(int cityID) => CityID = cityID;
    public void SetInitialRecipes(List<RecipeName> initialRecipes) => InitialRecipes = initialRecipes;
    public void AddInitialRecipe(RecipeName recipeName) => InitialRecipes.Add(recipeName);
    public void SetInitialVocations(List<ActorVocation> actorVocations) => InitialVocations = actorVocations;
    public void AddInitialVocation(ActorVocation actorVocation) => InitialVocations.Add(actorVocation);
}