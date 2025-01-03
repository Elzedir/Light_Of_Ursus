using System.Collections.Generic;
using Relationships;

namespace Faction
{
    public abstract class Faction_List
    {
        static Dictionary<uint, Faction_Data> _defaultFactions;
        public static Dictionary<uint, Faction_Data> DefaultFactions => _defaultFactions ??= _initialiseDefaultFactions();
        
        static Dictionary<uint, Faction_Data> _initialiseDefaultFactions()
        {
            return new Dictionary<uint, Faction_Data>
            {
                {
                    1, new Faction_Data(
                        factionID: 1,
                        factionName: "Wanderers",
                        allFactionActorIDs: new HashSet<uint>(),
                        allFactionRelations: new List<FactionRelationData>()
                    )
                },
                {
                    2, new Faction_Data(
                        factionID: 2,
                        factionName: "Player Faction",
                        allFactionActorIDs: new HashSet<uint>(),
                        allFactionRelations: new List<FactionRelationData>()
                    )
                }
            };
        }
    }
}