using System.Collections.Generic;
using Actor;
using Actors;
using Careers;
using Jobs;
using Recipes;

namespace ActorPresets
{
    public abstract class ActorPreset_List
    {
        static Dictionary<ulong, ActorPreset_Data> s_defaultActorDataPresets;
        public static Dictionary<ulong, ActorPreset_Data> S_DefaultActorDataPresets => s_defaultActorDataPresets ??= _initialiseDefaultActorDataPresets();
        
        static Dictionary<ulong, ActorPreset_Data> _initialiseDefaultActorDataPresets()
        {
            return new Dictionary<ulong, ActorPreset_Data>
            {
                {
                    (ulong)ActorDataPresetName.Wanderer_Beginner, new ActorPreset_Data
                    (
                        actorDataPresetName: ActorDataPresetName.Wanderer_Beginner,
                        actorDataCareer: new Actor_Data_Career
                        (
                            actorID: 0,
                            careerName: CareerName.Wanderer,
                            jobSiteID: 0
                        )
                    )
                },
                {
                    (ulong)ActorDataPresetName.Wanderer_Journeyman, new ActorPreset_Data
                    (
                        actorDataPresetName: ActorDataPresetName.Wanderer_Journeyman,
                        actorDataCareer: new Actor_Data_Career
                        (
                            actorID: 0,
                            careerName: CareerName.Wanderer,
                            jobSiteID: 0
                        )
                    )
                },
                {
                    (ulong)ActorDataPresetName.Logger_Journeyman, new ActorPreset_Data
                    (
                        actorDataPresetName: ActorDataPresetName.Logger_Journeyman,
                        actorDataCareer: new Actor_Data_Career
                        (
                            actorID: 0,
                            careerName: CareerName.Lumberjack,
                            jobSiteID: 0
                        ),

                        actorDataCrafting: new Actor_Data_Crafting
                        (
                            actorID: 0,
                            new List<RecipeName>
                            {
                                RecipeName.Log,
                                RecipeName.Plank
                            }
                        ),

                        actorDataVocation: new Actor_Data_Vocation
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
                    (ulong)ActorDataPresetName.Smith_Journeyman, new ActorPreset_Data
                    (
                        actorDataPresetName: ActorDataPresetName.Smith_Journeyman,
                        actorDataCareer: new Actor_Data_Career
                        (
                            actorID: 0,
                            careerName: CareerName.Smith,
                            jobSiteID: 0
                        ),

                        actorDataCrafting: new Actor_Data_Crafting
                        (
                            actorID: 0,
                            new List<RecipeName>
                            {
                                RecipeName.Iron_Ingot
                            }
                        ),

                        actorDataVocation: new Actor_Data_Vocation
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
        
        static Dictionary<JobName, ActorDataPresetName> s_actorDataPresetNameByJobName;
        public static Dictionary<JobName, ActorDataPresetName> S_ActorDataPresetNameByJobName => s_actorDataPresetNameByJobName ??= _initialiseActorDataPresetNameByJobName();
            
        static Dictionary<JobName, ActorDataPresetName> _initialiseActorDataPresetNameByJobName()
        {
            // For now, Sawyer has Logger preset.
            
            return new Dictionary<JobName, ActorDataPresetName>
            {
                { JobName.Wanderer, ActorDataPresetName.Wanderer_Journeyman },
                { JobName.Logger, ActorDataPresetName.Logger_Journeyman },
                { JobName.Sawyer, ActorDataPresetName.Logger_Journeyman },
                { JobName.Smith, ActorDataPresetName.Smith_Journeyman },
                { JobName.Hauler, ActorDataPresetName.Wanderer_Beginner },
                { JobName.Idle, ActorDataPresetName.Wanderer_Beginner }
            };
        }
    }
}
