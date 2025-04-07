using System.Collections.Generic;
using UnityEngine;

namespace Cities
{
    public abstract class Barony_Manager
    {
        const string _Barony_SOPath = "ScriptableObjects/Barony_SO";
        
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
            var Barony_SO = Resources.Load<Barony_SO>(_Barony_SOPath);
            
            if (Barony_SO is not null) return Barony_SO;
            
            Debug.LogError("Barony_SO not found. Creating temporary Barony_SO.");
            Barony_SO = ScriptableObject.CreateInstance<Barony_SO>();
            
            return Barony_SO;
        }

        public static Barony_Component GetNearestBarony(Vector3 position)
        {
            Barony_Component nearestBarony = null;

            var nearestDistance = float.MaxValue;

            foreach (var Barony in AllBaronies.Barony_Components.Values)
            {
                var distance = Vector3.Distance(position, Barony.transform.position);

                if (!(distance < nearestDistance)) continue;

                nearestBarony  = Barony;
                nearestDistance = distance;
            }

            return nearestBarony;
        }
        
        public static void ClearSOData()
        {
            AllBaronies.ClearSOData();
        }
    }
    
    public enum BaronyName
    {
        None,
        
        Metropolis, Barony, Town, Village, Hamlet, // Economic (Trade)
        
        Citadel, Fortress, Castle, Outpost, Watchtower, // Military defensive
        
        Fort, Encampment, Garrison, Barracks, Camp, // Military offensive
        
        Grand_Estate, Estate, Manor, Farmstead, // Production (food)
        
        Ore_Camp, Mine, Quarry, Dig_Site, // Production (Mineral)
        
        Grand_Port, Port, Harbour, Dock,  // Economic and production (Trade and food)
    }
}
