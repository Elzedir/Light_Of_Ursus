using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class Manager_Faction : MonoBehaviour, IDataPersistence
{
    static AllFactions_SO _allFactionsSO;
    public static AllFactions_SO AllFactions_SO { get { return _allFactionsSO ??= Resources.Load<AllFactions_SO>("ScriptableObjects/AllFactions_SO"); } }

    public static Dictionary<int, FactionData> AllFactionData = new();
    static int _lastUnusedFactionID = 1;

    public void SaveData(SaveData saveData)
    {
        saveData.SavedFactionData = new SavedFactionData(AllFactionData.Values.ToList());
        AllFactions_SO.AllFactionData = AllFactionData.Values.ToList();
    }
    public void LoadData(SaveData saveData) 
    {
        try
        {
            AllFactionData = saveData.SavedFactionData.AllFactionData.ToDictionary(x => x.FactionID);
        }
        catch
        {
            AllFactionData = new();
            Debug.Log("No Faction Data found in SaveData.");
        }
        
        AllFactions_SO.AllFactionData = AllFactionData.Values.ToList();
    } 

    public void OnSceneLoaded()
    {
        Manager_Initialisation.OnInitialiseManagerFaction += _initialise;
    }

    void _initialise()
    {
        _initialiseDefaultFactions();

        _initialiseAllFactions();
    }

    void _initialiseDefaultFactions()
    {
        if (!AllFactionData.ContainsKey(0))
        {
            AllFactionData.Add(0, new FactionData(
            factionID: 0,
            factionName: "Wanderers",
            new HashSet<int>(),
            new List<FactionRelationData>()
            ));
        }

        if (!AllFactionData.ContainsKey(1))
        {
            AllFactionData.Add(1, new FactionData(
            factionID: 1,
            factionName: "Player Faction",
            new HashSet<int>(),
            new List<FactionRelationData>()
            ));
        }
    }

    void _initialiseAllFactions()
    {
        foreach (var faction in AllFactionData)
        {
            faction.Value.InitialiseFaction();
        }
    }

    public static void AddToAllFactionData(FactionData factionData)
    {
        if (AllFactionData.ContainsKey(factionData.FactionID))
        {
            Debug.Log($"FactionData: {factionData.FactionID} already exists in AllFactionData.");
            return;
        }

        AllFactionData.Add(factionData.FactionID, factionData);
    }

    public static void UpdateAllFactionData(FactionData factionData) => AllFactionData[factionData.FactionID] = factionData;

    public static void RemoveFromFactionData(int factionID) => AllFactionData.Remove(factionID);

    public static FactionData GetFaction(int factionID)
    {
        if (!AllFactionData.ContainsKey(factionID))
        {
            Debug.LogError($"FactionData: {factionID} does not exist in AllFactionData.");
            return null;
        }

        return AllFactionData[factionID];
    }

    public int GetRandomFactionID()
    {
        while (AllFactionData.ContainsKey(_lastUnusedFactionID))
        {
            _lastUnusedFactionID++;
        }

        return _lastUnusedFactionID;
    }

    public static void AllocateActorToFactionGO(ActorComponent actor, int factionID)
    {
        var faction = GetFaction(factionID);

        if (faction == null)
        {
            Debug.LogError($"Faction: {factionID} not found.");
            return;
        }

        var factionGO = GameObject.Find($"{factionID}: {faction.FactionName}");

        if (factionGO == null)
        {
            Debug.LogError($"FactionGO: {factionID}: {faction.FactionName} not found.");
            return;
        }

        actor.transform.parent.SetParent(factionGO.transform);
    }
}

public class FactionGOChecker : EditorWindow
{
    [MenuItem("Tools/FactionGO Checker")]
    public static void ShowWindow()
    {
        GetWindow<FactionGOChecker>("FactionGO Checker");
        GetWindow<FactionGOChecker>("Move Actors To Factions");
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Check FactionGOs"))
        {
            CheckAndFixFactionGOs();
        }

        if (GUILayout.Button("Move Actors To Factions"))
        {
            MoveActorsToFactions();
        }
    }

