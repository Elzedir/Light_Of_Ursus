using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Managers;
using UnityEngine;

public class Manager_GameEvent
{
    public static List<GameEvent> AllGameEvents = new();
    public static HashSet<GameEvent> GameEventIDs = new();

    public static void Initialise()
    {
        _worldEvents();
    }

    static void _worldEvents()
    {
        AllGameEvents.Add(
            new GameEvent(
                eventID: "TestEvent_01",
                eventName: "Test_Event",
                eventDescription: "Test event happening",
                eventActions: new HashSet<(int, Action)> 
                {
                    
                },
                eventStartDate: new Date (1, 1, 100),
                expirationTime: -1
                )
            );
    }
}

[Serializable]
public class CharacterEventManager
{
    public List<GameEvent> Events = new();

    public CharacterEventManager()
    {

    }

    public void AddEvent(GameEvent gameEvent)
    {
        if (gameEvent == null) throw new ArgumentException("GameEvent cannot be null.");

        Events.Add(gameEvent);
    }
}

[Serializable]
public class GameEvent
{
    public string EventID;
    public string EventName;
    public string EventDescription;

    public HashSet<(int ActorID, Action ActorAction)> EventActions;

    public Date EventStartDate;
    public Date EventStartDateRange;
    public Date EventEndDate;
    public Date EventEndDateRange;

    public float ExperationTime; // If -1, then no expiration

    public GameEvent(string eventID, string eventName, string eventDescription, HashSet<(int, Action)> eventActions, Date eventStartDate, float expirationTime)
    {
        if (Manager_GameEvent.GameEventIDs.Any(ge => ge.EventID == eventID)) throw new ArgumentException($"EventID: {eventID} already exists.");

        EventID = eventID;
        EventName = eventName;
        EventDescription = eventDescription;

        EventActions = eventActions ?? new HashSet<(int, Action)>();

        EventStartDate = eventStartDate;
        ExperationTime = expirationTime;
    }
}
