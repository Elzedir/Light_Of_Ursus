using System;
using Initialisation;
using UnityEngine;

namespace Faction
{
    public class Faction_Component : MonoBehaviour
    {
        public uint FactionID => FactionData.FactionID;
        
        public Faction_Data FactionData;

        void Awake()
        {
            Manager_Initialisation.OnInitialiseFactions += _initialise;
        }
        
        void _initialise()
        {
            FactionData.InitialiseFactionData();
        }
    }
}
