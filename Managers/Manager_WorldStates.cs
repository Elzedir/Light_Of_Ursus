using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Manager_WorldStates : MonoBehaviour
{
    
}


[CreateAssetMenu(fileName = "WorldstateData", menuName = "WorldstateData", order = 0)]

public class Worldstate : ScriptableObject
{
    public enum State { Alive, Captured, Dead }

    public string WorldStateName;
    public int WorldstateID;
    public State WorldstateState;
    public Image WorldstateIcon;
}
