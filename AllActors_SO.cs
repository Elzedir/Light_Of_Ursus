using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "AllActors_SO", menuName = "SOList/AllActors_SO")]
[Serializable]
public class AllActors_SO : ScriptableObject
{
    public List<ActorData> AllActorData;

    public void ClearActorData() => AllActorData.Clear();

    public void CallSaveData() { Manager_Data.Instance.SaveGame(""); Debug.Log("Saved Game"); }
    public void CallLoadData() { Manager_Data.Instance.LoadGame(""); Debug.Log("Loaded Game"); }
}

[CustomEditor(typeof(AllActors_SO))]
public class AllActors_SOEditor : Editor
{
    int _selectedActorIndex = -1;
    int SelectedActorIndex { get { return _selectedActorIndex; } set { if (_selectedActorIndex == value) return; _selectedActorIndex = value; _resetIndexes(1); } }
    bool _showGameObjectProperties = false;
    bool _showSpeciesAndPersonality = false;

    Vector2 _actorScrollPos;
    
    void _resetIndexes(int i = -1)
    {
        _showGameObjectProperties = false;
        _showSpeciesAndPersonality = false;
        if (i == 1) return;
        _selectedActorIndex = -1;
    }

    public override void OnInspectorGUI()
    {
        AllActors_SO allActorSO = (AllActors_SO)target;

        if (GUILayout.Button("Save Data")) allActorSO.CallSaveData();
        if (GUILayout.Button("Load Data")) allActorSO.CallLoadData();

        if (GUILayout.Button("Clear Actor Data"))
        {
            _resetIndexes();
            allActorSO.ClearActorData();
            EditorUtility.SetDirty(allActorSO);
        }

        if (GUILayout.Button("Unselect All")) _resetIndexes();

        EditorGUILayout.LabelField("All Actors", EditorStyles.boldLabel);
        _actorScrollPos = EditorGUILayout.BeginScrollView(_actorScrollPos, GUILayout.Height(Math.Min(200, allActorSO.AllActorData.Count * 20)));
        SelectedActorIndex = GUILayout.SelectionGrid(SelectedActorIndex, GetActorNames(allActorSO), 1);
        EditorGUILayout.EndScrollView();

        if (SelectedActorIndex >= 0 && SelectedActorIndex < allActorSO.AllActorData.Count)
        {
            DrawActorData(allActorSO.AllActorData[SelectedActorIndex]);
        }
    }

    private string[] GetActorNames(AllActors_SO allActorsSO)
    {
        return allActorsSO.AllActorData.Select(a => $"{a.ActorID}: {a.ActorName.GetName()}").ToArray();
    }

    bool _showInventory = false;
    bool _showEquipment = false;
    Vector2 inventoryItemScrollPos;

    private void DrawActorData(ActorData actorData)
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

            _showInventory = EditorGUILayout.Toggle("Inventory", _showInventory);

            if (_showInventory)
            {
                DrawInventory(inventoryData);
            }

            var equipmentData = actorData.InventoryAndEquipment.EquipmentData;

            _showEquipment = EditorGUILayout.Toggle("Equipment", _showEquipment);

            if (_showEquipment)
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

    void DrawInventory(InventoryData data)
    {
        EditorGUILayout.LabelField("Gold", $"{data.Gold}");

        if (data.AllInventoryItems.Count == 1)
        {
            EditorGUILayout.LabelField($"{data.AllInventoryItems[0].ItemName}: {data.AllInventoryItems[0].ItemAmount}");
        }
        else
        {
            inventoryItemScrollPos = EditorGUILayout.BeginScrollView(inventoryItemScrollPos, GUILayout.Height(Math.Min(200, data.AllInventoryItems.Count * 20)));

            try
            {
                for (int i = 0; i < data.AllInventoryItems.Count; i++)
                {
                    EditorGUILayout.LabelField($"{data.AllInventoryItems[i].ItemName}: {data.AllInventoryItems[i].ItemAmount}");
                }
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
            finally
            {
                EditorGUILayout.EndScrollView();
            }
        }
    }

    void DrawEquipment(EquipmentData data)
    {

    }
}