    void CheckAndFixFactionGOs()
    {
        var factionsSO = Resources.Load<AllFactions_SO>("ScriptableObjects/AllFactions_SO");

        if (factionsSO == null)
        {
            Debug.LogError("No AllFactions_SO found.");
            return;
        }

        var factionsGO = GameObject.Find("Factions");

        if (factionsGO == null)
        {
            Debug.LogError("No Factions GameObject found.");
            return;
        }

        var existingFactionIDs = factionsSO.AllFactionData.Select(x => x.FactionID).ToList();

        if (existingFactionIDs.Count == 0)
        {
            Debug.LogWarning("No FactionIDs found in AllFactions_SO.");
        }

        // Maybe try Resources.FindObjectsOfTypeAll instead of FindObjectsOfType which might include inactive objects as well
        // And then get it to work for editor and not just runtime.

        // Something here is returning null when trying to create all the FactionGOs.

        FactionComponent[] existingFactionComponents = new FactionComponent[0];
        List<GameObject> existingFactionGOs = new();

        existingObjectsCheck();

        var factionIDsWithoutGOs = existingFactionIDs.Where(fID => existingFactionGOs.All(fgo => fgo.name != $"{fID}: {factionsSO.AllFactionData[fID].FactionName}")).ToList();

        var factionIDsToRemove = new List<int>();
        foreach (var factionID in factionIDsWithoutGOs)
        {
            Debug.LogWarning($"Creating FactionGO and FactionComponent for FactionID: {factionID}: {factionsSO.AllFactionData[factionID].FactionName}");
            _createFactionGO(factionsGO, factionID, factionsSO.AllFactionData[factionID].FactionName, factionsSO);
            factionIDsToRemove.Add(factionID);
        }

        foreach (var factionID in factionIDsToRemove)
        {
            factionIDsWithoutGOs.Remove(factionID);
        }

        existingObjectsCheck();

        var factionIDsWithoutComponents = existingFactionIDs.Where(fID => existingFactionComponents.All(fc => fc.FactionData.FactionID != fID)).ToList();
        var factionIDsWithoutComponentsToRemove = new List<int>();
        foreach (var factionID in factionIDsWithoutComponents)
        {
            var existingFactionGO = existingFactionGOs.FirstOrDefault(fgo => fgo.name == $"{factionID}: {factionsSO.AllFactionData[factionID].FactionName}");
            if (existingFactionGO != null)
            {
                Debug.LogWarning($"Updating FactionComponent with existing FactionGO to match existing FactionID: {factionID}: {factionsSO.AllFactionData[factionID].FactionName}");
                if (existingFactionGO.GetComponent<FactionComponent>() != null)
                {
                    Debug.LogWarning($"Destroying existing FactionComponent on FactionGO: {existingFactionGO.name}");
                    DestroyImmediate(existingFactionGO.GetComponent<FactionComponent>());
                }
                var factionComponent = existingFactionGO.gameObject.AddComponent<FactionComponent>();
                var factionData = factionsSO.AllFactionData.FirstOrDefault(x => x.FactionID == factionID);
                if (factionData == null)
                {
                    Debug.LogError($"FactionData not found for FactionGO: {existingFactionGO.name}");
                    continue;
                }
                factionComponent.FactionData = factionData;
                factionIDsWithoutComponentsToRemove.Add(factionID);
            }
        }

        foreach (var factionID in factionIDsWithoutComponentsToRemove)
        {
            factionIDsWithoutComponents.Remove(factionID);
        }

        existingObjectsCheck();

        var factionGOsWithoutComponents = existingFactionGOs.Where(fgo => fgo.GetComponent<FactionComponent>() == null).ToList();
        var factionGOsWithoutComponentsToRemove = new List<GameObject>();
        foreach (var factionGameObject in factionGOsWithoutComponents)
        {
            Debug.LogWarning($"Creating FactionComponent for FactionGO: {factionGameObject.name}");
            var factionComponent = factionGameObject.AddComponent<FactionComponent>();
            var factionData = factionsSO.AllFactionData.FirstOrDefault(x => x.FactionID == int.Parse(factionGameObject.name.Split(':')[0]));
            if (factionData == null)
            {
                Debug.LogError($"FactionData not found for FactionGO: {factionGameObject.name}");
                continue;
            }
            factionComponent.FactionData = factionData;
            factionGOsWithoutComponentsToRemove.Add(factionGameObject);
        }

        foreach (var factionGameObject in factionGOsWithoutComponentsToRemove)
        {
            factionGOsWithoutComponents.Remove(factionGameObject);
        }

        existingObjectsCheck();

        var factionComponentsWithoutGOs = existingFactionComponents.Where(fc => existingFactionGOs.All(fgo => fgo.name != $"{fc.FactionData.FactionID}: {fc.FactionData.FactionName}")).ToList();
        var factionComponentsWithoutGOsToRemove = new List<FactionComponent>();
        foreach (var factionComponent in factionComponentsWithoutGOs)
        {
            Debug.LogWarning($"Changing FactionGO name to match FactionComponent: {factionComponent.FactionData.FactionID}: {factionComponent.FactionData.FactionName}");
            factionComponent.gameObject.name = $"{factionComponent.FactionData.FactionID}: {factionComponent.FactionData.FactionName}";
            factionComponentsWithoutGOsToRemove.Add(factionComponent);
        }

        foreach (var factionComponent in factionComponentsWithoutGOsToRemove)
        {
            factionComponentsWithoutGOs.Remove(factionComponent);
        }

        if (factionGOsWithoutComponents.Count == 0 && factionComponentsWithoutGOs.Count == 0 && factionIDsWithoutGOs.Count == 0 && factionIDsWithoutComponents.Count == 0)
        {
            Debug.Log("All FactionGOs and FactionComponents are in sync.");
            return;
        }

        if (factionGOsWithoutComponents.Count > 0)
        {
            Debug.LogWarning($"FactionGOs without FactionComponents: {string.Join(", ", factionGOsWithoutComponents.Select(x => x.name))}");

            foreach (var factionGO in factionGOsWithoutComponents)
            {
                Debug.LogError($"FactionGO: {factionGO.name} does not have FactionComponent.");
            }
        }

        if (factionComponentsWithoutGOs.Count > 0)
        {
            Debug.LogWarning($"FactionComponents without FactionGOs: {string.Join(", ", factionComponentsWithoutGOs.Select(x => x.FactionData.FactionID))}");

            foreach (var factionComponent in factionComponentsWithoutGOs)
            {
                Debug.LogError($"FactionComponent: {factionComponent.FactionData.FactionID}: {factionComponent.FactionData.FactionName} does not have FactionGO.");
            }
        }

        if (factionIDsWithoutGOs.Count > 0)
        {
            Debug.LogWarning($"FactionIDs without FactionGOs: {string.Join(", ", factionIDsWithoutGOs.Select(x => x))}");

            foreach (var factionID in factionIDsWithoutGOs)
            {
                Debug.LogError($"FactionID: {factionID} does not have FactionGO.");
            }
        }

        if (factionIDsWithoutComponents.Count > 0)
        {
            Debug.LogWarning($"FactionIDs without FactionComponents: {string.Join(", ", factionIDsWithoutComponents.Select(x => x))}");

            foreach (var factionID in factionIDsWithoutComponents)
            {
                Debug.LogError($"FactionID: {factionID} does not have FactionComponent.");
            }
        }

        Debug.LogWarning("FactionGO and FactionComponent check and fix completed.");

        void existingObjectsCheck()
        {
            existingFactionComponents = FindObjectsByType<FactionComponent>(FindObjectsSortMode.None);

            existingFactionGOs = new List<GameObject>();

            foreach (Transform child in factionsGO.transform)
            {
                existingFactionGOs.Add(child.gameObject);
            }
        }
    }

