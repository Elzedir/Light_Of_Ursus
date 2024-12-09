using System.Collections.Generic;

namespace Faction
{
    public abstract class Faction_List
    {
        public static readonly Dictionary<uint, Faction_Data> DefaultFactions =
            new()
            {
                {
                    0, new Faction_Data(
                        factionID: 0,
                        factionName: "Wanderers",
                        allFactionActorIDs: new HashSet<uint>(),
                        allFactionRelations: new List<FactionRelationData>()
                    )  
                },
                {
                    1, new Faction_Data(
                        factionID: 1,
                        factionName: "Player Faction",
                        allFactionActorIDs: new HashSet<uint>(),
                        allFactionRelations: new List<FactionRelationData>()
                    )
                }
            };
    }
}