using System.Collections.Generic;
using Careers;
using Jobs;
using Recipe;

namespace Actor
{
    public abstract class ActorPreset_List
    {
        public static Dictionary<uint, ActorPreset_Data> GetAllDefaultActorDataPresets()
        {
            var allActorDataPresets = new Dictionary<uint, ActorPreset_Data>();

            foreach (var wanderer in _wanderer())
            {
                allActorDataPresets.Add(wanderer.Key, wanderer.Value);
            }

            foreach (var lumberjack in _lumberjack())
            {
                allActorDataPresets.Add(lumberjack.Key, lumberjack.Value);
            }

            foreach (var smith in _smith())
            {
                allActorDataPresets.Add(smith.Key, smith.Value);
            }

            return allActorDataPresets;
        }

        // Put a priority List in the tasks so you can check which tasks to do.
        
        static Dictionary<uint, ActorPreset_Data> _wanderer()
        {
            return new Dictionary<uint, ActorPreset_Data>
            {
                {
                    (uint)ActorDataPresetName.Wanderer_Journeyman, new ActorPreset_Data
                    (
                        actorDataPresetName: ActorDataPresetName.Wanderer_Journeyman,
                        careerData: new CareerData
                        (
                            actorID: 0,
                            careerName: CareerName.Wanderer,
                            jobsNotFromCareer: new HashSet<JobName>()
                        )
                    )
                }
            };
        }

        static Dictionary<uint, ActorPreset_Data> _lumberjack()
        {
            return new Dictionary<uint, ActorPreset_Data>
            {
                {
                    (uint)ActorDataPresetName.Lumberjack_Journeyman, new ActorPreset_Data
                    (
                        actorDataPresetName: ActorDataPresetName.Lumberjack_Journeyman,
                        careerData: new CareerData
                        (
                            actorID: 0,
                            careerName: CareerName.Lumberjack,
                            jobsNotFromCareer: new HashSet<JobName>()
                        ),

                        craftingData: new CraftingData
                        (
                            actorID: 0,
                            new List<RecipeName>
                            {
                                RecipeName.Log,
                                RecipeName.Plank
                            }
                        ),

                        vocationData: new VocationData
                        (
                            actorID: 0,
                            actorVocations: new Dictionary<VocationName, ActorVocation>
                            {
                                {
                                    VocationName.Logging,
                                    new ActorVocation(VocationName.Logging, 1000)    
                                },
                                {
                                    VocationName.Sawying,
                                    new ActorVocation(VocationName.Sawying, 1000)                                    
                                }
                            }
                        )
                    )
                }
            };
        }

        static Dictionary<uint, ActorPreset_Data> _smith()
        {
            return new Dictionary<uint, ActorPreset_Data>
            {
                {
                    (uint)ActorDataPresetName.Smith_Journeyman, new ActorPreset_Data
                    (
                        actorDataPresetName: ActorDataPresetName.Smith_Journeyman,
                        careerData: new CareerData
                        (
                            actorID: 0,
                            careerName: CareerName.Smith,
                            jobsNotFromCareer: new HashSet<JobName>()
                        ),

                        craftingData: new CraftingData
                        (
                            actorID: 0,
                            new List<RecipeName>
                            {
                                RecipeName.Iron_Ingot
                            }
                        ),

                        vocationData: new VocationData
                        (
                            actorID: 0,
                            actorVocations: new Dictionary<VocationName, ActorVocation>
                            {
                                {
                                    VocationName.Smithing,
                                    new ActorVocation(VocationName.Smithing, 1000)    
                                }
                            }
                        )
                    )
                }
            };
        }
    }
}
