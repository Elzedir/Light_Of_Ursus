using System.Collections.Generic;
using UnityEngine;

namespace Careers
{
    public class List_Career : MonoBehaviour
    {
        public static Dictionary<CareerName, Career_Master> GetAllDefaultCareers()
        {
            var allCareers = new Dictionary<CareerName, Career_Master>();

            // foreach (var none in _defaultNone())
            // {
            //     allRecipes.Add(none.Key, none.Value);
            // }
            
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

        static Dictionary<CareerName, Career_Master> _lumberjack()
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
                                taskName: TaskName.Chop_Trees,
                                taskDescription: "Chop trees",
                                taskAction: null
                            ),
                            new(
                                taskName: TaskName.Sell_Wood,
                                taskDescription: "Sell wood",
                                taskAction: null
                            )
                        }
                    )

                }
            };
        }

        static Dictionary<JobName, Job_Master> _smith()
        {
            return new Dictionary<JobName, Job_Master>()
            {
                {
                    JobName.Smith,
                    new Job_Master(
                        jobName: JobName.Smith,
                        jobDescription: "Smith something",
                        new List<Task_Master>
                        {
                            new(
                                taskName: TaskName.Beat_Iron,
                                taskDescription: "Beat iron",
                                taskAction: _smith
                            )
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
