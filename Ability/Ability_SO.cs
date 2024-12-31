using System;
using System.Collections.Generic;
using System.Linq;
using Tools;
using UnityEditor;
using UnityEngine;

namespace Ability
{
    [CreateAssetMenu(fileName = "Ability_SO", menuName = "SOList/Ability_SO")]
    [Serializable]
    public class Ability_SO : Data_SO<Ability_Data>
    {
        public Object_Data<Ability_Data>[] Abilities => Objects_Data;

        public Object_Data<Ability_Data> GetAbility_Master(AbilityName abilityName) =>
            GetObject_Data((uint)abilityName);

        public Ability GetAbility(AbilityName abilityName, uint currentLevel)
        {
            return new Ability(abilityName, currentLevel);
        }

        public override uint GetDataObjectID(int id) => (uint)Abilities[id].DataObject.AbilityName;

        public void UpdateAbility(uint abilityID, Ability_Data ability_Data) =>
            UpdateDataObject(abilityID, ability_Data);

        public void UpdateAllAbilities(Dictionary<uint, Ability_Data> allAbilities) =>
            UpdateAllDataObjects(allAbilities);

        public override void PopulateSceneData()
        {
            if (_defaultAbilities.Count == 0)
            {
                Debug.Log("No Default Items Found");
            }
        }

        protected override Dictionary<uint, Object_Data<Ability_Data>> _populateDefaultDataObjects()
        {
            var defaultAbilities = new Dictionary<uint, Ability_Data>();

            foreach (var defaultAbility in Ability_List.GetAllDefaultAbilities())
            {
                defaultAbilities.Add((uint)defaultAbility.Key, defaultAbility.Value);
            }

            return _convertDictionaryToDataObject(defaultAbilities);
        }

        protected override Object_Data<Ability_Data> _convertToDataObject(Ability_Data data)
        {
            return new Object_Data<Ability_Data>(
                dataObjectID: (uint)data.AbilityName,
                dataObject: data, 
                dataObjectTitle: $"{(uint)data.AbilityName}: {data.AbilityName}",
                data_Display: data.DataSO_Object(ToggleMissingDataDebugs));
        }

        static uint _lastUnusedAbilityID = 1;

        public uint GetUnusedAbilityID()
        {
            while (DataObjectIndexLookup.ContainsKey(_lastUnusedAbilityID))
            {
                _lastUnusedAbilityID++;
            }

            return _lastUnusedAbilityID;
        }

        Dictionary<uint, Object_Data<Ability_Data>> _defaultAbilities => DefaultDataObjects;
    }

    [CustomEditor(typeof(Ability_SO))]
    public class Ability_SOEditor : Data_SOEditor<Ability_Data>
    {
        public override Data_SO<Ability_Data> SO => _so ??= (Ability_SO)target;
    }
}