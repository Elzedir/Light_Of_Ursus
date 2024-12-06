using System;
using System.Collections.Generic;
using System.Linq;
using Initialisation;
using UnityEngine;

namespace Jobsite
{
    public class Manager_Jobsite : MonoBehaviour, IDataPersistence
    {
        static        Jobsite_SO _displayJobsite;
        public static Jobsite_SO DisplayJobsite { get => _displayJobsite ??= Resources.Load<Jobsite_SO>("ScriptableObjects/AllJobsites_SO"); }

        public static Dictionary<uint, Jobsite_Data>      AllJobsiteData       = new();
        static        uint                               _lastUnusedJobsiteID = 1;
        public static Dictionary<uint, Jobsite_Component> AllJobsiteComponents = new();

        public void SaveData(SaveData data) => data.SavedJobsiteData = new SavedJobsiteData(AllJobsiteData.Values.ToList());
        public void LoadData(SaveData data)
        {
            if (data == null)
            {
                //Debug.Log("No SaveData found in LoadData.");
                return;
            }
            if (data.SavedJobsiteData == null)
            {
                //Debug.Log("No SavedJobsiteData found in SaveData.");
                return;
            }
            if (data.SavedJobsiteData.AllJobsiteData == null)
            {
                //Debug.Log("No AllJobsiteData found in SavedJobsiteData.");
                return;
            }
            if (data.SavedJobsiteData.AllJobsiteData.Count == 0)
            {
                //Debug.Log("AllJobsiteData count is 0.");
                return;
            }
        
            AllJobsiteData = data.SavedJobsiteData.AllJobsiteData.ToDictionary(x => x.JobsiteID);
        }

        public void OnSceneLoaded()
        {
            Manager_Initialisation.OnInitialiseManagerJobsite += _initialise;
        }

        void _initialise()
        {
            foreach (var jobsite in _findAllJobsiteComponents())
            {
                if (jobsite.JobsiteData == null) { Debug.Log($"Jobsite: {jobsite.name} does not have JobsiteData."); continue; }

                if (!AllJobsiteComponents.ContainsKey(jobsite.JobsiteData.JobsiteID)) AllJobsiteComponents.Add(jobsite.JobsiteData.JobsiteID, jobsite);
                else
                {
                    if (AllJobsiteComponents[jobsite.JobsiteData.JobsiteID].gameObject == jobsite.gameObject) continue;
                    else
                    {
                        throw new ArgumentException($"JobsiteID {jobsite.JobsiteData.JobsiteID}: {jobsite.name} already exists for jobsite {AllJobsiteComponents[jobsite.JobsiteData.JobsiteID].name}");
                    }
                }

                if (!AllJobsiteData.ContainsKey(jobsite.JobsiteData.JobsiteID))
                {
                    //Debug.Log($"Jobsite: {jobsite.JobsiteData.JobsiteID}: {jobsite.JobsiteData.JobsiteName} was not in AllJobsiteData");
                    AddToAllJobsiteData(jobsite.JobsiteData);
                }

                jobsite.SetJobsiteData(GetJobsiteData(jobsite.JobsiteData.JobsiteID));
            }

            foreach (var jobsiteData in AllJobsiteData.Values)
            {
                jobsiteData.InitialiseJobsiteData();
            }

            DisplayJobsite.AllJobsiteData = AllJobsiteData.Values.ToList();

            foreach (var jobsite in AllJobsiteData)
            {
                jobsite.Value.ProsperityData.SetProsperity(50);
                jobsite.Value.ProsperityData.MaxProsperity = 100;
            }
        }

        static List<Jobsite_Component> _findAllJobsiteComponents()
        {
            return FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                   .OfType<Jobsite_Component>()
                   .ToList();
        }

        public static void GetNearestJobsite(Vector3 position, out Jobsite_Component nearestJobsite)
        {
            nearestJobsite = null;
            float nearestDistance = float.MaxValue;

            foreach (var jobsite in AllJobsiteComponents)
            {
                float distance = Vector3.Distance(position, jobsite.Value.transform.position);

                if (distance < nearestDistance)
                {
                    nearestJobsite  = jobsite.Value;
                    nearestDistance = distance;
                }
            }
        }

        public void AddToAllJobsiteData(Jobsite_Data jobsiteData)
        {
            if (AllJobsiteData.ContainsKey(jobsiteData.JobsiteID))
            {
                Debug.Log($"AllJobsiteData already contains JobsiteID: {jobsiteData.JobsiteID}");
                return;
            }

            AllJobsiteData.Add(jobsiteData.JobsiteID, jobsiteData);
        }

        public void UpdateAllJobsiteData(Jobsite_Data jobsiteData)
        {
            if (!AllJobsiteData.ContainsKey(jobsiteData.JobsiteID))
            {
                Debug.LogError($"JobsiteData: {jobsiteData.JobsiteID} does not exist in AllJobsiteData.");
                return;
            }

            AllJobsiteData[jobsiteData.JobsiteID] = jobsiteData;
        }

        public static Jobsite_Data GetJobsiteData(uint jobsiteID)
        {
            if (!AllJobsiteData.ContainsKey(jobsiteID))
            {
                Debug.LogError($"JobsiteData: {jobsiteID} does not exist in AllJobsiteData.");
                return null;
            }

            return AllJobsiteData[jobsiteID];
        }

    
        public static Jobsite_Component GetJobsite(uint jobsiteID)
        {
            if (!AllJobsiteComponents.ContainsKey(jobsiteID))
            {
                Debug.LogError($"JobsiteComponent: {jobsiteID} does not exist in AllJobsiteComponents.");
                return null;
            }

            return AllJobsiteComponents[jobsiteID];
        }

        public uint GetRandomJobsiteID()
        {
            while (AllJobsiteData.ContainsKey(_lastUnusedJobsiteID))
            {
                _lastUnusedJobsiteID++;
            }

            return _lastUnusedJobsiteID;
        }
    }
    
    public enum JobsiteName
    {
        None,

        Lumber_Yard,
        Smithy
    }
}
