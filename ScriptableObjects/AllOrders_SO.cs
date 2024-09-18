using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "AllOrders_SO", menuName = "SOList/AllOrders_SO")]
[Serializable]
public class AllOrders_SO : ScriptableObject
{
    public List<OrderData> AllOrderData;

    public void SetAllOperatingAreaData(List<OrderData> allOrderData)
    {
        AllOrderData = allOrderData;
    }

    public void LoadData(SaveData saveData)
    {
        AllOrderData = saveData.SavedOrderData.AllOrderData;
    }

    public void ClearOrderData()
    {
        AllOrderData.Clear();
    }
}

[CustomEditor(typeof(AllOrders_SO))]
public class AllOrdersSOEditor : Editor
{
    int _selectedOrderIndex = -1;

    Vector2 _orderScrollPos;

    public override void OnInspectorGUI()
    {
        AllOrders_SO allOrdersSO = (AllOrders_SO)target;

        if (GUILayout.Button("Clear All Orders"))
        {
            allOrdersSO.ClearOrderData();
            EditorUtility.SetDirty(allOrdersSO);
        }

        EditorGUILayout.LabelField("All Orders", EditorStyles.boldLabel);
        _orderScrollPos = EditorGUILayout.BeginScrollView(_orderScrollPos, GUILayout.Height(GetListHeight(allOrdersSO.AllOrderData.Count)));
        _selectedOrderIndex = GUILayout.SelectionGrid(_selectedOrderIndex, GetOrderTypes(allOrdersSO), 1);
        EditorGUILayout.EndScrollView();

        if (_selectedOrderIndex >= 0 && _selectedOrderIndex < allOrdersSO.AllOrderData.Count)
        {
            var selectedOrder = allOrdersSO.AllOrderData[_selectedOrderIndex];
            DrawOrderAdditionalData(selectedOrder);
        }
    }

    private string[] GetOrderTypes(AllOrders_SO allOrdersSO)
    {
        return allOrdersSO.AllOrderData.Select(o => o.AllOrderIDs.ToString()).ToArray();
    }

    private float GetListHeight(int itemCount)
    {
        return Mathf.Min(200, itemCount * 20);
    }

    private void DrawOrderAdditionalData(OrderData selectedOrderData)
    {
        EditorGUILayout.LabelField("Order Data", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("All Order IDs");

        Vector2 scrollPos = Vector2.zero;

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(100));

        try
        {
            foreach (var orderID in selectedOrderData.AllOrderIDs)
            {
                EditorGUILayout.LabelField($"- {orderID}");
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
}
