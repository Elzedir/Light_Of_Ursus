using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager_Ability
{
    public static List<Ability> AllAbilityList = new();

    static Coroutine _eagleStompCoroutine;

    public static Ability GetAbility(string name)
    {
        foreach(var ability in AllAbilityList)
        {
            if (ability.Name == name) return ability;
        }

        return null;
    }

    public static void Initialise()
    {
        _initialiseMeleeAbilities();
    }

    static void _initialiseMeleeAbilities()
    {
        AllAbilityList.Add(_charge());
        AllAbilityList.Add(_eagleStomp());
    }

    static Ability _charge()
    {
        return new Ability(
            name: "Charge",
            description: "A charge.",
            currentLevel: 0,
            maxLevel: 10,
            baseDamage: new List<(float, DamageType)> { (5, DamageType.Normal) },
            Resources.Load<AnimationClip>("Animators/Animations/Abilities/Charge"),
            null
            );
    }

    static Ability _eagleStomp()
    {
        IEnumerator eagleStomp()
        {
            yield return null;
        }

        return new Ability(
            name: "Eagle Stomp",
            description: "Fly high, little one.",
            currentLevel: 0,
            maxLevel: 10,
            baseDamage: new List<(float, DamageType)> { (5, DamageType.Normal) },
            Resources.Load<AnimationClip>("Animators/Animations/Abilities/EagleStomp"),
            eagleStomp()
            );
    }
}

public class Ability
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public int CurrentLevel { get; private set; }
    public int MaxLevel { get; private set; }
    public List<(float, DamageType)> BaseDamage { get; private set; }
    public AnimationClip AnimationClip { get; private set; }
    public IEnumerator Executable { get; private set; }

    public Ability(string name, string description, int currentLevel, int maxLevel, List<(float, DamageType)> baseDamage, AnimationClip animationClip, IEnumerator executable)
    {
        Name = name;
        Description = description;
        CurrentLevel = currentLevel;
        MaxLevel = maxLevel;
        BaseDamage = baseDamage;
        AnimationClip = animationClip;
        Executable = executable;
    }

    public void DealDamage()
    {
        // character.ReceiveDamager (new Damage(BaseDamage));
    }
}

