using System.Collections.Generic;

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
                        jobTasks: new HashSet<JobTaskName>
                        {
                            JobTaskName.Idle,
                        })
                },
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
                },
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
    }
}
