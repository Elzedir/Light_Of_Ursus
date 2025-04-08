using System.Collections.Generic;
using Baronies;
using UnityEngine;

namespace Cities
{
    public abstract class Barony_Manager
    {
        const string c_barony_SOPath = "ScriptableObjects/Barony_SO";
        
        static Barony_SO s_allBaronies;
        static Barony_SO AllBaronies => s_allBaronies ??= _getBarony_SO();
        
        public static Barony_Data GetBarony_Data(ulong baronyID)
        {
            return AllBaronies.GetBarony_Data(baronyID).Data_Object;
        }
        
        public static Barony_Data GetBarony_DataFromName(Barony_Component barony_Component)
        {
            return AllBaronies.GetDataFromName(barony_Component.name)?.Data_Object;
        }
        
        public static Barony_Component GetBarony_Component(ulong baronyID)
        {
            return AllBaronies.GetBarony_Component(baronyID);
        }
        
        public static List<ulong> GetAllBaronyIDs() => AllBaronies.GetAllDataIDs();
        
        static Barony_SO _getBarony_SO()
        {
            var barony_SO = Resources.Load<Barony_SO>(c_barony_SOPath);
            
            if (barony_SO is not null) return barony_SO;
            
            Debug.LogError("Barony_SO not found. Creating temporary Barony_SO.");
            barony_SO = ScriptableObject.CreateInstance<Barony_SO>();
            
            return barony_SO;
        }

        public static Barony_Component GetNearestBarony(Vector3 position)
        {
            Barony_Component nearestBarony = null;

            var nearestDistance = float.MaxValue;

            foreach (var barony in AllBaronies.Barony_Components.Values)
            {
                var distance = Vector3.Distance(position, barony.transform.position);

                if (!(distance < nearestDistance)) continue;

                nearestBarony  = barony;
                nearestDistance = distance;
            }

            return nearestBarony;
        }
        
        public static void ClearSOData()
        {
            AllBaronies.ClearSOData();
        }
    }
    
    public enum BaronyType
    {
        None,
        
        Metropolis, City, Town, Village, Hamlet, // Economic (Trade)
        
        Citadel, Fortress, Castle, Outpost, Watchtower, // Military defensive
        
        Fort, Encampment, Garrison, Barracks, Camp, // Military offensive
        
        Grand_Estate, Estate, Manor, Farmstead, // Production (food)
        
        Ore_Camp, Mine, Quarry, Dig_Site, // Production (Mineral)
        
        Grand_Port, Port, Harbour, Dock,  // Economic and production (Trade and food)
    }
}
