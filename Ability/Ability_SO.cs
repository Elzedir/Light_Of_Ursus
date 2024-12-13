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
    public class Ability_SO : Base_SO_Test<Ability_Master>
    {
        public Base_Object<Ability_Master>[] Abilities => BaseObjects;

        public Base_Object<Ability_Master> GetAbility_Master(AbilityName abilityName) =>
            GetBaseObject_Master((uint)abilityName);

        public Ability GetAbility(AbilityName abilityName, uint currentLevel)
        {
            return new Ability(abilityName, currentLevel);
        }

        public override uint GetBaseObjectID(int id) => (uint)Abilities[id].DataObject.AbilityName;

        public void UpdateAbility(uint abilityID, Ability_Master ability_Master) =>
            UpdateBaseObject(abilityID, ability_Master);

        public void UpdateAllAbilities(Dictionary<uint, Ability_Master> allAbilities) =>
            UpdateAllBaseObjects(allAbilities);

        public void PopulateSceneAbilities()
        {
            if (_defaultAbilities.Count == 0)
            {
                Debug.Log("No Default Items Found");
            }
        }

        protected override Dictionary<uint, Base_Object<Ability_Master>> _populateDefaultBaseObjects()
        {
            var defaultAbilities = new Dictionary<uint, Ability_Master>();

            foreach (var defaultAbility in Ability_List.GetAllDefaultAbilities())
            {
                defaultAbilities.Add((uint)defaultAbility.Key, defaultAbility.Value);
            }

            return _convertDictionaryToBaseObject(defaultAbilities);
        }

        protected override Dictionary<uint, Base_Object<Ability_Master>> _convertDictionaryToBaseObject(
            Dictionary<uint, Ability_Master> ability_Masters)
        {
            return ability_Masters.ToDictionary(ability_Master => ability_Master.Key,
                ability_Master => new Base_Object<Ability_Master>(ability_Master.Key,
                    GetDataToDisplay(ability_Master.Value), ability_Master.Value,
                    $"{ability_Master.Key}: {ability_Master.Value.AbilityName}"));
        }

        protected override Base_Object<Ability_Master> _convertToBaseObject(Ability_Master ability_Master)
        {
            return new Base_Object<Ability_Master>((uint)ability_Master.AbilityName, GetDataToDisplay(ability_Master),
                ability_Master,
                $"{(uint)ability_Master.AbilityName}{ability_Master.AbilityName}");
        }

        static uint _lastUnusedAbilityID = 1;

        public uint GetUnusedAbilityID()
        {
            while (BaseObjectIndexLookup.ContainsKey(_lastUnusedAbilityID))
            {
                _lastUnusedAbilityID++;
            }

            return _lastUnusedAbilityID;
        }

        Dictionary<uint, Base_Object<Ability_Master>> _defaultAbilities => DefaultBaseObjects;

        enum AbilityDataCategories
        {
            BaseAbilityData,
            AbilityCombatData,
            AbilityAnimationData
        }

        public override Dictionary<uint, DataToDisplay> GetDataToDisplay(Ability_Master actor_Data)
        {
            try
            {
                return new Dictionary<uint, DataToDisplay>
                {
                    {
                        (uint)AbilityDataCategories.BaseAbilityData, new DataToDisplay(
                            data: new List<string>
                            {
                                $"Ability ID: {(uint)actor_Data.AbilityName}",
                                $"Ability Name: {actor_Data.AbilityName}",
                                $"Ability Description: {actor_Data.AbilityDescription}",
                            },
                            dataDisplayType: DataDisplayType.Item)
                    },
                    {


                        (uint)AbilityDataCategories.AbilityCombatData, new DataToDisplay(
                            data: new List<string>(actor_Data.AbilityActions?.Select(action => $"ActionName: {action.Name}") ?? Array.Empty<string>())
                            {
                                $"Ability Max Level: {actor_Data.MaxLevel}",
                                $"Ability Base Damage: {actor_Data.BaseDamage}"
                            },
                            dataDisplayType: DataDisplayType.Item)
                    },
                    {

                        (uint)AbilityDataCategories.AbilityAnimationData, new DataToDisplay(
                            data: new List<string>
                            {
                                $"Has Ability Animation: {actor_Data.AnimationClip != null}",
                            },
                            dataDisplayType: DataDisplayType.Item)
                    }
                };
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

                Debug.LogWarning(actor_Data);
                Debug.LogWarning(actor_Data.AbilityActions);
                Debug.LogWarning(actor_Data.AbilityActions?.Select(action => action.Name));
                
                throw;
            }
        }
    }

    [CustomEditor(typeof(Ability_SO))]
    public class Ability_SOEditor : Base_SOEditor<Ability_Master>
    {
        public override Base_SO_Test<Ability_Master> SO => _so ??= (Ability_SO)target;
    }
}