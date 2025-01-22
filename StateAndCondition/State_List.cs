using System.Collections.Generic;
using UnityEngine;

namespace StateAndCondition
{
    public abstract class State_List
    {
        static Dictionary<uint, State_Data> _defaultStates;
        public static Dictionary<uint, State_Data> DefaultStates => _defaultStates ??= _initialiseDefaultStates();

        public State_Data GetState_Data(StateName stateName)
        {
            if (DefaultStates.TryGetValue((uint)stateName, out var stateData))
            {
                return stateData;
            }
            
            Debug.LogError($"State_Data not found for: {stateName}.");
            return null;
        }
        
        static Dictionary<uint, State_Data> _initialiseDefaultStates()
        {
            return new Dictionary<uint, State_Data>
            {
                { (uint)StateName.None, new State_Data( StateName.None, false, StateName.None) },
                
                { (uint)StateName.IsAlive, new State_Data(StateName.IsAlive, true, StateName.None) },
                
                { (uint)StateName.CanIdle, new State_Data(StateName.CanIdle, true, StateName.None) },
                { (uint)StateName.IsIdling, new State_Data(StateName.IsIdling, false, StateName.CanIdle) },
                
                { (uint)StateName.CanMove, new State_Data(StateName.CanMove, true, StateName.None) },
                { (uint)StateName.IsMoving, new State_Data(StateName.IsMoving, false, StateName.CanMove) },
                
                { (uint)StateName.CanJump, new State_Data(StateName.CanJump, true, StateName.None) },
                { (uint)StateName.IsJumping, new State_Data(StateName.IsJumping, false, StateName.CanJump) },
                
                { (uint)StateName.CanCombat, new State_Data(StateName.CanCombat, true, StateName.None) },
                { (uint)StateName.IsInCombat, new State_Data(StateName.IsInCombat, false, StateName.CanCombat) },
                
                { (uint)StateName.CanDodge, new State_Data(StateName.CanDodge, true, StateName.None) },
                { (uint)StateName.IsDodging, new State_Data(StateName.IsDodging, false, StateName.CanDodge) },
                
                { (uint)StateName.CanBlock, new State_Data(StateName.CanBlock, true, StateName.None) },
                { (uint)StateName.IsBlocking, new State_Data(StateName.IsBlocking, false, StateName.CanBlock) },
                
                { (uint)StateName.CanBerserk, new State_Data(StateName.CanBerserk, true, StateName.None) },
                { (uint)StateName.IsBerserking, new State_Data(StateName.IsBerserking, false, StateName.CanBerserk) },
                
                { (uint)StateName.CanTalk, new State_Data(StateName.CanTalk, true, StateName.None) },
                { (uint)StateName.IsTalking, new State_Data(StateName.IsTalking, false, StateName.CanTalk) },
                
                { (uint)StateName.Alerted, new State_Data(StateName.Alerted, false, StateName.None) },
                { (uint)StateName.Hostile, new State_Data(StateName.Hostile, false, StateName.None) },
                
                { (uint)StateName.CanGetPregnant, new State_Data(StateName.CanGetPregnant, true, StateName.None) },
                { (uint)StateName.IsPregnant, new State_Data(StateName.IsPregnant, false, StateName.CanGetPregnant) }
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