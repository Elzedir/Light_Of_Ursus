using System;
using System.Collections.Generic;
using System.Linq;
using Priority;
using ScriptableObjects;
using UnityEditor;
using UnityEngine;

namespace JobSite
{
    [CreateAssetMenu(fileName = "AllJobSites_SO", menuName = "SOList/AllJobSites_SO")]
    [Serializable]
    public class JobSite_SO : Base_SO<JobSite_Component>
    {
        public JobSite_Component[] JobSites                             => Objects;
        public JobSite_Data        GetJobSite_Data(uint      jobSiteID) => GetObject_Master(jobSiteID).JobSiteData;
        public JobSite_Component   GetJobSite_Component(uint jobSiteID) => GetObject_Master(jobSiteID);

        public JobSite_Data[] Save_SO()
        {
            return JobSites.Select(jobSite => jobSite.JobSiteData).ToArray();
        }
        
        public void Load_SO(JobSite_Data[] jobSiteData)
        {
            foreach (var jobSite in jobSiteData)
            {
                if (!_jobSite_Components.ContainsKey(jobSite.JobSiteID))
                {
                    Debug.LogError($"JobSite with ID {jobSite.JobSiteID} not found in JobSite_SO.");
                    continue;
                }
                
                _jobSite_Components[jobSite.JobSiteID].JobSiteData = jobSite;
            }

            LoadSO(_jobSite_Components.Values.ToArray());
        }

        public override uint GetObjectID(int id) => JobSites[id].JobSiteID;

        public void UpdateJobSite(uint jobSiteID, JobSite_Component jobSite_Component) => UpdateObject(jobSiteID, jobSite_Component);
        public void UpdateAllJobSites(Dictionary<uint, JobSite_Component> allJobSites) => UpdateAllObjects(allJobSites);

        public void PopulateSceneJobSites()
        {
            var allJobSiteComponents = FindObjectsByType<JobSite_Component>(FindObjectsSortMode.None);
            var allJobSiteData =
                allJobSiteComponents.ToDictionary(jobSite => jobSite.JobSiteID);

            UpdateAllJobSites(allJobSiteData);
            
            foreach (var jobSite in JobSites)
            {
                jobSite.JobSiteData.ProsperityData.SetProsperity(50);
                jobSite.JobSiteData.ProsperityData.MaxProsperity = 100;
            }
        }

        protected override Dictionary<uint, JobSite_Component> _populateDefaultObjects()
        {
            return FindObjectsByType<JobSite_Component>(FindObjectsSortMode.None).ToDictionary(
                jobSite => jobSite.JobSiteID);
        }
        
        static uint _lastUnusedJobSiteID = 1;
        
        public uint GetUnusedJobSiteID()
        {
            while (ObjectIndexLookup.ContainsKey(_lastUnusedJobSiteID))
            {
                _lastUnusedJobSiteID++;
            }

            return _lastUnusedJobSiteID;
        }
        
        Dictionary<uint, JobSite_Component> _jobSite_Components => DefaultObjects;
    }

    [CustomEditor(typeof(JobSite_SO))]
    public class JobSites_SOEditor : Editor
    {
        int  _selectedJobSiteIndex = -1;
        
        bool _showStations;
        bool _showProsperity;
        bool _showPriority;

        Vector2 _jobSiteScrollPos;
        Vector2 _stationScrollPos;

        public override void OnInspectorGUI()
        {
            var jobSiteSO = (JobSite_SO)target;

            EditorGUILayout.LabelField("All JobSites", EditorStyles.boldLabel);

            _jobSiteScrollPos = EditorGUILayout.BeginScrollView(_jobSiteScrollPos,
                GUILayout.Height(Mathf.Min(200, jobSiteSO.JobSites.Length * 20)));
            _selectedJobSiteIndex = GUILayout.SelectionGrid(_selectedJobSiteIndex, _getJobSiteNames(jobSiteSO), 1);
            EditorGUILayout.EndScrollView();

            if (_selectedJobSiteIndex < 0 || _selectedJobSiteIndex >= jobSiteSO.JobSites.Length) return;
            
            var selectedJobSiteData = jobSiteSO.JobSites[_selectedJobSiteIndex];
            _drawJobSiteAdditionalData(selectedJobSiteData);
        }

