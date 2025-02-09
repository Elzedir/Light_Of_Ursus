using System;
using System.Linq;
using Managers;
using UnityEditor;
using UnityEngine;

namespace Tools
{
    [CustomEditor(typeof(Data_SO<>), true)]
    public abstract class Data_SOEditor<T> : Editor where T : class
    {
        int _selectedBaseIndex = -1;
        protected Data_SO<T> _so;
        public abstract Data_SO<T> SO { get; }

        double _lastRefreshTime;
        
        Data<T>[] _nonNullDataObjects;

        public override void OnInspectorGUI()
        {
            SO.ToggleMissingDataDebugs = GUILayout.Toggle(SO.ToggleMissingDataDebugs, "Toggle Missing Data Debugs");
            
            if (!Manager_Game.Initialised) return;
            
            if (SO?.Data.Length is null or 0)
            {
                EditorGUILayout.LabelField("No BaseObjects Found", EditorStyles.boldLabel);
                return;
            }
            
            EditorGUILayout.LabelField("All BaseObjects", EditorStyles.boldLabel);
            
            _nonNullDataObjects ??= Array.Empty<Data<T>>();
                
            _nonNullDataObjects = SO.Data.Where(data_Object =>
                data_Object != null &&
                data_Object.DataID != 0).ToArray();

            SO.ScrollPosition = EditorGUILayout.BeginScrollView(SO.ScrollPosition,
                GUILayout.Height(Mathf.Min(200, _nonNullDataObjects.Length * 20)));
            _selectedBaseIndex =
                GUILayout.SelectionGrid(_selectedBaseIndex, _getBaseObjectNames(_nonNullDataObjects), 1);
            EditorGUILayout.EndScrollView();

            if (_selectedBaseIndex < 0 || _selectedBaseIndex >= _nonNullDataObjects.Length) return;

            var selectedDataObject = _nonNullDataObjects[_selectedBaseIndex];
            _drawDataToDisplay(selectedDataObject.GetDataToDisplay(SO.ToggleMissingDataDebugs), 50, false );
        }

        static string[] _getBaseObjectNames(Data<T>[] baseObjects)
        {
            return baseObjects.Select(base_Object => $"{base_Object.DataTitle}").ToArray();
        }

        static void _drawDataToDisplay(DataToDisplay dataToDisplay, int iteration, bool showToggle = true)
        {
            if (iteration-- <= 0)
            {
                Debug.LogWarning("DataToDisplay iteration limit reached.");
                return;
            }

            if (showToggle)
            {
                if (!(dataToDisplay.ShowData = EditorGUILayout.Toggle($"{dataToDisplay.Title}", dataToDisplay.ShowData)))
                    return;
            }
            else
            {
                dataToDisplay.ShowData = true;
            }

            _drawStringData(dataToDisplay);
            _drawSubData(dataToDisplay, iteration);
            _drawInteractableData(dataToDisplay, iteration);
        }

        static void _drawStringData(DataToDisplay dataToDisplay)
        {
            if (dataToDisplay.AllStringData == null) return;
            
            foreach (var stringGroup in dataToDisplay.AllStringData)
            {
                if (stringGroup.Value is null) continue;
                
                EditorGUILayout.LabelField(stringGroup.Key, EditorStyles.boldLabel);
                GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(5));
                
                foreach (var stringData in stringGroup.Value)
                {
                    var labelStyle = new GUIStyle(GUI.skin.label)
                    {
                        wordWrap = true
                    };
                    
                    GUILayout.Label($"{stringData.Key}: {stringData.Value}", labelStyle);
                }
                
                GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
                GUILayout.Space(10);
            }
        }

        static void _drawSubData(DataToDisplay dataToDisplay, int iteration)
        {
            var allSubData = dataToDisplay.AllSubData;

            if (allSubData is null || allSubData.Count <= 0) return;

            foreach (var subData in allSubData.Values.Where(subData => subData is not null))
            {
                _drawDataToDisplay(subData, iteration);
            }
        }

        static void _drawInteractableData(DataToDisplay dataToDisplay, int iteration)
        {
            var allInteractableData = dataToDisplay.AllInteractableData;
            
            if (allInteractableData is null || allInteractableData.Count is 0) return;

            dataToDisplay.ScrollPosition = EditorGUILayout.BeginScrollView(dataToDisplay.ScrollPosition,
                GUILayout.Height(Mathf.Min(200, allInteractableData.Count * 20)));
            dataToDisplay.SelectedIndex = GUILayout.SelectionGrid(dataToDisplay.SelectedIndex,
                allInteractableData
                    .Where(subData => subData.Value != null)
                    .Select(subData => subData.Value.Title).ToArray(), 1);

            EditorGUILayout.EndScrollView();

            if (allInteractableData.Count is 0 ||
                dataToDisplay.SelectedIndex < 0 ||
                dataToDisplay.SelectedIndex >= allInteractableData.Count) return;

            var selectedSubData = allInteractableData.ElementAt(dataToDisplay.SelectedIndex).Value;
            _drawDataToDisplay(selectedSubData, iteration);
        }
    }
}