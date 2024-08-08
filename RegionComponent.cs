using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegionComponent : MonoBehaviour
{
    public int RegionID;

    public RegionData Region_Data;
    public BoxCollider RegionArea;

    void Awake()
    {
        RegionArea = GetComponent<BoxCollider>();
    }
}
