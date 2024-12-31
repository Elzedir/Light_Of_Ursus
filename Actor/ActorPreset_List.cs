using System.Collections.Generic;
using Careers;
using Jobs;
using Recipes;
using UnityEngine;

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
                    (uint)ActorDataPresetName.Logger_Journeyman, new ActorPreset_Data
                    (
                        actorDataPresetName: ActorDataPresetName.Logger_Journeyman,
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
        
        public static ActorDataPresetName GetActorDataPresetNameByJobName(JobName jobName)
        {
            if (_actorDataPresetNameByJobName().TryGetValue(jobName, out var actorDataPresetName))
            {
                return actorDataPresetName;
            }
            
            Debug.LogError($"ActorDataPresetName not found for JobName: {jobName}");
            return ActorDataPresetName.No_Preset;
        }
            
        static Dictionary<JobName, ActorDataPresetName> _actorDataPresetNameByJobName()
        {
            // For now, Sawyer has Logger preset.
            
            return new Dictionary<JobName, ActorDataPresetName>
            {
                { JobName.Wanderer, ActorDataPresetName.Wanderer_Journeyman },
                { JobName.Logger, ActorDataPresetName.Logger_Journeyman },
                { JobName.Sawyer, ActorDataPresetName.Logger_Journeyman },
                { JobName.Smith, ActorDataPresetName.Smith_Journeyman }
            };
        }
    }
}
