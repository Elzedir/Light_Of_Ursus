using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

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
    int _selectedActorIndex = -1;
    int SelectedActorIndex { get { return _selectedActorIndex; } set { if (_selectedActorIndex == value) return; _selectedActorIndex = value; _resetIndexes(1); } }
    bool _showGameObjectProperties = false;
    bool _showSpeciesAndPersonality = false;
    bool _showCareerAndJobs = false;
    bool _showOrders = false;

    Vector2 _actorScrollPos;
    
    void _resetIndexes(int i = -1)
    {
        _showGameObjectProperties = false;
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
        _actorScrollPos = EditorGUILayout.BeginScrollView(_actorScrollPos, GUILayout.Height(Math.Min(200, allActorSO.AllActorData.Count * 20)));
        SelectedActorIndex = GUILayout.SelectionGrid(SelectedActorIndex, GetActorNames(allActorSO), 1);
        EditorGUILayout.EndScrollView();

        if (SelectedActorIndex >= 0 && SelectedActorIndex < allActorSO.AllActorData.Count)
        {
            DrawActorData(allActorSO.AllActorData[SelectedActorIndex]);
        }
    }

    private string[] GetActorNames(AllActors_SO allActorsSO)
    {
        return allActorsSO.AllActorData.Select(a => $"{a.ActorID}: {a.ActorName.GetName()}").ToArray();
    }

    bool _showInventory = false;
    bool _showEquipment = false;
    Vector2 inventoryItemScrollPos;

    private void DrawActorData(ActorData actorData)
    {
        EditorGUILayout.LabelField("Actor Data", EditorStyles.boldLabel);

        EditorGUILayout.LabelField("Actor ID", actorData.ActorID.ToString());

        EditorGUILayout.LabelField("Actor Name", $"{actorData.ActorName.Name} {actorData.ActorName.Surname}");

        if (actorData.FullIdentification != null)
        {
            EditorGUILayout.LabelField("Full Identification", EditorStyles.boldLabel);

            EditorGUILayout.LabelField("Actor ID", actorData.FullIdentification.ActorID.ToString());

            if (actorData.FullIdentification.ActorName != null)
            {
                EditorGUILayout.LabelField("Actor Name", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Name", actorData.FullIdentification.ActorName.Name);
                EditorGUILayout.LabelField("Surname", actorData.FullIdentification.ActorName.Surname);
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
                DrawGameObjectProperties(actorData.GameObjectProperties);
            }
        }

        if (actorData.CareerAndJobs != null)
        {
            _showCareerAndJobs = EditorGUILayout.Toggle("Career and Jobs", _showCareerAndJobs);

            if (_showCareerAndJobs)
            {
                DrawCareerAndJobs(actorData.CareerAndJobs);
            }
        }

        if (actorData.SpeciesAndPersonality != null)
        {
            _showSpeciesAndPersonality = EditorGUILayout.Toggle("Species and Personality", _showSpeciesAndPersonality);

            if (_showSpeciesAndPersonality)
            {
                DrawSpeciesAndPersonality(actorData.SpeciesAndPersonality);
            }
        }

        if (actorData.StatsAndAbilities != null)
        {
            EditorGUILayout.LabelField("Stats And Abilities", EditorStyles.boldLabel);
        }

        if (actorData.InventoryAndEquipment != null)
        {
            EditorGUILayout.LabelField("Inventory And Equipment", EditorStyles.boldLabel);

            var inventoryData = actorData.InventoryAndEquipment.InventoryData;

            _showInventory = EditorGUILayout.Toggle("Inventory", _showInventory);

            if (_showInventory)
            {
                DrawInventory(inventoryData);
            }

            var equipmentData = actorData.InventoryAndEquipment.EquipmentData;

            _showEquipment = EditorGUILayout.Toggle("Equipment", _showEquipment);

            if (_showEquipment)
            {
                DrawEquipment(equipmentData);
            }
        }

        if (actorData.ActorQuests != null)
        {
            //EditorGUILayout.LabelField("Actor Quests", EditorStyles.boldLabel);
            //EditorGUILayout.IntField("Active Quests", actorData.ActorQuests.ActiveQuests.Count);
        }

        var orderData = actorData.OrderData;

        _showOrders = EditorGUILayout.Toggle("Orders", _showOrders);

        if (_showOrders)
        {
            DrawOrderData(orderData);
        }
    }

    private void DrawGameObjectProperties(GameObjectProperties gameObjectProperties)
    {
        // Not sure if these are a good idea yet, since they'd just be for the SO.
        // EditorGUILayout.Vector3Field("Current Position", gameObjectProperties.ActorTransform.position);
        // EditorGUILayout.Vector3Field("Current Rotation", gameObjectProperties.ActorTransform.rotation.eulerAngles);
        // EditorGUILayout.Vector3Field("Current Scale", gameObjectProperties.ActorTransform.localScale);

        EditorGUILayout.Vector3Field("Last Saved Position", gameObjectProperties.LastSavedActorPosition);
        EditorGUILayout.Vector3Field("Last Saved Scale", gameObjectProperties.LastSavedActorScale);
        EditorGUILayout.Vector3Field("Last Saved Rotation", gameObjectProperties.LastSavedActorRotation.eulerAngles);
        EditorGUILayout.ObjectField("Mesh", gameObjectProperties.ActorMesh, typeof(Mesh), allowSceneObjects: true);
        EditorGUILayout.ObjectField("Material", gameObjectProperties.ActorMaterial, typeof(Material), allowSceneObjects: true);
    }

    private void DrawSpeciesAndPersonality(SpeciesAndPersonality speciesAndPersonality)
    {
        EditorGUILayout.LabelField("Species", speciesAndPersonality.ActorSpecies.ToString());
        EditorGUILayout.LabelField("Personality", speciesAndPersonality.ActorPersonality.ToString());
        // Add more details as needed
    }

    void DrawCareerAndJobs(CareerAndJobs careerAndJobs)
    {
        EditorGUILayout.LabelField("JobsActive", careerAndJobs.JobsActive.ToString());
        EditorGUILayout.LabelField("JobsiteID", careerAndJobs.JobsiteID.ToString());
        EditorGUILayout.LabelField("StationID", careerAndJobs.StationID.ToString());
        EditorGUILayout.LabelField("OperatingAreaID", careerAndJobs.OperatingAreaID.ToString());
        EditorGUILayout.LabelField("Employee Position", careerAndJobs.EmployeePosition.ToString());
    }

    void DrawInventory(InventoryData data)
    {
        EditorGUILayout.LabelField("Gold", $"{data.Gold}");

        if (data.AllInventoryItems.Count == 1)
        {
            EditorGUILayout.LabelField($"{data.AllInventoryItems[0].ItemName}: {data.AllInventoryItems[0].ItemAmount}");
        }
        else
        {
            inventoryItemScrollPos = EditorGUILayout.BeginScrollView(inventoryItemScrollPos, GUILayout.Height(Math.Min(200, data.AllInventoryItems.Count * 20)));

            try
            {
                for (int i = 0; i < data.AllInventoryItems.Count; i++)
                {
                    EditorGUILayout.LabelField($"{data.AllInventoryItems[i].ItemName}: {data.AllInventoryItems[i].ItemAmount}");
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

    void DrawEquipment(EquipmentData data)
    {

    }

    int _selectedCurrentOrderIndex = -1;
    Vector2 currentOrderScrollPos;

    int _selectedCompletedOrderIndex = -1;
    Vector2 completedOrderScrollPos;

    void DrawOrderData(OrderData data)
    {
        EditorGUILayout.LabelField("OrderData", EditorStyles.boldLabel);

        currentOrderScrollPos = EditorGUILayout.BeginScrollView(currentOrderScrollPos, GUILayout.Height(Math.Min(100, data.AllCurrentOrders.Count * 20)));
        _selectedCurrentOrderIndex = GUILayout.SelectionGrid(_selectedCurrentOrderIndex, GetOrderNames(data), 1);
        EditorGUILayout.EndScrollView();

        if (_selectedCurrentOrderIndex >= 0 && _selectedCurrentOrderIndex < data.AllCurrentOrders.Count)
        {
            DrawOrder(data.AllCurrentOrders[_selectedCurrentOrderIndex]);
        }

        completedOrderScrollPos = EditorGUILayout.BeginScrollView(completedOrderScrollPos, GUILayout.Height(Math.Min(100, data.AllCompletedOrders.Count * 20)));
        _selectedCompletedOrderIndex = GUILayout.SelectionGrid(_selectedCompletedOrderIndex, GetOrderNames(data), 1);
        EditorGUILayout.EndScrollView();

        if (_selectedCompletedOrderIndex >= 0 && _selectedCompletedOrderIndex < data.AllCompletedOrders.Count)
        {
            DrawOrder(data.AllCompletedOrders[_selectedCompletedOrderIndex]);
        }
    }

    private string[] GetOrderNames(OrderData data)
    {
        return data.AllCurrentOrders.Select(o => o.OrderType.ToString()).ToArray();
    }

    void DrawOrder(Order_Base order)
    {
        EditorGUILayout.LabelField("Order Type", order.OrderType.ToString());
        EditorGUILayout.LabelField("Order ID", order.OrderID.ToString());
        EditorGUILayout.LabelField("Order Status", order.OrderStatus.ToString());
    }
}