using System.Collections;
using System.Collections.Generic;
using Actor;
using UnityEngine;

namespace Jobs
{
    public abstract class Job_List
    {
        public static Dictionary<uint, Job_Data> GetAllDefaultJobs()
        {
            var allJobs = new Dictionary<uint, Job_Data>();

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

        static Dictionary<uint, Job_Data> _idle()
        {
            return new Dictionary<uint, Job_Data>
            {
                {
                    (uint)JobName.Idle, new Job_Data(
                        jobName: JobName.Idle,
                        jobDescription: "An idler",
                        jobTasks: new HashSet<JobTaskName>
                        {
                            JobTaskName.Idle,
                        })
                }
            };
        }

        static Dictionary<uint, Job_Data> _lumberjack()
        {
            return new Dictionary<uint, Job_Data>
            {
                {
                    (uint)JobName.Logger, new Job_Data(
                        jobName: JobName.Logger,
                        jobDescription: "A logger",
                        jobTasks: new HashSet<JobTaskName>
                        {
                            JobTaskName.Chop_Wood,
                            JobTaskName.Process_Logs,
                            JobTaskName.Fetch_Items,
                            JobTaskName.Deliver_Items
                        })
                },
                {
                    (uint)JobName.Sawyer, new Job_Data(
                        jobName: JobName.Sawyer,
                        jobDescription: "A sawyer",
                        jobTasks: new HashSet<JobTaskName>
                        {
                            JobTaskName.Process_Logs,
                            JobTaskName.Chop_Wood,
                            JobTaskName.Fetch_Items,
                            JobTaskName.Deliver_Items
                        })
                },
                {
                    (uint)JobName.Vendor, new Job_Data(
                        jobName: JobName.Vendor,
                        jobDescription: "A vendor",
                        jobTasks: new HashSet<JobTaskName>
                        {
                            JobTaskName.Stand_At_Counter,
                            JobTaskName.Restock_Shelves,
                            JobTaskName.Fetch_Items,
                            JobTaskName.Deliver_Items
                        })
                }
            };
        }

        static Dictionary<uint, Job_Data> _smith()
        {
            return new Dictionary<uint, Job_Data>
            {
                {
                    (uint)JobName.Smith, new Job_Data(
                        jobName: JobName.Smith,
                        jobDescription: "Smith something",
                        jobTasks: new HashSet<JobTaskName>
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
