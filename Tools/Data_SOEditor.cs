using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Tools
{
    [CustomEditor(typeof(Data_SO<>), true)]
    public abstract class Data_SOEditor<T> : Editor where T : class
    {
        int                        _selectedIndex = -1;
        protected       Data_SO<T> _so;
        public abstract Data_SO<T> SO { get; }

        public override void OnInspectorGUI()
        {
            if (SO?.Objects_Data is null)
            {
                EditorGUILayout.LabelField($"{SO} is null or {SO?.Objects_Data} is null", EditorStyles.boldLabel);
                return;
            }
            
            SO.ToggleMissingDataDebugs = GUILayout.Toggle(false, "Toggle Missing Data Debugs");
            
            if (GUILayout.Button("Refresh All Objects")) SO.RefreshDataObjects();

            if (SO.Objects_Data.Length is 0)
            {
                EditorGUILayout.LabelField("No BaseObjects Found", EditorStyles.boldLabel);
                return;
            }

            EditorGUILayout.LabelField("All BaseObjects", EditorStyles.boldLabel);

            var nonNullDataObjects = SO.Objects_Data.Where(data_Object =>
                data_Object              != null &&
                data_Object.DataObjectID != 0).ToArray();

            SO.ScrollPosition = EditorGUILayout.BeginScrollView(SO.ScrollPosition,
                GUILayout.Height(Mathf.Min(200, nonNullDataObjects.Length * 20)));
            _selectedIndex =
                GUILayout.SelectionGrid(_selectedIndex, _getBaseObjectNames(nonNullDataObjects), 1);
            EditorGUILayout.EndScrollView();

            if (_selectedIndex < 0 || _selectedIndex >= nonNullDataObjects.Length) return;

            var selectedDataObject = nonNullDataObjects[_selectedIndex];

            _drawData_Object(selectedDataObject.DataSO_Object, firstData: true);
        }

        static string[] _getBaseObjectNames(Object_Data<T>[] baseObjects)
        {
            return baseObjects.Select(base_Object => $"{base_Object.DataObjectTitle}").ToArray();
        }

        static void _drawData_Object(Data_Display dataSO_Object, bool firstData = false)
        {
            while (true)
            {
                // Fix the first data thing to apply when needed, like Item and Priority.
                
                if (!firstData && !(dataSO_Object.ShowData = EditorGUILayout.Toggle($"{dataSO_Object.Title}", dataSO_Object.ShowData)))
                    return;

                if (dataSO_Object.Data is not null)
                {
                    foreach (var data in dataSO_Object.Data)
                        GUILayout.Label(data);
                    
                    if (dataSO_Object.DataDisplayType == DataDisplayType.Item) 
                        return;
                }
                
                if (dataSO_Object.DataDisplayType == DataDisplayType.CheckBoxList && dataSO_Object.SubData is not null)
                {
                    foreach (var subData in dataSO_Object.SubData)
                        _drawData_Object(subData);
                    
                    return;
                }
                
                if (dataSO_Object.SubData == null || dataSO_Object.SubData.Count == 0)
                    return;

                dataSO_Object.ScrollPosition = EditorGUILayout.BeginScrollView(dataSO_Object.ScrollPosition,
                    GUILayout.Height(Mathf.Min(200, dataSO_Object.SubData.Count * 20)));
                dataSO_Object.SelectedIndex = GUILayout.SelectionGrid(dataSO_Object.SelectedIndex,
                    dataSO_Object.SubData.Select(subData => subData.Title).ToArray(), 1);

                EditorGUILayout.EndScrollView();

                if (dataSO_Object.SelectedIndex < 0 ||
                    dataSO_Object.SelectedIndex >= dataSO_Object.SubData.Count) return;

                var selectedBaseObject = dataSO_Object.SubData[dataSO_Object.SelectedIndex];

                selectedBaseObject.ShowData =
                    EditorGUILayout.Toggle($"{selectedBaseObject.Title}", selectedBaseObject.ShowData);

                if (!selectedBaseObject.ShowData) return;

                dataSO_Object = selectedBaseObject;
            }
        }
    }
}