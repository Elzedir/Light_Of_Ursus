using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager_Ability : MonoBehaviour
{
    public static List<Ability> AllAbilityList = new();

    static Dictionary<int, (Rigidbody RigidBody, bool Trigger)> _entities = new();

    public void OnSceneLoaded()
    {
        _initialiseMeleeAbilities();
    }

    public static Ability GetAbility(string name)
    {
        foreach(var ability in AllAbilityList)
        {
            if (ability.Name == name) return ability;
        }

        return null;
    }

    public static void SetCharacter(int ID, (Rigidbody, bool) data)
    {
        if (_entities.ContainsKey(ID))
        {
            _entities[ID] = data;
        }
        else
        {
            _entities.Add(ID, data);
        }
    }

    void _initialiseMeleeAbilities()
    {
        AllAbilityList.Add(_charge());
        AllAbilityList.Add(_eagleStomp());
    }

    Ability _charge()
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

    Ability _eagleStomp()
    {
        IEnumerator eagleStomp(int ID)
        {
            _entities[ID].RigidBody.AddForce(new Vector3(0, 15, 10), ForceMode.Impulse);

            float elapsedTime = 0;
            GameObject reticleGO = new GameObject("Reticle");
            reticleGO.transform.position = Vector3.zero;
            reticleGO.transform.parent = GameObject.Find("EagleStompTest").transform;
            reticleGO.transform.localScale = new Vector3(3, 3, 3);
            reticleGO.AddComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Sprites/Mine");

            while (elapsedTime < 3) // Or until landing
            {
                elapsedTime += Time.deltaTime;

                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit))
                {
                    reticleGO.transform.position = hit.point;

                    if (_entities[ID].Trigger)
                    {
                        Vector3 direction = (hit.point - _entities[ID].RigidBody.transform.position).normalized;
                        _entities[ID].RigidBody.AddForce(direction * 50, ForceMode.Impulse);
                        _entities[ID] = (_entities[ID].RigidBody, false);
                        break;
                    }
                }

                yield return null;
            }

            UnityEngine.Object.Destroy(reticleGO);
        }

        return new Ability(
            name: "Eagle Stomp",
            description: "Fly high, little one.",
            currentLevel: 0,
            maxLevel: 10,
            baseDamage: new List<(float, DamageType)> { (5, DamageType.Normal) },
            animationClip: null,
            abilityFunctions: new List<(string Name, Action<int> Function)>()
            {
                ("Eagle Stomp", (int ID) => StartCoroutine(eagleStomp(ID)))
            }
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
    public List<(string Name, Action<int> Function)> AbilityFunctions { get; private set; }

    public Ability(string name, string description, int currentLevel, int maxLevel, List<(float, DamageType)> baseDamage, AnimationClip animationClip, List<(string, Action<int>)> abilityFunctions)
    {
        Name = name;
        Description = description;
        CurrentLevel = currentLevel;
        MaxLevel = maxLevel;
        BaseDamage = baseDamage;
        AnimationClip = animationClip;
        AbilityFunctions = abilityFunctions;
    }

    public Action<int> GetAction(string functionName)
    {
        foreach(var function in AbilityFunctions)
        {
            if (function.Name == functionName)
            {
                return function.Function;
            }
        }

        return null;
    }

    public void DealDamage()
    {
        // character.ReceiveDamage (new Damage(BaseDamage));
    }
}

