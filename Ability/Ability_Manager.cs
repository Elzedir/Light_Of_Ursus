using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Actor;
using Initialisation;
using Inventory;
using Priority;
using UnityEditor;
using UnityEngine;

namespace Ability
{
    public class Ability_Manager : MonoBehaviour
    {
        const string _Ability_SOPath = "ScriptableObjects/Ability_SO";
        
        static Ability_SO _allAbilities;
        static Ability_SO AllAbilities => _allAbilities ??= _getOrCreateAbility_SO();

        // public static void OnSceneLoaded()
        // {
        //     Manager_Initialisation.OnInitialiseManagerAbility += _initialise;
        // }
        //
        // static void _initialise()
        // {
        //     AllAbilities.PopulateSceneAbilities();
        // }
        
        public static Ability_Master GetAbility_Master(AbilityName abilityName)
        {
            return AllAbilities.GetAbility_Master(abilityName);
        }
        
        public static Ability GetAbility(AbilityName abilityName, uint abilityLevel)
        {
            return AllAbilities.GetAbility(abilityName, abilityLevel);
        }
        
        static Ability_SO _getOrCreateAbility_SO()
        {
            var ability_SO = Resources.Load<Ability_SO>(_Ability_SOPath);
            
            if (ability_SO is not null) return ability_SO;
            
            ability_SO = ScriptableObject.CreateInstance<Ability_SO>();
            AssetDatabase.CreateAsset(ability_SO, $"Assets/Resources/{_Ability_SOPath}");
            AssetDatabase.SaveAssets();
            
            return ability_SO;
        }

        public static uint GetUnusedAbilityID()
        {
            return AllAbilities.GetUnusedAbilityID();
        }
    }

    public class Ability
    {
        public readonly AbilityName AbilityName;
        public uint         CurrentLevel;
        
        Ability_Master        _abilityMaster;
        public Ability_Master AbilityMaster => _abilityMaster ?? Ability_Manager.GetAbility_Master(AbilityName);
        
        public Ability(AbilityName abilityName, uint currentLevel)
        {
            AbilityName   = abilityName;
            CurrentLevel = currentLevel;
        }
    }

    public class Ability_Master
    {
        public readonly AbilityName                             AbilityName;
        public          string                                  AbilityDescription;
        public          uint                                     MaxLevel;
        public          List<(float, DamageType)>               BaseDamage;
        public          AnimationClip                           AnimationClip;
        public          List<(string Name, IEnumerator Action)> AbilityActions;

        public Ability_Master(AbilityName                 abilityName, string abilityDescription, uint maxLevel,
                              List<(float, DamageType)>   baseDamage, AnimationClip animationClip,
                              List<(string, IEnumerator)> abilityActions)
        {
            AbilityName           = abilityName;
            AbilityDescription    = abilityDescription;
            MaxLevel       = maxLevel;
            BaseDamage     = baseDamage;
            AnimationClip  = animationClip;
            AbilityActions = abilityActions;
        }

        public IEnumerator GetAction(string actionName)
        {
            if (!AbilityActions.Any(a => a.Name == actionName)) throw new ArgumentException($"AbilityActions does not contain ActionName: {actionName}");

            return AbilityActions.FirstOrDefault(a => a.Name == actionName).Action;
        }

        public void DealDamage()
        {
            // character.ReceiveDamage (new Damage(BaseDamage));
        }
    }

    [Serializable]
    public class Actor_Abilities : PriorityData
    {
        public Actor_Abilities(uint actorID, Dictionary<AbilityName, float> abilityList = null) : base(actorID, ComponentType.Actor)
        {
            CurrentAbilities = abilityList ?? new Dictionary<AbilityName, float>();
        }
    
        public Actor_Abilities(Actor_Abilities actorAbilities) : base(actorAbilities.Reference.ComponentID, ComponentType.Actor)
        {
            CurrentAbilities = actorAbilities.CurrentAbilities;
        }
    
        public ComponentReference_Actor ActorReference => Reference as ComponentReference_Actor;

        public Dictionary<AbilityName, float> CurrentAbilities;
        protected override bool                                                                         _priorityChangeNeeded(object dataChanged) => false;
        protected override Dictionary<PriorityUpdateTrigger, Dictionary<PriorityParameterName, object>> _priorityParameterList                    { get; set; } = new();
    }
    
    public enum AbilityName
    {
        Charge,
        Eagle_Stomp
    }
}