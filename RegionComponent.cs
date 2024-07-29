using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegionComponent : MonoBehaviour
{
    public Region_Data_SO Region_Data;
    public BoxCollider RegionArea;

    void Awake()
    {
        RegionArea = GetComponent<BoxCollider>();
    }
}
