using System.Collections.Generic;
using Actor;
using UnityEngine;

namespace Faction
{
    public abstract class Faction_Manager
    {
        const string _faction_SOPath = "ScriptableObjects/Faction_SO";
        
        static Faction_SO s_allFactions;
        static Faction_SO S_AllFactions => s_allFactions ??= _getFaction_SO();
        
        public static Faction_Data GetFaction_Data(ulong factionID)
        {
            return S_AllFactions.GetFaction_Data(factionID).Data_Object;
        }
        
        public static Faction_Data GetFaction_DataFromName(Faction_Component faction_Component)
        {
            return S_AllFactions.GetDataFromName(faction_Component.name)?.Data_Object;
        }
        
        public static Faction_Component GetFaction_Component(ulong factionID)
        {
            return S_AllFactions.GetFaction_Component(factionID);
        }
        
        public static List<ulong> GetAllFactionIDs() => S_AllFactions.GetAllDataIDs();
        
        static Faction_SO _getFaction_SO()
        {
            var faction_SO = Resources.Load<Faction_SO>(_faction_SOPath);
            
            if (faction_SO is not null) return faction_SO;
            
            Debug.LogError("Faction_SO not found. Creating temporary Faction_SO.");
            faction_SO = ScriptableObject.CreateInstance<Faction_SO>();
            
            return faction_SO;
        }

        public static void AllocateActorToFactionGO(Actor_Component actor, ulong factionID)
        {
            var faction = GetFaction_Component(factionID);
 
            if (faction is null)
            {
                Debug.LogError($"Faction: {factionID} not found.");
                return;
            }

            actor.transform.parent.SetParent(faction.transform);
        }
        
        public static void ClearSOData()
        {
            S_AllFactions.ClearSOData();
        }
    }
    
    public enum FactionName
    {
        None,
        Wanderers,
        Passive,
        Player,
        Bandit,
        Demon,
        Human,
        Orc
    }
    
    public enum FactionRelationshipName
    {
        None,
        Neutral,
        Ally,
        Friend,
        Hostile,
        Enemy
    }
}