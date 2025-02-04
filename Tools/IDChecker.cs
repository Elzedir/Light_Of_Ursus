using System.Collections.Generic;
using Actor;
using Actors;
using Cities;
using City;
using Faction;
using JobSites;
using Region;
using Regions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Tools
{
    public class IDChecker : EditorWindow
    {
        [MenuItem("Tools/ID Checker")]
        public static void ShowWindow()
        {
            GetWindow<IDChecker>("ID Checker");
        }

        private void OnGUI()
        {
            // Need to put in the functionality to factor in default actors and factions and regions so that they can't be overwritten.

            if (GUILayout.Button("Check and Fix All IDs"))
            {
                //CheckAndFixStationIDs();
                CheckAndFixJobsiteIDs();
                CheckAndFixCityIDs();
                CheckAndFixRegionIDs();
                CheckAndFixActorIDs();
                CheckAndFixFactionIDs();
            }

            // if (GUILayout.Button("Check and Fix Station IDs"))
            // {
            //     CheckAndFixStationIDs();
            // }

            if (GUILayout.Button("Check and Fix Jobsite IDs"))
            {
                CheckAndFixJobsiteIDs();
            }

            if (GUILayout.Button("Check and Fix City IDs"))
            {
                CheckAndFixCityIDs();
            }

            if (GUILayout.Button("Check and Fix Region IDs"))
            {
                CheckAndFixRegionIDs();
            }

            if (GUILayout.Button("Check and Fix Actor IDs"))
            {
                CheckAndFixActorIDs();
            }

            if (GUILayout.Button("Check and Fix Faction IDs"))
            {
                CheckAndFixFactionIDs();
            }
        }

        private ulong GetNewID(HashSet<ulong> existingIDs)
        {
            ulong newID = 1;
            while (existingIDs.Contains(newID))
            {
                newID++;
            }
            return newID;
        }

        // private void CheckAndFixStationIDs()
        // {
        //     var stations = FindObjectsByType<Station_Component>(FindObjectsSortMode.None);
        //     var existingIDs = new HashSet<ulong>();
        //     var duplicateStations = new List<Station_Component>();
        //
        //     foreach (var station in stations)
        //     {
        //         if (station.Station_Data == null)
        //         {
        //             Debug.LogWarning($"Station: {station.name} does not have StationData.");
        //             continue;
        //         }
        //
        //         if (!existingIDs.Add(station.Station_Data.StationID) || station.Station_Data.StationID == 0)
        //         {
        //             duplicateStations.Add(station);
        //         }
        //     }
        //
        //     foreach (var station in duplicateStations)
        //     {
        //         ulong newStationID = GetNewID(existingIDs);
        //         station.Station_Data.StationID = newStationID;
        //         existingIDs.Add(newStationID);
        //
        //         EditorUtility.SetDirty(station);
        //         EditorSceneManager.MarkSceneDirty(station.gameObject.scene);
        //
        //         Debug.Log($"Assigned new StationID {newStationID} to station {station.name}");
        //     }
        //
        //     Debug.Log("Station ID check and fix completed.");
        // }

        private void CheckAndFixJobsiteIDs()
        {
            var jobsites          = FindObjectsByType<JobSite_Component>(FindObjectsSortMode.None);
            var existingIDs       = new HashSet<ulong>();
            var duplicateJobsites = new List<JobSite_Component>();

            foreach (var jobsite in jobsites)
            {
                if (jobsite.JobSite_Data == null)
                {
                    Debug.LogWarning($"Jobsite: {jobsite.name} does not have JobsiteData.");
                    continue;
                }

                if (!existingIDs.Add(jobsite.JobSite_Data.JobSiteID))
                {
                    duplicateJobsites.Add(jobsite);
                }
            }

            foreach (var jobsite in duplicateJobsites)
            {
                ulong newJobsiteID = GetNewID(existingIDs);
                jobsite.JobSite_Data.JobSiteID = newJobsiteID;
                existingIDs.Add(newJobsiteID);

                EditorUtility.SetDirty(jobsite);
                EditorSceneManager.MarkSceneDirty(jobsite.gameObject.scene);

                Debug.Log($"Assigned new JobsiteID {newJobsiteID} to jobsite {jobsite.name}");
            }

            Debug.Log("Jobsite ID check and fix completed.");
        }

        private void CheckAndFixCityIDs()
        {
            var cities          = FindObjectsByType<City_Component>(FindObjectsSortMode.None);
            var existingIDs     = new HashSet<ulong>();
            var duplicateCities = new List<City_Component>();

            foreach (var city in cities)
            {
                if (city.CityData == null)
                {
                    Debug.LogWarning($"City: {city.name} does not have CityData.");
                    continue;
                }

                if (!existingIDs.Add(city.CityData.CityID))
                {
                    duplicateCities.Add(city);
                }
            }

            foreach (var city in duplicateCities)
            {
                ulong newCityID = GetNewID(existingIDs);
                city.CityData.CityID = newCityID;
                existingIDs.Add(newCityID);

                EditorUtility.SetDirty(city);
                EditorSceneManager.MarkSceneDirty(city.gameObject.scene);

                Debug.Log($"Assigned new CityID {newCityID} to city {city.name}");
            }

            Debug.Log("City ID check and fix completed.");
        }

        private void CheckAndFixRegionIDs()
        {
            var regions          = FindObjectsByType<Region_Component>(FindObjectsSortMode.None);
            var existingIDs      = new HashSet<ulong>();
            var duplicateRegions = new List<Region_Component>();

            foreach (var region in regions)
            {
                if (region.RegionData == null)
                {
                    Debug.LogWarning($"Region: {region.name} does not have RegionData.");
                    continue;
                }

                if (!existingIDs.Add(region.RegionData.RegionID))
                {
                    duplicateRegions.Add(region);
                }
            }

            foreach (var region in duplicateRegions)
            {
                ulong newRegionID = GetNewID(existingIDs);
                region.RegionData.RegionID = newRegionID;
                existingIDs.Add(newRegionID);

                EditorUtility.SetDirty(region);
                EditorSceneManager.MarkSceneDirty(region.gameObject.scene);

                Debug.Log($"Assigned new RegionID {newRegionID} to region {region.name}");
            }

            Debug.Log("Region ID check and fix completed.");
        }

        private void CheckAndFixActorIDs()
        {
            var actors          = FindObjectsByType<Actor_Component>(FindObjectsSortMode.None);
            var existingIDs     = new HashSet<ulong>();
            var duplicateActors = new List<Actor_Component>();

            foreach (var actor in actors)
            {
                if (actor.ActorData == null)
                {
                    Debug.LogWarning($"Actor: {actor.name} does not have ActorData.");
                    continue;
                }

                if (!existingIDs.Add(actor.ActorData.ActorID))
                {
                    duplicateActors.Add(actor);
                }
            }

            foreach (var actor in duplicateActors)
            {
                var newActorID = GetNewID(existingIDs);
                actor.ActorData.Identification.ActorID = newActorID;
                existingIDs.Add(newActorID);

                EditorUtility.SetDirty(actor);
                EditorSceneManager.MarkSceneDirty(actor.gameObject.scene);

                Debug.Log($"Assigned new ActorID {newActorID} to actor {actor.name}");
            }

            Debug.Log("Actor ID check and fix completed.");
        }

        private void CheckAndFixFactionIDs()
        {
            var factions          = FindObjectsByType<Faction_Component>(FindObjectsSortMode.None);
            var existingIDs       = new HashSet<ulong>();
            var duplicateFactions = new List<Faction_Component>();

            foreach (var faction in factions)
            {
                if (faction.FactionData == null)
                {
                    Debug.LogWarning($"Faction: {faction.name} does not have FactionData.");
                    continue;
                }

                if (!existingIDs.Add(faction.FactionData.FactionID))
                {
                    duplicateFactions.Add(faction);
                }
            }

            foreach (var faction in duplicateFactions)
            {
                ulong newFactionID = GetNewID(existingIDs);
                faction.FactionData.FactionID = newFactionID;
                existingIDs.Add(newFactionID);

                EditorUtility.SetDirty(faction);
                EditorSceneManager.MarkSceneDirty(faction.gameObject.scene);

                Debug.Log($"Assigned new FactionID {newFactionID} to faction {faction.name}");
            }

            Debug.Log("Faction ID check and fix completed.");
        }
    }
}