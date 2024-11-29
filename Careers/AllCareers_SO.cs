using System;
using System.Collections.Generic;
using System.Linq;
using Jobs;
using ScriptableObjects;
using UnityEditor;
using UnityEngine;

namespace Careers
{
    [CreateAssetMenu(fileName = "AllCareers_SO", menuName = "SOList/AllCareers_SO")]
    [Serializable]
    public class AllCareers_SO : Base_SO<Career_Master>
    {
        public Career_Master[] Careers                           => Objects;
        public Career_Master   GetCareer_Master(CareerName careerName) => GetObject_Master((uint)careerName);

        public override uint GetObjectID(int id) => (uint)Careers[id].CareerName;

        public void PopulateDefaultCareers()
        {
            if (_defaultCareers.Count == 0)
            {
                Debug.Log("No Default Careers Found");
            }
        }
        protected override Dictionary<uint, Career_Master> _populateDefaultObjects()
        {
            var defaultCareers = new Dictionary<uint, Career_Master>();

            foreach (var item in List_Career.GetAllDefaultCareers())
            {
                defaultCareers.Add(item.Key, item.Value);
            }

            return defaultCareers;
        }
        
        Dictionary<uint, Career_Master> _defaultCareers => DefaultObjects;
    }
    
    [CustomEditor(typeof(AllCareers_SO))]
    public class AllCareers_SOEditor : Editor
    {
        int _selectedCareerIndex = -1;

        Vector2 _careerScrollPos;

        bool _showCareerJobs;

        void _unselectAll()
        {
            _showCareerJobs         = false;
        }

        public override void OnInspectorGUI()
        {
            var allCareersSO = (AllCareers_SO)target;

            if (allCareersSO?.Careers is null || allCareersSO.Careers.Length is 0)
            {
                EditorGUILayout.LabelField("No Careers Found", EditorStyles.boldLabel);
                return;
            }

            if (GUILayout.Button("Clear Career Data"))
            {
                allCareersSO.ClearObjectData();
                EditorUtility.SetDirty(allCareersSO);
            }

            if (GUILayout.Button("Unselect All")) _unselectAll();

            EditorGUILayout.LabelField("All Careers", EditorStyles.boldLabel);

            var nonNullCareers = allCareersSO.Careers.Where(career =>
                career != null && (!string.IsNullOrEmpty(career.CareerDescription) ||
                                   career.CareerName != 0)).ToArray();

            _careerScrollPos = EditorGUILayout.BeginScrollView(_careerScrollPos,
                GUILayout.Height(Math.Min(200, nonNullCareers.Length * 20)));
            _selectedCareerIndex = GUILayout.SelectionGrid(_selectedCareerIndex, _getCareerNames(nonNullCareers), 1);
            EditorGUILayout.EndScrollView();

            if (_selectedCareerIndex >= 0 && _selectedCareerIndex < nonNullCareers.Length)
            {
                _drawCareerData(nonNullCareers[_selectedCareerIndex]);
            }
        }

        static string[] _getCareerNames(Career_Master[] careers) =>
            careers.Select(career => career.CareerName.ToString()).ToArray();


        void _drawCareerData(Career_Master career)
        {
            EditorGUILayout.LabelField("Career Data", EditorStyles.boldLabel);

            EditorGUILayout.LabelField("Career ID",   $"{career.CareerName}");
            EditorGUILayout.LabelField("Career Name", $"{career.CareerDescription}");

            if (career.CareerJobs != null)
            {
                EditorGUILayout.LabelField("CareerJobs", EditorStyles.boldLabel);

                var careerJobs = career.CareerJobs;

                _showCareerJobs = EditorGUILayout.Toggle("CareerJobs", _showCareerJobs);

                if (_showCareerJobs)
                {
                    _drawCareerJobs(careerJobs.ToList());
                }
            }
        }

        void _drawCareerJobs(List<JobName> careerJobs)
        {
            if (careerJobs.Count == 1)
            {
                EditorGUILayout.LabelField(careerJobs[0].ToString());
            }
            else
            {
                _careerScrollPos = EditorGUILayout.BeginScrollView(_careerScrollPos,
                    GUILayout.Height(Math.Min(200, careerJobs.Count * 20)));

                try
                {
                    foreach (var jobName in careerJobs)
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
