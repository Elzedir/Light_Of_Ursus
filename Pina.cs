using System.Collections;
using System.Collections.Generic;
using Managers;
using Pathfinding;
using UnityEngine;
using z_Abandoned;

public class Pina : MonoBehaviour
{
    Controller_Agent _agent;

    void Start()
    {
        _subscribeToEvents();
        _agent = gameObject.AddComponent<Controller_Agent>();
    }

    void _subscribeToEvents()
    {
        Manager_Dialogue.Instance.pinaIntroEvent?.AddListener(PinaIntro);
    }

    void PinaIntro()
    {
        FollowUrsus();
    }

    void FollowUrsus()
    {
        _agent.SetAgentDetails(new List<MoverType_Deprecated> { MoverType_Deprecated.Ground }, targetGO: Manager_Game.S_Instance.Player.gameObject, followDistance: 1.5f);
    }

    void OnDestroy()
    {
        Manager_Dialogue.Instance.luxIntroEvent?.RemoveListener(PinaIntro);
    }
}
