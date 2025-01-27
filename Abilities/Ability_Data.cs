using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Managers;
using Tools;
using UnityEngine;

namespace Abilities
{
    [Serializable]
    public class Ability_Data : Data_Class
    {
        public readonly AbilityName                             AbilityName;
        public          string                                  AbilityDescription;
        public          ulong                                    MaxLevel;
        public          List<(float, DamageType)>               BaseDamage;
        public          AnimationClip                           AnimationClip;
        public readonly List<(string Name, IEnumerator Action)> AbilityActions;

        public Ability_Data(AbilityName                 abilityName, string        abilityDescription, ulong maxLevel,
                            List<(float, DamageType)>   baseDamage,  AnimationClip animationClip,
                            List<(string, IEnumerator)> abilityActions)
        {
            AbilityName        = abilityName;
            AbilityDescription = abilityDescription;
            MaxLevel           = maxLevel;
            BaseDamage         = baseDamage;
            AnimationClip      = animationClip;
            AbilityActions     = abilityActions;
        }

        public IEnumerator GetAction(string actionName)
        {
            if (AbilityActions.All(a => a.Name != actionName))
                throw new ArgumentException($"AbilityActions does not contain ActionName: {actionName}");

            return AbilityActions.FirstOrDefault(a => a.Name == actionName).Action;
        }

        public void DealDamage()
        {
            // character.ReceiveDamage (new Damage(BaseDamage));
        }

        public override DataToDisplay GetDataToDisplay(bool toggleMissingDataDebugs)
        {
            _updateDataDisplay(DataToDisplay,
                title: "Ability Base Stats",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: GetStringData());

            return DataToDisplay;
        }

        public override Dictionary<string, string> GetStringData()
        {
            return new Dictionary<string, string>
            {
                { "Ability ID", $"{(ulong)AbilityName}" },
                { "Ability Name", $"{AbilityName}" },
                { "Ability Description", $"{AbilityDescription}" },
                { "Ability Actions", $"{AbilityActions?.Count}" },
                { "Ability Max Level", $"{MaxLevel}" },
                { "Ability Base Damage", $"{BaseDamage?.Count}" },
                { "Has Ability Animation", $"{AnimationClip is not null}" }
            };
        }
    }

    [Serializable]
    public class Ability
    {
        public readonly AbilityName AbilityName;
        public          ulong        CurrentLevel;

        Ability_Data        _abilityData;
        public Ability_Data AbilityData => _abilityData ?? Ability_Manager.GetAbility_Master(AbilityName);

        public Ability(AbilityName abilityName, ulong currentLevel)
        {
            AbilityName  = abilityName;
            CurrentLevel = currentLevel;
        }
    }

    public enum AbilityName
    {
        None,

        Charge,
        Eagle_Stomp
    }
}