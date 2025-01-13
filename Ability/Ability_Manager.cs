using System;
using System.Collections.Generic;
using Actor;
using Inventory;
using Priority;
using UnityEngine;

namespace Ability
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

        public static Ability GetAbility(AbilityName abilityName, uint abilityLevel)
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

        public static uint GetUnusedAbilityID()
        {
            return AllAbilities.GetUnusedAbilityID();
        }

        public static void ClearSOData()
        {
            AllAbilities.ClearSOData();
        }
    }

    [Serializable]
    public class Actor_Abilities : Priority_Updater
    {
        public Actor_Abilities(uint actorID, Dictionary<AbilityName, float> abilityList = null) : base(actorID,
            ComponentType.Actor)
        {
            CurrentAbilities = abilityList ?? new Dictionary<AbilityName, float>();
        }

        public Actor_Abilities(Actor_Abilities actorAbilities) : base(actorAbilities.Reference.ComponentID,
            ComponentType.Actor)
        {
            CurrentAbilities = actorAbilities.CurrentAbilities;
        }

        public             Dictionary<AbilityName, float> CurrentAbilities;
        protected override bool                           _priorityChangeNeeded(object dataChanged) => false;

        protected override Dictionary<PriorityUpdateTrigger, Dictionary<PriorityParameterName, object>>
            _priorityParameterList { get; set; } = new();
    }
}