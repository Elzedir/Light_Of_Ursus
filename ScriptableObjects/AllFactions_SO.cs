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

    public void ClearFactionData() => AllFactionData.Clear();
}

[CustomEditor(typeof(AllFactions_SO))]
public class AllFactions_SOEditor : Editor
{
    int _selectedFactionIndex = -1;
    bool _showFactionRelations = false;
    bool _showActors = false;

    Vector2 _factionScrollPos;
    Vector2 _actorScrollPos;
    Vector2 _factionRelationScrollPos;

    public override void OnInspectorGUI()
    {
        AllFactions_SO allFactionSO = (AllFactions_SO)target;

        if (GUILayout.Button("Clear Faction Data"))
        {
            allFactionSO.ClearFactionData();
            EditorUtility.SetDirty(allFactionSO);
        }

        EditorGUILayout.LabelField("All Factions", EditorStyles.boldLabel);
        _factionScrollPos = EditorGUILayout.BeginScrollView(_factionScrollPos, GUILayout.Height(Math.Min(200, allFactionSO.AllFactionData.Count * 20)));
        _selectedFactionIndex = GUILayout.SelectionGrid(_selectedFactionIndex, GetFactionNames(allFactionSO), 1);
        EditorGUILayout.EndScrollView();

        if (_selectedFactionIndex >= 0 && _selectedFactionIndex < allFactionSO.AllFactionData.Count)
        {
            DrawFactionData(allFactionSO.AllFactionData[_selectedFactionIndex]);
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

        if (factionData.AllFactionActorIDs != null)
        {
            _showActors = EditorGUILayout.Toggle("Actors", _showActors);

            if (_showActors)
            {
                DrawActorAdditionalData(factionData.AllFactionActorIDs);
            }
        }

        EditorGUILayout.LabelField("All Faction Relations", EditorStyles.boldLabel);

        if (factionData.AllFactionRelations != null)
        {
            _showFactionRelations = EditorGUILayout.Toggle("Faction Relations", _showFactionRelations);

            if (_showFactionRelations)
            {
                DrawFactionRelationDetails(factionData.AllFactionRelations);
            }
        }
    }

    void DrawFactionRelationDetails(List<FactionRelationData> data)
    {
        _factionRelationScrollPos = EditorGUILayout.BeginScrollView(_factionRelationScrollPos, GUILayout.Height(Math.Min(200, data.Count * 20)));
        
        foreach (var relation in data)
        {
            EditorGUILayout.LabelField("Faction ID", relation.FactionID.ToString());
            EditorGUILayout.LabelField("Faction Name", relation.FactionName);
            EditorGUILayout.LabelField("Faction Relation", relation.FactionRelation.ToString());
        }

        EditorGUILayout.EndScrollView();
    }

    void DrawActorAdditionalData(HashSet<int> actorIDs)
    {
        _actorScrollPos = EditorGUILayout.BeginScrollView(_actorScrollPos, GUILayout.Height(Math.Min(200, actorIDs.Count * 20)));

        try
        {
            foreach (var actorID in actorIDs)
            {
                EditorGUILayout.LabelField("Actor Data", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Actor ID", actorID.ToString());
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error: {e.Message}");
        }
        finally
        {
            EditorGUILayout.EndScrollView();
        }
    }
}