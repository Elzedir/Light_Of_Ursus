using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Managers
{
    public class Manager_Stats : MonoBehaviour
    {
    
    }

    [Serializable]
    public class CombatStats
    {
        #region Basic
        public                                      float CurrentHealth  = 1;
        [FormerlySerializedAs("MaxHealth")] public  float BaseMaxHealth  = 1;
        public                                      float CurrentMana    = 1;
        [FormerlySerializedAs("MaxMana")] public    float BaseMaxMana    = 1;
        public                                      float CurrentStamina = 1;
        [FormerlySerializedAs("MaxStamina")] public float BaseMaxStamina = 1;
        #endregion

        #region Attack
        public float BaseAttackDamage    = 1;
        public float BaseAttackSpeed     = 1;
        public float BaseAttackSwingTime = 1;
        public float BaseAttackRange     = 1;
        [FormerlySerializedAs("AttackPushForce")] public float BaseAttackPushForce = 1;
        [FormerlySerializedAs("AttackCooldown")]                                          public float BaseAttackCooldown  = 1;
        #endregion

        #region Defence
        [FormerlySerializedAs("PhysicalDefence")] public float BasePhysicalDefence = 0;
        [FormerlySerializedAs("MagicalDefence")]                                          public float BaseMagicalDefence  = 0;
        #endregion

        #region Movement
        [FormerlySerializedAs("MoveSpeed")] public float BaseMoveSpeed              = 1;
        [FormerlySerializedAs("DodgeCooldownReduction")]                                    public float BaseDodgeCooldownReduction = 0;
        #endregion

        public CombatStats(
            float currentHealth = -1, float baseMaxHealth = 1, 
            float currentMana = -1, float baseMaxMana = 1,
            float currentStamina = -1, float baseMaxStamina = 1,

            float baseAttackDamage = 1, float baseAttackSpeed = 1, float baseAttackSwingTime = 1, 
            float baseAttackRange = 1, float baseAttackPushForce = 1, float baseAttackCooldown = 1,

            float basePhysicalDefence = 0, float baseMagicalDefence = 0,

            float baseMoveSpeed = 1,
            float baseDodgeCooldownReduction = 0)
        {
            CurrentHealth = currentHealth > 0 ? currentHealth : baseMaxHealth;
            BaseMaxHealth = baseMaxHealth;
            CurrentMana = currentMana > 0 ? currentMana : baseMaxMana;
            BaseMaxMana = baseMaxMana;
            CurrentStamina = currentStamina > 0 ? currentStamina : baseMaxStamina;
            BaseMaxStamina = baseMaxStamina;

            BaseAttackDamage = baseAttackDamage;
            BaseAttackSpeed = baseAttackSpeed;
            BaseAttackSwingTime = baseAttackSwingTime;
            BaseAttackRange = baseAttackRange;
            BaseAttackPushForce = baseAttackPushForce;
            BaseAttackCooldown = baseAttackCooldown;

            BasePhysicalDefence = basePhysicalDefence;
            BaseMagicalDefence = baseMagicalDefence;

            BaseMoveSpeed = baseMoveSpeed;
            BaseDodgeCooldownReduction = baseDodgeCooldownReduction;
        }
    
        public CombatStats(CombatStats combatStats)
        {
            CurrentHealth = combatStats.CurrentHealth;
            BaseMaxHealth = combatStats.BaseMaxHealth;
            CurrentMana = combatStats.CurrentMana;
            BaseMaxMana = combatStats.BaseMaxMana;
            CurrentStamina = combatStats.CurrentStamina;
            BaseMaxStamina = combatStats.BaseMaxStamina;

            BaseAttackDamage = combatStats.BaseAttackDamage;
            BaseAttackSpeed = combatStats.BaseAttackSpeed;
            BaseAttackSwingTime = combatStats.BaseAttackSwingTime;
            BaseAttackRange = combatStats.BaseAttackRange;
            BaseAttackPushForce = combatStats.BaseAttackPushForce;
            BaseAttackCooldown = combatStats.BaseAttackCooldown;

            BasePhysicalDefence = combatStats.BasePhysicalDefence;
            BaseMagicalDefence = combatStats.BaseMagicalDefence;

            BaseMoveSpeed = combatStats.BaseMoveSpeed;
            BaseDodgeCooldownReduction = combatStats.BaseDodgeCooldownReduction;
        }

        public Dictionary<string, string> GetStringData()
        {
            return new Dictionary<string, string>
            {
                { "Current Health", $"{CurrentHealth}" },
                { "Base Max Health", $"{BaseMaxHealth}" },
                { "Current Mana", $"{CurrentMana}" },
                { "Base Max Mana", $"{BaseMaxMana}" },
                { "Current Stamina", $"{CurrentStamina}" },
                { "Base Max Stamina", $"{BaseMaxStamina}" },
            
                { "Base Attack Damage", $"{BaseAttackDamage}" },
                { "Base Attack Speed", $"{BaseAttackSpeed}" },
                { "Base Attack Swing Time", $"{BaseAttackSwingTime}" },
                { "Base Attack Range", $"{BaseAttackRange}" },
                { "Base Attack Push Force", $"{BaseAttackPushForce}" },
                { "Base Attack Cooldown", $"{BaseAttackCooldown}" },
            
                { "Base Physical Defence", $"{BasePhysicalDefence}" },
                { "Base Magical Defence", $"{BaseMagicalDefence}" },
            
                { "Base Move Speed", $"{BaseMoveSpeed}" },
                { "Base Dodge Cooldown Reduction", $"{BaseDodgeCooldownReduction}" }
            };
        }
    }

    [Serializable]
    public class Special
    {
        public int Agility; // Dexterity
        public int Charisma;
        public int Endurance; // Constitution
        public int Intelligence;
        public int Luck;
        public int Perception; // Wisdom
        public int Strength;

        public Special(int agility    = 0, int charisma = 0, int endurance = 0, int intelligence = 0, int luck = 0,
            int perception = 0, int strength = 0)
        {
            Agility      = agility;
            Charisma     = charisma;
            Endurance    = endurance;
            Intelligence = intelligence;
            Luck         = luck;
            Perception   = perception;
            Strength     = strength;
        }

        public Special(Special special)
        {
            Agility      = special.Agility;
            Charisma     = special.Charisma;
            Endurance    = special.Endurance;
            Intelligence = special.Intelligence;
            Luck         = special.Luck;
            Perception   = special.Perception;
            Strength     = special.Strength;
        }

        public Dictionary<string, string> GetStringData()
        {
            return new Dictionary<string, string>
            {
                { "Agility", $"{Agility}" },
                { "Charisma", $"{Charisma}" },
                { "Endurance", $"{Endurance}" },
                { "Intelligence", $"{Intelligence}" },
                { "Luck", $"{Luck}" },
                { "Perception", $"{Perception}" },
                { "Strength", $"{Strength}" }
            };
        }
    }
}