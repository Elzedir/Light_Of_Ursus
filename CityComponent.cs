using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CityComponent : MonoBehaviour
{
    public int CityID;

    public CityData CityData;
    public BoxCollider CityArea;

    public GameObject CityEntranceSpawnZone;

    public List<Jobsite_Base> AllJobsites = new();
    public ProsperityComponent ProsperityComponent;

    void Awake()
    {
        CityArea = GetComponent<BoxCollider>();
        CityEntranceSpawnZone = Manager_Game.FindTransformRecursively(transform, "CityEntranceSpawnZone").gameObject;

        Manager_Initialisation.OnInitialiseCities += _onInitialise;
        CurrentDate.NewDay += _refreshCity;
    }

    void _onInitialise()
    {
        ProsperityComponent = new ProsperityComponent(CityData.Prosperity.CurrentProsperity);
        AllJobsites = _getJobsitesInArea();
    }

    List<Jobsite_Base> _getJobsitesInArea()
    {
        return Physics.OverlapBox(CityArea.bounds.center, CityArea.bounds.extents)
        .Select(collider => collider.GetComponent<Jobsite_Base>())
        .Where(jobsite => jobsite != null)
        .ToList();
    }

    void _refreshCity()
    {
        


    }
}
