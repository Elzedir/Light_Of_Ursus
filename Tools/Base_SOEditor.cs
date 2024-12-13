using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Tools
{
    [CustomEditor(typeof(Base_SO_Test<>), true)]
    public abstract class Base_SOEditor<T> : Editor where T : class
    {
        int                             _selectedBaseObjectIndex = -1;
        protected       Base_SO_Test<T> _so;
        public abstract Base_SO_Test<T> SO { get; }

        public override void OnInspectorGUI()
        {
            if (SO?.BaseObjects is null)
            {
                EditorGUILayout.LabelField($"{SO} is null or {SO?.BaseObjects} is null", EditorStyles.boldLabel);
                return;
            }
            
            if (GUILayout.Button("Refresh All Objects")) SO.RefreshBaseObjects();

            if (SO.BaseObjects.Length is 0)
            {
                EditorGUILayout.LabelField("No BaseObjects Found", EditorStyles.boldLabel);
                return;
            }
            
            EditorGUILayout.LabelField("All BaseObjects", EditorStyles.boldLabel);

            var nonNullBaseObjects = SO.BaseObjects.Where(base_Object =>
                base_Object              != null &&
                base_Object.BaseObjectID != 0).ToArray();
            
            SO.BaseScrollPosition = EditorGUILayout.BeginScrollView(SO.BaseScrollPosition,
                GUILayout.Height(Mathf.Min(200, nonNullBaseObjects.Length * 20)));
            _selectedBaseObjectIndex = GUILayout.SelectionGrid(_selectedBaseObjectIndex, _getBaseObjectNames(nonNullBaseObjects), 1);
            EditorGUILayout.EndScrollView();

            if (_selectedBaseObjectIndex < 0 || _selectedBaseObjectIndex >= nonNullBaseObjects.Length) return;

            var selectedBaseObject = nonNullBaseObjects[_selectedBaseObjectIndex];
            _drawBaseObjectData(selectedBaseObject);
        }

        static string[] _getBaseObjectNames(Base_Object<T>[] baseObjects)
        {
            return baseObjects.Select(base_Object => $"{base_Object.BaseObjectTitle}").ToArray();
        }

        void _drawBaseObjectData(Base_Object<T> selectedBaseObject)
        {
            for (uint i = 0; i < selectedBaseObject.AllDataToDisplay.Count; i++)
            {
                var dataToDisplay = selectedBaseObject.AllDataToDisplay[i];
                _drawData(dataToDisplay);
            }
        }

        void _drawData(DataToDisplay dataToDisplay)
        {
            switch (dataToDisplay.DataDisplayType)
            {
                case DataDisplayType.Item:
                    foreach (var data in dataToDisplay.Data)
                    {
                        EditorGUILayout.LabelField(data);
                    }

                    break;

                case DataDisplayType.List:

                    dataToDisplay.ScrollPosition = EditorGUILayout.BeginScrollView(dataToDisplay.ScrollPosition,
                        GUILayout.Height(Mathf.Min(200, dataToDisplay.Data.Count * 20)));

                    try
                    {
                        foreach (var data in dataToDisplay.Data)
                        {
                            EditorGUILayout.LabelField(data);
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

                    break;
                case DataDisplayType.SelectableList:
                    
                    try
                    {
                        dataToDisplay.ScrollPosition = EditorGUILayout.BeginScrollView(dataToDisplay.ScrollPosition,
                            GUILayout.Height(Mathf.Min(200, dataToDisplay.Data.Count * 20)));
                    
                        dataToDisplay.SelectedIndex = GUILayout.SelectionGrid(dataToDisplay.SelectedIndex,
                            dataToDisplay.Data.Select(data => $"{data}").ToArray(), 1);
                        EditorGUILayout.EndScrollView();
                        var nonNullData = dataToDisplay.Data.Where(data => !string.IsNullOrEmpty(data)).ToArray();
                        
                        if (_selectedBaseObjectIndex < 0 || _selectedBaseObjectIndex >= nonNullData.Length) return;

                        var selectedData = nonNullData[_selectedBaseObjectIndex];
                        
                        EditorGUILayout.LabelField(selectedData);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Error: {e.Message}");
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}