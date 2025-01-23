using Actor;
using UnityEngine;

namespace Faction
{
    public abstract class Faction_Manager
    {
        const string _faction_SOPath = "ScriptableObjects/Faction_SO";
        
        static Faction_SO _allFactions;
        static Faction_SO AllFactions => _allFactions ??= _getFaction_SO();
        
        public static Faction_Data GetFaction_Data(uint factionID)
        {
            return AllFactions.GetFaction_Data(factionID).Data_Object;
        }
        
        public static Faction_Data GetFaction_DataFromName(Faction_Component faction_Component)
        {
            return AllFactions.GetDataFromName(faction_Component.name)?.Data_Object;
        }
        
        public static Faction_Component GetFaction_Component(uint factionID)
        {
            return AllFactions.GetFaction_Component(factionID);
        }
        
        static Faction_SO _getFaction_SO()
        {
            var faction_SO = Resources.Load<Faction_SO>(_faction_SOPath);
            
            if (faction_SO is not null) return faction_SO;
            
            Debug.LogError("Faction_SO not found. Creating temporary Faction_SO.");
            faction_SO = ScriptableObject.CreateInstance<Faction_SO>();
            
            return faction_SO;
        }

        public static uint GetUnusedFactionID()
        {
            return AllFactions.GetUnusedFactionID();
        }

