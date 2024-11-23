using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Items;
using Jobs;
using Lists;
using Managers;
using ScriptableObjects;
using UnityEditor;
using UnityEngine;

namespace Jobs
{
    [CreateAssetMenu(fileName = "AllJobs_SO", menuName = "SOList/AllJobs_SO")]
    [Serializable]
    public class AllJobs_SO : Base_SO<Job_Master>
    {
        public Job_Master[] Jobs                           => Objects;
        public Job_Master   GetJob_Master(JobName jobName) => GetObject_Master((uint)jobName);

        public override uint GetObjectID(int id) => (uint)Jobs[id].JobName;

        public override Dictionary<uint, Job_Master> PopulateDefaultObjects()
        {
            var defaultJobs = new Dictionary<uint, Job_Master>();

            foreach (var item in List_Job.GetAllDefaultJobs())
            {
                defaultJobs.Add(item.Key, item.Value);
            }

            return defaultJobs;
        }
        
        public Dictionary<uint, Job_Master> DefaultJobs => DefaultObjects;
    }

    [CustomEditor(typeof(AllJobs_SO))]
    public class AllJobs_SOEditor : Editor
    {
        int _selectedJobIndex = -1;

        Vector2 _JobScrollPos;

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
            var allJobsSO = (AllJobs_SO)target;

            if (allJobsSO?.Jobs is null || allJobsSO.Jobs.Length is 0)
            {
                EditorGUILayout.LabelField("No Jobs Found", EditorStyles.boldLabel);
                return;
            }

            if (GUILayout.Button("Clear Job Data"))
            {
                allJobsSO.ClearJobData();
                EditorUtility.SetDirty(allJobsSO);
            }

            if (GUILayout.Button("Unselect All")) _unselectAll();

            EditorGUILayout.LabelField("All Jobs", EditorStyles.boldLabel);

            var nonNullJobs = allJobsSO.Jobs.Where(Job =>
                Job        != null &&
                Job.JobID != 0).ToArray();

            _JobScrollPos = EditorGUILayout.BeginScrollView(_JobScrollPos,
                GUILayout.Height(Math.Min(200, nonNullJobs.Length * 20)));
            _selectedJobIndex = GUILayout.SelectionGrid(_selectedJobIndex, _getJobNames(nonNullJobs), 1);
            EditorGUILayout.EndScrollView();

            if (_selectedJobIndex >= 0 && _selectedJobIndex < nonNullJobs.Length)
            {
                _drawJobData(nonNullJobs[_selectedJobIndex]);
            }
        }

        string[] _getJobNames(Job_Master[] Jobs) =>
            Jobs.Select(Job => Job.CommonStats_Job.JobName.ToString()).ToArray();


