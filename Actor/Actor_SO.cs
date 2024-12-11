using System;
using System.Collections.Generic;
using System.Linq;
using Inventory;
using ScriptableObjects;
using UnityEditor;
using UnityEngine;

namespace Actor
{
    [CreateAssetMenu(fileName = "Actor_SO", menuName = "SOList/Actor_SO")]
    [Serializable]
    public class Actor_SO : Base_SO<Actor_Data>
    {
        public Actor_Data[]                        Actors                           => Objects;
        public Actor_Data                          GetActor_Data(uint      actorID) => GetObject_Master(actorID);
        public Dictionary<uint, Actor_Component> ActorComponents = new();

        public Actor_Component GetActor_Component(uint actorID)
        {
            if (ActorComponents.TryGetValue(actorID, out var component))
            {
                return component;
            }   
            
            Debug.LogError($"Actor with ID {actorID} not found in Actor_SO.");
            return null;
        }

        public override uint GetObjectID(int id) => Actors[id].ActorID;

        public void UpdateActor(uint actorID, Actor_Data actor_Data) => UpdateObject(actorID, actor_Data);
        public void UpdateAllActors(Dictionary<uint, Actor_Data> allActors) => UpdateAllObjects(allActors);

        public void PopulateSceneActors()
        {
            var allActorComponents = FindObjectsByType<Actor_Component>(FindObjectsSortMode.None);
            var allActorData =
                allActorComponents.ToDictionary(actor => actor.ActorID, actor => actor.ActorData);

            // Make sure default actors can't be overridden by rewriting save file, or put in a check if it has happened.
            
            UpdateAllActors(allActorData);
        }

        protected override Dictionary<uint, Actor_Data> _populateDefaultObjects()
        {
            var defaultFactions = new Dictionary<uint, Actor_Data>();

            foreach (var defaultActor in Actor_List.DefaultActors)
            {
                defaultFactions.Add(defaultActor.Key, new Actor_Data(defaultActor.Value));
            }

            return defaultFactions;
        }

        static uint _lastUnusedActorID = 1;

        public uint GetUnusedActorID()
        {
            while (ObjectIndexLookup.ContainsKey(_lastUnusedActorID))
            {
                _lastUnusedActorID++;
            }

            return _lastUnusedActorID;
        }
    }

    [CustomEditor(typeof(Actor_SO))]
    public class Actor_SOEditor : Editor
    {
        int  _selectedActorIndex = -1;
        int  SelectedActorIndex { get => _selectedActorIndex;
            set { if (_selectedActorIndex == value) return; _selectedActorIndex = value; _resetIndexes(1); } }
        
        bool    _showFullIdentification;
        bool    _showGameObjectProperties;
        bool    _showSpeciesAndPersonality;
        bool    _showStatsAndAbilities;
        bool    _showCareerAndJobs;
        bool    _showInventory;
        bool    _showEquipment;
        

        Vector2 _actorScrollPos;
        Vector2 _inventoryItemScrollPos;
    
        void _resetIndexes(int i = -1)
        {
            _showGameObjectProperties  = false;
            _showSpeciesAndPersonality = false;
            if (i == 1) return;
            _selectedActorIndex = -1;
        }

        public override void OnInspectorGUI()
        {
            var actorSO = (Actor_SO)target;
            
            if (GUILayout.Button("Unselect All")) _resetIndexes();

            EditorGUILayout.LabelField("All Actors", EditorStyles.boldLabel);
            _actorScrollPos = EditorGUILayout.BeginScrollView(_actorScrollPos,
                GUILayout.Height(Mathf.Min(200, actorSO.Actors.Length * 20)));
            _selectedActorIndex = GUILayout.SelectionGrid(_selectedActorIndex, _getActorNames(actorSO), 1);
            EditorGUILayout.EndScrollView();

            if (_selectedActorIndex < 0 || _selectedActorIndex >= actorSO.Actors.Length) return;

            var selectedActorData = actorSO.Actors[_selectedActorIndex];
            _drawActorAdditionalData(selectedActorData);
        }

        static string[] _getActorNames(Actor_SO actorSO)
        {
            return actorSO.Actors.Select(c => c.ActorName.GetName()).ToArray();
        }

