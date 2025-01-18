using System;
using System.Collections.Generic;
using Managers;
using Tools;

namespace Items
{
    [Serializable]
    public class Item_FixedModifiers : Data_Class
    {
        public float CurrentHealth;
        public float CurrentMana;
        public float CurrentStamina;
        public float MaxHealth;
        public float MaxMana;
        public float MaxStamina;
        public float PushRecovery;

        public float HealthRecovery;

        public List<Damage_Data> AttackDamage;
        public float                     AttackSpeed;
        public float                     AttackSwingTime;
        public float                     AttackRange;
        public float                     AttackPushForce;
        public float                     AttackCooldown;

        public float PhysicalArmour;
        public float MagicArmour;

        public float MoveSpeed;
        public float DodgeCooldownReduction;

        // public float DamagePerAbilityLevel;
        // public float DamagePerItemLevel;
        // public SPECIAL SPECIAL;

        public Item_FixedModifiers(
            float maxHealth    = 0,
            float maxMana      = 0,
            float maxStamina   = 0,
            float pushRecovery = 0,

            float healthRecovery = 0,

            List<Damage_Data> attackDamage    = null,
            float                     attackSpeed     = 0,
            float                     attackSwingTime = 0,
            float                     attackRange     = 0,
            float                     attackPushForce = 0,
            float                     attackCooldown  = 0,

            float physicalArmour = 0,
            float magicArmour    = 0,

            float moveSpeed              = 0,
            float dodgeCooldownReduction = 0
        )
        {
            MaxHealth    = maxHealth;
            MaxMana      = maxMana;
            MaxStamina   = maxStamina;
            PushRecovery = pushRecovery;

            HealthRecovery = healthRecovery;

            AttackDamage    = attackDamage;
            AttackSpeed     = attackSpeed;
            AttackSwingTime = attackSwingTime;
            AttackRange     = attackRange;
            AttackPushForce = attackPushForce;
            AttackCooldown  = attackCooldown;

            PhysicalArmour         = physicalArmour;
            MagicArmour            = magicArmour;
            MoveSpeed              = moveSpeed;
            DodgeCooldownReduction = dodgeCooldownReduction;
        }

        public Item_FixedModifiers(Item_FixedModifiers other)
        {
            CurrentHealth  = other.CurrentHealth;
            CurrentMana    = other.CurrentMana;
            CurrentStamina = other.CurrentStamina;
            
            MaxHealth      = other.MaxHealth;
            MaxMana        = other.MaxMana;
            MaxStamina     = other.MaxStamina;
            PushRecovery   = other.PushRecovery;

            HealthRecovery = other.HealthRecovery;

            AttackDamage    = other.AttackDamage != null ? new List<Damage_Data>(other.AttackDamage) : null;
            AttackSpeed     = other.AttackSpeed;
            AttackSwingTime = other.AttackSwingTime;
            AttackRange     = other.AttackRange;
            AttackPushForce = other.AttackPushForce;
            AttackCooldown  = other.AttackCooldown;

            PhysicalArmour         = other.PhysicalArmour;
            MagicArmour            = other.MagicArmour;
            MoveSpeed              = other.MoveSpeed;
            DodgeCooldownReduction = other.DodgeCooldownReduction;
        }

        public override Dictionary<string, string> GetStringData()
        {
            return new Dictionary<string, string>
            {
                { "Current Health", $"{CurrentHealth}" },
                { "Current Mana", $"{CurrentMana}" },
                { "Current Stamina", $"{CurrentStamina}" },
                { "Max Health", $"{MaxHealth}" },
                { "Max Mana", $"{MaxMana}" },
                { "Max Stamina", $"{MaxStamina}" },
                { "Push Recovery", $"{PushRecovery}" },
                { "Health Recovery", $"{HealthRecovery}" },
                
                { "Attack Damage", $"{AttackDamage}" },
                { "Attack Speed", $"{AttackSpeed}" },
                { "Attack Swing Time", $"{AttackSwingTime}" },
                { "Attack Range", $"{AttackRange}" },
                { "Attack Push Force", $"{AttackPushForce}" },
                { "Attack Cooldown", $"{AttackCooldown}" },
                
                { "Physical Armour", $"{PhysicalArmour}" },
                { "Magic Armour", $"{MagicArmour}" },
                
                { "Move Speed", $"{MoveSpeed}" },
                { "Dodge Cooldown Reduction", $"{DodgeCooldownReduction}" }
            };
        }

        public override DataToDisplay GetDataToDisplay(bool toggleMissingDataDebugs)
        {
            _updateDataDisplay(DataToDisplay,
                title: "Fixed Modifiers",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: GetStringData());

            return DataToDisplay;
        }
    }
}