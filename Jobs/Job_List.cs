using System.Collections.Generic;
using Actor;
using ActorActions;

namespace Jobs
{
    public abstract class Job_List
    {
        static Dictionary<uint, Job_Data> _defaultJobs;
        public static Dictionary<uint, Job_Data> DefaultJobs => _defaultJobs ??= _initialiseDefaultJobs();
        
        static Dictionary<uint, Job_Data> _initialiseDefaultJobs()
        {
            return new Dictionary<uint, Job_Data>
            {
                {
                    (uint)JobName.Idle, new Job_Data(
                        jobName: JobName.Idle,
                        jobDescription: "An idler",
                        jobActions: new HashSet<ActorActionName>
                        {
                            ActorActionName.Idle,
                        })
                },
                {
                    (uint)JobName.Logger, new Job_Data(
                        jobName: JobName.Logger,
                        jobDescription: "A logger",
                        jobActions: new HashSet<ActorActionName>
                        {
                            ActorActionName.Chop_Wood,
                            ActorActionName.Process_Logs,
                            ActorActionName.Fetch_Items,
                            ActorActionName.Deliver_Items
                        })
                },
                {
                    (uint)JobName.Sawyer, new Job_Data(
                        jobName: JobName.Sawyer,
                        jobDescription: "A sawyer",
                        jobActions: new HashSet<ActorActionName>
                        {
                            ActorActionName.Process_Logs,
                            ActorActionName.Chop_Wood,
                            ActorActionName.Fetch_Items,
                            ActorActionName.Deliver_Items
                        })
                },
                {
                    (uint)JobName.Vendor, new Job_Data(
                        jobName: JobName.Vendor,
                        jobDescription: "A vendor",
                        jobActions: new HashSet<ActorActionName>
                        {
                            ActorActionName.Stand_At_Counter,
                            ActorActionName.Restock_Shelves,
                            ActorActionName.Fetch_Items,
                            ActorActionName.Deliver_Items
                        })
                },
                {
                    (uint)JobName.Smith, new Job_Data(
                        jobName: JobName.Smith,
                        jobDescription: "Smith something",
                        jobActions: new HashSet<ActorActionName>
                        {
                            ActorActionName.Beat_Metal
                        })
                }
            };
        }
    }
}
