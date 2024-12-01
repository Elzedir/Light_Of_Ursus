using System;
using UnityEngine;

public class Manager_Stats : MonoBehaviour
{
    
}

[Serializable]
public class CombatStats
{
    #region Basic
    public float CurrentHealth = 1;
    public float MaxHealth = 1;
    public float CurrentMana = 1;
    public float MaxMana = 1;
    public float CurrentStamina = 1;
    public float MaxStamina = 1;
    #endregion

    #region Attack
    public float AttackDamage = 1;
    public float AttackSpeed = 1;
    public float AttackSwingTime = 1;
    public float AttackRange = 1;
    public float AttackPushForce = 1;
    public float AttackCooldown = 1;
    #endregion

    #region Defence
    public float PhysicalDefence = 0;
    public float MagicalDefence = 0;
    #endregion

    #region Movement
    public float MoveSpeed = 1;
    public float DodgeCooldownReduction = 0;
    #endregion

    public CombatStats(
        float currentHealth = 1, float maxHealth = 1, 
        float currentMana = 1, float maxMana = 1,
        float currentStamina = 1, float maxStamina = 1,

        float attackDamage = 1, float attackSpeed = 1, float attackSwingTime = 1, float attackRange = 1, float attackPushForce = 1, float attackCooldown = 1,

        float physicalDefence = 0, float magicalDefence = 0,

        float moveSpeed = 1,
        float dodgeCooldownReduction = 0)
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
        DodgeCooldownReduction = dodgeCooldownReduction;
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

    public Special(int agility = 0, int charisma = 0, int endurance = 0, int intelligence = 0, int luck = 0, int perception = 0, int strength = 0)
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
