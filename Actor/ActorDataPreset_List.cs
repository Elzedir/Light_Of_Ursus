using System.Collections.Generic;
using Career;
using Jobs;
using Recipes;
using UnityEngine;

namespace Actor
{
    public abstract class ActorDataPreset_List
    {
        public static Dictionary<uint, Actor_Data> GetAllDefaultActorDataPresets()
        {
            var allActorDataPresets = new Dictionary<uint, Actor_Data>();

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
            
            foreach (var actorDataPreset in allActorDataPresets)
            {
                Debug.Log((ActorDataPresetName)actorDataPreset.Key);
            }

            return allActorDataPresets;
        }

        // Put a priority List in the tasks so you can check which tasks to do.
        
        static Dictionary<uint, Actor_Data> _wanderer()
        {
            return new Dictionary<uint, Actor_Data>
            {
                {
                    (uint)ActorDataPresetName.Wanderer_Journeyman, new Actor_Data
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

        static Dictionary<uint, Actor_Data> _lumberjack()
        {
            return new Dictionary<uint, Actor_Data>
            {
                {
                    (uint)ActorDataPresetName.Lumberjack_Journeyman, new Actor_Data
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

        static Dictionary<uint, Actor_Data> _smith()
        {
            return new Dictionary<uint, Actor_Data>
            {
                {
                    (uint)ActorDataPresetName.Smith_Journeyman, new Actor_Data
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
