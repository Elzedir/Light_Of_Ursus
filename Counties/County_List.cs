using System.Collections.Generic;
using Baronies;
using Tools;

namespace Counties
{
    public abstract class County_List
    {
        static Dictionary<ulong, County_Data> s_defaultCounties;
        public static Dictionary<ulong, County_Data> DefaultCounties => s_defaultCounties ??= _initialiseDefaultCounties();
        
        static Dictionary<ulong, County_Data> _initialiseDefaultCounties()
        {
            return new Dictionary<ulong, County_Data>
            {
                {
                    1, new County_Data(
                        id: 1,
                        rulerID: 0,
                        name: "The Westlands",
                        description: "The land of west",
                        allBaronies: new SerializableDictionary<ulong, Barony_Data>())
                },
                {
                    2, new County_Data(
                        id: 2,
                        rulerID: 0,
                        name: "The Northlands",
                        description: "The land of the north",
                        allBaronies: new SerializableDictionary<ulong, Barony_Data>())
                },
                {
                    3, new County_Data(
                        id: 3,
                        rulerID: 0,
                        name: "The Southlands",
                        description: "The land of the south",
                        allBaronies: new SerializableDictionary<ulong, Barony_Data>())
                },
                {
                    4, new County_Data(
                        id: 4,
                        rulerID: 0,
                        name: "The Eastlands",
                        description: "The land of the east",
                        allBaronies: new SerializableDictionary<ulong, Barony_Data>())
                },
            };
        }
    }
}