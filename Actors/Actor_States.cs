using System;

[System.Serializable]
public class Actor_States
{
    public bool Dead { get; private set; }

    public bool Alerted { get; private set; }
    public bool Hostile { get; private set; }
    
    public bool Jumping { get; private set; }
    public bool Berserk { get; private set; }
    public bool OnFire { get; private set; }
    public bool InFire { get; private set; }
    public bool Talkable { get; private set; }
    public bool Talking { get; private set; }

    public bool DodgeAvailable { get; private set; }
    public bool Dodging { get; private set; }

    public bool CanBlock { get; private set; }
    public bool Blocking { get; private set; }

    private float _lastTimeDodged;

    public Actor_States(
        bool dead = false,
        bool hostile = true,
        bool alerted = false,
        bool jumping = false,
        bool berserk = false,
        bool onFire = false,
        bool inFire = false,
        bool talkable = false,
        bool talking = false,
        bool dodgeAvailable = true,
        bool dodging = false,
        bool blocking = false)
    {
        Dead = dead;
        Hostile = hostile;
        Alerted = alerted;
        Jumping = jumping;
        Berserk = berserk;
        OnFire = onFire;
        InFire = inFire;
        Talkable = talkable;
        Talking = talking;
        DodgeAvailable = dodgeAvailable;
        Dodging = dodging;
        Blocking = blocking;
    }

    public void SetState(string stateName, bool state)
    {
        var property = GetType().GetProperty(stateName);

        if (property != null && property.PropertyType == typeof(bool)) property.SetValue(this, state);

        else throw new ArgumentException($"No boolean property named '{stateName}' found.");
    }
}