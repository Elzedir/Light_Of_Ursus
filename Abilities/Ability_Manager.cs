using System;
using System.Collections.Generic;
using System.Linq;
using ActorActions;
using Inventory;
using Priorities;
using Priority;
using Tools;
using UnityEngine;

namespace Abilities
{
    public abstract class Ability_Manager
    {
        const string _ability_SOPath = "ScriptableObjects/Ability_SO";

        static Ability_SO _allAbilities;
        static Ability_SO AllAbilities => _allAbilities ??= _getAbility_SO();

        public static Ability_Data GetAbility_Master(AbilityName abilityName)
        {
            return AllAbilities.GetAbility_Master(abilityName).Data_Object;
        }

        public static Abilities.Ability GetAbility(AbilityName abilityName, ulong abilityLevel)
        {
            return AllAbilities.GetAbility(abilityName, abilityLevel);
        }

        static Ability_SO _getAbility_SO()
        {
            var ability_SO = Resources.Load<Ability_SO>(_ability_SOPath);

            if (ability_SO is not null) return ability_SO;

            Debug.LogError("Ability_SO not found. Creating temporary Ability_SO.");
            ability_SO = ScriptableObject.CreateInstance<Ability_SO>();

            return ability_SO;
        }

        public static void ClearSOData()
        {
            AllAbilities.ClearSOData();
        }
    }

    [Serializable]
    public class Actor_Abilities : Priority_Class
    {
        public Actor_Abilities(ulong actorID, SerializableDictionary<AbilityName, float> abilityList = null) : base(actorID,
            ComponentType.Actor)
        {
            _currentAbilities = abilityList ?? new SerializableDictionary<AbilityName, float>();
        }

        public Actor_Abilities(Actor_Abilities actorAbilities) : base(actorAbilities.Reference.ComponentID,
            ComponentType.Actor)
        {
            _currentAbilities = actorAbilities.CurrentAbilities;
        }
        
        public override List<ActorActionName> GetAllowedActions()
        {
            //* Have some abilities allow certain actions to be done, like people who can fly able to fly home from work.
            return new List<ActorActionName>();
        }

        public override DataToDisplay GetDataToDisplay(bool toggleMissingDataDebugs)
        {
            _updateDataDisplay(DataToDisplay,
                title: "Actor Abilities",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: GetStringData());

            return DataToDisplay;
        }

        public override Dictionary<string, string> GetStringData()
        {
            return CurrentAbilities.ToDictionary(
                ability => $"{ability.Key}", 
                ability => $"{ability.Value}");
        }

        SerializableDictionary<AbilityName, float> _currentAbilities;
        public SerializableDictionary<AbilityName, float> CurrentAbilities => _currentAbilities; //??= InitialiseAbilities();
    }
}