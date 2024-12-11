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
    public class JobSite_SO : Base_SO<JobSite_Data>
    {
        public JobSite_Data[] JobSites                             => Objects;
        public JobSite_Data        GetJobSite_Data(uint      jobSiteID) => GetObject_Master(jobSiteID);
        public Dictionary<uint, JobSite_Component> JobSiteComponents = new();

        public JobSite_Component GetJobSite_Component(uint jobSiteID)
        {
            if (JobSiteComponents.TryGetValue(jobSiteID, out var component))
            {
                return component;
            }   
            
            Debug.LogError($"JobSite with ID {jobSiteID} not found in JobSite_SO.");
            return null;
        }

        public override uint GetObjectID(int id) => JobSites[id].JobSiteID;

        public void UpdateJobSite(uint jobSiteID, JobSite_Data jobSite_Component) => UpdateObject(jobSiteID, jobSite_Component);
        public void UpdateAllJobSites(Dictionary<uint, JobSite_Data> allJobSites) => UpdateAllObjects(allJobSites);

        public void PopulateSceneJobSites()
        {
            var allJobSiteComponents = FindObjectsByType<JobSite_Component>(FindObjectsSortMode.None);
            var allJobSiteData =
                allJobSiteComponents.ToDictionary(jobSite => jobSite.JobSiteID, jobSite => jobSite.JobSiteData);

            UpdateAllJobSites(allJobSiteData);
            
            foreach (var jobSite in JobSites)
            {
                jobSite.ProsperityData.SetProsperity(50);
                jobSite.ProsperityData.MaxProsperity = 100;
            }
        }

        protected override Dictionary<uint, JobSite_Data> _populateDefaultObjects()
        {
            return FindObjectsByType<JobSite_Component>(FindObjectsSortMode.None).ToDictionary(
                jobSite => jobSite.JobSiteID, jobSite => jobSite.JobSiteData);
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
            return jobSiteSO.JobSites.Select(j => j.JobSite_Component.JobSiteName.ToString()).ToArray();
        }

        void _drawJobSiteAdditionalData(JobSite_Data selectedJobSiteData)
        {
            EditorGUILayout.LabelField("JobSite Data", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("JobSite Name", selectedJobSiteData.JobSite_Component.JobSiteName.ToString());
            EditorGUILayout.LabelField("JobSite ID",   selectedJobSiteData.JobSiteID.ToString());
            EditorGUILayout.LabelField("City ID",      selectedJobSiteData.CityID.ToString());

            if (selectedJobSiteData.AllStationIDs != null)
            {
                _showStations = EditorGUILayout.Toggle("JobSites", _showStations);

                if (_showStations)
                {
                    _drawJobSiteAdditionalData(selectedJobSiteData.AllStationIDs);
                }
            }

            if (selectedJobSiteData.ProsperityData != null)
            {
                _showProsperity = EditorGUILayout.Toggle("Prosperity", _showProsperity);

                if (_showProsperity)
                {
                    _drawProsperityDetails(selectedJobSiteData.ProsperityData);
                }
            }
            
            if (selectedJobSiteData?.JobSite_Component.PriorityComponent != null)
            {
                _showPriority = EditorGUILayout.Toggle("Priority", _showPriority);

                if (_showPriority)
                {
                    _drawPriorityDetails(selectedJobSiteData.JobSite_Component.PriorityComponent);
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