using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager_Combat : MonoBehaviour
{

}

public enum DamageType { Normal, Blunt, Slash, Pierce, Fire, Lightning, Magic, Pure }

public class Damage
{
    public List<(float DamageAmount, DamageType DamageType)> TotalDamage = new();
    
    public Damage(List<(float, DamageType)> totalDamage)
    {
        TotalDamage = totalDamage;
    }
}
