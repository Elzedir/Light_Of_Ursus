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
    public class Faction_SO : Base_SO<Faction_Component>
    {
        public Faction_Component[] Factions                         => Objects;
        public Faction_Data        GetFaction_Data(uint      factionID) => GetObject_Master(factionID).FactionData;
        public Faction_Component   GetFaction_Component(uint factionID) => GetObject_Master(factionID);

        public Faction_Data[] Save_SO()
        {
            return Factions.Select(faction => faction.FactionData).ToArray();
        }

        public void Load_SO(Faction_Data[] factionData)
        {
            foreach (var faction in factionData)
            {
                if (!_faction_Components.ContainsKey(faction.FactionID))
                {
                    Debug.LogError($"Faction with ID {faction.FactionID} not found in Faction_SO.");
                    continue;
                }

                _faction_Components[faction.FactionID].FactionData = faction;
            }

            LoadSO(_faction_Components.Values.ToArray());
        }

        public override uint GetObjectID(int id) => Factions[id].FactionID;

        public void UpdateFaction(uint factionID, Faction_Component faction_Component) => UpdateObject(factionID, faction_Component);
        public void UpdateAllFactions(Dictionary<uint, Faction_Component> allFactions) => UpdateAllObjects(allFactions);

        public void PopulateSceneFactions()
        {
            var allFactionComponents = FindObjectsByType<Faction_Component>(FindObjectsSortMode.None);
            var allFactionData =
                allFactionComponents.ToDictionary(faction => faction.FactionID);

            UpdateAllFactions(allFactionData);
        }

        protected override Dictionary<uint, Faction_Component> _populateDefaultObjects()
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

        Dictionary<uint, Faction_Component> _faction_Components => DefaultObjects;
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
            return factionSO.Factions.Select(c => c.FactionData.FactionName).ToArray();
        }

        void _drawFactionAdditionalData(Faction_Component selectedFaction)
        {
            EditorGUILayout.LabelField("Faction Data", EditorStyles.boldLabel);
            
            EditorGUILayout.LabelField("Faction Name", $"{selectedFaction.FactionData.FactionName}");
            EditorGUILayout.LabelField("Faction ID",   $"{selectedFaction.FactionID}");

            if (selectedFaction.FactionData.AllFactionActorIDs != null)
            {
                _showActors = EditorGUILayout.Toggle("Actors", _showActors);

                if (_showActors)
                {
                    _drawActorAdditionalData(selectedFaction.FactionData.AllFactionActorIDs);
                }
            }

            if (selectedFaction.FactionData.AllFactionRelations != null)
            {
                _showFactionRelations = EditorGUILayout.Toggle("Faction Relations", _showFactionRelations);

                if (_showFactionRelations)
                {
                    _drawFactionRelationDetails(selectedFaction.FactionData.AllFactionRelations);
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