        public static void AllocateActorToFactionGO(Actor_Component actor, uint factionID)
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
            AllFactions.ClearSOData();
        }
    }

    // public class FactionGOChecker : EditorWindow
    // {
    //     [MenuItem("Tools/FactionGO Checker")]
    //     public static void ShowWindow()
    //     {
    //         GetWindow<FactionGOChecker>("FactionGO Checker");
    //         GetWindow<FactionGOChecker>("Move Actors To Factions");
    //     }
    //
    //     void OnGUI()
    //     {
    //         if (GUILayout.Button("Check FactionGOs"))
    //         {
    //             CheckAndFixFactionGOs();
    //         }
    //
    //         if (GUILayout.Button("Move Actors To Factions"))
    //         {
    //             MoveActorsToFactions();
    //         }
    //     }
    //
    //     void CheckAndFixFactionGOs()
    //     {
    //         var factionsSO = Resources.Load<Faction_SO>("ScriptableObjects/AllFactions_SO");
    //
    //         if (factionsSO == null)
    //         {
    //             Debug.LogError("No AllFactions_SO found.");
    //             return;
    //         }
    //
    //         var factionsGO = GameObject.Find("Factions");
    //
    //         if (factionsGO == null)
    //         {
    //             Debug.LogError("No Factions GameObject found.");
    //             return;
    //         }
    //
    //         var existingFactionIDs = factionsSO.AllFactionData.Select(x => x.FactionID).ToList();
    //
    //         if (existingFactionIDs.Count == 0)
    //         {
    //             Debug.LogWarning("No FactionIDs found in AllFactions_SO.");
    //         }
    //
    //         // Maybe try Resources.FindObjectsOfTypeAll instead of FindObjectsOfType which might include inactive objects as well
    //         // And then get it to work for editor and not just runtime.
    //
    //         // Something here is returning null when trying to create all the FactionGOs.
    //
    //         Faction_Component[] existingFactionComponents = new Faction_Component[0];
    //         List<GameObject>   existingFactionGOs        = new();
    //
    //         existingObjectsCheck();
    //
    //         var factionIDsWithoutGOs = existingFactionIDs.Where(fID => existingFactionGOs.All(fgo => fgo.name != $"{fID}: {factionsSO.AllFactionData[(int)fID].FactionName}")).ToList();
    //
    //         var factionIDsToRemove = new List<uint>();
    //         foreach (var factionID in factionIDsWithoutGOs)
    //         {
    //             Debug.LogWarning($"Creating FactionGO and FactionComponent for FactionID: {factionID}: {factionsSO.AllFactionData[(int)factionID].FactionName}");
    //             _createFactionGO(factionsGO, factionID, factionsSO.AllFactionData[(int)factionID].FactionName, factionsSO);
    //             factionIDsToRemove.Add(factionID);
    //         }
    //
    //         foreach (var factionID in factionIDsToRemove)
    //         {
    //             factionIDsWithoutGOs.Remove(factionID);
    //         }
    //
    //         existingObjectsCheck();
    //
    //         var factionIDsWithoutComponents         = existingFactionIDs.Where(fID => existingFactionComponents.All(fc => fc.FactionData.FactionID != fID)).ToList();
    //         var factionIDsWithoutComponentsToRemove = new List<uint>();
    //         foreach (var factionID in factionIDsWithoutComponents)
    //         {
    //             var existingFactionGO = existingFactionGOs.FirstOrDefault(fgo => fgo.name == $"{factionID}: {factionsSO.AllFactionData[(int)factionID].FactionName}");
    //             if (existingFactionGO != null)
    //             {
    //                 Debug.LogWarning($"Updating FactionComponent with existing FactionGO to match existing FactionID: {factionID}: {factionsSO.AllFactionData[(int)factionID].FactionName}");
    //                 if (existingFactionGO.GetComponent<Faction_Component>() != null)
    //                 {
    //                     Debug.LogWarning($"Destroying existing FactionComponent on FactionGO: {existingFactionGO.name}");
    //                     DestroyImmediate(existingFactionGO.GetComponent<Faction_Component>());
    //                 }
    //                 var factionComponent = existingFactionGO.gameObject.AddComponent<Faction_Component>();
    //                 var factionData      = factionsSO.AllFactionData.FirstOrDefault(x => x.FactionID == factionID);
    //                 if (factionData == null)
    //                 {
    //                     Debug.LogError($"FactionData not found for FactionGO: {existingFactionGO.name}");
    //                     continue;
    //                 }
    //                 factionComponent.FactionData = factionData;
    //                 factionIDsWithoutComponentsToRemove.Add(factionID);
    //             }
    //         }
    //
    //         foreach (var factionID in factionIDsWithoutComponentsToRemove)
    //         {
    //             factionIDsWithoutComponents.Remove(factionID);
    //         }
    //
    //         existingObjectsCheck();
    //
    //         var factionGOsWithoutComponents         = existingFactionGOs.Where(fgo => fgo.GetComponent<Faction_Component>() == null).ToList();
    //         var factionGOsWithoutComponentsToRemove = new List<GameObject>();
    //         foreach (var factionGameObject in factionGOsWithoutComponents)
    //         {
    //             Debug.LogWarning($"Creating FactionComponent for FactionGO: {factionGameObject.name}");
    //             var factionComponent = factionGameObject.AddComponent<Faction_Component>();
    //             var factionData      = factionsSO.AllFactionData.FirstOrDefault(x => x.FactionID == int.Parse(factionGameObject.name.Split(':')[0]));
    //             if (factionData == null)
    //             {
    //                 Debug.LogError($"FactionData not found for FactionGO: {factionGameObject.name}");
    //                 continue;
    //             }
    //             factionComponent.FactionData = factionData;
    //             factionGOsWithoutComponentsToRemove.Add(factionGameObject);
    //         }
    //
    //         foreach (var factionGameObject in factionGOsWithoutComponentsToRemove)
    //         {
    //             factionGOsWithoutComponents.Remove(factionGameObject);
    //         }
    //
    //         existingObjectsCheck();
    //
    //         var factionComponentsWithoutGOs         = existingFactionComponents.Where(fc => existingFactionGOs.All(fgo => fgo.name != $"{fc.FactionData.FactionID}: {fc.FactionData.FactionName}")).ToList();
    //         var factionComponentsWithoutGOsToRemove = new List<Faction_Component>();
    //         foreach (var factionComponent in factionComponentsWithoutGOs)
    //         {
    //             Debug.LogWarning($"Changing FactionGO name to match FactionComponent: {factionComponent.FactionData.FactionID}: {factionComponent.FactionData.FactionName}");
    //             factionComponent.gameObject.name = $"{factionComponent.FactionData.FactionID}: {factionComponent.FactionData.FactionName}";
    //             factionComponentsWithoutGOsToRemove.Add(factionComponent);
    //         }
    //
    //         foreach (var factionComponent in factionComponentsWithoutGOsToRemove)
    //         {
    //             factionComponentsWithoutGOs.Remove(factionComponent);
    //         }
    //
    //         if (factionGOsWithoutComponents.Count == 0 && factionComponentsWithoutGOs.Count == 0 && factionIDsWithoutGOs.Count == 0 && factionIDsWithoutComponents.Count == 0)
    //         {
    //             Debug.Log("All FactionGOs and FactionComponents are in sync.");
    //             return;
    //         }
    //
    //         if (factionGOsWithoutComponents.Count > 0)
    //         {
    //             Debug.LogWarning($"FactionGOs without FactionComponents: {string.Join(", ", factionGOsWithoutComponents.Select(x => x.name))}");
    //
    //             foreach (var factionGO in factionGOsWithoutComponents)
    //             {
    //                 Debug.LogError($"FactionGO: {factionGO.name} does not have FactionComponent.");
    //             }
    //         }
    //
    //         if (factionComponentsWithoutGOs.Count > 0)
    //         {
    //             Debug.LogWarning($"FactionComponents without FactionGOs: {string.Join(", ", factionComponentsWithoutGOs.Select(x => x.FactionData.FactionID))}");
    //
    //             foreach (var factionComponent in factionComponentsWithoutGOs)
    //             {
    //                 Debug.LogError($"FactionComponent: {factionComponent.FactionData.FactionID}: {factionComponent.FactionData.FactionName} does not have FactionGO.");
    //             }
    //         }
    //
    //         if (factionIDsWithoutGOs.Count > 0)
    //         {
    //             Debug.LogWarning($"FactionIDs without FactionGOs: {string.Join(", ", factionIDsWithoutGOs.Select(x => x))}");
    //
    //             foreach (var factionID in factionIDsWithoutGOs)
    //             {
    //                 Debug.LogError($"FactionID: {factionID} does not have FactionGO.");
    //             }
    //         }
    //
    //         if (factionIDsWithoutComponents.Count > 0)
    //         {
    //             Debug.LogWarning($"FactionIDs without FactionComponents: {string.Join(", ", factionIDsWithoutComponents.Select(x => x))}");
    //
    //             foreach (var factionID in factionIDsWithoutComponents)
    //             {
    //                 Debug.LogError($"FactionID: {factionID} does not have FactionComponent.");
    //             }
    //         }
    //
    //         Debug.LogWarning("FactionGO and FactionComponent check and fix completed.");
    //
    //         void existingObjectsCheck()
    //         {
    //             existingFactionComponents = FindObjectsByType<Faction_Component>(FindObjectsSortMode.None);
    //
    //             existingFactionGOs = new List<GameObject>();
    //
    //             foreach (Transform child in factionsGO.transform)
    //             {
    //                 existingFactionGOs.Add(child.gameObject);
    //             }
    //         }
    //     }
    //
    //     void _createFactionGO(GameObject factionsGO, uint factionID, string factionName, Faction_SO factionsSO)
    //     {
    //         var factionGO = new GameObject($"{factionID}: {factionName}");
    //         factionGO.transform.SetParent(factionsGO.transform);
    //         factionGO.transform.position = Vector3.zero;
    //         var factionComponent = factionGO.AddComponent<Faction_Component>();
    //         var factionData      = factionsSO.AllFactionData.FirstOrDefault(x => x.FactionID == factionID);
    //         if (factionData == null)
    //         {
    //             Debug.LogError($"FactionData not found for FactionGO: {factionGO.name}");
    //             return;
    //         }
    //
    //         factionComponent.FactionData = factionData;
    //     }
    //
    //     void MoveActorsToFactions()
    //     {
    //         var actors = FindObjectsByType<Actor_Component>(FindObjectsSortMode.None);
    //
    //         foreach (var actor in actors)
    //         {
    //             var actorsSO = Resources.Load<Actor_SO>("ScriptableObjects/AllActors_SO");
    //
    //             if (actorsSO == null)
    //             {
    //                 Debug.LogError("No AllActors_SO found.");
    //                 return;
    //             }
    //
    //             var actorData = actorsSO.AllActorData.FirstOrDefault(x => x.ActorID == actor.ActorData.ActorID);
    //
    //             if (actorData == null)
    //             {
    //                 Debug.LogError($"ActorData not found for Actor: {actor.name}");
    //                 continue;
    //             }
    //
    //             var factionsSO = Resources.Load<Faction_SO>("ScriptableObjects/AllFactions_SO");
    //
    //             if (factionsSO == null)
    //             {
    //                 Debug.LogError("No AllFactions_SO found.");
    //                 return;
    //             }
    //
    //             var factionID = actorData.ActorFactionID;
    //
    //             var faction = factionsSO.AllFactionData.FirstOrDefault(x => x.FactionID == factionID);
    //
    //             if (faction == null)
    //             {
    //                 Debug.LogError($"Faction: {factionID} not found.");
    //                 return;
    //             }
    //
    //             var factionGO = GameObject.Find($"{factionID}: {faction.FactionName}");
    //
    //             if (factionGO == null)
    //             {
    //                 Debug.LogError($"FactionGO: {factionID}: {faction.FactionName} not found.");
    //                 return;
    //             }
    //
    //             actor.transform.SetParent(factionGO.transform);
    //
    //             Debug.Log($"Moved Actor: {actor.name} to Faction: {factionID}: {faction.FactionName}");
    //         }
    //
    //         Debug.Log("Actors moved to Factions.");
    //     }
    // }

    public enum DefaultFactionName
    {
        Wanderers,
        
        PlayerFaction
    }
}