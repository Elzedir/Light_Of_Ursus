using System;
using System.Collections;
using System.Collections.Generic;
using Actors;
using Managers;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Abilities
{
    public abstract class Ability_List
    {
        static Dictionary<ulong, Ability_Data> s_defaultAbilities;
        public static Dictionary<ulong, Ability_Data> DefaultAbilities => s_defaultAbilities ??= _initialiseDefaultAbilities();
        
        static Dictionary<ulong, Ability_Data> _initialiseDefaultAbilities()
        {
            return new Dictionary<ulong, Ability_Data>
            {
                {
                    (ulong)AbilityName.Eagle_Stomp, new Ability_Data(
                        abilityName: AbilityName.Eagle_Stomp,
                        abilityDescription: "Fly high, little one.",
                        maxLevel: 10,
                        baseDamage: new List<(float, DamageType)> { (5, DamageType.Normal) },
                        animationClip: null,
                        abilityActions: new List<(string Name, IEnumerator Action)>
                        {
                            ("Eagle Stomp", _eagleStomp())
                        }
                    )
                },
                {
                    (ulong)AbilityName.Charge, new Ability_Data(
                        abilityName: AbilityName.Charge,
                        abilityDescription: "A charge.",
                        maxLevel: 10,
                        baseDamage: new List<(float, DamageType)> { (5, DamageType.Normal) },
                        Resources.Load<AnimationClip>("Animations/Abilities/Charge"),
                        null
                    )
                }
            };
        }

        static IEnumerator _eagleStomp(Actor_Component actor = null)
        {
            if (actor == null) throw new ArgumentException("Actor is null.");

            if (Camera.main == null)
            {
                Debug.LogError("Main camera is null.");
                yield break;
            }

            actor.RigidBody.AddForce(
                new Vector3(Camera.main.transform.forward.x, 25, Camera.main.transform.forward.z),
                ForceMode.Impulse);

            float elapsedTime = 0;

            var reticuleGO = new GameObject("Reticule")
            {
                transform =
                {
                    position   = Vector3.zero,
                    parent     = GameObject.Find("EagleStompTest").transform,
                    localScale = new Vector3(3, 3, 3)
                }
            };

            reticuleGO.AddComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Sprites/Mine");

            while (elapsedTime < 3)
            {
                elapsedTime += Time.deltaTime;

                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit,
                        Mathf.Infinity, ~LayerMask.GetMask("Player")))
                {
                    reticuleGO.transform.position = hit.point;

                    if (Input.GetMouseButtonDown(0))
                    {
                        _stomp(hit, actor);
                        break;
                    }
                }

                yield return null;
            }

            Object.Destroy(reticuleGO);
        }

        static void _stomp(RaycastHit hit, Actor_Component actor)
        {
            var direction = (hit.point - actor.RigidBody.transform.position).normalized;
            actor.RigidBody.AddForce(direction * 50, ForceMode.Impulse);
        }
    }
}