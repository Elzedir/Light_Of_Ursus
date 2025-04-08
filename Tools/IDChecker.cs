using System.Collections.Generic;
using Actors;
using Baronies;
using Buildings;
using Counties;
using Faction;
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
                CheckAndFixBuildingIDs();
                CheckAndFixBaronyIDs();
                CheckAndFixRegionIDs();
                CheckAndFixActorIDs();
                CheckAndFixFactionIDs();
            }

            // if (GUILayout.Button("Check and Fix Station IDs"))
            // {
            //     CheckAndFixStationIDs();
            // }

            if (GUILayout.Button("Check and Fix Building IDs"))
            {
                CheckAndFixBuildingIDs();
            }

            if (GUILayout.Button("Check and Fix Barony IDs"))
            {
                CheckAndFixBaronyIDs();
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

        private void CheckAndFixBuildingIDs()
        {
            var buildings          = FindObjectsByType<Building_Component>(FindObjectsSortMode.None);
            var existingIDs       = new HashSet<ulong>();
            var duplicateBuildings = new List<Building_Component>();

            foreach (var building in buildings)
            {
                if (building.Building_Data == null)
                {
                    Debug.LogWarning($"Building: {building.name} does not have BuildingData.");
                    continue;
                }

                if (!existingIDs.Add(building.Building_Data.ID))
                {
                    duplicateBuildings.Add(building);
                }
            }

            foreach (var building in duplicateBuildings)
            {
                ulong newBuildingID = GetNewID(existingIDs);
                building.Building_Data.ID = newBuildingID;
                existingIDs.Add(newBuildingID);

                EditorUtility.SetDirty(building);
                EditorSceneManager.MarkSceneDirty(building.gameObject.scene);

                Debug.Log($"Assigned new BuildingID {newBuildingID} to building {building.name}");
            }

            Debug.Log("Building ID check and fix completed.");
        }

        void CheckAndFixBaronyIDs()
        {
            var baronies          = FindObjectsByType<Barony_Component>(FindObjectsSortMode.None);
            var existingIDs     = new HashSet<ulong>();
            var duplicateCities = new List<Barony_Component>();

            foreach (var barony in baronies)
            {
                if (barony.Barony_Data == null)
                {
                    Debug.LogWarning($"Barony: {barony.name} does not have BaronyData.");
                    continue;
                }

                if (!existingIDs.Add(barony.Barony_Data.ID))
                {
                    duplicateCities.Add(barony);
                }
            }

            foreach (var barony in duplicateCities)
            {
                var newBaronyID = GetNewID(existingIDs);
                barony.Barony_Data.ID = newBaronyID;
                existingIDs.Add(newBaronyID);

                EditorUtility.SetDirty(barony);
                EditorSceneManager.MarkSceneDirty(barony.gameObject.scene);

                Debug.Log($"Assigned new BaronyID {newBaronyID} to Barony {barony.name}");
            }

            Debug.Log("Barony ID check and fix completed.");
        }

        private void CheckAndFixRegionIDs()
        {
            var regions          = FindObjectsByType<County_Component>(FindObjectsSortMode.None);
            var existingIDs      = new HashSet<ulong>();
            var duplicateRegions = new List<County_Component>();

            foreach (var region in regions)
            {
                if (region.County_Data == null)
                {
                    Debug.LogWarning($"Region: {region.name} does not have RegionData.");
                    continue;
                }

                if (!existingIDs.Add(region.County_Data.ID))
                {
                    duplicateRegions.Add(region);
                }
            }

            foreach (var region in duplicateRegions)
            {
                ulong newRegionID = GetNewID(existingIDs);
                region.County_Data.ID = newRegionID;
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