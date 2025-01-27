using System;
using System.Collections.Generic;
using Tools;
using UnityEditor;
using UnityEngine;

namespace Abilities
{
    [CreateAssetMenu(fileName = "Ability_SO", menuName = "SOList/Ability_SO")]
    [Serializable]
    public class Ability_SO : Data_SO<Ability_Data>
    {
        public Data<Ability_Data>[] Abilities => Data;

        public Data<Ability_Data> GetAbility_Master(AbilityName abilityName) =>
            GetData((ulong)abilityName);

        public Abilities.Ability GetAbility(AbilityName abilityName, ulong currentLevel)
        {
            return new Abilities.Ability(abilityName, currentLevel);
        }

        public void UpdateAbility(ulong abilityID, Ability_Data ability_Data) =>
            UpdateData(abilityID, ability_Data);

        public void UpdateAllAbilities(Dictionary<ulong, Ability_Data> allAbilities) =>
            UpdateAllData(allAbilities);

        protected override Dictionary<ulong, Data<Ability_Data>> _getDefaultData() => 
            _convertDictionaryToData(Ability_List.DefaultAbilities);

        protected override Data<Ability_Data> _convertToData(Ability_Data data)
        {
            return new Data<Ability_Data>(
                dataID: (ulong)data.AbilityName,
                data_Object: data, 
                dataTitle: $"{(ulong)data.AbilityName}: {data.AbilityName}",
                getDataToDisplay: data.GetDataToDisplay);
        }
    }

    [CustomEditor(typeof(Ability_SO))]
    public class Ability_SOEditor : Data_SOEditor<Ability_Data>
    {
        public override Data_SO<Ability_Data> SO => _so ??= (Ability_SO)target;
    }
}