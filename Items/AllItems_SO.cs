using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Lists;
using Managers;
using Priority;
using ScriptableObjects;
using Station;
using UnityEditor;
using UnityEngine;

namespace Items
{
    [CreateAssetMenu(fileName = "AllItems_SO", menuName = "SOList/AllItems_SO")]
    [Serializable]
    public class AllItems_SO : Base_SO<Item_Master>
    {
        public Item_Master[] Items                       => Objects;
        public Item_Master   GetItem_Master(uint itemID) => GetObject_Master(itemID);
        
        public override uint GetObjectID(int id) => Items[id].ItemID;

        public void PopulateDefaultItems()
        {
            if (_defaultItems.Count == 0)
            {
                Debug.Log("No Default Items Found");
            }
        }    
        
        protected override Dictionary<uint, Item_Master> _populateDefaultObjects()
        {
            var defaultItems = new Dictionary<uint, Item_Master>();

            foreach (var item in List_Weapon.GetAllDefaultWeapons())
            {
                defaultItems.Add(item.Key, item.Value);
            }

            foreach (var item in List_Armour.GetAllDefaultArmour())
            {
                defaultItems.Add(item.Key, item.Value);
            }

            foreach (var item in List_Consumable.GetAllDefaultConsumables())
            {
                defaultItems.Add(item.Key, item.Value);
            }

            foreach (var rawMaterial in List_RawMaterial.GetAllDefaultRawMaterials())
            {
                defaultItems.Add(rawMaterial.Key, rawMaterial.Value);
            }

            foreach (var processedMaterial in List_ProcessedMaterial.GetAllDefaultProcessedMaterials())
            {
                defaultItems.Add(processedMaterial.Key, processedMaterial.Value);
            }

            return defaultItems;
        }

        Dictionary<uint, Item_Master> _defaultItems => DefaultObjects;
    }

    [CustomEditor(typeof(AllItems_SO))]
    public class AllItems_SOEditor : Editor
    {
        int _selectedItemIndex = -1;

        Vector2 _itemScrollPos;

        bool _showCommonStats;
        bool _showVisualStats;
        bool _showWeaponStats;
        bool _showArmourStats;
        bool _showFixedModifiers;
        bool _showPercentageModifiers;
        bool _showPriorityStats;

        void _unselectAll()
        {
            _showCommonStats         = false;
            _showVisualStats         = false;
            _showWeaponStats         = false;
            _showArmourStats         = false;
            _showFixedModifiers      = false;
            _showPercentageModifiers = false;
            _showPriorityStats       = false;
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
                allItemsSO.ClearObjectData();
                EditorUtility.SetDirty(allItemsSO);
            }

            if (GUILayout.Button("Unselect All")) _unselectAll();

            EditorGUILayout.LabelField("All Items", EditorStyles.boldLabel);

