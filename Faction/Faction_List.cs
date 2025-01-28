using System.Collections.Generic;

namespace Faction
{
    public abstract class Faction_List
    {
        static Dictionary<ulong, Faction_Data> s_defaultFactions;
        public static Dictionary<ulong, Faction_Data> S_DefaultFactions => s_defaultFactions ??= _initialiseDefaultFactions();
        
        static Dictionary<ulong, Faction_Data> _initialiseDefaultFactions()
        {
            return new Dictionary<ulong, Faction_Data>
            {
                {
                    1, new Faction_Data(
                        factionID: (ulong)FactionName.Wanderers,
                        factionName: $"{FactionName.Wanderers}",
                        allFactionActorIDs: new List<ulong>(),
                        allFactionRelations: new Dictionary<ulong, float>()
                    )
                },
                {
                    2, new Faction_Data(
                        factionID: 2,
                        factionName: "Player Faction",
                        allFactionActorIDs: new List<ulong>(),
                        allFactionRelations: new Dictionary<ulong, float>()
                    )
                }
            };
        }
    }
}