using System;
using System.Collections.Generic;
using Actor;
using Actors;
using City;
using Faction;
using JobSites;
using Region;
using Regions;
using Station;
using Tools;
using UnityEngine;

namespace DataPersistence
{
    [Serializable]
    public class Save_Data
    {
        // Profile Info
        public SavedProfileData SavedProfileData;

        // All Game Info
        public SavedRegionData SavedRegionData;
        public SavedCityData SavedCityData;
        
        public SavedJobSiteData SavedJobSiteData;

        public SavedStationData SavedStationData;
        public SavedFactionData SavedFactionData;
        public SavedActorData SavedActorData;

        // Player info
        public Vector3 PlayerPosition;
        public bool StaffPickedUp;

        // Current Scene Info
        public string SceneName;
        public string LastScene;

        public SerializableDictionary<string, string> QuestSaveData;
        public List<QuestClass> QuestData;
        
        public SerializableDictionary<string, string> PuzzleSaveData;
        public List<PuzzleData> PuzzleData;

        public Save_Data(ulong currentProfileID, string currentProfileName)
        {
            SavedProfileData = new SavedProfileData(currentProfileID, currentProfileName);
        }
    }

    [Serializable]
    public class SavedProfileData
    {
        public long LastUpdated;
        public ulong SaveDataID;
        public string SaveDataName;
        public ulong ProfileID;
        public string ProfileName;

        public SavedProfileData(ulong profileID, string profileName)
        {
            ProfileID = profileID;
            ProfileName = profileName;
        }
    }

    [Serializable]
    public class SavedRegionData
    {
        public readonly Region_Data[] AllRegionData;

        public SavedRegionData(Region_Data[] allRegionData) => AllRegionData = allRegionData;
    }

    [Serializable]
    public class SavedCityData
    {
        public readonly City_Data[] AllCityData;

        public SavedCityData(City_Data[] allCityData) => AllCityData = allCityData;
    }

    [Serializable]
    public class SavedJobSiteData
    {
        public readonly JobSite_Data[] AllJobSiteData;

        public SavedJobSiteData(JobSite_Data[] allJobSiteData) => AllJobSiteData = allJobSiteData;
    }

    [Serializable]
    public class SavedStationData
    {
        public readonly Station_Data[] AllStationData;

        public SavedStationData(Station_Data[] allStationData) => AllStationData = allStationData;
    }

    [Serializable]
    public class SavedFactionData
    {
        public Faction_Data[] AllFactionData;
        public SavedFactionData(Faction_Data[] allFactionData) => AllFactionData = allFactionData;
    }

    [Serializable]
    public class SavedActorData
    {
        public readonly Actor_Data[] AllActorData;

        public SavedActorData(Actor_Data[] allActorData) => AllActorData = allActorData;
    }
}