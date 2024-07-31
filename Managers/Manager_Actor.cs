using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Manager_Actor
{
    public static HashSet<Actor_Base> AllActors = new();
    public static HashSet<int> AllActorIDs = new();
    static int _lastUnusedID = 100000;

    public static void AddToAllActorList(Actor_Base actor)
    {
        AllActors.Add(actor);
    }

    public static int GetRandomActorID()
    {
        while (AllActorIDs.Contains(_lastUnusedID))
        {
            _lastUnusedID++;
        }

        AllActorIDs.Add(_lastUnusedID);

        return _lastUnusedID;
    }

    public static ActorName GetRandomActorName(Actor_Base actor)
    {
        // Get name based on culture, religion, species, etc.

        return new ActorName($"Test_{UnityEngine.Random.Range(0, _lastUnusedID)}", $"of Tester");
    }

    public static Actor_Base GetActor(int actorID)
    {
        return AllActors.FirstOrDefault(a => a.ActorData.ActorID == actorID);
    }

    public static Actor_Base InitialiseNewActorOnGO(Vector3 spawnPoint)
    {
        GameObject actorGO = _createNewActorGO(spawnPoint);

        Actor_Base actor = actorGO.AddComponent<Actor_Base>();

        Actor_Data_SO actorData = ScriptableObject.CreateInstance<Actor_Data_SO>();
        actorData.InitialiseNewData(
            actorID: GetRandomActorID(), actor: actor, actorName: GetRandomActorName(actor), 
            );
        actor.Initialise(actorData);

        actorGO.transform.parent.name = $"{actor.ActorData.ActorName.Name}Body";
        actorGO.name = $"{actor.ActorData.ActorName.Name}";

        return actor;
    }

    static GameObject _createNewActorGO(Vector3 spawnPoint)
    {
        GameObject actorBody = new GameObject();
        actorBody.transform.position = spawnPoint;
        actorBody.AddComponent<Rigidbody>();

        GameObject actorGO = new GameObject();
        actorGO.transform.parent = actorBody.transform;
        actorGO.transform.localPosition = Vector3.zero;

        actorGO.AddComponent<BoxCollider>();
        actorGO.AddComponent<Animator>();
        actorGO.AddComponent<Animation>();

        return actorGO;
    }
}
