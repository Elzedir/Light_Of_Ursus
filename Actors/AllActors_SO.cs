using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "AllActors_SO", menuName = "SOList/AllActors_SO")]
public class AllActors_SO : ScriptableObject
{
    public List<ActorData> AllActorData;

    public List<int> AllActorIDs; //Can change later to a hashset for efficiency but for now need display
    public int LastUnusedActorID = 0;

    // For now, save all data of every actor to this list, but later find a better way to save the info as thousands
    // or tens of thousands of actors would become too much and inefficient.

    public void PrepareToInitialise()
    {
        Manager_Initialisation.OnInitialiseAllActorSO += _initialise;
    }

    void _initialise()
    {
        _addAllActorDataFromJSON();

        _addAdditionalActorDataFromScene();

        _addAddAllEditorActorIDs();

        _addAllRuntimeActorIDs();
    }

    void _addAllActorDataFromJSON()
    {
        //Load from JSON
    }

    void _addAdditionalActorDataFromScene()
    {
        foreach (var actor in FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None).OfType<Actor_Base>().ToList())
        {
            if (!AllActorData.Any(a => a.ActorID == actor.ActorData.ActorID))
            {
                Debug.Log($"Actor: {actor.ActorData.ActorName.GetName()} with ID: {actor.ActorData.ActorID} was not in AllActorData");

                AllActorData.Add(actor.ActorData);
            }
        }

        for (int i = 0; i < AllActorData.Count; i++)
        {
            AllActorData[i].InitialiseActorData();
        }
    }

    void _addAddAllEditorActorIDs()
    {
        AllActorIDs.Clear();
        LastUnusedActorID = 0;

        foreach (var actorData in AllActorData)
        {
            AllActorIDs.Add(actorData.ActorID);
        }
    }

    void _addAllRuntimeActorIDs()
    {

    }

    public void AddToOrUpdateAllActorsDataList(ActorData actorData)
    {
        var existingActor = AllActorData.FirstOrDefault(a => a.ActorID == actorData.ActorID);

        if (existingActor == null) AllActorData.Add(actorData);
        else AllActorData[AllActorData.IndexOf(existingActor)] = actorData;
    }

    public ActorData GetActorData(int actorID)
    {
        if (!AllActorData.Any(a => a.ActorID == actorID)) { Debug.Log($"AllActorData does not contain ActorID: {actorID}"); return null; }

        return AllActorData.FirstOrDefault(a => a.ActorID == actorID);
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

    public void ClearActorData()
    {
        AllActorData.Clear();
    }
}

[CustomEditor(typeof(AllActors_SO))]
public class AllActorsSOEditor : Editor
{
    private int selectedActorIndex = -1;

    private Vector2 actorScrollPos;

    public override void OnInspectorGUI()
    {
        AllActors_SO allActorsSO = (AllActors_SO)target;

        if (GUILayout.Button("Clear Actor Data"))
        {
            allActorsSO.ClearActorData();
            EditorUtility.SetDirty(allActorsSO);
        }

        EditorGUILayout.LabelField("Actors", EditorStyles.boldLabel);
        actorScrollPos = EditorGUILayout.BeginScrollView(actorScrollPos, GUILayout.Height(GetListHeight(allActorsSO.AllActorData.Count)));
        selectedActorIndex = GUILayout.SelectionGrid(selectedActorIndex, GetActorNames(allActorsSO), 1);
        EditorGUILayout.EndScrollView();

        //if (selectedRegionIndex >= 0 && selectedRegionIndex < allActorsSO.AllRegionData.Count)
        //{
        //    RegionData selectedRegion = allActorsSO.AllRegionData[selectedRegionIndex];
        //    EditorGUILayout.LabelField($"Cities in {selectedRegion.RegionName}", EditorStyles.boldLabel);
        //    cityScrollPos = EditorGUILayout.BeginScrollView(cityScrollPos, GUILayout.Height(GetListHeight(selectedRegion.AllCityData.Count)));
        //    selectedCityIndex = GUILayout.SelectionGrid(selectedCityIndex, GetCityNames(selectedRegion), 1);
        //    EditorGUILayout.EndScrollView();
        //}

        DrawAdditionalFields(allActorsSO);
    }

