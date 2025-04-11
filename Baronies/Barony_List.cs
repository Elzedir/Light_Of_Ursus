using System.Collections.Generic;
using IDs;
using Settlements;
using Tools;

namespace Baronies
{
    public abstract class Barony_List
    {
        static Dictionary<ulong, Barony_Data> s_preExistingBaronies;

        public static Dictionary<ulong, Barony_Data> S_PreExistingBaronies =>
            s_preExistingBaronies ??= _initialisePreExistingBaronies();

        static Dictionary<ulong, Barony_Data> _initialisePreExistingBaronies()
        {
            var baronies = new Dictionary<ulong, Barony_Data>();

            const ulong heartlandsID = 400000;
            baronies.Add(heartlandsID, new Barony_Data(
                id: heartlandsID,
                name: "The Heartlands",
                description: "The land of hearts",
                countyID: 1,
                allSettlements: new Dictionary<ulong, Settlement_Data>()
            ));

            const ulong southlandsID = 400001;
            baronies.Add(southlandsID, new Barony_Data(
                id: southlandsID,
                name: "The Southlands",
                description: "The land of south",
                countyID: 1,
                allSettlements: new Dictionary<ulong, Settlement_Data>()
            ));

            const ulong wherelandsID = 400002;
            baronies.Add(wherelandsID, new Barony_Data(
                id: wherelandsID,
                name: "The Wherelands",
                description: "The land of where",
                countyID: 1,
                allSettlements: new Dictionary<ulong, Settlement_Data>()
            ));

            const ulong aID = 400003;
            baronies.Add(aID, new Barony_Data(
                id: aID,
                name: "The A",
                description: "The land of A",
                countyID: 1,
                allSettlements: new Dictionary<ulong, Settlement_Data>()
            ));

            const ulong bID = 400004;
            baronies.Add(bID, new Barony_Data(
                id: bID,
                name: "The B",
                description: "The land of B",
                countyID: 2,
                allSettlements: new Dictionary<ulong, Settlement_Data>()
            ));

            const ulong cID = 400005;
            baronies.Add(cID, new Barony_Data(
                id: cID,
                name: "The C",
                description: "The land of C",
                countyID: 2,
                allSettlements: new Dictionary<ulong, Settlement_Data>()
            ));

            const ulong dID = 400006;
            baronies.Add(dID, new Barony_Data(
                id: dID,
                name: "The D",
                description: "The land of D",
                countyID: 2,
                allSettlements: new Dictionary<ulong, Settlement_Data>()
            ));

            const ulong eID = 400007;
            baronies.Add(eID, new Barony_Data(
                id: eID,
                name: "The E",
                description: "The land of E",
                countyID: 2,
                allSettlements: new Dictionary<ulong, Settlement_Data>()
            ));

            const ulong fID = 400008;
            baronies.Add(fID, new Barony_Data(
                id: fID,
                name: "The F",
                description: "The land of F",
                countyID: 3,
                allSettlements: new Dictionary<ulong, Settlement_Data>()
            ));

            const ulong gID = 400009;
            baronies.Add(gID, new Barony_Data(
                id: gID,
                name: "The G",
                description: "The land of G",
                countyID: 3,
                allSettlements: new Dictionary<ulong, Settlement_Data>()
            ));

            const ulong hID = 400010;
            baronies.Add(hID, new Barony_Data(
                id: hID,
                name: "The H",
                description: "The land of H",
                countyID: 3,
                allSettlements: new Dictionary<ulong, Settlement_Data>()
            ));

            const ulong iID = 400011;
            baronies.Add(iID, new Barony_Data(
                id: iID,
                name: "The I",
                description: "The land of I",
                countyID: 3,
                allSettlements: new Dictionary<ulong, Settlement_Data>()
            ));

            const ulong jID = 400012;
            baronies.Add(jID, new Barony_Data(
                id: jID,
                name: "The J",
                description: "The land of J",
                countyID: 4,
                allSettlements: new Dictionary<ulong, Settlement_Data>()
            ));

            const ulong kID = 400013;
            baronies.Add(kID, new Barony_Data(
                id: kID,
                name: "The K",
                description: "The land of K",
                countyID: 4,
                allSettlements: new Dictionary<ulong, Settlement_Data>()
            ));

            const ulong lID = 400014;
            baronies.Add(lID, new Barony_Data(
                id: lID,
                name: "The L",
                description: "The land of L",
                countyID: 4,
                allSettlements: new Dictionary<ulong, Settlement_Data>()
            ));

            const ulong mID = 400015;
            baronies.Add(mID, new Barony_Data(
                id: mID,
                name: "The M",
                description: "The land of M",
                countyID: 4,
                allSettlements: new Dictionary<ulong, Settlement_Data>()
            ));

            return baronies;
        }
    }
}