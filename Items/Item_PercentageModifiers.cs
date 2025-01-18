using System;
using System.Collections.Generic;
using Tools;

namespace Items
{
    [Serializable]
    public class Item_PercentageModifiers : Data_Class
    {
        public float CurrentHealth;
        public float CurrentMana;
        public float CurrentStamina;
        public float MaxHealth;
        public float MaxMana;
        public float MaxStamina;
        public float PushRecovery;

        public float AttackDamage;
        public float AttackSpeed;
        public float AttackSwingTime;
        public float AttackRange;
        public float AttackPushForce;
        public float AttackCooldown;

        public float PhysicalDefence;
        public float MagicalDefence;

        public float MoveSpeed;
        public float DodgeCooldownReduction;

        public Item_PercentageModifiers(
            float maxHealth    = 1,
            float maxMana      = 1,
            float maxStamina   = 1,
            float pushRecovery = 1,

            float attackDamage    = 1,
            float attackSpeed     = 1,
            float attackSwingTime = 1,
            float attackRange     = 1,
            float attackPushForce = 1,
            float attackCooldown  = 1,

            float physicalDefence = 1,
            float magicalDefence  = 1,

            float moveSpeed              = 1,
            float dodgeCooldownReduction = 1
        )
        {
            MaxHealth              = maxHealth;
            MaxMana                = maxMana;
            MaxStamina             = maxStamina;
            PushRecovery           = pushRecovery;
            AttackDamage           = attackDamage;
            AttackSpeed            = attackSpeed;
            AttackSwingTime        = attackSwingTime;
            AttackRange            = attackRange;
            AttackPushForce        = attackPushForce;
            AttackCooldown         = attackCooldown;
            PhysicalDefence        = physicalDefence;
            MagicalDefence         = magicalDefence;
            MoveSpeed              = moveSpeed;
            DodgeCooldownReduction = dodgeCooldownReduction;
        }

        public Item_PercentageModifiers(Item_PercentageModifiers other)
        {
            CurrentHealth  = other.CurrentHealth;
            CurrentMana    = other.CurrentMana;
            CurrentStamina = other.CurrentStamina;
            MaxHealth      = other.MaxHealth;
            MaxMana        = other.MaxMana;
            MaxStamina     = other.MaxStamina;
            PushRecovery   = other.PushRecovery;

            AttackDamage    = other.AttackDamage;
            AttackSpeed     = other.AttackSpeed;
            AttackSwingTime = other.AttackSwingTime;
            AttackRange     = other.AttackRange;
            AttackPushForce = other.AttackPushForce;
            AttackCooldown  = other.AttackCooldown;

            PhysicalDefence = other.PhysicalDefence;
            MagicalDefence  = other.MagicalDefence;

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
                
                { "Attack Damage", $"{AttackDamage}" },
                { "Attack Speed", $"{AttackSpeed}" },
                { "Attack Swing Time", $"{AttackSwingTime}" },
                { "Attack Range", $"{AttackRange}" },
                { "Attack Push Force", $"{AttackPushForce}" },
                { "Attack Cooldown", $"{AttackCooldown}" },
                
                { "Physical Defence", $"{PhysicalDefence}" },
                { "Magical Defence", $"{MagicalDefence}" },
                
                { "Move Speed", $"{MoveSpeed}" },
                { "Dodge Cooldown Reduction", $"{DodgeCooldownReduction}" }
            };
        }

        public override DataToDisplay GetDataToDisplay(bool toggleMissingDataDebugs)
        {
            _updateDataDisplay(DataToDisplay,
                title: "Percentage Modifiers",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: GetStringData());

            return DataToDisplay;
        }
    }
}