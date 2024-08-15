using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Manager_Faction : MonoBehaviour
{
    public static AllFactions_SO AllFactions_SO;

    public void OnSceneLoaded()
    {
        AllFactions_SO = Resources.Load<AllFactions_SO>("ScriptableObjects/AllFactions_SO");
        AllFactions_SO.PrepareToInitialise();

        Manager_Initialisation.OnInitialiseManagerFaction += _initialise;
    }

    void _initialise()
    {

    }

    public static FactionData GetFaction(int factionID)
    {
        return AllFactions_SO.GetFaction(factionID);
    }
}
