using System;
using System.Collections.Generic;
using UnityEngine;

namespace Managers
{
    public class Manager_Combat : MonoBehaviour
    {

    }

    public enum DamageType
    {
        None,
        
        Normal, 
        Practice,
        Blunt, 
        Slash, 
        Pierce, 
        Fire, 
        Lightning,
        Magic, 
        Pure
    }

    [Serializable]
    public class Damage_Data
    {
        public Dictionary<DamageType, float> TotalDamage;
    
        public Damage_Data(Dictionary<DamageType, float> totalDamage)
        {
            TotalDamage = totalDamage;
        }
    }
}