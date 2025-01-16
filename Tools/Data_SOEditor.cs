using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Tools
{
    [CustomEditor(typeof(Data_SO<>), true)]
    public abstract class Data_SOEditor<T> : Editor where T : class
    {
        int                        _selectedBaseIndex = -1;
        protected       Data_SO<T> _so;
        public abstract Data_SO<T> SO { get; }

        double _lastRefreshTime;

        public override void OnInspectorGUI()
        {
            if (SO?.Data is null)
            {
                EditorGUILayout.LabelField($"{SO} is null or {SO?.Data} is null", EditorStyles.boldLabel);
                return;
            }

            SO.ToggleMissingDataDebugs = GUILayout.Toggle(SO.ToggleMissingDataDebugs, "Toggle Missing Data Debugs");

            if (EditorApplication.timeSinceStartup - _lastRefreshTime >= 1)
            {
                SO.RefreshData();
                _lastRefreshTime = EditorApplication.timeSinceStartup;
            }

            if (SO.Data.Length is 0)
            {
                EditorGUILayout.LabelField("No BaseObjects Found", EditorStyles.boldLabel);
                return;
            }

            EditorGUILayout.LabelField("All BaseObjects", EditorStyles.boldLabel);

            Data<T>[] nonNullDataObjects = Array.Empty<Data<T>>();
            
            for (var i = 0; i < 2; i++)
            {
                nonNullDataObjects = SO.Data.Where(data_Object =>
                    data_Object              != null &&
                    data_Object.DataID != 0).ToArray();

                if (nonNullDataObjects.Length == 0)
                {
                    SO.RefreshData(true);
                }    
            }
            
            SO.ScrollPosition = EditorGUILayout.BeginScrollView(SO.ScrollPosition,
                GUILayout.Height(Mathf.Min(200, nonNullDataObjects.Length * 20)));
            _selectedBaseIndex =
                GUILayout.SelectionGrid(_selectedBaseIndex, _getBaseObjectNames(nonNullDataObjects), 1);
            EditorGUILayout.EndScrollView();

            if (_selectedBaseIndex < 0 || _selectedBaseIndex >= nonNullDataObjects.Length) return;

            var selectedDataObject = nonNullDataObjects[_selectedBaseIndex];

            _drawData_Object(selectedDataObject.GetDataToDisplay(SO.ToggleMissingDataDebugs));
        }

        static string[] _getBaseObjectNames(Data<T>[] baseObjects)
        {
            return baseObjects.Select(base_Object => $"{base_Object.DataTitle}").ToArray();
        }

        static void _drawData_Object(DataToDisplay dataToDisplay)
        {
            while (true)
            {
                //* First Data is not working, so I will always have open the data first to see it, even if its First Data. Maybe one day find a way around it.
                
                if (!(dataToDisplay.ShowData = EditorGUILayout.Toggle($"{dataToDisplay.Title}", dataToDisplay.ShowData))) 
                    return;

                foreach (var data in dataToDisplay.StringData)
                {
                    GUILayout.Label($"{data.Key}: {data.Value}");
                }

                var allSubData = dataToDisplay.SubData?.Values;

                if (allSubData?.Count > 0)
                {
                    foreach (var subData in allSubData)
                    {
                        _drawData_Object(subData);
                    }    
                }
                
                if (dataToDisplay.InteractableData is null || dataToDisplay.InteractableData.Count is 0) return;

                dataToDisplay.ScrollPosition = EditorGUILayout.BeginScrollView(dataToDisplay.ScrollPosition,
                    GUILayout.Height(Mathf.Min(200, dataToDisplay.InteractableData.Count * 20)));
                dataToDisplay.SelectedIndex = GUILayout.SelectionGrid(dataToDisplay.SelectedIndex,
                    dataToDisplay.InteractableData.Select(subData => subData.Value.Title).ToArray(), 1);

                EditorGUILayout.EndScrollView();

                if (dataToDisplay.SelectedIndex < 0 || dataToDisplay.SelectedIndex >= dataToDisplay.InteractableData.Count) return;
            
                var selectedSubData = dataToDisplay.InteractableData.ElementAt(dataToDisplay.SelectedIndex).Value;
                
                dataToDisplay = selectedSubData;
            }
        }
    }
}