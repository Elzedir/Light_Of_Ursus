using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;


[CreateAssetMenu(fileName = "AllFactions_SO", menuName = "SOList/AllFactions_SO")]
[Serializable]
public class AllFactions_SO : ScriptableObject
{
    public List<FactionData> AllFactionData;

    public List<int> AllFactionIDs;
    public int LastUnusedFactionID = 0;

    public List<int> AllActorIDs; //Can change later to a hashset for efficiency but for now need display
    public int LastUnusedActorID = 1;

    // For now, save all data of every actor to this list, but later find a better way to save the info as thousands
    // or tens of thousands of actors would become too much and inefficient.

    public void PrepareToInitialise()
    {
        Manager_Initialisation.OnInitialiseAllFactionSO += _initialise;
    }

    void _initialise()
    {
        //_addAllFactionsFromJSON();

        _initialiseDefaultFactions();

        _initialiseAllFactions();

        _addAdditionalActorDataFromScene();

        _addAddAllEditorIDs();

        _addAllRuntimeIDs();

        
    }

    void _initialiseDefaultFactions()
    {
        if (!AllFactionData.Any(f => f.FactionID == 0))
        {
            AllFactionData.Add(new FactionData(
            factionID: 0,
            factionName: "Wanderers",
            new List<ActorData>(),
            new List<FactionRelationData>()
            ));
        }

        if (!AllFactionData.Any(f => f.FactionID == 1))
        {
            AllFactionData.Add(new FactionData(
            factionID: 1,
            factionName: "Player Faction",
            new List<ActorData>(),
            new List<FactionRelationData>()
            ));
        }
    }

    void _initialiseAllFactions()
    {
        foreach(var faction in AllFactionData)
        {
            faction.InitialiseFaction();
        }
    }

    void _addAdditionalActorDataFromScene()
    {
        foreach (var actor in FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None).OfType<Actor_Base>().ToList())
        {
            if (!AllFactionData.Any(f => f.AllFactionActors.Any(a => a.ActorID == actor.ActorData.ActorID)))
            {
                Debug.Log($"Actor: {actor.ActorData.ActorID}: {actor.ActorData.ActorName.GetName()} was not in any FactionActorData");

                GetFaction(actor.ActorData.ActorFactionID).AllFactionActors.Add(actor.ActorData);
            }
        }
    }

    void _addAddAllEditorIDs()
    {
        AllFactionIDs.Clear();
        AllActorIDs.Clear();
        LastUnusedFactionID = 0;
        LastUnusedActorID = 1;

        for(int i = 0; i < AllFactionData.Count; i++)
        {
            var factionData = AllFactionData[i];
            int tempFactionID = -1;

            while (AllFactionIDs.Contains(factionData.FactionID))
            {
                tempFactionID = factionData.FactionID;
                factionData.FactionID = GetRandomFactionID();
            }

            if (tempFactionID != -1) Debug.Log($"Overwrote factionID from {tempFactionID} to {factionData.FactionID}: {factionData.FactionName}");

            AllFactionIDs.Add(factionData.FactionID);

            var factionActorData = AllFactionData[i].AllFactionActors;

            for (int j = 0; j < factionActorData.Count; j++)
            {
                var actorData = factionActorData[j];
                int tempActorID = -1;

                while (AllActorIDs.Contains(actorData.ActorID))
                {
                    tempActorID = actorData.ActorID;
                    actorData.ActorID = GetRandomActorID();
                }

                if (tempActorID != -1) Debug.Log($"Overwrote actor ID from {tempActorID} to {actorData.ActorID}: {actorData.ActorName.GetName()}");

                AllActorIDs.Add(actorData.ActorID);
            }
        }
    }

    void _addAllRuntimeIDs()
    {

    }

    public void AddToOrUpdateFactionActorsDataList(int factionID, ActorData actorData)
    {
        AllFactionData.FirstOrDefault(f => f.FactionID == factionID).AddToOrUpdateFactionActorsDataList(actorData);
    }

    public void RemoveFromFactionActorsDataList(int factionID, ActorData actorData)
    {
        //AllFactionData.FirstOrDefault(f => f.FactionID == factionID).RemoveFromFactionActorsDataList(actorData);
    }

    public ActorData GetActorData(int actorID, int factionID)
    {
        return AllFactionData.FirstOrDefault(f => f.FactionID == factionID)?.GetActorData(actorID)
            ?? AllFactionData
            .SelectMany(f => f.AllFactionActors)
            .FirstOrDefault(a => a.ActorID == actorID);
    }

    public int GetRandomFactionID()
    {
        while (AllFactionIDs.Contains(LastUnusedFactionID))
        {
            LastUnusedFactionID++;
        }

        AllFactionIDs.Add(LastUnusedFactionID);

        return LastUnusedFactionID;
    }

    public int GetRandomActorID()
    {
        while (AllActorIDs.Contains(LastUnusedActorID))
        {
            LastUnusedActorID++;
        }

        AllActorIDs.Add(LastUnusedActorID);

        return LastUnusedActorID;
    }

    public void ClearFactionData() => AllFactionData.Clear();

    public void CallSaveData() => Manager_Data.Instance.SaveGame();
    public void CallLoadData() => Manager_Data.Instance.LoadGame();

    public FactionData GetFaction(int factionID)
    {
        if (!AllFactionData.Any(f => f.FactionID == factionID)) { Debug.Log($"AllFactionData does not contain FactionID: {factionID}"); return null; }

        return AllFactionData.FirstOrDefault(f => f.FactionID == factionID);
    }
}

