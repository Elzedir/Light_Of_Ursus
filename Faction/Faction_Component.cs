using System;
using System.Collections.Generic;
using System.Linq;
using Initialisation;
using Station;
using UnityEngine;

namespace Faction
{
    public class Faction_Component : MonoBehaviour
    {
        public uint FactionID => FactionData.FactionID;
        
        public Faction_Data FactionData;

        public void SetFactionData(Faction_Data factionData)
        {
            FactionData = factionData;   
        }

        void Awake()
        {
            Manager_Initialisation.OnInitialiseFactions += _initialise;
        }

        void _initialise()
        {
            var factionData = Faction_Manager.GetFaction_DataFromName(this);
            
            if (factionData is null)
            {
                Debug.LogWarning($"Faction with name {name} not found in Faction_SO.");
                return;
            }
            
            SetFactionData(factionData);
            
            FactionData.InitialiseFactionData();
        }
    }
}
