using System;
using System.Collections.Generic;
using System.Linq;
using ScriptableObjects;
using Tools;
using UnityEditor;
using UnityEngine;

namespace Ability
{
    [CreateAssetMenu(fileName = "Ability_SO", menuName = "SOList/Ability_SO")]
    [Serializable]
    public class Ability_SO : Base_SO<Ability_Master>
    {
        public Ability_Master[] Abilities                         => Objects;
        public Ability_Master        GetAbility_Master(AbilityName      abilityName) => GetObject_Master((uint)abilityName);

        public Ability GetAbility(AbilityName abilityName, uint currentLevel)
        {
            return new Ability(abilityName, currentLevel);
        }

        public override uint GetObjectID(int id) => (uint)Abilities[id].AbilityName;

        public void UpdateAbility(uint abilityID, Ability_Master ability_Master) => UpdateObject(abilityID, ability_Master);
        public void UpdateAllAbilities(Dictionary<uint, Ability_Master> allAbilities) => UpdateAllObjects(allAbilities);

        public void PopulateSceneAbilities()
        {
            if (_defaultAbilities.Count == 0)
            {
                Debug.Log("No Default Items Found");
            }
        }

        protected override Dictionary<uint, Ability_Master> _populateDefaultObjects()
        {
            var defaultAbilities = new Dictionary<uint, Ability_Master>();

            foreach (var defaultAbility in Ability_List.GetAllDefaultAbilities())
            {
                defaultAbilities.Add((uint)defaultAbility.Key, defaultAbility.Value);
            }

            return defaultAbilities;
        }

        static uint _lastUnusedAbilityID = 1;

        public uint GetUnusedAbilityID()
        {
            while (ObjectIndexLookup.ContainsKey(_lastUnusedAbilityID))
            {
                _lastUnusedAbilityID++;
            }

            return _lastUnusedAbilityID;
        }

        Dictionary<uint, Ability_Master> _defaultAbilities => DefaultObjects;
    }

    [CustomEditor(typeof(Ability_SO))]
    public class Ability_SOEditor : Editor
    {
        int  _selectedAbilityIndex = -1;
        bool _showJobSites;
        bool _showPopulation;
        bool _showProsperity;

        Vector2 _abilityScrollPos;
        Vector2 _jobSiteScrollPos;
        Vector2 _populationScrollPos;

        public override void OnInspectorGUI()
        {
            var ability_SO = (Ability_SO)target;
            
            if (ability_SO?.Abilities is null || ability_SO.Abilities.Length is 0)
            {
                EditorGUILayout.LabelField("No Abilities Found", EditorStyles.boldLabel);
                return;
            }

            EditorGUILayout.LabelField("All Abilities", EditorStyles.boldLabel);
            
            var nonNullAbilities = ability_SO.Abilities.Where(ability =>
                ability        != null &&
                ability.AbilityName != 0).ToArray();
            
            _abilityScrollPos = EditorGUILayout.BeginScrollView(_abilityScrollPos,
                GUILayout.Height(Mathf.Min(200, nonNullAbilities.Length * 20)));
            _selectedAbilityIndex = GUILayout.SelectionGrid(_selectedAbilityIndex, _getAbilityNames(nonNullAbilities), 1);
            EditorGUILayout.EndScrollView();

            if (_selectedAbilityIndex < 0 || _selectedAbilityIndex >= nonNullAbilities.Length) return;

            var selectedAbilityMaster = nonNullAbilities[_selectedAbilityIndex];
            _drawAbilityAdditionalMaster(selectedAbilityMaster);
        }

        static string[] _getAbilityNames(Ability_Master[] abilities)
        {
            return abilities.Select(a => $"{a.AbilityName}").ToArray();
        }

        static void _drawAbilityAdditionalMaster(Ability_Master selectedAbility)
        {
            EditorGUILayout.LabelField("Ability Master", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Ability Name", $"{selectedAbility.AbilityName}");
        }
    }
}