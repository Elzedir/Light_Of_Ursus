using System;
using System.Collections.Generic;
using System.Linq;
using Actor;
using ScriptableObjects;
using UnityEditor;
using UnityEngine;

namespace EmployeePosition
{
    [CreateAssetMenu(fileName = "EmployeePosition_SO", menuName = "SOList/EmployeePosition_SO")]
    [Serializable]
    public class EmployeePosition_SO : Base_SO<EmployeePosition_Master>
    {
        public EmployeePosition_Master[] EmployeePositions                           => Objects;

        public EmployeePosition_Master GetEmployeePosition_Master(EmployeePositionName employeePositionName) =>
            GetObject_Master((uint)employeePositionName);

        public EmployeePosition_Requirements GetEmployeeMinimumRequirements(EmployeePositionName employeePositionName) =>
            EmployeePosition_List.GetEmployeeMinimumRequirements(employeePositionName);
        
        
        public override uint GetObjectID(int id) => (uint)EmployeePositions[id].EmployeePositionName;

        public void PopulateDefaultEmployeePositions()
        {
            if (_defaultEmployeePositions.Count == 0)
            {
                Debug.Log("No Default EmployeePosition Positions Found");
            }
        }
        protected override Dictionary<uint, EmployeePosition_Master> _populateDefaultObjects()
        {
            var defaultEmployeePositions = new Dictionary<uint, EmployeePosition_Master>();

            foreach (var defaultEmployeePosition in EmployeePosition_List.GetAllDefaultEmployeePositions())
            {
                defaultEmployeePositions.Add(defaultEmployeePosition.Key, defaultEmployeePosition.Value);
            }
            
            foreach (var employee in defaultEmployeePositions)
            {
                Debug.Log($"Employee Position: {employee.Value.EmployeePositionName}");
            }

            return defaultEmployeePositions;
        }
        
        Dictionary<uint, EmployeePosition_Master> _defaultEmployeePositions => DefaultObjects;
    }
    
    [CustomEditor(typeof(EmployeePosition_SO))]
    public class AllEmployeePositions_SOEditor : Editor
    {
        int _selectedEmployeePositionIndex = -1;

        Vector2 _employeePositionScrollPos;
        Vector2 _employeeRecipeScrollPos;
        Vector2 _employeeVocationScrollPos;
        Vector2 _employeePositionStateScrollPos;

        bool _showEmployeeActorData;
        bool _showEmployeeCareerData;
        bool _showEmployeeStatesData;

        void _unselectAll()
        {
            _showEmployeeActorData         = false;
        }

        public override void OnInspectorGUI()
        {
            var allEmployeePositionsSO = (EmployeePosition_SO)target;

            if (allEmployeePositionsSO?.EmployeePositions is null ||
                allEmployeePositionsSO.EmployeePositions.Length is 0)
            {
                allEmployeePositionsSO?.PopulateDefaultEmployeePositions();
            }
            
            if (allEmployeePositionsSO?.EmployeePositions is null || allEmployeePositionsSO.EmployeePositions.Length is 0)
            {
                EditorGUILayout.LabelField("No EmployeePositions Found", EditorStyles.boldLabel);
                return;
            }

            if (GUILayout.Button("Clear EmployeePosition Data"))
            {
                allEmployeePositionsSO.ClearObjectData();
                EditorUtility.SetDirty(allEmployeePositionsSO);
            }

            if (GUILayout.Button("Unselect All")) _unselectAll();

            EditorGUILayout.LabelField("All EmployeePositions", EditorStyles.boldLabel);

            var nonNullEmployeePositions = allEmployeePositionsSO.EmployeePositions.Where(employeePosition =>
                employeePosition != null && employeePosition.EmployeeDataPreset != ActorDataPresetName.No_Preset).ToArray();

            _employeePositionScrollPos = EditorGUILayout.BeginScrollView(_employeePositionScrollPos,
                GUILayout.Height(Math.Min(200, nonNullEmployeePositions.Length * 20)));
            _selectedEmployeePositionIndex = GUILayout.SelectionGrid(_selectedEmployeePositionIndex, _getEmployeePositionNames(nonNullEmployeePositions), 1);
            EditorGUILayout.EndScrollView();

            if (_selectedEmployeePositionIndex >= 0 && _selectedEmployeePositionIndex < nonNullEmployeePositions.Length)
            {
                _drawEmployeePositionData(nonNullEmployeePositions[_selectedEmployeePositionIndex]);
            }
        }

