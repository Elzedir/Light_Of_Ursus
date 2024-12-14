using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Tools
{
    [CustomEditor(typeof(Data_SO<>), true)]
    public abstract class Base_SOEditor<T> : Editor where T : class
    {
        int                             _selectedBaseObjectIndex = -1;
        protected       Data_SO<T> _so;
        public abstract Data_SO<T> SO { get; }

        public override void OnInspectorGUI()
        {
            if (SO?.DataObjects is null)
            {
                EditorGUILayout.LabelField($"{SO} is null or {SO?.DataObjects} is null", EditorStyles.boldLabel);
                return;
            }

            if (GUILayout.Button("Refresh All Objects")) SO.RefreshBaseObjects();

            if (SO.DataObjects.Length is 0)
            {
                EditorGUILayout.LabelField("No BaseObjects Found", EditorStyles.boldLabel);
                return;
            }

            EditorGUILayout.LabelField("All BaseObjects", EditorStyles.boldLabel);

            var nonNullBaseObjects = SO.DataObjects.Where(data_Object =>
                data_Object              != null &&
                data_Object.DataObjectID != 0).ToArray();
                
                Debug.Log($"{nonNullBaseObjects.Length} non-null BaseObjects found.");

            SO.BaseScrollPosition = EditorGUILayout.BeginScrollView(SO.BaseScrollPosition,
                GUILayout.Height(Mathf.Min(200, nonNullBaseObjects.Length * 20)));
            _selectedBaseObjectIndex =
                GUILayout.SelectionGrid(_selectedBaseObjectIndex, _getBaseObjectNames(nonNullBaseObjects), 1);
            EditorGUILayout.EndScrollView();

            if (_selectedBaseObjectIndex < 0 || _selectedBaseObjectIndex >= nonNullBaseObjects.Length) return;

            var selectedBaseObject = nonNullBaseObjects[_selectedBaseObjectIndex];
            
            selectedBaseObject.DisplayData?.Invoke();
        }

        static string[] _getBaseObjectNames(Data_Object<T>[] baseObjects)
        {
            return baseObjects.Select(base_Object => $"{base_Object.DataObjectTitle}").ToArray();
        }

        public static void DrawData(DataSO_Object dataSO_Object)
        {
            dataSO_Object.ShowData = EditorGUILayout.Toggle($"{dataSO_Object.Title}", dataSO_Object.ShowData);

            if (!dataSO_Object.ShowData) return;

            var nonNullData = dataSO_Object.Data.Where(data => data != null).ToArray();

            foreach (var data in nonNullData)
            {
                data.DisplayData?.Invoke();
            }
        }
    }
}