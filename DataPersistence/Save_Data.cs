using System;
using System.Collections.Generic;
using Actor;
using Actors;
using Baronies;
using Buildings;
using Cities;
using Counties;
using Faction;
using Settlements;
using Station;
using Tools;
using UnityEngine;
using UnityEngine.Serialization;

namespace DataPersistence
{
    [Serializable]
    public class Save_Data
    {
        // Profile Info
        public SavedProfileData SavedProfileData;

        // All Game Info
        public SavedCountyData SavedCountyData;
        public SavedBaronyData SavedBaronyData;
        public SavedSettlementData SavedSettlementData;
        
        public SavedBuildingData SavedBuildingData;

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
    public class SavedCountyData
    {
        public readonly County_Data[] AllCountyData;

        public SavedCountyData(County_Data[] allCountyData) => AllCountyData = allCountyData;
    }
    
    [Serializable]
    public class SavedBaronyData
    {
        public readonly Barony_Data[] AllBaronyData;

        public SavedBaronyData(Barony_Data[] allBaronyData) => AllBaronyData = allBaronyData;
    }

    [Serializable]
    public class SavedSettlementData
    {
        public readonly Settlement_Data[] AllSettlementData;

        public SavedSettlementData(Settlement_Data[] allSettlementData) => AllSettlementData = allSettlementData;
    }

    [Serializable]
    public class SavedBuildingData
    {
        public readonly Building_Data[] AllBuildingData;

        public SavedBuildingData(Building_Data[] allBuildingData) => AllBuildingData = allBuildingData;
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