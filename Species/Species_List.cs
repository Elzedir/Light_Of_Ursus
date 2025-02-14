using System.Collections.Generic;
using Pathfinding;

namespace Species
{
    public abstract class Species_List
    {
        static Dictionary<ulong, Species_Data> s_defaultSpeciesData;
        public static Dictionary<ulong, Species_Data> DefaultSpeciesData => s_defaultSpeciesData ??= _getDefaultSpeciesData();

        public static Species_Data GetSpeciesData(SpeciesName speciesName)
        {
            if (DefaultSpeciesData.TryGetValue((ulong)speciesName, out var speciesData)) return speciesData;
            
            throw new KeyNotFoundException($"DefaultSpecies: {speciesName} ({(ulong)speciesName}) not found.");
        }
        
        static Dictionary<ulong, Species_Data> _getDefaultSpeciesData()
        {
            return new Dictionary<ulong, Species_Data>
            {
                {
                    (ulong)SpeciesName.Human, new Species_Data(
                        speciesName: SpeciesName.Human,
                        moverTypes: new List<MoverType>
                        {
                            MoverType.Land
                        }
                    )
                }
            };
        }
    }
}