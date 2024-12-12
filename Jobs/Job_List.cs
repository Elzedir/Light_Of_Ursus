using System.Collections;
using System.Collections.Generic;
using Actor;
using UnityEngine;

namespace Jobs
{
    public abstract class Job_List
    {
        public static Dictionary<uint, Job_Master> GetAllDefaultJobs()
        {
            var allJobs = new Dictionary<uint, Job_Master>();

            foreach (var idle in _idle())
            {
                allJobs.Add(idle.Key, idle.Value);
            }
            
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

        static Dictionary<uint, Job_Master> _idle()
        {
            return new Dictionary<uint, Job_Master>
            {
                {
                    (uint)JobName.Idle, new Job_Master(
                        jobName: JobName.Idle,
                        jobDescription: "An idler",
                        new HashSet<JobTaskName>
                        {
                            JobTaskName.Idle,
                        })
                }
            };
        }

        static Dictionary<uint, Job_Master> _lumberjack()
        {
            return new Dictionary<uint, Job_Master>
            {
                {
                    (uint)JobName.Logger, new Job_Master(
                        jobName: JobName.Logger,
                        jobDescription: "A logger",
                        new HashSet<JobTaskName>
                        {
                            JobTaskName.Chop_Wood,
                            JobTaskName.Process_Logs,
                            JobTaskName.Drop_Off_Wood
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
                        new HashSet<JobTaskName>
                        {
                            JobTaskName.Beat_Metal
                        })
                }
            };
        }


        static IEnumerator _smith(Actor_Component actor, int jobsite)
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
