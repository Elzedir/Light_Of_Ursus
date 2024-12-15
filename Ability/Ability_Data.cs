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
            AbilityActions    = abilityActions;
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

        protected override Data_Display _getDataSO_Object(bool toggleMissingDataDebugs)
        {
            var dataObjects = new List<Data_Display>();

            try
            {
                dataObjects.Add(new Data_Display(
                    title: "Ability Base Stats",
                    dataDisplayType: DataDisplayType.Item,
                    data: new List<string>
                    {
                        $"Ability ID: {(uint)AbilityName}",
                        $"Ability Name: {AbilityName}",
                        $"Ability Description: {AbilityDescription}"
                    }));
            }
            catch
            {
                Debug.Log("Error in Ability Base Stats");
            }

            try
            {
                dataObjects.Add(new Data_Display(
                    title: "Ability Combat Data",
                    dataDisplayType: DataDisplayType.Item,
                    data: new List<string>
                    {
                        $"Ability Actions: {AbilityActions.Count}",
                        $"Ability Max Level: {MaxLevel}",
                        $"Ability Base Damage: {BaseDamage.Count}"
                    }));
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
                dataObjects.Add(new Data_Display(
                    title: "Ability Animation Data",
                    dataDisplayType: DataDisplayType.Item,
                    data: new List<string>
                    {
                        $"Has Ability Animation: {AnimationClip != null}"
                    }));
            }
            catch
            {
                Debug.Log("Error in Ability Animation Data");
            }

            return new Data_Display(
                title: $"{(uint)AbilityName}: {AbilityName}",
                dataDisplayType: DataDisplayType.CheckBoxList,
                subData: new List<Data_Display>(dataObjects));
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