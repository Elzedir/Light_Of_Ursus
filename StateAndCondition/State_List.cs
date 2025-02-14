using System.Collections.Generic;
using UnityEngine;

namespace StateAndCondition
{
    public abstract class State_List
    {
        static Dictionary<ulong, State_Data> s_defaultStates;
        public static Dictionary<ulong, State_Data> DefaultStates => s_defaultStates ??= _initialiseDefaultStates();

        public State_Data GetState_Data(StateName stateName)
        {
            if (DefaultStates.TryGetValue((ulong)stateName, out var stateData))
            {
                return stateData;
            }
            
            Debug.LogError($"State_Data not found for: {stateName}.");
            return null;
        }
        
        static Dictionary<ulong, State_Data> _initialiseDefaultStates()
        {
            return new Dictionary<ulong, State_Data>
            {
                { (ulong)StateName.None, new State_Data( StateName.None, false, StateName.None) },
                
                { (ulong)StateName.IsAlive, new State_Data(StateName.IsAlive, true, StateName.None) },
                
                { (ulong)StateName.CanIdle, new State_Data(StateName.CanIdle, true, StateName.None) },
                { (ulong)StateName.IsIdling, new State_Data(StateName.IsIdling, false, StateName.CanIdle) },
                
                { (ulong)StateName.CanMove, new State_Data(StateName.CanMove, true, StateName.None) },
                { (ulong)StateName.IsMoving, new State_Data(StateName.IsMoving, false, StateName.CanMove) },
                
                { (ulong)StateName.CanJump, new State_Data(StateName.CanJump, true, StateName.None) },
                { (ulong)StateName.IsJumping, new State_Data(StateName.IsJumping, false, StateName.CanJump) },
                
                { (ulong)StateName.CanCombat, new State_Data(StateName.CanCombat, true, StateName.None) },
                { (ulong)StateName.IsInCombat, new State_Data(StateName.IsInCombat, false, StateName.CanCombat) },
                
                { (ulong)StateName.CanDodge, new State_Data(StateName.CanDodge, true, StateName.None) },
                { (ulong)StateName.IsDodging, new State_Data(StateName.IsDodging, false, StateName.CanDodge) },
                
                { (ulong)StateName.CanBlock, new State_Data(StateName.CanBlock, true, StateName.None) },
                { (ulong)StateName.IsBlocking, new State_Data(StateName.IsBlocking, false, StateName.CanBlock) },
                
                { (ulong)StateName.CanBerserk, new State_Data(StateName.CanBerserk, true, StateName.None) },
                { (ulong)StateName.IsBerserking, new State_Data(StateName.IsBerserking, false, StateName.CanBerserk) },
                
                { (ulong)StateName.CanTalk, new State_Data(StateName.CanTalk, true, StateName.None) },
                { (ulong)StateName.IsTalking, new State_Data(StateName.IsTalking, false, StateName.CanTalk) },
                
                { (ulong)StateName.Alerted, new State_Data(StateName.Alerted, false, StateName.None) },
                { (ulong)StateName.Hostile, new State_Data(StateName.Hostile, false, StateName.None) },
                
                { (ulong)StateName.CanGetPregnant, new State_Data(StateName.CanGetPregnant, true, StateName.None) },
                { (ulong)StateName.IsPregnant, new State_Data(StateName.IsPregnant, false, StateName.CanGetPregnant) }
            };
        }
    }

    public enum StateName
    {
        None,

        IsAlive,

        //= Idling

        CanIdle,
        IsIdling,

        //= Movement

        CanMove,
        IsMoving,

        CanJump,
        IsJumping,

        //= Combat

        CanCombat,
        IsInCombat,

        CanDodge,
        IsDodging,

        CanBlock,
        IsBlocking,

        CanBerserk,
        IsBerserking,

        //= Interaction

        CanTalk,
        IsTalking,

        Alerted,
        Hostile,

        //= Other

        CanGetPregnant,
        IsPregnant,

        CanBeDepressed,
        IsDepressed,

        CanDrown,
        IsDrowning,
        CanSuffocate,
        IsSuffocating,

        CanReanimate,
        IsReanimated,

        InFire,
        OnFire,
    }
}