        static string[] _getEmployeePositionNames(EmployeePosition_Master[] employeePositions) =>
            employeePositions.Select(employeePosition => employeePosition.EmployeePositionName.ToString()).ToArray();


        void _drawEmployeePositionData(EmployeePosition_Master employeePosition)
        {
            EditorGUILayout.LabelField("EmployeePosition Data", EditorStyles.boldLabel);

            EditorGUILayout.LabelField("EmployeePosition Name", $"{employeePosition.EmployeePositionName}");

            var actorDataPreset = ActorDataPreset_Manager.GetActorDataPreset(employeePosition.EmployeeDataPreset);

            EditorGUILayout.LabelField("Actor Generation Parameters", EditorStyles.boldLabel);

            _showEmployeeActorData = EditorGUILayout.Toggle("ActorData", _showEmployeeActorData);

            if (_showEmployeeActorData)
            {
                EditorGUILayout.LabelField("ActorID",        $"{actorDataPreset.ActorID}");
                EditorGUILayout.LabelField("ActorName",      $"{actorDataPreset.ActorName}");
                EditorGUILayout.LabelField("ActorFactionID", $"{actorDataPreset.FullIdentification.ActorFactionID}");
                EditorGUILayout.LabelField("ActorCityID",    $"{actorDataPreset.FullIdentification.ActorCityID}");
            }
            
            EditorGUILayout.LabelField("Employee Position Career Data", EditorStyles.boldLabel);

            _showEmployeeCareerData = EditorGUILayout.Toggle("EmployeePositionCareerData", _showEmployeeCareerData);

            if (_showEmployeeCareerData)
            {
                _drawEmployeeCareerData(actorDataPreset);
            }
            
            EditorGUILayout.LabelField("Employee Position State Data", EditorStyles.boldLabel);
            
            _showEmployeeStatesData = EditorGUILayout.Toggle("EmployeePositionStateData", _showEmployeeStatesData);
            
            if (_showEmployeeStatesData)
            {
                _drawEmployeePositionStateData(actorDataPreset);
            }
        }

        void _drawEmployeeCareerData(Actor_Data actorDataPreset)
        {
            EditorGUILayout.LabelField("Career Name", $"{actorDataPreset.CareerData.CareerName}");

            var initialRecipes = actorDataPreset.CraftingData.KnownRecipes;

            if (initialRecipes.Count == 1)
            {
                EditorGUILayout.LabelField(initialRecipes[0].ToString());
            }
            else
            {
                _employeeRecipeScrollPos = EditorGUILayout.BeginScrollView(_employeeRecipeScrollPos,
                    GUILayout.Height(Math.Min(200, initialRecipes.Count * 20)));

                try
                {
                    foreach (var recipe in initialRecipes)
                    {
                        EditorGUILayout.LabelField(recipe.ToString());
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

            var initialVocations = actorDataPreset.VocationData.ActorVocations;

            if (initialVocations.Count == 1)
            {
                EditorGUILayout.LabelField(initialVocations[0].ToString());
            }
            else
            {
                _employeeVocationScrollPos = EditorGUILayout.BeginScrollView(_employeeVocationScrollPos,
                    GUILayout.Height(Math.Min(200, initialVocations.Count * 20)));

                try
                {
                    foreach (var vocation in initialVocations)
                    {
                        EditorGUILayout.LabelField(vocation.ToString());
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

        void _drawEmployeePositionStateData(Actor_Data actorDataPreset)
        {
            var employeePositionStates = actorDataPreset.StatesAndConditionsData;
            
            EditorGUILayout.LabelField("Employee Position States Not initialised yet", EditorStyles.boldLabel);
        }
    }
}