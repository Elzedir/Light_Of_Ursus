using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CityComponent : MonoBehaviour
{
    public City_Data_SO CityData;
    public BoxCollider CityArea;

    public List<Jobsite_Base> AllJobsites = new();
    public ProsperityComponent ProsperityComponent;

    void Awake()
    {
        CityArea = GetComponent<BoxCollider>();

        Manager_Initialisation.OnInitialiseCities += _onInitialise;
        CurrentDate.NewDay += _refreshCity;

        _onInitialise();
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
