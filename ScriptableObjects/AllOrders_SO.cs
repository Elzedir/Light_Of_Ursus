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
    int _selectedOrderDataIndex = -1;

    Vector2 _orderDataScrollPos;

    public override void OnInspectorGUI()
    {
        AllOrders_SO allOrdersSO = (AllOrders_SO)target;

        if (GUILayout.Button("Clear All Orders"))
        {
            allOrdersSO.ClearOrderData();
            EditorUtility.SetDirty(allOrdersSO);
        }

        EditorGUILayout.LabelField("All OrderDatas", EditorStyles.boldLabel);
        _orderDataScrollPos = EditorGUILayout.BeginScrollView(_orderDataScrollPos, GUILayout.Height(GetListHeight(allOrdersSO.AllOrderData.Count)));
        _selectedOrderDataIndex = GUILayout.SelectionGrid(_selectedOrderDataIndex, GetOrderTypes(allOrdersSO), 1);
        EditorGUILayout.EndScrollView();

        if (_selectedOrderDataIndex >= 0 && _selectedOrderDataIndex < allOrdersSO.AllOrderData.Count)
        {
            var selectedOrderData = allOrdersSO.AllOrderData[_selectedOrderDataIndex];
            DrawOrderAdditionalData(selectedOrderData);
        }
    }

    private string[] GetOrderTypes(AllOrders_SO allOrdersSO)
    {
        return allOrdersSO.AllOrderData.Select(o => o.AllCurrentOrders.ToString()).ToArray();
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
            foreach (var orderID in selectedOrderData.AllCurrentOrders)
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
