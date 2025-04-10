using System.Collections.Generic;
using Settlements;
using Tools;

namespace Baronies
{
    public abstract class Barony_List
    {
        static Dictionary<ulong, Barony_Data> s_preExistingBaronies;
        public static Dictionary<ulong, Barony_Data> S_PreExistingBaronies => s_preExistingBaronies ??= _initialisePreExistingBaronies();

        static Dictionary<ulong, Barony_Data> _initialisePreExistingBaronies()
        {
            return new Dictionary<ulong, Barony_Data>
            {
                {
                    1, new Barony_Data
                    (
                        id: 1,
                        name: "The Heartlands",
                        description: "The land of hearts",
                        countyID: 1,
                        allSettlements: new Dictionary<ulong, Settlement_Data>()
                    )
                },
                {
                    2, new Barony_Data
                    (
                        id: 2,
                        name: "The Southlands",
                        description: "The land of south",
                        countyID: 1,
                        allSettlements: new Dictionary<ulong, Settlement_Data>()
                    )
                },
                {
                    3, new Barony_Data
                    (
                        id: 3,
                        name: "The Wherelands",
                        description: "The land of where",
                        countyID: 1,
                        allSettlements: new Dictionary<ulong, Settlement_Data>()
                    )
                },
                {
                    4, new Barony_Data
                    (
                        id: 4,
                        name: "The A",
                        description: "The land of A",
                        countyID: 1,
                        allSettlements: new Dictionary<ulong, Settlement_Data>()
                    )
                },
                {
                    5, new Barony_Data
                    (
                        id: 5,
                        name: "The B",
                        description: "The land of B",
                        countyID: 2,
                        allSettlements: new Dictionary<ulong, Settlement_Data>()
                    )
                },
                {
                    6, new Barony_Data
                    (
                        id: 6,
                        name: "The C",
                        description: "The land of C",
                        countyID: 2,
                        allSettlements: new Dictionary<ulong, Settlement_Data>()
                    )
                },
                {
                    7, new Barony_Data
                    (
                        id: 7,
                        name: "The D",
                        description: "The land of D",
                        countyID: 2,
                        allSettlements: new Dictionary<ulong, Settlement_Data>()
                    )
                },
                {
                    8, new Barony_Data
                    (
                        id: 8,
                        name: "The E",
                        description: "The land of E",
                        countyID: 2,
                        allSettlements: new Dictionary<ulong, Settlement_Data>()
                    )
                },
                {
                    9, new Barony_Data
                    (
                        id: 9,
                        name: "The F",
                        description: "The land of F",
                        countyID: 3,
                        allSettlements: new Dictionary<ulong, Settlement_Data>()
                    )
                },
                {
                    10, new Barony_Data
                    (
                        id: 10,
                        name: "The G",
                        description: "The land of G",
                        countyID: 3,
                        allSettlements: new Dictionary<ulong, Settlement_Data>()
                    )
                },
                {
                    11, new Barony_Data
                    (
                        id: 11,
                        name: "The H",
                        description: "The land of H",
                        countyID: 3,
                        allSettlements: new Dictionary<ulong, Settlement_Data>()
                    )
                },
                {
                    12, new Barony_Data
                    (
                        id: 12,
                        name: "The I",
                        description: "The land of I",
                        countyID: 3,
                        allSettlements: new Dictionary<ulong, Settlement_Data>()
                    )
                },
                {
                    13, new Barony_Data
                    (
                        id: 13,
                        name: "The J",
                        description: "The land of J",
                        countyID: 4,
                        allSettlements: new Dictionary<ulong, Settlement_Data>()
                    )
                },
                {
                    14, new Barony_Data
                    (
                        id: 14,
                        name: "The K",
                        description: "The land of K",
                        countyID: 4,
                        allSettlements: new Dictionary<ulong, Settlement_Data>()
                    )
                },
                {
                    15, new Barony_Data
                    (
                        id: 15,
                        name: "The L",
                        description: "The land of L",
                        countyID: 4,
                        allSettlements: new Dictionary<ulong, Settlement_Data>()
                    )
                },
                {
                    16, new Barony_Data
                    (
                        id: 16,
                        name: "The M",
                        description: "The land of M",
                        countyID: 4,
                        allSettlements: new Dictionary<ulong, Settlement_Data>()
                    )
                },
            };
        }
    }
}