            var nonNullItems = allItemsSO.Items.Where(item =>
                item        != null &&
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

            EditorGUILayout.LabelField("Item ID",   $"{item.ItemID}");
            EditorGUILayout.LabelField("Item Name", $"{item.CommonStats_Item.ItemName}");

            if (item.CommonStats_Item != null)
            {
                EditorGUILayout.LabelField("CommonStats", EditorStyles.boldLabel);

                var commonStats = item.CommonStats_Item;

                _showCommonStats = EditorGUILayout.Toggle("CommonStats", _showCommonStats);

                if (_showCommonStats)
                {
                    _drawCommonStats(commonStats);
                }
            }

            if (item.VisualStats_Item != null)
            {
                EditorGUILayout.LabelField("VisualStats", EditorStyles.boldLabel);

                var visualStats = item.VisualStats_Item;

                _showVisualStats = EditorGUILayout.Toggle("VisualStats", _showVisualStats);

                if (_showVisualStats)
                {
                    _drawVisualStats(visualStats);
                }
            }

            if (item.WeaponStats_Item != null)
            {
                EditorGUILayout.LabelField("WeaponStats", EditorStyles.boldLabel);

                var weaponStats = item.WeaponStats_Item;

                _showWeaponStats = EditorGUILayout.Toggle("WeaponStats", _showWeaponStats);

                if (_showWeaponStats)
                {
                    _drawWeaponStats(weaponStats);
                }
            }

            if (item.ArmourStats_Item != null)
            {
                EditorGUILayout.LabelField("ArmourStats", EditorStyles.boldLabel);

                var armourStats = item.ArmourStats_Item;

                _showArmourStats = EditorGUILayout.Toggle("ArmourStats", _showArmourStats);

                if (_showArmourStats)
                {
                    _drawArmourStats(armourStats);
                }
            }

            if (item.FixedModifiers_Item != null)
            {
                EditorGUILayout.LabelField("FixedModifiers", EditorStyles.boldLabel);

                var fixedModifiers = item.FixedModifiers_Item;

                _showFixedModifiers = EditorGUILayout.Toggle("FixedModifiers", _showFixedModifiers);

                if (_showFixedModifiers)
                {
                    _drawFixedModifiers(fixedModifiers);
                }
            }

            if (item.PercentageModifiers_Item != null)
            {
                EditorGUILayout.LabelField("PercentageModifiers", EditorStyles.boldLabel);

                var percentageModifiers = item.PercentageModifiers_Item;

                _showPercentageModifiers = EditorGUILayout.Toggle("PercentageModifiers", _showPercentageModifiers);

                if (_showPercentageModifiers)
                {
                    _drawPercentageModifiers(percentageModifiers);
                }
            }

            if (item.PriorityStats_Item != null)
            {
                EditorGUILayout.LabelField("PriorityStats", EditorStyles.boldLabel);

                var priorityStats = item.PriorityStats_Item;

                _showPriorityStats = EditorGUILayout.Toggle("PriorityStats", _showPriorityStats);

                if (_showPriorityStats)
                {
                    _drawPriorityStats(priorityStats);
                }
            }
        }

        void _drawCommonStats(CommonStats_Item commonStats)
        {
            EditorGUILayout.LabelField("Item ID", $"{commonStats.ItemID}");
            EditorGUILayout.LabelField("Item Name", commonStats.ItemName);
            EditorGUILayout.LabelField("Item Type", commonStats.ItemType.ToString());
            EditorGUILayout.LabelField("Equipment Slots", string.Join(", ", commonStats.EquipmentSlots.ToString()));
            EditorGUILayout.LabelField("Max Stack Size", commonStats.MaxStackSize.ToString());
            EditorGUILayout.LabelField("Item Level", commonStats.ItemLevel.ToString());
            EditorGUILayout.LabelField("Item Quality", commonStats.ItemQuality.ToString());
            EditorGUILayout.LabelField("Item Value", commonStats.ItemValue.ToString());
            EditorGUILayout.LabelField("Item Weight", commonStats.ItemWeight.ToString(CultureInfo.InvariantCulture));
            EditorGUILayout.LabelField("Item Equippable", commonStats.ItemEquippable.ToString());
        }

        void _drawVisualStats(VisualStats_Item visualStats)
        {
            EditorGUILayout.LabelField("Item Mesh",     visualStats.ItemMesh.ToString());
            EditorGUILayout.LabelField("Item Material", visualStats.ItemMaterial.ToString());
            EditorGUILayout.LabelField("Item Position", visualStats.ItemPosition.ToString());
            EditorGUILayout.LabelField("Item Rotation", visualStats.ItemRotation.ToString());
            EditorGUILayout.LabelField("Item Scale",    visualStats.ItemScale.ToString());
        }

        void _drawWeaponStats(WeaponStats_Item weaponStats)
        {
            EditorGUILayout.LabelField("Weapon Type",  string.Join(", ", weaponStats.WeaponTypeArray.ToString()));
            EditorGUILayout.LabelField("Weapon Class", string.Join(", ", weaponStats.WeaponClassArray.ToString()));
            EditorGUILayout.LabelField("Max Charge Time",
                weaponStats.MaxChargeTime.ToString(CultureInfo.InvariantCulture));
        }

