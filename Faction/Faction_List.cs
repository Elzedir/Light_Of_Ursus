using System.Collections.Generic;
using Relationships;

namespace Faction
{
    public abstract class Faction_List
    {
        static Dictionary<ulong, Faction_Data> _defaultFactions;
        public static Dictionary<ulong, Faction_Data> DefaultFactions => _defaultFactions ??= _initialiseDefaultFactions();
        
        static Dictionary<ulong, Faction_Data> _initialiseDefaultFactions()
        {
            return new Dictionary<ulong, Faction_Data>
            {
                {
                    1, new Faction_Data(
                        factionID: 1,
                        factionName: "Wanderers",
                        allFactionActorIDs: new HashSet<ulong>(),
                        allFactionRelations: new List<FactionRelationData>()
                    )
                },
                {
                    2, new Faction_Data(
                        factionID: 2,
                        factionName: "Player Faction",
                        allFactionActorIDs: new HashSet<ulong>(),
                        allFactionRelations: new List<FactionRelationData>()
                    )
                }
            };
        }
    }
}