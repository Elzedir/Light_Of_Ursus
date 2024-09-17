using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class Manager_Faction : MonoBehaviour, IDataPersistence
{
    public static AllFactions_SO AllFactions_SO;

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
        AllFactions_SO = Resources.Load<AllFactions_SO>("ScriptableObjects/AllFactions_SO");

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

    public static FactionData GetFaction(int factionID) => AllFactions_SO.AllFactionData[factionID];

    public int GetRandomFactionID()
    {
        while (AllFactionData.ContainsKey(_lastUnusedFactionID))
        {
            _lastUnusedFactionID++;
        }

        return _lastUnusedFactionID;
    }
}

public class FactionGOChecker : EditorWindow
{
    [MenuItem("Tools/FactionGO Checker")]
    public static void ShowWindow()
    {
        GetWindow<FactionGOChecker>("FactionGO Checker");
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Check FactionGOs"))
        {
            CheckAndFixFactionGOs();
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

asdf

        // Maybe try Resources.FindObjectsOfTypeAll instead of FindObjectsOfType which might include inactive objects as well
        // And then get it to work for editor and not just runtime.

        // Something here is returning null when trying to create all the FactionGOs.

        var existingFactionComponents = FindObjectsByType<FactionComponent>(FindObjectsSortMode.None);
        var existingFactionGOs = factionsGO.GetComponentsInChildren<Transform>().Select(x => x.gameObject).ToList();

        var factionGOsWithoutComponents = existingFactionGOs.Where(fgo => fgo.GetComponent<FactionComponent>() == null).ToList();

        for (int i = 0; i < factionGOsWithoutComponents.Count; i++)
        {
            var factionGameObject = factionGOsWithoutComponents[i];
            Debug.LogWarning($"Creating FactionComponent for FactionGO: {factionGameObject.name}");
            factionGameObject.AddComponent<FactionComponent>();
            factionGOsWithoutComponents.Remove(factionGameObject);
        }

        var factionComponentsWithoutGOs = existingFactionComponents.Where(fc => existingFactionGOs.All(fgo => fgo.name != $"{fc.FactionData.FactionID}: {fc.FactionData.FactionName}")).ToList();

        for (int i = 0; i < factionComponentsWithoutGOs.Count; i++)
        {
            var factionComponent = factionComponentsWithoutGOs[i];
            Debug.LogWarning($"Changing FactionGO name to match FactionComponent: {factionComponent.FactionData.FactionID}: {factionComponent.FactionData.FactionName}");
            factionComponent.gameObject.name = $"{factionComponent.FactionData.FactionID}: {factionComponent.FactionData.FactionName}";
            factionComponentsWithoutGOs.Remove(factionComponent);
        }

        var factionIDsWithoutGOs = existingFactionIDs.Where(fID => existingFactionGOs.All(fgo => fgo.name != $"{fID}: {factionsSO.AllFactionData[fID].FactionName}")).ToList();

        for (int i = 0; i < factionIDsWithoutGOs.Count; i++)
        {
            var factionID = factionIDsWithoutGOs[i];
            Debug.LogWarning($"Creating FactionGO and FactionComponent for FactionID: {factionID}: {factionsSO.AllFactionData[factionID].FactionName}");
            _createFactionGO(factionsGO, factionID, factionsSO.AllFactionData[factionID].FactionName);
            factionIDsWithoutGOs.Remove(factionID);
        }
        
        var factionIDsWithoutComponents = existingFactionIDs.Where(fID => existingFactionComponents.All(fc => fc.FactionData.FactionID != fID)).ToList();

        for (int i = 0; i < factionIDsWithoutComponents.Count; i++)
        {
            var factionID = factionIDsWithoutComponents[i];
            var existingFactionGO = existingFactionGOs.FirstOrDefault(fgo => fgo.name == $"{factionID}: {factionsSO.AllFactionData[factionID].FactionName}");

            if (existingFactionGO != null)
            {
                Debug.LogWarning($"Updating FactionComponent with existing FactionGO to match existing FactionID: {factionID}: {factionsSO.AllFactionData[factionID].FactionName}");

                if (existingFactionGO.GetComponent<FactionComponent>() != null)
                {
                    Debug.LogWarning($"Destroying existing FactionComponent on FactionGO: {existingFactionGO.name}");
                    DestroyImmediate(existingFactionGO.GetComponent<FactionComponent>());
                }

                existingFactionGO.gameObject.AddComponent<FactionComponent>();
                factionIDsWithoutComponents.Remove(factionID);

                continue;
            }

            Debug.LogWarning($"FactionID: {factionID} does not have FactionGO or FactionComponent in the end. Recheck.");
        }

        if (factionGOsWithoutComponents.Count == 0 && factionComponentsWithoutGOs.Count == 0 && factionIDsWithoutGOs.Count == 0 && factionIDsWithoutComponents.Count == 0)
        {
            Debug.Log("All FactionGOs and FactionComponents are in sync.");
            return;
        }

        if (factionGOsWithoutComponents.Count > 0)
        {
            Debug.LogWarning($"FactionGOs without FactionComponents: {string.Join(", ", factionGOsWithoutComponents.Select(x => x.name))}");
        }

        if (factionComponentsWithoutGOs.Count > 0)
        {
            Debug.LogWarning($"FactionComponents without FactionGOs: {string.Join(", ", factionComponentsWithoutGOs.Select(x => x.FactionData.FactionID))}");
        }

        if (factionIDsWithoutGOs.Count > 0)
        {
            Debug.LogWarning($"FactionIDs without FactionGOs: {string.Join(", ", factionIDsWithoutGOs.Select(x => x))}");
        }

        if (factionIDsWithoutComponents.Count > 0)
        {
            Debug.LogWarning($"FactionIDs without FactionComponents: {string.Join(", ", factionIDsWithoutComponents.Select(x => x))}");
        }

        Debug.LogWarning("FactionGO and FactionComponent check and fix completed.");
    }

    void _createFactionGO(GameObject factionsGO, int factionID, string factionName)
    {
        foreach(Transform child in factionsGO.transform)
        {
            if (child.name == $"{factionID}: {factionName}") return;

            var factionGO = new GameObject($"{factionID}: {factionName}");
            factionGO.transform.SetParent(factionsGO.transform);
            factionGO.transform.position = Vector3.zero;

            factionGO.AddComponent<FactionComponent>();
        }
    }
}
