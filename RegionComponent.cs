using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegionComponent : MonoBehaviour
{
    public RegionData RegionData;
    public BoxCollider RegionArea;

    void Awake()
    {
        RegionArea = GetComponent<BoxCollider>();
    }

    public void SetRegionData(RegionData regionData)
    {
        if (RegionData.OverwriteDataInRegion)
        {
            RegionData = regionData;
        }
    }
}
