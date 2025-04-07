using System.Collections.Generic;

namespace Counties
{
    public abstract class County_PreExisting
    {
        static Dictionary<ulong, County_Data> s_defaultCounties;
        public static Dictionary<ulong, County_Data> DefaultCounties => s_defaultCounties ??= _initialiseDefaultCounties();
        
        static Dictionary<ulong, County_Data> _initialiseDefaultCounties()
        {
            return new Dictionary<ulong, County_Data>
            {
                {
                    1, new County_Data(
                        regionID: 1,
                        regionName: "The Heartlands",
                        regionDescription: "The land of hearts",
                        regionFactionID: 0,
                        allCityIDs: new List<ulong>
                        {
                            1
                        })
                }
            };
        }
    }
}