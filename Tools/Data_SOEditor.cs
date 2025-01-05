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

            var nonNullDataObjects = SO.Data.Where(data_Object =>
                data_Object              != null &&
                data_Object.DataID != 0).ToArray();

            SO.ScrollPosition = EditorGUILayout.BeginScrollView(SO.ScrollPosition,
                GUILayout.Height(Mathf.Min(200, nonNullDataObjects.Length * 20)));
            _selectedBaseIndex =
                GUILayout.SelectionGrid(_selectedBaseIndex, _getBaseObjectNames(nonNullDataObjects), 1);
            EditorGUILayout.EndScrollView();

            if (_selectedBaseIndex < 0 || _selectedBaseIndex >= nonNullDataObjects.Length) return;

            var selectedDataObject = nonNullDataObjects[_selectedBaseIndex];

            _drawData_Object(selectedDataObject.GetData_Display(SO.ToggleMissingDataDebugs));
        }

        static string[] _getBaseObjectNames(Data<T>[] baseObjects)
        {
            return baseObjects.Select(base_Object => $"{base_Object.DataTitle}").ToArray();
        }

        static void _drawData_Object(Data_Display dataSO_Object)
        {
            while (true)
            {
                //* First Data is not working, so I will always have open the data first to see it, even if its First Data. Maybe one day find a way around it.
                if (!(dataSO_Object.ShowData = EditorGUILayout.Toggle($"{dataSO_Object.Title}", dataSO_Object.ShowData))) 
                    return;

                foreach (var data in dataSO_Object.Data)
                {
                    GUILayout.Label($"{data.Key}: {data.Value}");
                }

                if (dataSO_Object.DataDisplayType == DataDisplayType.List_CheckBox)
                {
                    foreach (var subData in dataSO_Object.SubData.Values)
                    {
                        _drawData_Object(subData);
                    }
                    return;
                }

                dataSO_Object.ScrollPosition = EditorGUILayout.BeginScrollView(dataSO_Object.ScrollPosition,
                    GUILayout.Height(Mathf.Min(200, dataSO_Object.SubData.Count * 20)));
                dataSO_Object.SelectedIndex = GUILayout.SelectionGrid(dataSO_Object.SelectedIndex,
                    dataSO_Object.SubData.Select(subData => subData.Value.Title).ToArray(), 1);

                EditorGUILayout.EndScrollView();

                if (dataSO_Object.SelectedIndex < 0 || dataSO_Object.SelectedIndex >= dataSO_Object.SubData.Count) return;
            
                var selectedSubData = dataSO_Object.SubData.ElementAt(dataSO_Object.SelectedIndex).Value;
                
                dataSO_Object = selectedSubData;
            }
        }
    }
}