using System;
using System.Collections.Generic;
using System.Linq;
using Jobs;
using ScriptableObjects;
using Tools;
using UnityEditor;
using UnityEngine;

namespace Actor
{
    [CreateAssetMenu(fileName = "ActorDataPreset_SO", menuName = "SOList/ActorDataPreset_SO")]
    public class ActorDataPreset_SO : Base_SO<Actor_Data>
    {
        public Actor_Data[] ActorDataPresets                                 => Objects;
        public Actor_Data   GetActorDataPreset(ActorDataPresetName actorDataPresetName) => GetObject_Master((uint)actorDataPresetName);

        public override uint GetObjectID(int id) => (uint)ActorDataPresets[id].ActorDataPresetName; // Use the ActorDataPresetName

        public void PopulateDefaultActorDataPresets()
        {
            if (_defaultActorDataPresets.Count == 0)
            {
                Debug.Log("No Default Actor Data Presets Found");
            }
        }
        protected override Dictionary<uint, Actor_Data> _populateDefaultObjects()
        {
            var defaultActorDataPresets = new Dictionary<uint, Actor_Data>();

            foreach (var actorDataPreset in ActorDataPreset_List.GetAllDefaultActorDataPresets())
            {
                defaultActorDataPresets.Add(actorDataPreset.Key, actorDataPreset.Value);
            }

            return defaultActorDataPresets;
        }
        
        Dictionary<uint, Actor_Data> _defaultActorDataPresets => DefaultObjects;
    }
    
     [CustomEditor(typeof(ActorDataPreset_SO))]
    public class AllActorDataPresets_SOEditor : Editor
    {
        int _selectedActorDataPresetIndex = -1;

        Vector2 _actorDataPresetScrollPos;

        bool _showActorDataPresetJobs;

        void _unselectAll()
        {
            _showActorDataPresetJobs         = false;
        }

        public override void OnInspectorGUI()
        {
            var allActorDataPresetsSO = (ActorDataPreset_SO)target;

            if (allActorDataPresetsSO?.ActorDataPresets is null || allActorDataPresetsSO.ActorDataPresets.Length is 0)
            {
                Debug.Log(allActorDataPresetsSO?.ActorDataPresets?.Length);
                allActorDataPresetsSO.PopulateDefaultActorDataPresets();
            }
            
            if (allActorDataPresetsSO?.ActorDataPresets is null || allActorDataPresetsSO.ActorDataPresets.Length is 0)
            {
                EditorGUILayout.LabelField("No ActorDataPresets Found", EditorStyles.boldLabel);
                return;
            }

            if (GUILayout.Button("Clear ActorDataPreset Data"))
            {
                allActorDataPresetsSO.ClearObjectData();
                EditorUtility.SetDirty(allActorDataPresetsSO);
            }

            if (GUILayout.Button("Unselect All")) _unselectAll();

            EditorGUILayout.LabelField("All ActorDataPresets", EditorStyles.boldLabel);

            var nonNullActorDataPresets = allActorDataPresetsSO.ActorDataPresets.Where(actorDataPreset =>
                actorDataPreset != null && actorDataPreset.ActorDataPresetName != 0).ToArray();

            _actorDataPresetScrollPos = EditorGUILayout.BeginScrollView(_actorDataPresetScrollPos,
                GUILayout.Height(Math.Min(200, nonNullActorDataPresets.Length * 20)));
            _selectedActorDataPresetIndex = GUILayout.SelectionGrid(_selectedActorDataPresetIndex, _getActorDataPresetNames(nonNullActorDataPresets), 1);
            EditorGUILayout.EndScrollView();

            if (_selectedActorDataPresetIndex >= 0 && _selectedActorDataPresetIndex < nonNullActorDataPresets.Length)
            {
                _drawActorDataPresetData(nonNullActorDataPresets[_selectedActorDataPresetIndex]);
            }
        }

        static string[] _getActorDataPresetNames(Actor_Data[] actorDataPresets) =>
            actorDataPresets.Select(actorDataPreset => actorDataPreset.FullIdentification.ToString()).ToArray();


        void _drawActorDataPresetData(Actor_Data actorDataPreset)
        {
            EditorGUILayout.LabelField("ActorDataPreset Data", EditorStyles.boldLabel);

            EditorGUILayout.LabelField("ActorDataPreset ID",   $"{actorDataPreset.ActorDataPresetName}");
            EditorGUILayout.LabelField("ActorDataPreset Name", $"{actorDataPreset.FullIdentification.ActorID}");

            // if (actorDataPreset.ActorDataPresetJobs != null)
            // {
            //     EditorGUILayout.LabelField("ActorDataPresetJobs", EditorStyles.boldLabel);
            //
            //     var ActorDataPresetJobs = actorDataPreset.ActorDataPresetJobs;
            //
            //     _showActorDataPresetJobs = EditorGUILayout.Toggle("ActorDataPresetJobs", _showActorDataPresetJobs);
            //
            //     if (_showActorDataPresetJobs)
            //     {
            //         _drawActorDataPresetJobs(ActorDataPresetJobs.ToList());
            //     }
            // }
        }

        void _drawActorDataPresetJobs(List<JobName> ActorDataPresetJobs)
        {
            if (ActorDataPresetJobs.Count == 1)
            {
                EditorGUILayout.LabelField(ActorDataPresetJobs[0].ToString());
            }
            else
            {
                _actorDataPresetScrollPos = EditorGUILayout.BeginScrollView(_actorDataPresetScrollPos,
                    GUILayout.Height(Math.Min(200, ActorDataPresetJobs.Count * 20)));

                try
                {
                    foreach (var jobName in ActorDataPresetJobs)
                    {
                        EditorGUILayout.LabelField(jobName.ToString());
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
    }
}