        void _drawArmourStats(ArmourStats_Item armourStats)
        {
            EditorGUILayout.LabelField("Equipment Slots", armourStats.EquipmentSlot.ToString());
            EditorGUILayout.LabelField("Armour Coverage",
                armourStats.ItemCoverage.ToString(CultureInfo.InvariantCulture));
        }

        void _drawFixedModifiers(FixedModifiers_Item fixedModifiers)
        {
            EditorGUILayout.LabelField("CurrentHealth",
                fixedModifiers.CurrentHealth.ToString(CultureInfo.InvariantCulture));
            EditorGUILayout.LabelField("CurrentMana",
                fixedModifiers.CurrentMana.ToString(CultureInfo.InvariantCulture));
            EditorGUILayout.LabelField("CurrentStamina",
                fixedModifiers.CurrentStamina.ToString(CultureInfo.InvariantCulture));
            EditorGUILayout.LabelField("MaxHealth",  fixedModifiers.MaxHealth.ToString(CultureInfo.InvariantCulture));
            EditorGUILayout.LabelField("MaxMana",    fixedModifiers.MaxMana.ToString(CultureInfo.InvariantCulture));
            EditorGUILayout.LabelField("MaxStamina", fixedModifiers.MaxStamina.ToString(CultureInfo.InvariantCulture));
            EditorGUILayout.LabelField("PushRecovery",
                fixedModifiers.PushRecovery.ToString(CultureInfo.InvariantCulture));

            EditorGUILayout.LabelField("HealthRecovery",
                fixedModifiers.HealthRecovery.ToString(CultureInfo.InvariantCulture));

            EditorGUILayout.LabelField("AttackDamage", string.Join(", ", fixedModifiers.AttackDamage.ToString()));
            EditorGUILayout.LabelField("AttackSpeed",
                fixedModifiers.AttackSpeed.ToString(CultureInfo.InvariantCulture));
            EditorGUILayout.LabelField("AttackSwingTime",
                fixedModifiers.AttackSwingTime.ToString(CultureInfo.InvariantCulture));
            EditorGUILayout.LabelField("AttackRange",
                fixedModifiers.AttackRange.ToString(CultureInfo.InvariantCulture));
            EditorGUILayout.LabelField("AttackPushForce",
                fixedModifiers.AttackPushForce.ToString(CultureInfo.InvariantCulture));
            EditorGUILayout.LabelField("AttackCooldown",
                fixedModifiers.AttackCooldown.ToString(CultureInfo.InvariantCulture));

            EditorGUILayout.LabelField("PhysicalArmour",
                fixedModifiers.PhysicalArmour.ToString(CultureInfo.InvariantCulture));
            EditorGUILayout.LabelField("MagicArmour",
                fixedModifiers.MagicArmour.ToString(CultureInfo.InvariantCulture));

            EditorGUILayout.LabelField("MoveSpeed", fixedModifiers.MoveSpeed.ToString(CultureInfo.InvariantCulture));
            EditorGUILayout.LabelField("DodgeCooldownReduction",
                fixedModifiers.DodgeCooldownReduction.ToString(CultureInfo.InvariantCulture));
        }