    void _createFactionGO(GameObject factionsGO, int factionID, string factionName, AllFactions_SO factionsSO)
    {
        var factionGO = new GameObject($"{factionID}: {factionName}");
        factionGO.transform.SetParent(factionsGO.transform);
        factionGO.transform.position = Vector3.zero;
        var factionComponent = factionGO.AddComponent<FactionComponent>();
        var factionData = factionsSO.AllFactionData.FirstOrDefault(x => x.FactionID == factionID);
        if (factionData == null)
        {
            Debug.LogError($"FactionData not found for FactionGO: {factionGO.name}");
            return;
        }

        factionComponent.FactionData = factionData;
    }

    void MoveActorsToFactions()
    {
        var actors = FindObjectsByType<ActorComponent>(FindObjectsSortMode.None);

        foreach (var actor in actors)
        {
            var actorsSO = Resources.Load<AllActors_SO>("ScriptableObjects/AllActors_SO");

            if (actorsSO == null)
            {
                Debug.LogError("No AllActors_SO found.");
                return;
            }

            var actorData = actorsSO.AllActorData.FirstOrDefault(x => x.ActorID == actor.ActorData.ActorID);

            if (actorData == null)
            {
                Debug.LogError($"ActorData not found for Actor: {actor.name}");
                continue;
            }

            var factionsSO = Resources.Load<AllFactions_SO>("ScriptableObjects/AllFactions_SO");

            if (factionsSO == null)
            {
                Debug.LogError("No AllFactions_SO found.");
                return;
            }

            var factionID = actorData.ActorFactionID;

            var faction = factionsSO.AllFactionData.FirstOrDefault(x => x.FactionID == factionID);

            if (faction == null)
            {
                Debug.LogError($"Faction: {factionID} not found.");
                return;
            }

            var factionGO = GameObject.Find($"{factionID}: {faction.FactionName}");

            if (factionGO == null)
            {
                Debug.LogError($"FactionGO: {factionID}: {faction.FactionName} not found.");
                return;
            }

            actor.transform.SetParent(factionGO.transform);

            Debug.Log($"Moved Actor: {actor.name} to Faction: {factionID}: {faction.FactionName}");
        }

        Debug.Log("Actors moved to Factions.");
    }
}
