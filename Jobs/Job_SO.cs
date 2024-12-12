using System;
using System.Collections.Generic;
using System.Linq;
using ScriptableObjects;
using UnityEditor;
using UnityEngine;

namespace Jobs
{
    [CreateAssetMenu(fileName = "AllJobs_SO", menuName = "SOList/AllJobs_SO")]
    [Serializable]
    public class AllJobs_SO : Base_SO<Job_Master>
    {
        public Job_Master[] Jobs                           => Objects;
        public Job_Master   GetJob_Master(JobName jobName) => GetObject_Master((uint)jobName);

        public override uint GetObjectID(int id) => (uint)Jobs[id].JobName;

        public void PopulateDefaultJobs()
        {
            if (_defaultJobs.Count == 0)
            {
                Debug.Log("No Default Jobs Found");
            }
        }
        
        protected override Dictionary<uint, Job_Master> _populateDefaultObjects()
        {
            var defaultJobs = new Dictionary<uint, Job_Master>();

            foreach (var item in List_Job.GetAllDefaultJobs())
            {
                defaultJobs.Add(item.Key, item.Value);
            }

            return defaultJobs;
        }
        
        Dictionary<uint, Job_Master> _defaultJobs => DefaultObjects;
    }

    [CustomEditor(typeof(AllJobs_SO))]
    public class AllJobs_SOEditor : Editor
    {
        int _selectedJobIndex = -1;

        Vector2 _jobTaskScrollPos;

        bool _showJobTasks;

        void _unselectAll()
        {
            _showJobTasks         = false;
        }

        public override void OnInspectorGUI()
        {
            var allJobsSO = (AllJobs_SO)target;

            if (allJobsSO?.Jobs is null || allJobsSO.Jobs.Length is 0)
            {
                EditorGUILayout.LabelField("No Jobs Found", EditorStyles.boldLabel);
                return;
            }

            if (GUILayout.Button("Clear Job Data"))
            {
                allJobsSO.ClearObjectData();
                EditorUtility.SetDirty(allJobsSO);
            }

            if (GUILayout.Button("Unselect All")) _unselectAll();

            EditorGUILayout.LabelField("All Jobs", EditorStyles.boldLabel);

            var nonNullJobs = allJobsSO.Jobs.Where(job =>
                job != null && (!string.IsNullOrEmpty(job.JobDescription) ||
                                job.JobName != 0)).ToArray();

            _jobTaskScrollPos = EditorGUILayout.BeginScrollView(_jobTaskScrollPos,
                GUILayout.Height(Math.Min(200, nonNullJobs.Length * 20)));
            _selectedJobIndex = GUILayout.SelectionGrid(_selectedJobIndex, _getJobNames(nonNullJobs), 1);
            EditorGUILayout.EndScrollView();

            if (_selectedJobIndex >= 0 && _selectedJobIndex < nonNullJobs.Length)
            {
                _drawJobData(nonNullJobs[_selectedJobIndex]);
            }
        }

        string[] _getJobNames(Job_Master[] jobs) =>
            jobs.Select(job => job.JobName.ToString()).ToArray();


        void _drawJobData(Job_Master job)
        {
            EditorGUILayout.LabelField("Job Data", EditorStyles.boldLabel);

            EditorGUILayout.LabelField("Job ID",   $"{job.JobName}");
            EditorGUILayout.LabelField("Job Name", $"{job.JobDescription}");

            if (job.JobTasks != null)
            {
                EditorGUILayout.LabelField("JobTasks", EditorStyles.boldLabel);

                var jobTasks = job.JobTasks;

                _showJobTasks = EditorGUILayout.Toggle("JobTasks", _showJobTasks);

                if (_showJobTasks)
                {
                    _drawJobTasks(jobTasks.ToList());
                }
            }
        }

        void _drawJobTasks(List<JobTaskName> jobTasks)
        {
            if (jobTasks.Count == 1)
            {
                EditorGUILayout.LabelField(jobTasks[0].ToString());
            }
            else
            {
                _jobTaskScrollPos = EditorGUILayout.BeginScrollView(_jobTaskScrollPos,
                    GUILayout.Height(Math.Min(200, jobTasks.Count * 20)));

                try
                {
                    foreach (var taskName in jobTasks)
                    {
                        EditorGUILayout.LabelField(taskName.ToString());
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