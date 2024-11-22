using System;
using System.Collections;
using System.Collections.Generic;
using Actors;
using Managers;
using UnityEngine;

namespace Lists
{
    public class List_Job
    {
        public static Dictionary<RecipeName, Recipe_Master> GetAllDefaultRecipes()
        {
            var allRecipes = new Dictionary<RecipeName, Recipe_Master>();

            foreach (var none in _defaultNone())
            {
                allRecipes.Add(none.Key, none.Value);
            }
            
            foreach (var lumberjack in _lumberjack())
            {
                allRecipes.Add(lumberjack.Key, lumberjack.Value);
            }

            foreach (var smith in _smith())
            {
                allRecipes.Add(smith.Key, smith.Value);
            }
            
            return allRecipes;
        }
        
        // Put a priority List in the tasks so you can check which tasks to do.

        static Dictionary<JobName, Job_Master> _lumberjack()
        {
            return new Dictionary<JobName, Job_Master>
            {
                {
                    JobName.Lumberjack, new Job_Master
                    (
                        jobName: JobName.Lumberjack,
                        jobDescription: "Lumberjack",
                        new List<Task_Master>
                        {
                            new(
                                taskName: JobTaskName.Chop_Trees,
                                taskDescription: "Chop trees",
                                jobName: JobName.Lumberjack,
                                taskAnimationClips: null,
                                taskAction: null
                            ),
                            new(
                                taskName: JobTaskName.Process_Trees,
                                taskDescription: "Process logs into wood",
                                jobName: JobName.Lumberjack,
                                taskAnimationClips: null,
                                taskAction: null
                            ),
                            new(
                                taskName: JobTaskName.Drop_Off_Wood,
                                taskDescription: "Drop wood in woodpile",
                                jobName: JobName.Lumberjack,
                                taskAnimationClips: null,
                                taskAction: null
                            ),
                            new(
                                taskName: JobTaskName.Sell_Wood,
                                taskDescription: "Sell wood",
                                jobName: JobName.Lumberjack,
                                taskAnimationClips: null,
                                taskAction: null
                            )
                        }
                    )

                }
            };
        }

        Job_Master _smith()
        {
            IEnumerator smith(ActorComponent actor, int jobsite)
            {
                if (actor == null) throw new ArgumentException("Actor is null;");

                yield return null;
            }

            return new Job_Master(
                jobName: JobName.Smith,
                jobDescription: "Smith something",
                new List<Task_Master>
                {
                    new Task_Master(
                        taskName: JobTaskName.Beat_Iron,
                        taskDescription: "Beat iron",
                        jobName: JobName.Smith,
                        taskAnimationClips: null,
                        taskAction: smith
                    )
                });
        }
    }
}
