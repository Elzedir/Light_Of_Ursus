using System.Collections.Generic;
using Settlements;
using UnityEngine;

namespace Settlements
{
    public abstract class Settlement_Manager
    {
        const string c_settlement_SOPath = "ScriptableObjects/Settlement_SO";
        
        static Settlement_SO s_allSettlements;
        static Settlement_SO AllSettlements => s_allSettlements ??= _getSettlement_SO();
        
        public static Settlement_Data GetSettlement_Data(ulong settlementID)
        {
            return AllSettlements.GetSettlement_Data(settlementID).Data_Object;
        }
        
        public static Settlement_Data GetSettlement_DataFromName(Settlement_Component settlement_Component)
        {
            return AllSettlements.GetDataFromName(settlement_Component.name)?.Data_Object;
        }
        
        public static Settlement_Component GetSettlement_Component(ulong settlementID)
        {
            return AllSettlements.GetSettlement_Component(settlementID);
        }
        
        public static List<ulong> GetAllSettlementIDs() => AllSettlements.GetAllDataIDs();
        
        static Settlement_SO _getSettlement_SO()
        {
            var settlement_SO = Resources.Load<Settlement_SO>(c_settlement_SOPath);
            
            if (settlement_SO is not null) return settlement_SO;
            
            Debug.LogError("Settlement_SO not found. Creating temporary Settlement_SO.");
            settlement_SO = ScriptableObject.CreateInstance<Settlement_SO>();
            
            return settlement_SO;
        }

        public static Settlement_Component GetNearestSettlement(Vector3 position)
        {
            Settlement_Component nearestSettlement = null;

            var nearestDistance = float.MaxValue;

            foreach (var settlement in AllSettlements.Settlement_Components.Values)
            {
                var distance = Vector3.Distance(position, settlement.transform.position);

                if (!(distance < nearestDistance)) continue;

                nearestSettlement  = settlement;
                nearestDistance = distance;
            }

            return nearestSettlement;
        }
        
        public static void ClearSOData()
        {
            AllSettlements.ClearSOData();
        }
    }
    
    public enum SettlementType
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
