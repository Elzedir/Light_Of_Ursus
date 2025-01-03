using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Managers;
using Tools;
using UnityEngine;

namespace Ability
{
    [Serializable]
    public class Ability_Data : Data_Class
    {
        public readonly AbilityName                             AbilityName;
        public          string                                  AbilityDescription;
        public          uint                                    MaxLevel;
        public          List<(float, DamageType)>               BaseDamage;
        public          AnimationClip                           AnimationClip;
        public readonly List<(string Name, IEnumerator Action)> AbilityActions;

        public Ability_Data(AbilityName                 abilityName, string        abilityDescription, uint maxLevel,
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

        protected override Data_Display _getDataSO_Object(bool toggleMissingDataDebugs, Data_Display dataSO_Object)
        {
            if (dataSO_Object.Data is null && dataSO_Object.SubData is null)
                dataSO_Object = new Data_Display(
                    title: "Ability Data",
                    dataDisplayType: DataDisplayType.List_CheckBox,
                    existingDataSO_Object: null,
                    data: new Dictionary<string, string>(),
                    firstData: true);

            try
            {
                if (!dataSO_Object.SubData.TryGetValue("Ability Base Stats", out var abilityBaseStats))
                {
                    dataSO_Object.SubData["Ability Base Stats"] = new Data_Display(
                        title: "Ability Base Stats",
                        dataDisplayType: DataDisplayType.List_Item,
                        existingDataSO_Object: dataSO_Object,
                        data: new Dictionary<string, string>());
                }

                if (abilityBaseStats is not null)
                {
                    abilityBaseStats.Data = new Dictionary<string, string>
                    {
                        { "Ability ID", $"{(uint)AbilityName}" },
                        { "Ability Name", $"{AbilityName}" },
                        { "Ability Description", $"{AbilityDescription}" }
                    };
                }
            }
            catch
            {
                Debug.Log("Error in Ability Base Stats");
            }

            try
            {
                if (!dataSO_Object.SubData.TryGetValue("Ability Combat Data", out var abilityCombatData))
                {
                    dataSO_Object.SubData["Ability Combat Data"] = new Data_Display(
                        title: "Ability Combat Data",
                        dataDisplayType: DataDisplayType.List_CheckBox,
                        existingDataSO_Object: dataSO_Object,
                        data: new Dictionary<string, string>());
                }

                if (abilityCombatData is not null)
                {
                    abilityCombatData.Data = new Dictionary<string, string>
                    {
                        { "Ability Actions", $"{AbilityActions.Count}" },
                        { "Ability Max Level", $"{MaxLevel}" },
                        { "Ability Base Damage", $"{BaseDamage.Count}" }
                    };
                }
            }
            catch
            {
                if (toggleMissingDataDebugs)
                {
                    Debug.Log("Error in Ability Combat Data");

                    Debug.LogWarning(AbilityActions);
                    Debug.LogWarning(AbilityActions?.Select(action => action.Name));
                }
            }

            try
            {
                if (!dataSO_Object.SubData.TryGetValue("Ability Animation Data", out var abilityAnimationData))
                {
                    dataSO_Object.SubData["Ability Animation Data"] = new Data_Display(
                        title: "Ability Animation Data",
                        dataDisplayType: DataDisplayType.List_CheckBox,
                        existingDataSO_Object: dataSO_Object,
                        data: new Dictionary<string, string>());
                }

                if (abilityAnimationData is not null)
                {
                    abilityAnimationData.Data = new Dictionary<string, string>
                    {
                        { "Has Ability Animation", $"{AnimationClip is not null}" }
                    };
                }

                
            }
            catch
            {
                Debug.Log("Error in Ability Animation Data");
            }

            return dataSO_Object;
        }
    }

    [Serializable]
    public class Ability
    {
        public readonly AbilityName AbilityName;
        public          uint        CurrentLevel;

        Ability_Data        _abilityData;
        public Ability_Data AbilityData => _abilityData ?? Ability_Manager.GetAbility_Master(AbilityName);

        public Ability(AbilityName abilityName, uint currentLevel)
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