    private string[] GetActorNames(AllActors_SO allActorsSO)
    {
        return allActorsSO.AllActorData.Select(r => r.ActorName.GetName()).ToArray();
    }

    private float GetListHeight(int itemCount)
    {
        return Mathf.Min(200, itemCount * 20);
    }

    private void DrawAdditionalFields(AllActors_SO allActorsSO)
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Additional Data", EditorStyles.boldLabel);

        //else if (selectedCityIndex >= 0)
        //{
        //    DrawCityAdditionalData(allActorsSO.AllRegionData[selectedRegionIndex]
        //        .AllCityData[selectedCityIndex]);
        //}
        if (selectedActorIndex >= 0)
        {
            DrawActorAdditionalData(allActorsSO.AllActorData[selectedActorIndex]);
        }
        else
        {
            DrawGlobalAdditionalData(allActorsSO);
        }
    }

    private void DrawGlobalAdditionalData(AllActors_SO allActorsSO)
    {
        EditorGUILayout.LabelField("Global Data", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Actor IDs", string.Join(", ", allActorsSO.AllActorIDs));

        allActorsSO.LastUnusedActorID = EditorGUILayout.IntField("Last Unused Region ID", allActorsSO.LastUnusedActorID);
    }

    private void DrawActorAdditionalData(ActorData actorData)
    {
        EditorGUILayout.LabelField("Actor Data", EditorStyles.boldLabel);

        EditorGUILayout.LabelField("Actor ID", actorData.ActorID.ToString());

        EditorGUILayout.LabelField("Actor Name", $"{actorData.ActorName.Name} {actorData.ActorName.Surname}");

        actorData.OverwriteDataInActor = EditorGUILayout.Toggle("Overwrite Data In Actor", actorData.OverwriteDataInActor);

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

            EditorGUILayout.LabelField("Faction", actorData.FullIdentification.ActorFaction.ToString());

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
            //EditorGUILayout.LabelField("Game Object Properties", EditorStyles.boldLabel);
            //EditorGUILayout.Vector3Field("Position", actorData.GameObjectProperties.ActorPosition);
            //EditorGUILayout.Vector3Field("Scale", actorData.GameObjectProperties.ActorScale);
            //EditorGUILayout.Vector3Field("Rotation", actorData.GameObjectProperties.ActorRotation.eulerAngles);
            //EditorGUILayout.ObjectField("Mesh", actorData.GameObjectProperties.ActorMesh, typeof(Mesh), allowSceneObjects: true);
            //EditorGUILayout.ObjectField("Material", actorData.GameObjectProperties.ActorMaterial, typeof(Material), allowSceneObjects: true);
        }

        if (actorData.CareerAndJobs != null)
        {
            //EditorGUILayout.LabelField("Career And Jobs", EditorStyles.boldLabel);
            //EditorGUILayout.LabelField("Career", actorData.CareerAndJobs.ActorCareer.ToString());
            //EditorGUILayout.Toggle("Jobs Active", actorData.CareerAndJobs.JobsActive);
        }

        if (actorData.SpeciesAndPersonality != null)
        {
            //EditorGUILayout.LabelField("Species And Personality", EditorStyles.boldLabel);
            //EditorGUILayout.LabelField("Species", actorData.SpeciesAndPersonality.ActorSpecies.ToString());
            //EditorGUILayout.LabelField("Personality", actorData.SpeciesAndPersonality.ActorPersonality.ToString());
        }

        if (actorData.StatsAndAbilities != null)
        {
            //EditorGUILayout.LabelField("Stats And Abilities", EditorStyles.boldLabel);
        }

        if (actorData.InventoryAndEquipment != null)
        {
            //EditorGUILayout.LabelField("Inventory And Equipment", EditorStyles.boldLabel);
        }

        if (actorData.ActorQuests != null)
        {
            //EditorGUILayout.LabelField("Actor Quests", EditorStyles.boldLabel);
            //EditorGUILayout.IntField("Active Quests", actorData.ActorQuests.ActiveQuests.Count);
        }
    }
}