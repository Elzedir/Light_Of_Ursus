using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Actor;
using Inventory;
using Priority;
using UnityEngine;

namespace Ability
{
    public class Manager_Ability : MonoBehaviour
    {
        
        public static Ability_Master GetAbility_Master(AbilityName abilityName)
        {
            return AllAbilities.GetAbility_Master(abilityName);
        }
    }

    public class Ability
    {
        public AbilityName Name;
        public int         CurrentLevel;
        
        Ability_Master        _abilityMaster;
        public Ability_Master AbilityMaster => _abilityMaster ??;
    }

    public class Ability_Master
    {
        public readonly AbilityName                             AbilityName;
        public          string                                  AbilityDescription;
        public          int                                     MaxLevel;
        public          List<(float, DamageType)>               BaseDamage;
        public          AnimationClip                           AnimationClip;
        public          List<(string Name, IEnumerator Action)> AbilityActions;

        public Ability_Master(AbilityName                 abilityName, string abilityDescription, int maxLevel,
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
        public Actor_Abilities(uint actorID, Dictionary<Ability_Master,float> abilityList = null) : base(actorID, ComponentType.Actor)
        {
            AbilityList = abilityList ?? new Dictionary<Ability_Master, float>();
        }
    
        public Actor_Abilities(Actor_Abilities actorAbilities) : base(actorAbilities.Reference.ComponentID, ComponentType.Actor)
        {
            AbilityList = actorAbilities.AbilityList;
        }
    
        public ComponentReference_Actor ActorReference => Reference as ComponentReference_Actor;

        public             HashSet<AbilityName>                                                         Abilities;
        public             Dictionary<Ability_Master, float>                                                   AbilityCooldowns;
        protected override bool                                                                         _priorityChangeNeeded(object dataChanged) => false;
        protected override Dictionary<PriorityUpdateTrigger, Dictionary<PriorityParameterName, object>> _priorityParameterList                    { get; set; } = new();
    }
    
    public enum AbilityName
    {
        Charge,
        EagleStomp
    }
}