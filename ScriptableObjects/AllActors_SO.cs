using System;
using System.Collections.Generic;
using System.Linq;
using Actors;
using Managers;
using UnityEditor;
using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "AllActors_SO", menuName = "SOList/AllActors_SO")]
    [Serializable]
    public class AllActors_SO : ScriptableObject
    {
        public List<ActorData> AllActorData;

        public void LoadData(SaveData saveData)
        {
            AllActorData = saveData.SavedActorData.AllActorData;
        }

        public void ClearActorData() => AllActorData.Clear();
    }

    [CustomEditor(typeof(AllActors_SO))]
    public class AllActors_SOEditor : Editor
    {
        int  _selectedActorIndex = -1;
        int  SelectedActorIndex { get => _selectedActorIndex;
            set { if (_selectedActorIndex == value) return; _selectedActorIndex = value; _resetIndexes(1); } }
        bool _showGameObjectProperties;
        bool _showSpeciesAndPersonality;
        bool _showCareerAndJobs;

        Vector2 _actorScrollPos;
    
        void _resetIndexes(int i = -1)
        {
            _showGameObjectProperties  = false;
            _showSpeciesAndPersonality = false;
            if (i == 1) return;
            _selectedActorIndex = -1;
        }

        public override void OnInspectorGUI()
        {
            AllActors_SO allActorSO = (AllActors_SO)target;

            if (GUILayout.Button("Clear Actor Data"))
            {
                _resetIndexes();
                allActorSO.ClearActorData();
                EditorUtility.SetDirty(allActorSO);
            }

            if (GUILayout.Button("Unselect All")) _resetIndexes();

            EditorGUILayout.LabelField("All Actors", EditorStyles.boldLabel);
            _actorScrollPos    = EditorGUILayout.BeginScrollView(_actorScrollPos, GUILayout.Height(Math.Min(200, allActorSO.AllActorData.Count * 20)));
            SelectedActorIndex = GUILayout.SelectionGrid(SelectedActorIndex, _getActorNames(allActorSO), 1);
            EditorGUILayout.EndScrollView();

            if (SelectedActorIndex >= 0 && SelectedActorIndex < allActorSO.AllActorData.Count)
            {
                _drawActorData(allActorSO.AllActorData[SelectedActorIndex]);
            }
        }

        string[] _getActorNames(AllActors_SO allActorsSO)
        {
            return allActorsSO.AllActorData.Select(a => $"{a.ActorID}: {a.ActorName.GetName()}").ToArray();
        }

        bool    _showInventory;
        bool    _showEquipment;
        Vector2 _inventoryItemScrollPos;

        void _drawActorData(ActorData actorData)
        {
            EditorGUILayout.LabelField("Actor Data", EditorStyles.boldLabel);

            EditorGUILayout.LabelField("Actor ID", actorData.ActorID.ToString());

            EditorGUILayout.LabelField("Actor Name", $"{actorData.ActorName.Name} {actorData.ActorName.Surname}");

            if (actorData.FullIdentification != null)
            {
                EditorGUILayout.LabelField("Full Identification", EditorStyles.boldLabel);

                EditorGUILayout.LabelField("Actor ID", actorData.FullIdentification.ActorReference.ActorID.ToString());

                if (actorData.FullIdentification.ActorName != null)
                {
                    EditorGUILayout.LabelField("Actor Name", EditorStyles.boldLabel);
                    EditorGUILayout.LabelField("Name",       actorData.FullIdentification.ActorName.Name);
                    EditorGUILayout.LabelField("Surname",    actorData.FullIdentification.ActorName.Surname);
                    // Title
                    // Available Titles
                }

                // ActorFamily

                EditorGUILayout.LabelField("Faction", actorData.FullIdentification.ActorFactionID.ToString());

                if (actorData.FullIdentification.Background != null)
                {
                    //EditorGUILayout.LabelField("Background", EditorStyles.boldLabel);
                    //EditorGUILayout.LabelField("Birthplace", actorData.FullIdentification.Background.Birthplace);
                    //EditorGUILayout.LabelField("Birthdate", actorData.FullIdentification.Background.Birthdate.ToString());
                    //EditorGUILayout.LabelField("Religion", actorData.FullIdentification.Background.Religion);
                }
            }

            if (actorData.GameObjectProperties != null)
            {
                _showGameObjectProperties = EditorGUILayout.Toggle("GameObjectProperties", _showGameObjectProperties);

                if (_showGameObjectProperties)
                {
                    _drawGameObjectProperties(actorData.GameObjectProperties);
                }
            }

            if (actorData.CareerAndJobs != null)
            {
                _showCareerAndJobs = EditorGUILayout.Toggle("Career and Jobs", _showCareerAndJobs);

                if (_showCareerAndJobs)
                {
                    _drawCareerAndJobs(actorData.CareerAndJobs);
                }
            }

            if (actorData.SpeciesAndPersonality != null)
            {
                _showSpeciesAndPersonality = EditorGUILayout.Toggle("Species and Personality", _showSpeciesAndPersonality);

                if (_showSpeciesAndPersonality)
                {
                    _drawSpeciesAndPersonality(actorData.SpeciesAndPersonality);
                }
            }

            if (actorData.StatsAndAbilities != null)
            {
                EditorGUILayout.LabelField("Stats And Abilities", EditorStyles.boldLabel);
            }

            if (actorData.InventoryData != null)
            {
                EditorGUILayout.LabelField("Inventory And Equipment", EditorStyles.boldLabel);

                var inventoryData = actorData.InventoryData;

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

            if (actorData.ActorQuests != null)
            {
                //EditorGUILayout.LabelField("Actor Quests", EditorStyles.boldLabel);
                //EditorGUILayout.IntField("Active Quests", actorData.ActorQuests.ActiveQuests.Count);
            }

            // var orderData = actorData.OrderData;

            // _showOrders = EditorGUILayout.Toggle("Orders", _showOrders);

            // if (_showOrders)
            // {
            //     DrawOrderData(orderData);
            // }

            var order = actorData.CurrentOrder;

            if (order != null)
            {
                EditorGUILayout.LabelField("Current Order", EditorStyles.boldLabel);
                _drawOrder(order);
            }
        }

        void _drawGameObjectProperties(GameObjectProperties gameObjectProperties)
        {
            // Not sure if these are a good idea yet, since they'd just be for the SO.
            // EditorGUILayout.Vector3Field("Current Position", gameObjectProperties.ActorTransform.position);
            // EditorGUILayout.Vector3Field("Current Rotation", gameObjectProperties.ActorTransform.rotation.eulerAngles);
            // EditorGUILayout.Vector3Field("Current Scale", gameObjectProperties.ActorTransform.localScale);

            EditorGUILayout.Vector3Field("Last Saved Position", gameObjectProperties.LastSavedActorPosition);
            EditorGUILayout.Vector3Field("Last Saved Scale",    gameObjectProperties.LastSavedActorScale);
            EditorGUILayout.Vector3Field("Last Saved Rotation", gameObjectProperties.LastSavedActorRotation.eulerAngles);
            EditorGUILayout.ObjectField("Mesh",     gameObjectProperties.ActorMesh,     typeof(Mesh),     allowSceneObjects: true);
            EditorGUILayout.ObjectField("Material", gameObjectProperties.ActorMaterial, typeof(Material), allowSceneObjects: true);
        }

        void _drawSpeciesAndPersonality(SpeciesAndPersonality speciesAndPersonality)
        {
            EditorGUILayout.LabelField("Species",     speciesAndPersonality.ActorSpecies.ToString());
            EditorGUILayout.LabelField("Personality", speciesAndPersonality.ActorPersonality.ToString());
            // Add more details as needed
        }

        void _drawCareerAndJobs(CareerAndJobs careerAndJobs)
        {
            EditorGUILayout.LabelField("JobsActive",        careerAndJobs.JobsActive.ToString());
            EditorGUILayout.LabelField("JobSiteID",         careerAndJobs.JobsiteID.ToString());
            EditorGUILayout.LabelField("JobName",       careerAndJobs.CurrentActorJob.JobName.ToString());
            EditorGUILayout.LabelField("Employee Position", careerAndJobs.EmployeePosition.ToString());
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

        void _drawEquipment()
        {

        }
        
        Vector2 _currentOrderScrollPos;

        Vector2 _completedOrderScrollPos;

        void _drawOrder(Order_Base order)
        {
            EditorGUILayout.LabelField("Order Type",   order.OrderType.ToString());
            EditorGUILayout.LabelField("Order ID",     order.OrderID.ToString());
            EditorGUILayout.LabelField("Order Status", order.OrderStatus.ToString());
        }
    }
}