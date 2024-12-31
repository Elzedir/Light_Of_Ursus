using System;
using System.Collections.Generic;
using System.Linq;
using DataPersistence;
using Managers;
using UnityEditor;
using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "AllOrders_SO", menuName = "SOList/AllOrders_SO")]
    [Serializable]
    public class AllOrders_SO : ScriptableObject
    {
        public OrderData[] AllOrderData;

        public void SetAllOperatingAreaData(OrderData[] allOrderData)
        {
            AllOrderData = allOrderData;
        }

        public void LoadData(SaveData saveData)
        {
            AllOrderData = saveData.SavedOrderData.AllOrderData;
        }
    }

    [CustomEditor(typeof(AllOrders_SO))]
    public class AllOrdersSOEditor : Editor
    {
        int _selectedOrderDataIndex = -1;

        Vector2 _orderDataScrollPos;

        public override void OnInspectorGUI()
        {
            var allOrdersSO = (AllOrders_SO)target;

            EditorGUILayout.LabelField("All OrderDatas", EditorStyles.boldLabel);
            _orderDataScrollPos = EditorGUILayout.BeginScrollView(_orderDataScrollPos,
                GUILayout.Height(GetListHeight(allOrdersSO.AllOrderData.Length)));
            _selectedOrderDataIndex = GUILayout.SelectionGrid(_selectedOrderDataIndex, GetOrderTypes(allOrdersSO), 1);
            EditorGUILayout.EndScrollView();

            if (_selectedOrderDataIndex >= 0 && _selectedOrderDataIndex < allOrdersSO.AllOrderData.Length)
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
}