        void _drawActorAdditionalData(Actor_Data selectedActorData)
        {
            EditorGUILayout.LabelField("Actor Data", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Actor Name", $"{selectedActorData.ActorName.GetName()}");
            EditorGUILayout.LabelField("Actor ID",   $"{selectedActorData.ActorID}");
            EditorGUILayout.LabelField("Faction ID",  $"{selectedActorData.ActorFactionID}");

            if (selectedActorData.FullIdentification != null)
            {
                _showFullIdentification = EditorGUILayout.Toggle("Population", _showFullIdentification);

                if (_showFullIdentification)
                {
                    _drawFullIdentificationDetails(selectedActorData.FullIdentification);
                }
            }

            if (selectedActorData.GameObjectData != null)
            {
                _showGameObjectProperties = EditorGUILayout.Toggle("GameObjectProperties", _showGameObjectProperties);

                if (_showGameObjectProperties)
                {
                    _drawGameObjectProperties(selectedActorData.GameObjectData);
                }
            }

            if (selectedActorData.CareerData != null)
            {
                _showCareerAndJobs = EditorGUILayout.Toggle("Career and Jobs", _showCareerAndJobs);

                if (_showCareerAndJobs)
                {
                    _drawCareerAndJobs(selectedActorData.CareerData);
                }
            }

            if (selectedActorData.SpeciesAndPersonality != null)
            {
                _showSpeciesAndPersonality = EditorGUILayout.Toggle("Species and Personality", _showSpeciesAndPersonality);

                if (_showSpeciesAndPersonality)
                {
                    _drawSpeciesAndPersonality(selectedActorData.SpeciesAndPersonality);
                }
            }

            if (selectedActorData.StatsAndAbilities != null)
            {
                EditorGUILayout.LabelField("Stats And Abilities", EditorStyles.boldLabel);
                
                _showStatsAndAbilities = EditorGUILayout.Toggle("Stats and Abilities", _showStatsAndAbilities);
                
                if (_showStatsAndAbilities)
                {
                    _drawStatsAndAbilities(selectedActorData.StatsAndAbilities);
                }
            }

            if (selectedActorData.InventoryData != null)
            {
                EditorGUILayout.LabelField("Inventory And Equipment", EditorStyles.boldLabel);

                var inventoryData = selectedActorData.InventoryData;

                _showInventory = EditorGUILayout.Toggle("Inventory", _showInventory);

                if (_showInventory)
                {
                    _drawInventory(inventoryData);
                }

                _showEquipment = EditorGUILayout.Toggle("Equipment", _showEquipment);

                if (_showEquipment)
                {
                    _drawEquipment();
                }
            }

            if (selectedActorData.ActorQuests != null)
            {
                //EditorGUILayout.LabelField("Actor Quests", EditorStyles.boldLabel);
                //EditorGUILayout.IntField("Active Quests", actorData.ActorQuests.ActiveQuests.Count);
            }
        }

        void _drawFullIdentificationDetails(FullIdentification fullIdentification)
        {
            EditorGUILayout.LabelField("Full Identification", EditorStyles.boldLabel);

            EditorGUILayout.LabelField("Actor ID", $"{fullIdentification.ActorReference.ActorID}");

            if (fullIdentification.ActorName != null)
            {
                EditorGUILayout.LabelField("Actor Name", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Name",       fullIdentification.ActorName.Name);
                EditorGUILayout.LabelField("Surname",    fullIdentification.ActorName.Surname);
                // Title
                // Available Titles
            }

            // ActorFamily

            EditorGUILayout.LabelField("Faction", fullIdentification.ActorFactionID.ToString());

            if (fullIdentification.Background != null)
            {
                //EditorGUILayout.LabelField("Background", EditorStyles.boldLabel);
                //EditorGUILayout.LabelField("Birthplace", actorData.FullIdentification.Background.Birthplace);
                //EditorGUILayout.LabelField("Birthdate", actorData.FullIdentification.Background.Birthdate.ToString());
                //EditorGUILayout.LabelField("Religion", actorData.FullIdentification.Background.Religion);
            }
        }

        static void _drawStatsAndAbilities(StatsAndAbilities statsAndAbilities)
        {
            
        }

        static void _drawGameObjectProperties(GameObjectData gameObjectData)
        {
            // Not sure if these are a good idea yet, since they'd just be for the SO.
            // EditorGUILayout.Vector3Field("Current Position", gameObjectProperties.ActorTransform.position);
            // EditorGUILayout.Vector3Field("Current Rotation", gameObjectProperties.ActorTransform.rotation.eulerAngles);
            // EditorGUILayout.Vector3Field("Current Scale", gameObjectProperties.ActorTransform.localScale);

            EditorGUILayout.Vector3Field("Last Saved Position", gameObjectData.LastSavedActorPosition);
            EditorGUILayout.Vector3Field("Last Saved Scale",    gameObjectData.LastSavedActorScale);
            EditorGUILayout.Vector3Field("Last Saved Rotation", gameObjectData.LastSavedActorRotation.eulerAngles);
            EditorGUILayout.ObjectField("Mesh",     gameObjectData.ActorMesh,     typeof(Mesh),     allowSceneObjects: true);
            EditorGUILayout.ObjectField("Material", gameObjectData.ActorMaterial, typeof(Material), allowSceneObjects: true);
        }

        static void _drawSpeciesAndPersonality(SpeciesAndPersonality speciesAndPersonality)
        {
            EditorGUILayout.LabelField("Species",     speciesAndPersonality.ActorSpecies.ToString());
            EditorGUILayout.LabelField("Personality", speciesAndPersonality.ActorPersonality.ToString());
            // Add more details as needed
        }

        static void _drawCareerAndJobs(CareerData careerData)
        {
            EditorGUILayout.LabelField("JobsActive",        careerData.JobsActive.ToString());
            EditorGUILayout.LabelField("JobSiteID",         careerData.JobsiteID.ToString());
            EditorGUILayout.LabelField("JobName",       careerData.CurrentJob.JobName.ToString());
            EditorGUILayout.LabelField("Employee Position", careerData.EmployeePositionName.ToString());
        }

        void _drawInventory(InventoryData data)
        {
            EditorGUILayout.LabelField("Gold", $"{data.Gold}");

            if (data.AllInventoryItems.Count == 1)
            {
                EditorGUILayout.LabelField($"{data.AllInventoryItems[0].ItemName}: {data.AllInventoryItems[0].ItemAmount}");
            }
            else
            {
                _inventoryItemScrollPos = EditorGUILayout.BeginScrollView(_inventoryItemScrollPos, GUILayout.Height(Math.Min(200, data.AllInventoryItems.Count * 20)));

                try
                {
                    foreach (var item in data.AllInventoryItems.Values)
                    {
                        EditorGUILayout.LabelField($"{item.ItemName}: {item.ItemAmount}");
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

        static void _drawEquipment()
        {

        }
    }
}