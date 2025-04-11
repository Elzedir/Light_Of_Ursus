using System.Collections.Generic;
using Baronies;
using IDs;

namespace Counties
{
    public abstract class County_List
    {
        static Dictionary<ulong, County_Data> s_defaultCounties;
        public static Dictionary<ulong, County_Data> DefaultCounties => 
            s_defaultCounties?.Count > 0
                ? s_defaultCounties
                : s_defaultCounties = _initialiseDefaultCounties();
        
        static Dictionary<ulong, County_Data> _initialiseDefaultCounties()
        {
            var counties = new Dictionary<ulong, County_Data>();
            
            const ulong theWestlandsID = 500000; 
            counties.Add(theWestlandsID, new County_Data(
                id: theWestlandsID,
                rulerID: 0,
                name: "The Westlands",
                description: "The land of west",
                allBaronies: new Dictionary<ulong, Barony_Data>()));
            
            const ulong theNorthlandsID = 500001;
            counties.Add(theNorthlandsID, new County_Data(
                id: theNorthlandsID,
                rulerID: 0,
                name: "The Northlands",
                description: "The land of the north",
                allBaronies: new Dictionary<ulong, Barony_Data>()));
            
            const ulong theSouthlandsID = 500002;
            counties.Add(theSouthlandsID, new County_Data(
                id: theSouthlandsID,
                rulerID: 0,
                name: "The Southlands",
                description: "The land of the south",
                allBaronies: new Dictionary<ulong, Barony_Data>()));
            
            const ulong theEastlandsID = 500003;
            counties.Add(theEastlandsID, new County_Data(
                id: theEastlandsID,
                rulerID: 0,
                name: "The Eastlands",
                description: "The land of the east",
                allBaronies: new Dictionary<ulong, Barony_Data>()));

            return counties;
        }
    }
}