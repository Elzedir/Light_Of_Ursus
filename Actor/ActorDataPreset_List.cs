using System.Collections.Generic;
using Career;
using Jobs;
using Recipes;

namespace Actor
{
    public class ActorDataPreset_List
    {
        public static Dictionary<uint, Actor_Data> GetAllDefaultActorDataPresets()
        {
            var allCareers = new Dictionary<uint, Actor_Data>();

            foreach (var wanderer in _wanderer())
            {
                allCareers.Add(wanderer.Key, wanderer.Value);
            }

            foreach (var lumberjack in _lumberjack())
            {
                allCareers.Add(lumberjack.Key, lumberjack.Value);
            }

            foreach (var smith in _smith())
            {
                allCareers.Add(smith.Key, smith.Value);
            }

            return allCareers;
        }

        // Put a priority List in the tasks so you can check which tasks to do.

        static Dictionary<uint, Actor_Data> _wanderer()
        {
            return new Dictionary<uint, Actor_Data>
            {
                {
                    (uint)ActorDataPresetName.Wanderer_Journeyman, new Actor_Data
                    (
                        careerName: CareerName.Wanderer,
                        careerDescription: "A wanderer",
                        new HashSet<JobName>()
                        {
                            JobName.Wanderer
                        }
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
                        careerData: new CareerData
                        (
                            careerName: CareerName.Lumberjack,
                            allJobs: new HashSet<JobName>()
                            // Change so that you only add jobs that won't be added by the career default.
                        ),

                        craftingData: new CraftingData
                        (
                            new List<RecipeName>
                            {
                                RecipeName.Log,
                                RecipeName.Plank
                            }
                        ),

                        vocationData: new VocationData
                        (
                            new List<ActorVocation>
                            {
                                new(VocationName.Logging, 1000)
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
                    (uint)CareerName.Smith,
                    new Career_Master(
                        careerName: CareerName.Smith,
                        careerDescription: "A smith",
                        new HashSet<JobName>
                        {
                            JobName.Smith
                        })
                }
            };
        }
    }
}
