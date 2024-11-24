using System.Collections;
using System.Collections.Generic;
using Actors;
using UnityEngine;

namespace Jobs
{
    public abstract class List_Job
    {
        public static Dictionary<uint, Job_Master> GetAllDefaultJobs()
        {
            var allJobs = new Dictionary<uint, Job_Master>();

            // foreach (var none in _defaultNone())
            // {
            //     allRecipes.Add(none.Key, none.Value);
            // }
            
            foreach (var lumberjack in _lumberjack())
            {
                allJobs.Add(lumberjack.Key, lumberjack.Value);
            }

            foreach (var smith in _smith())
            {
                allJobs.Add(smith.Key, smith.Value);
            }
            
            return allJobs;
        }
        
        // Put a priority List in the tasks so you can check which tasks to do.

        static Dictionary<uint, Job_Master> _lumberjack()
        {
            return new Dictionary<uint, Job_Master>
            {
                {
                    (uint)JobName.Lumberjack, new Job_Master(
                        jobName: JobName.Lumberjack,
                        jobDescription: "Lumberjack",
                        new Dictionary<TaskName, Task_Master>
                        {
                            {
                                TaskName.Chop_Trees, new Task_Master(
                                    taskName: TaskName.Chop_Trees,
                                    taskDescription: "Chop trees",
                                    taskAction: null
                                )
                            },

                            {
                                TaskName.Chop_Trees, new Task_Master(
                                    taskName: TaskName.Sell_Wood,
                                    taskDescription: "Sell wood",
                                    taskAction: null
                                )
                            }
                        })
                }
            };
        }

        static Dictionary<uint, Job_Master> _smith()
        {
            return new Dictionary<uint, Job_Master>()
            {
                {
                    (uint)JobName.Smith, new Job_Master(
                        jobName: JobName.Smith,
                        jobDescription: "Smith something",
                        new Dictionary<TaskName, Task_Master>()
                        {
                            {
                                TaskName.Beat_Iron, new Task_Master(
                                    taskName: TaskName.Beat_Iron,
                                    taskDescription: "Beat iron",
                                    taskAction: _smith)
                            }

                        })
                }
            };
        }


        static IEnumerator _smith(ActorComponent actor, int jobsite)
        {
            if (actor is null)
            {
                Debug.LogWarning("Actor is null");
                yield break;
            }

            yield return null;
        }
    }
}
