using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

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
        _agent.SetAgentDetails(new List<MoverType> { MoverType.Ground }, targetGO: Manager_Game.Instance.Player.gameObject, followDistance: 1.5f);
    }

    void OnDestroy()
    {
        Manager_Dialogue.Instance.luxIntroEvent?.RemoveListener(PinaIntro);
    }
}
