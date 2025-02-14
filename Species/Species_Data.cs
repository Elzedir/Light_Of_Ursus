using System.Collections.Generic;
using Pathfinding;

namespace Species
{
    public class Species_Data
    {
        public readonly SpeciesName SpeciesName;
        public readonly List<MoverType> MoverTypes;
        
        public Species_Data(SpeciesName speciesName, List<MoverType> moverTypes)
        {
            SpeciesName = speciesName;
            MoverTypes = moverTypes;
        }
    }
    
    public enum SpeciesName
    {
        None,
        
        Human,
        Demon,
        Orc
    }
}