[CustomEditor(typeof(AllFactions_SO))]
public class AllFactions_SOEditor : Editor
{
    int _selectedFactionIndex = -1;
    int selectedFactionIndex { get { return _selectedFactionIndex; } set { if (_selectedFactionIndex == value) return; _selectedFactionIndex = value; _resetIndexes(0); } }
    int _selectedActorIndex = -1;
    int selectedActorIndex { get { return _selectedActorIndex; } set { if (_selectedActorIndex == value) return; _selectedActorIndex = value; _resetIndexes(1); } }
    bool _showGameObjectProperties = false;
    bool _showSpeciesAndPersonality = false;
    int selectedFactionRelationIndex = -1;

    void _resetIndexes(int i = -1)
    {
        _showGameObjectProperties = false;
        _showSpeciesAndPersonality = false;
        selectedFactionRelationIndex = -1;
        if (i == 1) return;
        _selectedActorIndex = -1;
        if (i == 0) return;
        _selectedFactionIndex = 0;
    }

    Vector2 factionScrollPos;
    Vector2 actorScrollPos;
    Vector2 factionRelationScrollPos;

    public override void OnInspectorGUI()
    {
        AllFactions_SO allFactionSO = (AllFactions_SO)target;

        if (GUILayout.Button("Save Data")) allFactionSO.CallSaveData();
        if (GUILayout.Button("Load Data")) allFactionSO.CallLoadData();

        EditorGUILayout.LabelField("Global Data", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Faction IDs", string.Join(", ", allFactionSO.AllFactionIDs));
        allFactionSO.LastUnusedFactionID = EditorGUILayout.IntField("Last Unused Faction ID", allFactionSO.LastUnusedFactionID);
        EditorGUILayout.LabelField("Actor IDs", string.Join(", ", allFactionSO.AllActorIDs));
        allFactionSO.LastUnusedActorID = EditorGUILayout.IntField("Last Unused Actor ID", allFactionSO.LastUnusedActorID);

        if (GUILayout.Button("Clear Faction Data"))
        {
            _resetIndexes();
            allFactionSO.ClearFactionData();
            EditorUtility.SetDirty(allFactionSO);
        }

        if (GUILayout.Button("Unselect All")) _resetIndexes();

        EditorGUILayout.LabelField("All Factions", EditorStyles.boldLabel);
        factionScrollPos = EditorGUILayout.BeginScrollView(factionScrollPos, GUILayout.Height(Math.Min(200, allFactionSO.AllFactionData.Count * 20)));
        selectedFactionIndex = GUILayout.SelectionGrid(selectedFactionIndex, GetFactionNames(allFactionSO), 1);
        EditorGUILayout.EndScrollView();

        if (selectedFactionIndex >= 0 && selectedFactionIndex < allFactionSO.AllFactionData.Count)
        {
            DrawFactionData(allFactionSO.AllFactionData[selectedFactionIndex]);
        }
    }

    private string[] GetFactionNames(AllFactions_SO allFactionsSO)
    {
        return allFactionsSO.AllFactionData.Select(f => $"{f.FactionID}: {f.FactionName}").ToArray();
    }

    void DrawFactionData(FactionData factionData)
    {
        EditorGUILayout.LabelField("Faction Data", EditorStyles.boldLabel);

        EditorGUILayout.LabelField("Faction ID", factionData.FactionID.ToString());
        EditorGUILayout.LabelField("Faction Name", factionData.FactionName);

        EditorGUILayout.LabelField("All Faction Actors", EditorStyles.boldLabel);

        if (factionData.AllFactionActors != null)
        {
            actorScrollPos = EditorGUILayout.BeginScrollView(actorScrollPos, GUILayout.Height(Math.Min(200, factionData.AllFactionActors.Count * 20)));
            selectedActorIndex = GUILayout.SelectionGrid(selectedActorIndex, GetFactionActorData(), 1);
            EditorGUILayout.EndScrollView();

            if (selectedActorIndex >= 0 && selectedActorIndex < factionData.AllFactionActors.Count)
            {
                DrawActorAdditionalData(factionData.AllFactionActors[selectedActorIndex]);
            }
        }

        string[] GetFactionActorData()
        {
            return factionData.AllFactionActors.Select(a => $"{a.ActorID}: {a.ActorName.GetName()}").ToArray();
        }

        EditorGUILayout.LabelField("All Faction Relations", EditorStyles.boldLabel);

        if (factionData.AllFactionRelations != null)
        {
            factionRelationScrollPos = EditorGUILayout.BeginScrollView(factionRelationScrollPos, GUILayout.Height(Math.Min(200, factionData.AllFactionRelations.Count * 20)));
            selectedFactionRelationIndex = GUILayout.SelectionGrid(selectedFactionRelationIndex, GetFactionRelationData(), 1);
            EditorGUILayout.EndScrollView();

            if (selectedFactionRelationIndex >= 0 && selectedFactionRelationIndex < factionData.AllFactionRelations.Count)
            {
                DrawFactionRelationDetails(factionData.AllFactionRelations[selectedFactionRelationIndex]);
            }
        }

        string[] GetFactionRelationData()
        {
            return factionData.AllFactionRelations.Select(f => $"{f.FactionID}: {f.FactionName}").ToArray();
        }
    }

    int selectedInventoryIndex = -1;

    Vector2 inventoryItemScrollPos;

    int selectedEquipmentIndex = -1;

    Vector2 equipmentItemScrollPos;

    private void DrawActorAdditionalData(ActorData actorData)
    {
        EditorGUILayout.LabelField("Actor Data", EditorStyles.boldLabel);

        EditorGUILayout.LabelField("Actor ID", actorData.ActorID.ToString());

        EditorGUILayout.LabelField("Actor Name", $"{actorData.ActorName.Name} {actorData.ActorName.Surname}");

        if (actorData.FullIdentification != null)
        {
            EditorGUILayout.LabelField("Full Identification", EditorStyles.boldLabel);

            EditorGUILayout.LabelField("Actor ID", actorData.FullIdentification.ActorID.ToString());

            if (actorData.FullIdentification.ActorName != null)
            {
                EditorGUILayout.LabelField("Actor Name", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Name", actorData.FullIdentification.ActorName.Name);
                EditorGUILayout.LabelField("Surname", actorData.FullIdentification.ActorName.Surname);
                // Title
                // Available Titles
            }

            // ActorFamily

            EditorGUILayout.LabelField("Faction", actorData.FullIdentification.ActorFactionID.ToString());

            if (actorData.FullIdentification.Background != null)
            {
                //EditorGUILayout.LabelField("Background", EditorStyles.boldLabel);
                //EditorGUILayout.LabelField("Birthplace", actorData.FullIdentification.Background.Birthplace);
                //EditorGUILayout.LabelField("Birthdate", actorData.FullIdentification.Background.Birthdate.ToString());
                //EditorGUILayout.LabelField("Religion", actorData.FullIdentification.Background.Religion);
            }
        }

        if (actorData.GameObjectProperties != null)
        {
            _showGameObjectProperties = EditorGUILayout.Toggle("GameObjectProperties", _showGameObjectProperties);

            if (_showGameObjectProperties)
            {
                DrawGameObjectProperties(actorData.GameObjectProperties);
            }
        }

        if (actorData.CareerAndJobs != null)
        {
            actorData.CareerAndJobs.JobsActive = EditorGUILayout.Toggle("Jobs Active", actorData.CareerAndJobs.JobsActive);

            EditorGUILayout.LabelField("Employee Position", actorData.CareerAndJobs.EmployeePosition.ToString());
        }

        if (actorData.SpeciesAndPersonality != null)
        {
            _showSpeciesAndPersonality = EditorGUILayout.Toggle("Species and Personality", _showSpeciesAndPersonality);

            if (_showSpeciesAndPersonality)
            {
                DrawSpeciesAndPersonality(actorData.SpeciesAndPersonality);
            }
        }

        if (actorData.StatsAndAbilities != null)
        {
            EditorGUILayout.LabelField("Stats And Abilities", EditorStyles.boldLabel);
        }

        if (actorData.InventoryAndEquipment != null)
        {
            EditorGUILayout.LabelField("Inventory And Equipment", EditorStyles.boldLabel);

            var inventoryData = actorData.InventoryAndEquipment.InventoryData;
            var equipmentData = actorData.InventoryAndEquipment.EquipmentData;

            selectedInventoryIndex = GUILayout.SelectionGrid(selectedInventoryIndex, new string[] { "Inventory" }, 1);

            if (selectedInventoryIndex == 0)
            {
                DrawInventory(inventoryData);
            }

            selectedEquipmentIndex = GUILayout.SelectionGrid(selectedEquipmentIndex, new string[] { "Equipment" }, 1);

            if (selectedEquipmentIndex == 0)
            {
                DrawEquipment(equipmentData);
            }
        }

        if (actorData.ActorQuests != null)
        {
            //EditorGUILayout.LabelField("Actor Quests", EditorStyles.boldLabel);
            //EditorGUILayout.IntField("Active Quests", actorData.ActorQuests.ActiveQuests.Count);
        }
    }

    private void DrawGameObjectProperties(GameObjectProperties gameObjectProperties)
    {
        // Not sure if these are a good idea yet, since they'd just be for the SO.
        // EditorGUILayout.Vector3Field("Current Position", gameObjectProperties.ActorTransform.position);
        // EditorGUILayout.Vector3Field("Current Rotation", gameObjectProperties.ActorTransform.rotation.eulerAngles);
        // EditorGUILayout.Vector3Field("Current Scale", gameObjectProperties.ActorTransform.localScale);

        EditorGUILayout.Vector3Field("Last Saved Position", gameObjectProperties.LastSavedActorPosition);
        EditorGUILayout.Vector3Field("Last Saved Scale", gameObjectProperties.LastSavedActorScale);
        EditorGUILayout.Vector3Field("Last Saved Rotation", gameObjectProperties.LastSavedActorRotation.eulerAngles);
        EditorGUILayout.ObjectField("Mesh", gameObjectProperties.ActorMesh, typeof(Mesh), allowSceneObjects: true);
        EditorGUILayout.ObjectField("Material", gameObjectProperties.ActorMaterial, typeof(Material), allowSceneObjects: true);
    }

    private void DrawSpeciesAndPersonality(SpeciesAndPersonality speciesAndPersonality)
    {
        EditorGUILayout.LabelField("Species", speciesAndPersonality.ActorSpecies.ToString());
        EditorGUILayout.LabelField("Personality", speciesAndPersonality.ActorPersonality.ToString());
        // Add more details as needed
    }

    void DrawFactionRelationDetails(FactionRelationData data)
    {
        EditorGUILayout.LabelField("Faction ID", data.FactionID.ToString());
        EditorGUILayout.LabelField("Faction Name", data.FactionName);
        EditorGUILayout.LabelField("Faction Relation", data.FactionRelation.ToString());
    }

    void DrawInventory(InventoryData data)
    {
        EditorGUILayout.LabelField("Gold", $"{data.Gold}");

        if (data.InventoryItems.Count == 1)
        {
            EditorGUILayout.LabelField($"{data.InventoryItems[0].CommonStats_Item.ItemName}: {data.InventoryItems[0].CommonStats_Item.CurrentStackSize}");
        }
        else
        {
            inventoryItemScrollPos = EditorGUILayout.BeginScrollView(inventoryItemScrollPos, GUILayout.Height(Math.Min(200, data.InventoryItems.Count * 20)));

            for (int i = 0; i < data.InventoryItems.Count; i++)
            {
                EditorGUILayout.LabelField($"{data.InventoryItems[i].CommonStats_Item.ItemName}: {data.InventoryItems[i].CommonStats_Item.CurrentStackSize}");
            }

            EditorGUILayout.EndScrollView();
        }
    }

    void DrawEquipment(EquipmentData data)
    {

    }
}