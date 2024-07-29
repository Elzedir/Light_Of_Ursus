using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CityComponent : MonoBehaviour
{
    public City_Data_SO CityData;
    public BoxCollider CityArea;

    void Awake()
    {
        CityArea = GetComponent<BoxCollider>();

        Manager_Initialisation.OnInitialiseActors += _onInitialise;
        CurrentDate.NewDay += _refreshCity;
    }

    void _onInitialise()
    {

    }

    void _refreshCity()
    {
        foreach (DisplayCityCareers careerName in CityData.RequiredCityCareers)
        {
            Career career = Manager_Career.GetCareer(careerName.CareerName);


        }
    }
}
