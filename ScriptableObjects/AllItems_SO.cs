using System;
using System.Collections.Generic;
using System.Linq;
using Lists;
using Managers;
using ScriptableObjects;
using UnityEditor;
using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "AllItems_SO", menuName = "SOList/AllItems_SO")]
    [Serializable]
    public class AllItems_SO : ScriptableObject
    {
        [SerializeField] Item_Master[] _items;
        public           Item_Master[] Items => _items ??= InitialiseAllItems();
        Dictionary<uint, int>          _itemIndexLookup;
        public Dictionary<uint, int>   ItemIndexLookup => _itemIndexLookup ??= _buildIndexLookup();
        int                            _currentIndex;
        
        public Item_Master[] InitialiseAllItems()
        {
            _items = new Item_Master[_defaultItems.Count * 2];
            Array.Copy(_defaultItems.Values.ToArray(), Items, _defaultItems.Count);
            _currentIndex = _defaultItems.Count;
            _buildIndexLookup();
            return Items ?? throw new NullReferenceException("Items is null.");
        }

        Dictionary<uint, int> _buildIndexLookup()
        {
            var newIndexLookup = new Dictionary<uint, int>();
            
            for (var i = 0; i < Items.Length; i++)
            {
                if (Items[i] != null)
                {
                    newIndexLookup[Items[i].ItemID] = i;
                }
            }
            
            return newIndexLookup;
        }

        public Item_Master GetItem_Master(uint itemID)
        {
            if (Items == null || Items.Length is 0) InitialiseAllItems();
                
            if (ItemIndexLookup.TryGetValue(itemID, out var index))
            {
                return Items?[index];
            }

            Debug.LogWarning($"Item {itemID} does not exist in Items.");
            return null;
        }

        static uint _lastUnusedID = 100000;

        public void AddItem(Item_Master item)
        {
            if (ItemIndexLookup.ContainsKey(item.ItemID))
            {
                if (Items[ItemIndexLookup[item.ItemID]].ItemName == item.ItemName)
                {
                    Debug.LogError($"Item {item.ItemID} already exists in Items with the same name. Returning.");
                    return;
                }
                
                Debug.LogWarning($"Item {item.ItemID} already exists in Items with a different name. Continuing with new ID.");
                
                var alreadyUsedID = item.ItemID;

                while (ItemIndexLookup.ContainsKey(_lastUnusedID))
                {
                    _lastUnusedID++;
                }

                item.CommonStats_Item.ItemID = _lastUnusedID;
                Debug.Log(
                    $"ItemName: {item.ItemName} ItemID is now: {item.ItemID} as old ItemID: {alreadyUsedID} " +
                    $"is used by ItemName: {Items[alreadyUsedID].ItemName}");
            }

            if (_currentIndex >= Items.Length)
            {
                _compactAndResizeArray();
            }

            Items[_currentIndex]         = item;
            ItemIndexLookup[item.ItemID] = _currentIndex;
            _currentIndex++;
        }

        public void RemoveItem(uint itemID)
        {
            if (!ItemIndexLookup.TryGetValue(itemID, out var index))
            {
                Debug.LogWarning($"Item {itemID} does not exist in ItemIndex.");
                return;
            }

            Items[index] = null;
            ItemIndexLookup.Remove(itemID);
            
            if (ItemIndexLookup.Count < Items.Length / 4)
            {
                _compactAndResizeArray();
            }
        }
        
        void _compactAndResizeArray()
        {
            var newSize = 0;
            
            for (var i = 0; i < Items.Length; i++)
            {
                if (Items[i] == null) continue;
                
                Items[newSize]                         = Items[i];
                ItemIndexLookup[Items[i].ItemID] = newSize;
                newSize++;
            }

            Array.Resize(ref _items, Math.Max(newSize * 2, Items.Length));
            _currentIndex = newSize;
        }

        public void UpdateItem(Item_Master item)
        {
            if (ItemIndexLookup.TryGetValue(item.ItemID, out var index))
            {
                Items[index] = item;
            }
            else
            {
                AddItem(item);
            }
        }

        public void ClearItemData()
        {
            _items = Array.Empty<Item_Master>();
            ItemIndexLookup.Clear();
            _currentIndex = 0;
        }
        
        static readonly Dictionary<uint, Item_Master> _defaultItems = new()
        {
            
        };
        
        public void AttachWeaponScript(Item_Master item, Equipment_Base equipmentSlot)
    {
        //GameManager.Destroy(equipmentSlot.GetComponent<Weapon>());

        foreach (WeaponType weaponType in item.WeaponStats_Item.WeaponTypeArray)
        {
            switch (weaponType)
            {
                case WeaponType.OneHandedMelee:
                case WeaponType.TwoHandedMelee:
                    foreach (WeaponClass weaponClass in item.WeaponStats_Item.WeaponClassArray)
                    {
                        switch (weaponClass)
                        {
                            case WeaponClass.Axe:
                                //equipmentSlot.AddComponent<Weapon_Axe>();
                                break;
                            case WeaponClass.ShortSword:
                                //equipmentSlot.AddComponent<Weapon_ShortSword>();
                                break;
                                // Add more cases here
                        }
                    }
                    break;
                case WeaponType.OneHandedRanged:
                case WeaponType.TwoHandedRanged:
                    //equipmentSlot.AddComponent<Weapon_Bow>();
                    break;
                case WeaponType.OneHandedMagic:
                case WeaponType.TwoHandedMagic:
                    foreach (WeaponClass weaponClass in item.WeaponStats_Item.WeaponClassArray)
                    {
                        //switch (weaponClass)
                        //{
                        //    case WeaponClass.Staff:
                        //        equipmentSlot.AddComponent<Weapon_Staff>();
                        //        break;
                        //    case WeaponClass.Wand:
                        //        equipmentSlot.AddComponent<Weapon_Wand>();
                        //        break;
                        //         Add more cases here
                        //}
                    }
                    break;
            }
        }
    }
    }

    [CustomEditor(typeof(AllItems_SO))]
    public class AllItems_SOEditor : Editor
    {
        int _selectedItemIndex = -1;

        Vector2 _itemScrollPos;

        bool _show

        void _unselectAll()
        {
            
        }

        public override void OnInspectorGUI()
        {
            var allItemsSO = (AllItems_SO)target;

            if (allItemsSO?.Items is null || allItemsSO.Items.Length is 0)
            {
                EditorGUILayout.LabelField("No Items Found", EditorStyles.boldLabel);
                return;
            }

            if (GUILayout.Button("Clear Item Data"))
            {
                allItemsSO.ClearItemData();
                EditorUtility.SetDirty(allItemsSO);
            }

            if (GUILayout.Button("Unselect All")) _unselectAll();

            EditorGUILayout.LabelField("All Items", EditorStyles.boldLabel);

            var nonNullItems = allItemsSO.Items.Where(item =>
                item            != null &&
                item.ItemID != 0).ToArray();

            _itemScrollPos = EditorGUILayout.BeginScrollView(_itemScrollPos,
                GUILayout.Height(Math.Min(200, nonNullItems.Length * 20)));
            _selectedItemIndex = GUILayout.SelectionGrid(_selectedItemIndex, _getItemNames(nonNullItems), 1);
            EditorGUILayout.EndScrollView();

            if (_selectedItemIndex >= 0 && _selectedItemIndex < nonNullItems.Length)
            {
                _drawItemData(nonNullItems[_selectedItemIndex]);
            }
        }

        string[] _getItemNames(Item_Master[] items) =>
            items.Select(item => item.CommonStats_Item.ItemName.ToString()).ToArray();


        void _drawItemData(Item_Master item)
        {
            EditorGUILayout.LabelField("Item Data", EditorStyles.boldLabel);

            EditorGUILayout.LabelField("Item ID",         $"{item.ItemID}");
            EditorGUILayout.LabelField("Item Name",         $"{item.CommonStats_Item.ItemName}");

            if (item.RequiredIngredients != null)
            {
                EditorGUILayout.LabelField("RequiredIngredients", EditorStyles.boldLabel);

                var requiredIngredients = item.RequiredIngredients;

                _showIngredients = EditorGUILayout.Toggle("Ingredients", _showIngredients);

                if (_showIngredients)
                {
                    _drawIngredients(requiredIngredients);
                }
            }

            if (item.RecipeProducts != null)
            {
                EditorGUILayout.LabelField("Recipe Products", EditorStyles.boldLabel);

                var recipeProducts = item.RecipeProducts;

                _showProducts = EditorGUILayout.Toggle("Products", _showProducts);

                if (_showProducts)
                {
                    _drawProducts(recipeProducts);
                }
            }

            if (item.RequiredVocations != null)
            {
                EditorGUILayout.LabelField("Required Vocations", EditorStyles.boldLabel);

                var requiredVocations = item.RequiredVocations;

                _showVocations = EditorGUILayout.Toggle("Vocations", _showVocations);

                if (_showVocations)
                {
                    _drawVocations(requiredVocations);
                }
            }
        }

        void _drawIngredients(List<Item> requiredIngredients)
        {
            if (requiredIngredients.Count == 1)
            {
                EditorGUILayout.LabelField($"{requiredIngredients[0].ItemName}: {requiredIngredients[0].ItemAmount}");
            }
            else
            {
                _itemScrollPos = EditorGUILayout.BeginScrollView(_itemScrollPos,
                    GUILayout.Height(Math.Min(200, requiredIngredients.Count * 20)));

                try
                {
                    foreach (var item in requiredIngredients)
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

        void _drawProducts(List<Item> products)
        {
            if (products.Count == 1)
            {
                EditorGUILayout.LabelField($"{products[0].ItemName}: {products[0].ItemAmount}");
            }
            else
            {
                _itemScrollPos = EditorGUILayout.BeginScrollView(_itemScrollPos,
                    GUILayout.Height(Math.Min(200, products.Count * 20)));

                try
                {
                    foreach (var item in products)
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

        void _drawVocations(List<VocationRequirement> requiredVocations)
        {
            if (requiredVocations.Count == 1)
            {
                EditorGUILayout.LabelField(
                    $"{requiredVocations[0].VocationName} "                     +
                    $"- Min: {requiredVocations[0].ExpectedVocationExperience}" +
                    $"- Expected: {requiredVocations[0].ExpectedVocationExperience}"
                );
            }
            else
            {
                _itemScrollPos = EditorGUILayout.BeginScrollView(_itemScrollPos,
                    GUILayout.Height(Math.Min(200, requiredVocations.Count * 20)));

                try
                {
                    foreach (var vocation in requiredVocations)
                    {
                        EditorGUILayout.LabelField(
                            $"{vocation.VocationName} "                     +
                            $"- Min: {vocation.ExpectedVocationExperience}" +
                            $"- Expected: {vocation.ExpectedVocationExperience}"
                        );
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
    }
}