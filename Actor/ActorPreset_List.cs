using System.Collections.Generic;
using Careers;
using Jobs;
using Recipes;

namespace Actor
{
    public abstract class ActorPreset_List
    {
        static Dictionary<uint, ActorPreset_Data> _defaultActorDataPresets;
        public static Dictionary<uint, ActorPreset_Data> DefaultActorDataPresets => _defaultActorDataPresets ??= _initialiseDefaultActorDataPresets();
        
        static Dictionary<uint, ActorPreset_Data> _initialiseDefaultActorDataPresets()
        {
            return new Dictionary<uint, ActorPreset_Data>
            {
                {
                    (uint)ActorDataPresetName.Wanderer_Journeyman, new ActorPreset_Data
                    (
                        actorDataPresetName: ActorDataPresetName.Wanderer_Journeyman,
                        careerDataPreset: new Career_Data_Preset
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
                        careerDataPreset: new Career_Data_Preset
                        (
                            actorID: 0,
                            careerName: CareerName.Lumberjack,
                            jobsNotFromCareer: new HashSet<JobName>()
                        ),

                        craftingDataPreset: new Crafting_Data_Preset
                        (
                            actorID: 0,
                            new List<RecipeName>
                            {
                                RecipeName.Log,
                                RecipeName.Plank
                            }
                        ),

                        vocationDataPreset: new Vocation_Data_Preset
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
                        careerDataPreset: new Career_Data_Preset
                        (
                            actorID: 0,
                            careerName: CareerName.Smith,
                            jobsNotFromCareer: new HashSet<JobName>()
                        ),

                        craftingDataPreset: new Crafting_Data_Preset
                        (
                            actorID: 0,
                            new List<RecipeName>
                            {
                                RecipeName.Iron_Ingot
                            }
                        ),

                        vocationDataPreset: new Vocation_Data_Preset
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
        
        static Dictionary<JobName, ActorDataPresetName> _actorDataPresetNameByJobName;
        public static Dictionary<JobName, ActorDataPresetName> ActorDataPresetNameByJobName => _actorDataPresetNameByJobName ??= _initialiseActorDataPresetNameByJobName();
            
        static Dictionary<JobName, ActorDataPresetName> _initialiseActorDataPresetNameByJobName()
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
