using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Manager_Ability : MonoBehaviour
{
    public static List<Ability> AllAbilityList = new();

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
            Resources.Load<AnimationClip>("Animations/Abilities/Charge"),
            null
            );
    }

    Ability _eagleStomp()
    {
        IEnumerator eagleStomp(Actor_Base actor = null)
        {
            if (actor == null) throw new ArgumentException("Actor is null.");

            actor.ActorBody.AddForce(new Vector3(Camera.main.transform.forward.x, 25, Camera.main.transform.forward.z), ForceMode.Impulse);

            float elapsedTime = 0;
            GameObject reticleGO = new GameObject("Reticle");
            reticleGO.transform.position = Vector3.zero;
            reticleGO.transform.parent = GameObject.Find("EagleStompTest").transform;
            reticleGO.transform.localScale = new Vector3(3, 3, 3);
            reticleGO.AddComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Sprites/Mine");

            while (elapsedTime < 3)
            {
                elapsedTime += UnityEngine.Time.deltaTime;

                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, Mathf.Infinity, ~LayerMask.GetMask("Player")))
                {
                    reticleGO.transform.position = hit.point;

                    if (Input.GetMouseButtonDown(0))
                    {
                        stomp(hit, actor);
                        break;
                    }
                }

                yield return null;

                
            }

            UnityEngine.Object.Destroy(reticleGO);
        }

        void stomp(RaycastHit hit, Actor_Base actor)
        {
            Vector3 direction = (hit.point - actor.ActorBody.transform.position).normalized;
            actor.ActorBody.AddForce(direction * 50, ForceMode.Impulse);
        }

        return new Ability(
            name: "Eagle Stomp",
            description: "Fly high, little one.",
            currentLevel: 0,
            maxLevel: 10,
            baseDamage: new List<(float, DamageType)> { (5, DamageType.Normal) },
            animationClip: null,
            abilityActions: new List<(string Name, IEnumerator Action)>()
            {
                ("Eagle Stomp", eagleStomp())
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
    public List<(string Name, IEnumerator Action)> AbilityActions { get; private set; }

    public Ability(string name, string description, int currentLevel, int maxLevel, List<(float, DamageType)> baseDamage, AnimationClip animationClip, List<(string, IEnumerator)> abilityActions)
    {
        Name = name;
        Description = description;
        CurrentLevel = currentLevel;
        MaxLevel = maxLevel;
        BaseDamage = baseDamage;
        AnimationClip = animationClip;
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
public class ActorAbilities
{
    public Dictionary<Ability, float> AbilityList = new();
}
