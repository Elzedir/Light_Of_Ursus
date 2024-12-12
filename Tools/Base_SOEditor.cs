using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Tools
{
    [CustomEditor(typeof(Base_SO<>), true)]
    public abstract class Base_SOEditor<T> : Editor where T : ScriptableObject
    {
        int               _selectedBaseObjectIndex = -1;
        Base_SO<T>        _so;
        public Base_SO<T> SO => _so ??= (Base_SO<T>)target;

        public override void OnInspectorGUI()
        {
            if (SO?.BaseObjects is null || SO.BaseObjects.Length is 0)
            {
                EditorGUILayout.LabelField("No BaseObjects Found", EditorStyles.boldLabel);
                return;
            }

            EditorGUILayout.LabelField("All BaseObjects", EditorStyles.boldLabel);

            var nonNullBaseObjects = SO.BaseObjects.Where(o =>
                o              != null &&
                o.BaseObjectID != 0).ToArray();

            var category = SO.AllCategories[0];
            category.ScrollPosition = EditorGUILayout.BeginScrollView(SO.AllCategories[0].ScrollPosition,
                GUILayout.Height(Mathf.Min(200, nonNullBaseObjects.Length * 20)));
            _selectedBaseObjectIndex = GUILayout.SelectionGrid(_selectedBaseObjectIndex, _getBaseObjectNames(nonNullBaseObjects), 1);
            EditorGUILayout.EndScrollView();
            SO.AllCategories[0] = category;

            if (_selectedBaseObjectIndex < 0 || _selectedBaseObjectIndex >= nonNullBaseObjects.Length) return;

            var selectedBaseObject = nonNullBaseObjects[_selectedBaseObjectIndex];
            _drawBaseObjectData(selectedBaseObject);
        }

        static string[] _getBaseObjectNames(Base_Object<T>[] baseObjects)
        {
            return baseObjects.Select(o => $"{o.BaseObjectID}").ToArray();
        }

        void _drawBaseObjectData(Base_Object<T> selectedBaseObject)
        {
            for (var i = 0; i < selectedBaseObject.AllDataCategories.Count; i++)
            {
                var baseObjectData = selectedBaseObject.AllDataCategories[i];
                _drawData(baseObjectData, i);
            }
        }

        void _drawData((DataDisplayType DataDisplayType, DataToDisplay DataToDisplay) baseObjectData, int i)
        {
            var category = SO.AllCategories[i];
            var data     = baseObjectData.DataToDisplay.Data;

            if (!category.ShowCategory) return;

            switch (baseObjectData.DataDisplayType)
            {
                case DataDisplayType.Single:
                    foreach (var (key, value) in data)
                    {
                        EditorGUILayout.LabelField(key, value);
                    }

                    break;

                case DataDisplayType.ScrollView:

                    category.ScrollPosition = EditorGUILayout.BeginScrollView(category.ScrollPosition,
                        GUILayout.Height(Mathf.Min(200, baseObjectData.DataToDisplay.Data.Count * 20)));

                    try
                    {
                        foreach (var (key, value) in data)
                        {
                            EditorGUILayout.LabelField(key, value);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Error: {e.Message}");
                    }
                    finally
                    {
                        EditorGUILayout.EndScrollView();

                        SO.AllCategories[i] = category;
                    }

                    break;
            }
        }
    }
}