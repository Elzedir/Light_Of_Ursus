using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Manager_Actor : MonoBehaviour
{
    public static HashSet<Actor_Base> AllActors;

    public static IEnumerator OnSceneLoaded()
    {
        foreach(Actor_Base actor in _findAllActors())
        {
            if (AllActors.Any(a => actor)) continue;

            if (AllActors.Any(a => a.ActorData.ActorID == actor.ActorData.ActorID))
            {
                Debug.Log($"ActorID is used by multiple actors:");

                foreach (var actorSameID in AllActors.Where(a => a.ActorData.ActorID == actor.ActorData.ActorID).ToList())
                {
                    Debug.Log($"{actorSameID.ActorData.ActorName}");
                }
            }
            
            yield return AllActors.Add(actor);
        }
    }

    static List<Actor_Base> _findAllActors()
    {
        return new List<Actor_Base>(FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None).OfType<Actor_Base>());
    }

    public static Actor_Base GetActor(int actorID)
    {
        return AllActors.FirstOrDefault(a => a.ActorData.ActorID == actorID);
    }
}
