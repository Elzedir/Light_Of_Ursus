using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager_Stats : MonoBehaviour
{
    
}

[System.Serializable]
public class CombatStats
{
    #region Basic
    public float CurrentHealth;
    public float MaxHealth;
    public float CurrentMana;
    public float MaxMana;
    public float CurrentStamina;
    public float MaxStamina;
    #endregion

    #region Attack
    public float AttackDamage;
    public float AttackSpeed;
    public float AttackSwingTime;
    public float AttackRange;
    public float AttackPushForce;
    public float AttackCooldown;
    #endregion

    #region Defence
    public float PhysicalDefence;
    public float MagicalDefence;
    #endregion

    #region Movement
    public float MoveSpeed;
    public float DodgeCooldownReduction;
    #endregion

    public CombatStats( bool initialised = true,
        float currentHealth = 0, float maxHealth = 1, 
        float currentMana = 0, float maxMana = 1,
        float currentStamina = 0, float maxStamina = 1,

        float attackDamage = 1, float attackSpeed = 1, float attackSwingTime = 1, float attackRange = 1, float attackPushForce = 1, float attackCooldown = 1,

        float physicalDefence = 0, float magicalDefence = 0,

        float moveSpeed = 1,
        float dodgeCooldown = 1)
    {
        CurrentHealth = currentHealth;
        MaxHealth = maxHealth;
        CurrentMana = currentMana; 
        MaxMana = maxMana;
        CurrentStamina = currentStamina;
        MaxStamina = maxStamina;

        AttackDamage = attackDamage;
        AttackSpeed = attackSpeed;
        AttackSwingTime = attackSwingTime;
        AttackRange = attackRange;
        AttackPushForce = attackPushForce;
        AttackCooldown = attackCooldown;

        PhysicalDefence = physicalDefence;
        MagicalDefence = magicalDefence;

        MoveSpeed = moveSpeed;
        DodgeCooldownReduction = dodgeCooldown;
    }
}

[Serializable]
public class SPECIAL
{
    public int Agility; // Dexterity
    public int Charisma;
    public int Endurance; // Constitution
    public int Intelligence;
    public int Luck;
    public int Perception; // Wisdom
    public int Strength;

    public SPECIAL(int agility = 0, int charisma = 0, int endurance = 0, int intelligence = 0, int luck = 0, int perception = 0, int strength = 0)
    {
        Agility = agility;
        Charisma = charisma;
        Endurance = endurance;
        Intelligence = intelligence;
        Luck = luck;
        Perception = perception;
        Strength = strength;
    }
}
