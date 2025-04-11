using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Actors;
using Buildings;
using Cities;
using Counties;
using Faction;
using Items;
using Jobs;
using Settlements;
using Station;
using UnityEngine;

namespace IDs
{
    public abstract class ID_Manager
    {
        List<ulong> _id_Save;

        static readonly HashSet<ulong> s_ids = new();
        static readonly Dictionary<IDType, ulong> s_lastUnusedIDs = new();
        static readonly Dictionary<IDType, List<ulong>> s_preExistingIDLists = new();
        
        public static void AddNewID(ulong id, IDType idType)
        {
            if (!s_ids.Add(id))
                throw new Exception($"Error: ID {id} already exists in IDType {idType}.");

            s_lastUnusedIDs[idType] = id;
        }
        
        public static ulong GetNewID(IDType idType)
        {
            s_ids.Add(0);
            
            var iteration = 100000;

            if (!s_lastUnusedIDs.ContainsKey(idType))
                s_lastUnusedIDs[idType] = _initialiseLastUnusedID(idType);
            
            while (!s_ids.Add(s_lastUnusedIDs[idType]) && iteration > 0)
            {
                s_lastUnusedIDs[idType]++;
                iteration--;
            }
            
            if (iteration == 0)
                //* Later, if we start hitting the limit, make the ranges expandable. So for items and actors, just add 1 000 000 to them. So from 1 - 99 999,
                //* it becomes 1 - 99999, and 1 000 001 - 1 099 999. Then we can just keep adding 1 000 000 to the end of each range we need to.
                //* Later we can use a better solution to expand it exponentially rather than linearly.
                throw new Exception($"Error: Infinite loop detected while trying to find new ID for IDType {idType}.");
            
            return s_lastUnusedIDs[idType];
        }

        public static ulong GetGameObjectID(GameObject gameObject)
        {
            var match = new Regex(@"\d+").Match(gameObject.name);

            if (match.Success && ulong.TryParse(match.Value, out var id)) 
                return id;

            throw new ArgumentException($"No valid ID found in the name of GameObject: {gameObject.name}");
        }
        
        public static IDType GetIDType(ulong id)
        {
            foreach (var idType in Enum.GetValues(typeof(IDType)).Cast<IDType>())
            {
                var range = _getRange(idType);
                if (id >= range.Item1 && id <= range.Item2)
                    return idType;
            }

            throw new Exception($"Error: ID {id} not found in any IDType range.");
        }

        static ulong _initialiseLastUnusedID(IDType idType)
        {
            var existingIDs = _getExistingIDList(idType);
            var minimumValue = _getRange(idType).Item1;
            var iteration = 100000;

            while (existingIDs.Contains(minimumValue) && iteration > 0)
            {
                minimumValue++;
                iteration--;
            }

            if (iteration == 0)
                throw new Exception(
                    $"Error: Infinite loop detected while trying to find minimum value for IDType {idType}.");

            return minimumValue;
        }

        static List<ulong> _getExistingIDList(IDType idType)
        {
            if (s_preExistingIDLists.TryGetValue(idType, out var action))
                return action;
            
            s_preExistingIDLists[idType] = _getExistingIDLists(idType);
            
            return s_preExistingIDLists[idType];
        }

        static List<ulong> _getExistingIDLists(IDType idType)
        {
            return idType switch
            {
                IDType.None => new List<ulong> { 0 },
                IDType.Actor => Actor_Manager.GetAllActorIDs(),
                IDType.Item => Item_Manager.GetAllItemIDs(),
                IDType.Job => Job_Manager.GetAllJobIDs(),
                IDType.Settlement => Settlement_Manager.GetAllSettlementIDs(),
                IDType.Station => Station_Manager.GetAllStationIDs(),
                IDType.Building => Building_Manager.GetAllBuildingIDs(),
                IDType.Barony => Barony_Manager.GetAllBaronyIDs(),
                IDType.County => County_Manager.GetAllCountyIDs(),
                IDType.Faction => Faction_Manager.GetAllFactionIDs(),
                _ => throw new ArgumentOutOfRangeException(nameof(idType), $"Unhandled IDType: {idType}")
            };
        }
        
        static (ulong, ulong) _getRange(IDType idType)
        {
            if (S_ID_Ranges.TryGetValue(idType, out var range))
                return range;
            
            throw new Exception($"Error: IDType {idType} not found in ID_Ranges.");
        }
        
        static Dictionary<IDType, (ulong, ulong)> s_iD_Ranges;
        static Dictionary<IDType, (ulong, ulong)> S_ID_Ranges => s_iD_Ranges ??= _initialiseIDRanges();

        static Dictionary<IDType, (ulong, ulong)> _initialiseIDRanges()
        {
            var idRanges = new Dictionary<IDType, (ulong, ulong)>
            {
                { IDType.None, (0, 0) },
                { IDType.Actor, (1, 99999) },
                { IDType.Station, (100000, 199999) },
                { IDType.Building, (200000, 299999) },
                { IDType.Settlement, (300000, 399999) },
                { IDType.Barony, (400000, 499999) },
                { IDType.County, (500000, 599999) },
                { IDType.Faction, (600000, 699999) },
                { IDType.Profile, (700000, 799999) },
                { IDType.Item, (800000, 899999) },
                { IDType.Job, (900000, 999999) },
            };

            _validateIDRanges(idRanges);
            
            return idRanges;
        }

        static void _validateIDRanges(Dictionary<IDType, (ulong, ulong)> id_Ranges)
        {
            var keys = id_Ranges.Keys.ToList();

            for (var i = 0; i < keys.Count; i++)
            {
                var typeA = keys[i];
                var rangeA = id_Ranges[typeA];

                for (var j = i + 1; j < keys.Count; j++)
                {
                    var typeB = keys[j];
                    var rangeB = id_Ranges[typeB];

                    if (_rangesOverlap(rangeA, rangeB))
                    {
                        throw new Exception($"Error: {typeA} and {typeB} have overlapping ranges: {rangeA} and {rangeB}");
                    }
                }
            }
        }

        static bool _rangesOverlap((ulong start, ulong end) range1, (ulong start, ulong end) range2)
        {
            return range1.start <= range2.end && range2.start <= range1.end;
        }
    }

    public enum IDType
    {
        None,
        
        Actor,
        Station,
        Building,
        Settlement,
        Barony,
        County,
        Faction,
        Profile,
        Item,
        Job
    }
}