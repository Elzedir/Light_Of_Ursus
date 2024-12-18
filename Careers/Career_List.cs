using System.Collections.Generic;
using Jobs;

namespace Careers
{
    public abstract class Career_List
    {
        // Put a priority List in the tasks so you can check which tasks to do.
        
        public static readonly Dictionary<uint, Career_Data> DefaultCareers = new()
        {
            {
                (uint)CareerName.Wanderer, new Career_Data
                (
                    careerName: CareerName.Wanderer,
                    careerDescription: "A wanderer",
                    careerBaseJobs: new HashSet<JobName>()
                    {
                        JobName.Wanderer
                    },
                    careerSpecialistJobs: new Dictionary<JobName, JobRequirement>())
            },

            {
                (uint)CareerName.Lumberjack, new Career_Data
                (
                    careerName: CareerName.Lumberjack,
                    careerDescription: "A lumberjack",
                    careerBaseJobs: new HashSet<JobName>
                    {
                        JobName.Logger,
                        JobName.Sawmiller,
                        JobName.Hauler,
                        JobName.Vendor,
                    },
                    careerSpecialistJobs: new Dictionary<JobName, JobRequirement>())
            },

            {
                (uint)CareerName.Smith,
                new Career_Data(
                    careerName: CareerName.Smith,
                    careerDescription: "A smith",
                    careerBaseJobs: new HashSet<JobName>
                    {
                        JobName.Smith
                    },
                    careerSpecialistJobs: new Dictionary<JobName, JobRequirement>())
            }
        };
    }
}
