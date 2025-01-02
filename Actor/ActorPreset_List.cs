using System.Collections.Generic;
using Careers;
using Jobs;
using Recipes;
using UnityEngine;

namespace Actor
{
    public abstract class ActorPreset_List
    {
        public static readonly Dictionary<uint, ActorPreset_Data> DefaultActorDataPresets = new()
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
            },
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
            },
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
