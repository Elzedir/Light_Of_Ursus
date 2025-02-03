using System.Collections.Generic;
using Actor;
using ActorActions;

namespace Jobs
{
    public abstract class Job_List
    {
        static Dictionary<ulong, Job_Data> _defaultJobs;
        public static Dictionary<ulong, Job_Data> DefaultJobs => _defaultJobs ??= _initialiseDefaultJobs();
        
        static Dictionary<ulong, Job_Data> _initialiseDefaultJobs()
        {
            return new Dictionary<ulong, Job_Data>
            {
                {
                    (ulong)JobName.Idle, new Job_Data(
                        jobName: JobName.Idle,
                        jobDescription: "An idler",
                        jobActions: new List<ActorActionName>
                        {
                            ActorActionName.Idle,
                        })
                },
                {
                    (ulong)JobName.Logger, new Job_Data(
                        jobName: JobName.Logger,
                        jobDescription: "A logger",
                        jobActions: new List<ActorActionName>
                        {
                            ActorActionName.Chop_Wood,
                            ActorActionName.Haul_Fetch,
                            ActorActionName.Haul_Deliver
                        })
                },
                {
                    (ulong)JobName.Sawyer, new Job_Data(
                        jobName: JobName.Sawyer,
                        jobDescription: "A sawyer",
                        jobActions: new List<ActorActionName>
                        {
                            ActorActionName.Process_Logs,
                            ActorActionName.Haul_Fetch,
                            ActorActionName.Haul_Deliver
                        })
                },
                {
                    (ulong)JobName.Vendor, new Job_Data(
                        jobName: JobName.Vendor,
                        jobDescription: "A vendor",
                        jobActions: new List<ActorActionName>
                        {
                            ActorActionName.Stand_At_Counter,
                            ActorActionName.Restock_Shelves,
                            ActorActionName.Haul_Fetch,
                            ActorActionName.Haul_Deliver
                        })
                },
                {
                    (ulong)JobName.Smith, new Job_Data(
                        jobName: JobName.Smith,
                        jobDescription: "Smith something",
                        jobActions: new List<ActorActionName>
                        {
                            ActorActionName.Beat_Metal
                        })
                }
            };
        }
    }
}