        static string[] _getJobSiteNames(JobSite_SO jobSiteSO)
        {
            return jobSiteSO.JobSites.Select(j => j.JobSiteName.ToString()).ToArray();
        }

        void _drawJobSiteAdditionalData(JobSite_Component selectedJobSiteComponent)
        {
            EditorGUILayout.LabelField("JobSite Data", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("JobSite Name", selectedJobSiteComponent.JobSiteName.ToString());
            EditorGUILayout.LabelField("JobSite ID",   selectedJobSiteComponent.JobSiteID.ToString());
            EditorGUILayout.LabelField("City ID",      selectedJobSiteComponent.JobSiteData.CityID.ToString());

            if (selectedJobSiteComponent.JobSiteData.AllStationIDs != null)
            {
                _showStations = EditorGUILayout.Toggle("JobSites", _showStations);

                if (_showStations)
                {
                    _drawJobSiteAdditionalData(selectedJobSiteComponent.JobSiteData.AllStationIDs);
                }
            }

            if (selectedJobSiteComponent.JobSiteData.ProsperityData != null)
            {
                _showProsperity = EditorGUILayout.Toggle("Prosperity", _showProsperity);

                if (_showProsperity)
                {
                    _drawProsperityDetails(selectedJobSiteComponent.JobSiteData.ProsperityData);
                }
            }
            
            if (selectedJobSiteComponent.PriorityComponent != null)
            {
                _showPriority = EditorGUILayout.Toggle("Priority", _showPriority);

                if (_showPriority)
                {
                    _drawPriorityDetails(selectedJobSiteComponent.PriorityComponent);
                }
            }
        }

        void _drawJobSiteAdditionalData(List<uint> allStationIDs)
        {
            _stationScrollPos = EditorGUILayout.BeginScrollView(_stationScrollPos,
                GUILayout.Height(Mathf.Min(200, allStationIDs.Count * 20)));

            try
            {
                foreach (var stationID in allStationIDs)
                {
                    EditorGUILayout.LabelField("Station Data", EditorStyles.boldLabel);
                    //EditorGUILayout.LabelField("Station Name", station.StationName.ToString());
                    EditorGUILayout.LabelField("Station ID", stationID.ToString());
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
        }

        static void _drawProsperityDetails(ProsperityData prosperityData)
        {
            EditorGUILayout.LabelField("Current Prosperity",             $"{prosperityData.CurrentProsperity}");
            EditorGUILayout.LabelField("Max Prosperity",                 $"{prosperityData.MaxProsperity}");
            EditorGUILayout.LabelField("Base Prosperity Growth Per Day", $"{prosperityData.BaseProsperityGrowthPerDay}");
        }
        
        void _drawPriorityDetails(PriorityComponent_JobSite priorityComponent)
        {
            EditorGUILayout.LabelField("Priorities", EditorStyles.boldLabel);
            
            var allPriorities = priorityComponent.PriorityQueue.PeekAll();
            
            _jobSiteScrollPos = EditorGUILayout.BeginScrollView(_jobSiteScrollPos,
                GUILayout.Height(Mathf.Min(200, allPriorities.Length * 20)));

            try
            {
                foreach (var priority in allPriorities)
                {
                    EditorGUILayout.LabelField("Priority", EditorStyles.boldLabel);
                    
                    //EditorGUILayout.LabelField("Station Name", station.StationName.ToString());
                    EditorGUILayout.LabelField("PriorityID", $"{priority.PriorityID}");
                    EditorGUILayout.LabelField("PriorityValue", $"{priority.PriorityValue}");
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
        }
    }
}