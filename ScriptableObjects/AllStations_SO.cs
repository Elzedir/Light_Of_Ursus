using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "AllStations_SO", menuName = "SOList/AllStations_SO")]
[Serializable]
public class AllStations_SO : ScriptableObject
{
    public List<StationData> AllStationData;

    public void SetAllStationData(List<StationData> allStationData)
    {
        AllStationData = allStationData;
    }

    public void LoadData(SaveData saveData)
    {
        AllStationData = saveData.SavedStationData.AllStationData;
    }

    public void ClearStationData()
    {
        AllStationData.Clear();
    }
}

[CustomEditor(typeof(AllStations_SO))]
public class AllStationsSOEditor : Editor
{
    int _selectedStationIndex = -1;
    bool _showOperatingAreas = false;
    bool _showInventory = false;
    bool _showOrders = false;

    Vector2 _stationScrollPos;
    Vector2 _operatingAreaScrollPos;

    public override void OnInspectorGUI()
    {
        AllStations_SO allStationsSO = (AllStations_SO)target;

        if (GUILayout.Button("Clear Station Data"))
        {
            allStationsSO.ClearStationData();
            EditorUtility.SetDirty(allStationsSO);
        }

        EditorGUILayout.LabelField("All Stations", EditorStyles.boldLabel);
        _stationScrollPos = EditorGUILayout.BeginScrollView(_stationScrollPos, GUILayout.Height(GetListHeight(allStationsSO.AllStationData.Count)));
        _selectedStationIndex = GUILayout.SelectionGrid(_selectedStationIndex, GetStationNames(allStationsSO), 1);
        EditorGUILayout.EndScrollView();

        if (_selectedStationIndex >= 0 && _selectedStationIndex < allStationsSO.AllStationData.Count)
        {
            var selectedStationData = allStationsSO.AllStationData[_selectedStationIndex];
            DrawStationAdditionalData(selectedStationData);
        }
    }

    private string[] GetStationNames(AllStations_SO allStationsSO)
    {
        return allStationsSO.AllStationData.Select(s => s.StationID.ToString()).ToArray();
    }

    private float GetListHeight(int itemCount)
    {
        return Mathf.Min(200, itemCount * 20);
    }

    private void DrawStationAdditionalData(StationData selectedStationData)
    {
        EditorGUILayout.LabelField("Station Data", EditorStyles.boldLabel);
        //EditorGUILayout.LabelField("Station Name", selectedStationData.StationName.ToString());
        EditorGUILayout.LabelField("Station ID", selectedStationData.StationID.ToString());
        EditorGUILayout.LabelField("Jobsite ID", selectedStationData.JobsiteID.ToString());

        if (selectedStationData.AllOperatingAreaIDs != null)
        {
            _showOperatingAreas = EditorGUILayout.Toggle("Operating Areas", _showOperatingAreas);

            if (_showOperatingAreas)
            {
                DrawOperatingAreaAdditionalData(selectedStationData.AllOperatingAreaIDs);
            }

            _showInventory = EditorGUILayout.Toggle("Inventory", _showInventory);

            if (_showInventory)
            {
                DrawInventoryAdditionalData(selectedStationData.InventoryData);
            }

            // _showOrders = EditorGUILayout.Toggle("Orders", _showOrders);

            // if (_showOrders)
            // {
            //     DrawOrderAdditionalData(selectedStationData.Orders);
            // }
        }
    }

    private void DrawOperatingAreaAdditionalData(List<uint> allOperatingAreaData)
    {
        _operatingAreaScrollPos = EditorGUILayout.BeginScrollView(_operatingAreaScrollPos, GUILayout.Height(GetListHeight(allOperatingAreaData.Count)));

        try
        {
            foreach (var operatingAreaID in allOperatingAreaData)
            {
                EditorGUILayout.LabelField("Operating Area Data", EditorStyles.boldLabel);
                //EditorGUILayout.LabelField("Operating Area Name", operatingArea.OperatingAreaName.ToString());
                EditorGUILayout.LabelField("Operating Area ID", operatingAreaID.ToString());
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error: {e.Message}");
        }
        finally
        {
            EditorGUILayout.EndScrollView();
        }
    }

    private void DrawInventoryAdditionalData(InventoryData inventoryData)
    {
        EditorGUILayout.LabelField("Inventory Data", EditorStyles.boldLabel);

        foreach (var inventoryItem in inventoryData.AllInventoryItems)
        {
            EditorGUILayout.LabelField("Item ID", inventoryItem.ItemID.ToString());
            EditorGUILayout.LabelField("Item Name", inventoryItem.ItemName);
            EditorGUILayout.LabelField("Item Quantity", inventoryItem.ItemAmount.ToString());
        }
    }

    private void DrawOrderAdditionalData(Dictionary<(int ActorID, int OrderID), Order_Base> allOrderData)
    {
        EditorGUILayout.LabelField("Order Data", EditorStyles.boldLabel);

        foreach (var orderTuple in allOrderData)
        {
            EditorGUILayout.LabelField("Order ID", orderTuple.Key.ToString());
            var orderData = orderTuple.Value;

            if (orderData is Order_Base orderHaul)
            {
                EditorGUILayout.LabelField("Order Type", Enum.GetName(typeof(OrderType), orderHaul.OrderType));
                EditorGUILayout.LabelField("Actor ID", orderHaul.ActorID.ToString());
                EditorGUILayout.LabelField("Source Station ID", orderHaul.StationID_Source.ToString());
                EditorGUILayout.LabelField("Destination Station ID", orderHaul.StationID_Destination.ToString());
                EditorGUILayout.LabelField("Order Status", orderHaul.OrderStatus.ToString());
                EditorGUILayout.LabelField("Order Items", orderHaul.OrderItems.ToString());
            }
        }
    }
}
