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
    public override void OnInspectorGUI()
    {
        AllActors_SO myScript = (AllActors_SO)target;

        SerializedProperty allActorDataProp = serializedObject.FindProperty("AllActorData");
        EditorGUILayout.PropertyField(allActorDataProp, true);

        if (GUILayout.Button("Clear Actor Data"))
        {
            myScript.ClearActorData();
            EditorUtility.SetDirty(myScript);
        }

        DrawPropertiesExcluding(serializedObject, "AllActorData");

        serializedObject.ApplyModifiedProperties();
    }
}