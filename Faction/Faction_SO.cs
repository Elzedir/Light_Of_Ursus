using System;
using System.Collections.Generic;
using System.Linq;
using ScriptableObjects;
using UnityEditor;
using UnityEngine;

namespace Faction
{
    [CreateAssetMenu(fileName = "AllFactions_SO", menuName = "SOList/AllFactions_SO")]
    [Serializable]
    public class Faction_SO : Base_SO<Faction_Data>
    {
        public Faction_Data[]                      Factions                             => Objects;
        public Faction_Data                        GetFaction_Data(uint      factionID) => GetObject_Master(factionID);
        public Dictionary<uint, Faction_Component> FactionComponents = new();

        public Faction_Component GetFaction_Component(uint factionID)
        {
            if (FactionComponents.TryGetValue(factionID, out var component))
            {
                return component;
            }   
            
            Debug.LogError($"Faction with ID {factionID} not found in Faction_SO.");
            return null;
        }

        public override uint GetObjectID(int id) => Factions[id].FactionID;

        public void UpdateFaction(uint factionID, Faction_Data faction_Data) => UpdateObject(factionID, faction_Data);
        public void UpdateAllFactions(Dictionary<uint, Faction_Data> allFactions) => UpdateAllObjects(allFactions);

        public void PopulateSceneFactions()
        {
            var allFactionComponents = FindObjectsByType<Faction_Component>(FindObjectsSortMode.None);
            var allFactionData =
                allFactionComponents.ToDictionary(faction => faction.FactionID, faction => faction.FactionData);

            UpdateAllFactions(allFactionData);
        }

        protected override Dictionary<uint, Faction_Data> _populateDefaultObjects()
        {
            var defaultFactions = new Dictionary<uint, Faction_Data>();

            foreach (var defaultFaction in Faction_List.DefaultFactions)
            {
                defaultFactions.Add(defaultFaction.Key, new Faction_Data(defaultFaction.Value));
            }

            return defaultFactions;
        }

        static uint _lastUnusedFactionID = 1;

        public uint GetUnusedFactionID()
        {
            while (ObjectIndexLookup.ContainsKey(_lastUnusedFactionID))
            {
                _lastUnusedFactionID++;
            }

            return _lastUnusedFactionID;
        }
    }

    [CustomEditor(typeof(Faction_SO))]
    public class AllFactions_SOEditor : Editor
    {
        int  _selectedFactionIndex = -1;
        bool _showFactionRelations;
        bool _showActors;

        Vector2 _factionScrollPos;
        Vector2 _actorScrollPos;
        Vector2 _factionRelationScrollPos;

        public override void OnInspectorGUI()
        {
            var factionSO = (Faction_SO)target;

            EditorGUILayout.LabelField("All Factions", EditorStyles.boldLabel);
            _factionScrollPos = EditorGUILayout.BeginScrollView(_factionScrollPos,
                GUILayout.Height(Mathf.Min(200, factionSO.Factions.Length * 20)));
            _selectedFactionIndex = GUILayout.SelectionGrid(_selectedFactionIndex, _getFactionNames(factionSO), 1);
            EditorGUILayout.EndScrollView();

            if (_selectedFactionIndex < 0 || _selectedFactionIndex >= factionSO.Factions.Length) return;

            var selectedFactionData = factionSO.Factions[_selectedFactionIndex];
            _drawFactionAdditionalData(selectedFactionData);
        }

        static string[] _getFactionNames(Faction_SO factionSO)
        {
            return factionSO.Factions.Select(f => f.FactionName).ToArray();
        }

        void _drawFactionAdditionalData(Faction_Data selectedFaction)
        {
            EditorGUILayout.LabelField("Faction Data", EditorStyles.boldLabel);
            
            EditorGUILayout.LabelField("Faction Name", $"{selectedFaction.FactionName}");
            EditorGUILayout.LabelField("Faction ID",   $"{selectedFaction.FactionID}");

            if (selectedFaction.AllFactionActorIDs != null)
            {
                _showActors = EditorGUILayout.Toggle("Actors", _showActors);

                if (_showActors)
                {
                    _drawActorAdditionalData(selectedFaction.AllFactionActorIDs);
                }
            }

            if (selectedFaction.AllFactionRelations != null)
            {
                _showFactionRelations = EditorGUILayout.Toggle("Faction Relations", _showFactionRelations);

                if (_showFactionRelations)
                {
                    _drawFactionRelationDetails(selectedFaction.AllFactionRelations);
                }
            }
        }

        void _drawFactionRelationDetails(List<FactionRelationData> data)
        {
            _factionRelationScrollPos = EditorGUILayout.BeginScrollView(_factionRelationScrollPos, GUILayout.Height(Math.Min(200, data.Count * 20)));
        
            foreach (var relation in data)
            {
                EditorGUILayout.LabelField("Faction ID",       relation.FactionID.ToString());
                EditorGUILayout.LabelField("Faction Name",     relation.FactionName);
                EditorGUILayout.LabelField("Faction Relation", relation.FactionRelation.ToString());
            }

            EditorGUILayout.EndScrollView();
        }

        void _drawActorAdditionalData(HashSet<uint> actorIDs)
        {
            _actorScrollPos = EditorGUILayout.BeginScrollView(_actorScrollPos, GUILayout.Height(Math.Min(200, actorIDs.Count * 20)));

            try
            {
                foreach (var actorID in actorIDs)
                {
                    EditorGUILayout.LabelField("Actor Data", EditorStyles.boldLabel);
                    EditorGUILayout.LabelField("Actor ID",   actorID.ToString());
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
}