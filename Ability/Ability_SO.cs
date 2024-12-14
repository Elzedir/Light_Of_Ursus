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
    public class Ability_SO : Data_SO<Ability_Master>
    {
        public Data_Object<Ability_Master>[] Abilities => DataObjects;

        public Data_Object<Ability_Master> GetAbility_Master(AbilityName abilityName) =>
            GetDataObject_Master((uint)abilityName);

        public Ability GetAbility(AbilityName abilityName, uint currentLevel)
        {
            return new Ability(abilityName, currentLevel);
        }

        public override uint GetDataObjectID(int id) => (uint)Abilities[id].DataObject.AbilityName;

        public void UpdateAbility(uint abilityID, Ability_Master ability_Master) =>
            UpdateDataObject(abilityID, ability_Master);

        public void UpdateAllAbilities(Dictionary<uint, Ability_Master> allAbilities) =>
            UpdateAllDataObjects(allAbilities);

        public void PopulateSceneAbilities()
        {
            if (_defaultAbilities.Count == 0)
            {
                Debug.Log("No Default Items Found");
            }
        }

        protected override Dictionary<uint, Data_Object<Ability_Master>> _populateDefaultDataObjects()
        {
            var defaultAbilities = new Dictionary<uint, Ability_Master>();

            foreach (var defaultAbility in Ability_List.GetAllDefaultAbilities())
            {
                defaultAbilities.Add((uint)defaultAbility.Key, defaultAbility.Value);
            }

            return _convertDictionaryToDataObject(defaultAbilities);
        }

        protected override Data_Object<Ability_Master> _convertToDataObject(Ability_Master data)
        {
            return new Data_Object<Ability_Master>(
                dataObjectID: (uint)data.AbilityName,
                dataObject: data, 
                dataObjectTitle: $"{(uint)data.AbilityName}{data.AbilityName}",
                dataSO_Object: data.DataSO_Object);
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

        Dictionary<uint, Data_Object<Ability_Master>> _defaultAbilities => DefaultDataObjects;
    }

    [CustomEditor(typeof(Ability_SO))]
    public class Ability_SOEditor : Data_SOEditor<Ability_Master>
    {
        public override Data_SO<Ability_Master> SO => _so ??= (Ability_SO)target;
    }
}