        void _drawPercentageModifiers(PercentageModifiers_Item percentageModifiers)
        {
            EditorGUILayout.LabelField("CurrentHealth",
                percentageModifiers.CurrentHealth.ToString(CultureInfo.InvariantCulture));
            EditorGUILayout.LabelField("CurrentMana",
                percentageModifiers.CurrentMana.ToString(CultureInfo.InvariantCulture));
            EditorGUILayout.LabelField("CurrentStamina",
                percentageModifiers.CurrentStamina.ToString(CultureInfo.InvariantCulture));
            EditorGUILayout.LabelField("MaxHealth",
                percentageModifiers.MaxHealth.ToString(CultureInfo.InvariantCulture));
            EditorGUILayout.LabelField("MaxMana", percentageModifiers.MaxMana.ToString(CultureInfo.InvariantCulture));
            EditorGUILayout.LabelField("MaxStamina",
                percentageModifiers.MaxStamina.ToString(CultureInfo.InvariantCulture));
            EditorGUILayout.LabelField("PushRecovery",
                percentageModifiers.PushRecovery.ToString(CultureInfo.InvariantCulture));

            EditorGUILayout.LabelField("AttackDamage",
                string.Join(", ", percentageModifiers.AttackDamage.ToString(CultureInfo.InvariantCulture)));
            EditorGUILayout.LabelField("AttackSpeed",
                percentageModifiers.AttackSpeed.ToString(CultureInfo.InvariantCulture));
            EditorGUILayout.LabelField("AttackSwingTime",
                percentageModifiers.AttackSwingTime.ToString(CultureInfo.InvariantCulture));
            EditorGUILayout.LabelField("AttackRange",
                percentageModifiers.AttackRange.ToString(CultureInfo.InvariantCulture));
            EditorGUILayout.LabelField("AttackPushForce",
                percentageModifiers.AttackPushForce.ToString(CultureInfo.InvariantCulture));
            EditorGUILayout.LabelField("AttackCooldown",
                percentageModifiers.AttackCooldown.ToString(CultureInfo.InvariantCulture));

            EditorGUILayout.LabelField("PhysicalDefence",
                percentageModifiers.PhysicalDefence.ToString(CultureInfo.InvariantCulture));
            EditorGUILayout.LabelField("MagicDefence",
                percentageModifiers.MagicalDefence.ToString(CultureInfo.InvariantCulture));

            EditorGUILayout.LabelField("MoveSpeed",
                percentageModifiers.MoveSpeed.ToString(CultureInfo.InvariantCulture));
            EditorGUILayout.LabelField("DodgeCooldownReduction",
                percentageModifiers.DodgeCooldownReduction.ToString(CultureInfo.InvariantCulture));
        }

        bool _showCriticalPriority;
        bool _showHighPriority;
        bool _showMediumPriority;
        bool _showLowPriority;

        Vector2 _criticalPriorityScrollPos;
        Vector2 _highPriorityScrollPos;
        Vector2 _mediumPriorityScrollPos;
        Vector2 _lowPriorityScrollPos;

        void _drawPriorityStats(PriorityStats_Item priorityStats)
        {
            foreach (var priority in priorityStats.Priority_Stations)
            {
                switch (priority.Key)
                {
                    case PriorityImportance.Critical:
                        _drawPriority(priority, ref _showCriticalPriority, ref _criticalPriorityScrollPos);
                        break;
                    case PriorityImportance.High:
                        _drawPriority(priority, ref _showHighPriority, ref _highPriorityScrollPos);
                        break;
                    case PriorityImportance.Medium:
                        _drawPriority(priority, ref _showMediumPriority, ref _mediumPriorityScrollPos);
                        break;
                    case PriorityImportance.Low:
                        _drawPriority(priority, ref _showLowPriority, ref _lowPriorityScrollPos);
                        break;
                }
            }
        }

        void _drawPriority(KeyValuePair<PriorityImportance, List<StationName>> priority, ref bool showPriority,
                           ref Vector2                                         scrollPos)
        {
            if (priority.Value.Count <= 0) return;

            showPriority = EditorGUILayout.Toggle(priority.Key.ToString(), showPriority);

            if (showPriority)
            {
                _drawPriorityStations(priority.Value, ref scrollPos);
            }
        }

        void _drawPriorityStations(List<StationName> priorityStations, ref Vector2 scrollPos)
        {
            if (priorityStations.Count == 1)
            {
                EditorGUILayout.LabelField(priorityStations[0].ToString());
            }
            else
            {
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos,
                    GUILayout.Height(Math.Min(200, priorityStations.Count * 20)));

                try
                {
                    foreach (var stationName in priorityStations)
                    {
                        EditorGUILayout.LabelField(stationName.ToString());
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