        void _drawJobData(Job_Master Job)
        {
            EditorGUILayout.LabelField("Job Data", EditorStyles.boldLabel);

            EditorGUILayout.LabelField("Job ID",   $"{Job.JobID}");
            EditorGUILayout.LabelField("Job Name", $"{Job.CommonStats_Job.JobName}");

            if (Job.CommonStats_Job != null)
            {
                EditorGUILayout.LabelField("CommonStats", EditorStyles.boldLabel);

                var commonStats = Job.CommonStats_Job;

                _showCommonStats = EditorGUILayout.Toggle("CommonStats", _showCommonStats);

                if (_showCommonStats)
                {
                    _drawCommonStats(commonStats);
                }
            }

            if (Job.VisualStats_Job != null)
            {
                EditorGUILayout.LabelField("VisualStats", EditorStyles.boldLabel);

                var visualStats = Job.VisualStats_Job;

                _showVisualStats = EditorGUILayout.Toggle("VisualStats", _showVisualStats);

                if (_showVisualStats)
                {
                    _drawVisualStats(visualStats);
                }
            }

            if (Job.WeaponStats_Job != null)
            {
                EditorGUILayout.LabelField("WeaponStats", EditorStyles.boldLabel);

                var weaponStats = Job.WeaponStats_Job;

                _showWeaponStats = EditorGUILayout.Toggle("WeaponStats", _showWeaponStats);

                if (_showWeaponStats)
                {
                    _drawWeaponStats(weaponStats);
                }
            }

            if (Job.ArmourStats_Job != null)
            {
                EditorGUILayout.LabelField("ArmourStats", EditorStyles.boldLabel);

                var armourStats = Job.ArmourStats_Job;

                _showArmourStats = EditorGUILayout.Toggle("ArmourStats", _showArmourStats);

                if (_showArmourStats)
                {
                    _drawArmourStats(armourStats);
                }
            }

            if (Job.FixedModifiers_Job != null)
            {
                EditorGUILayout.LabelField("FixedModifiers", EditorStyles.boldLabel);

                var fixedModifiers = Job.FixedModifiers_Job;

                _showFixedModifiers = EditorGUILayout.Toggle("FixedModifiers", _showFixedModifiers);

                if (_showFixedModifiers)
                {
                    _drawFixedModifiers(fixedModifiers);
                }
            }

            if (Job.PercentageModifiers_Job != null)
            {
                EditorGUILayout.LabelField("PercentageModifiers", EditorStyles.boldLabel);

                var percentageModifiers = Job.PercentageModifiers_Job;

                _showPercentageModifiers = EditorGUILayout.Toggle("PercentageModifiers", _showPercentageModifiers);

                if (_showPercentageModifiers)
                {
                    _drawPercentageModifiers(percentageModifiers);
                }
            }

            if (Job.PriorityStats_Job != null)
            {
                EditorGUILayout.LabelField("PriorityStats", EditorStyles.boldLabel);

                var priorityStats = Job.PriorityStats_Job;

                _showPriorityStats = EditorGUILayout.Toggle("PriorityStats", _showPriorityStats);

                if (_showPriorityStats)
                {
                    _drawPriorityStats(priorityStats);
                }
            }
        }

        void _drawCommonStats(CommonStats_Job commonStats)
        {
            EditorGUILayout.LabelField("Job ID", commonStats.JobID.ToString());
            EditorGUILayout.LabelField("Job Name", commonStats.JobName);
            EditorGUILayout.LabelField("Job Type", commonStats.JobType.ToString());
            EditorGUILayout.LabelField("Equipment Slots", string.Join(", ", commonStats.EquipmentSlots.ToString()));
            EditorGUILayout.LabelField("Max Stack Size", commonStats.MaxStackSize.ToString());
            EditorGUILayout.LabelField("Job Level", commonStats.JobLevel.ToString());
            EditorGUILayout.LabelField("Job Quality", commonStats.JobQuality.ToString());
            EditorGUILayout.LabelField("Job Value", commonStats.JobValue.ToString());
            EditorGUILayout.LabelField("Job Weight", commonStats.JobWeight.ToString(CultureInfo.InvariantCulture));
            EditorGUILayout.LabelField("Job Equippable", commonStats.JobEquippable.ToString());
        }

        void _drawVisualStats(VisualStats_Job visualStats)
        {
            EditorGUILayout.LabelField("Job Mesh",     visualStats.JobMesh.ToString());
            EditorGUILayout.LabelField("Job Material", visualStats.JobMaterial.ToString());
            EditorGUILayout.LabelField("Job Position", visualStats.JobPosition.ToString());
            EditorGUILayout.LabelField("Job Rotation", visualStats.JobRotation.ToString());
            EditorGUILayout.LabelField("Job Scale",    visualStats.JobScale.ToString());
        }

        void _drawWeaponStats(WeaponStats_Job weaponStats)
        {
            EditorGUILayout.LabelField("Weapon Type",  string.Join(", ", weaponStats.WeaponTypeArray.ToString()));
            EditorGUILayout.LabelField("Weapon Class", string.Join(", ", weaponStats.WeaponClassArray.ToString()));
            EditorGUILayout.LabelField("Max Charge Time",
                weaponStats.MaxChargeTime.ToString(CultureInfo.InvariantCulture));
        }

        void _drawArmourStats(ArmourStats_Job armourStats)
        {
            EditorGUILayout.LabelField("Equipment Slots", armourStats.EquipmentSlot.ToString());
            EditorGUILayout.LabelField("Armour Coverage",
                armourStats.JobCoverage.ToString(CultureInfo.InvariantCulture));
        }

        void _drawFixedModifiers(FixedModifiers_Job fixedModifiers)
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

        void _drawPercentageModifiers(PercentageModifiers_Job percentageModifiers)
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

        void _drawPriorityStats(PriorityStats_Job priorityStats)
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