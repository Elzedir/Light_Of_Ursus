using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Manager_Faction : MonoBehaviour, IDataPersistence
{
    public static AllFactions_SO AllFactions_SO;

    public static Dictionary<int, FactionData> AllFactionData = new();

    public void SaveData(SaveData saveData) => saveData.SavedFactionData = new SavedFactionData(AllFactionData.Values.ToList());
    public void LoadData(SaveData saveData) => AllFactionData = saveData.SavedFactionData.AllFactionData.ToDictionary(x => x.FactionID);

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

        AllFactions_SO.AllFactionData = AllFactionData.Values.ToList();
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
        int factionID = 1;
        while (AllFactionData.ContainsKey(factionID))
        {
            factionID++;
        }
        return factionID;
    }
}
