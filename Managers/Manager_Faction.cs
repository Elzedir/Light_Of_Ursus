using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Manager_Faction : MonoBehaviour
{
    public static List<Faction_Data_SO> AllFactions = new();

    public static void InitialiseFactions()
    {
        AllFactions.Clear();

        Faction_Data_SO[] factions = Resources.LoadAll<Faction_Data_SO>("Resources_Factions");

        foreach (Faction_Data_SO faction in factions)
        {
            AllFactions.Add(faction);

            foreach (FactionRelationship relationship in faction.FactionData)
            {
                relationship.RefreshRelationship();
            }
        }

        // Run through all the factions and if the value is not 0 (meaning it has been manually added), set it according to the values of the list of faction relationships
        // LoadFactionData();
    }

    private static Faction_Data_SO CreateFaction(FactionName name)
    {
        Faction_Data_SO faction = new Faction_Data_SO();
        faction.FactionName = name;
        faction.FactionData = new();
        return faction;
    }

    public static Faction_Data_SO GetFaction(FactionName factionName)
    {
        if (!AllFactions.Any(f => f.FactionName == factionName)) return null;

        return AllFactions.FirstOrDefault(f => f.FactionName == factionName);
    } 

    public static void SetFaction(ActorData actorData, FactionName faction)
    {
        actorData.FullIdentification.ActorFaction = faction;
    }
}
