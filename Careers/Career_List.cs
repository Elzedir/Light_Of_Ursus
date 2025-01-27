using System.Collections.Generic;
using Jobs;

namespace Careers
{
    public abstract class Career_List
    {
        static        Dictionary<ulong, Career_Data> _defaultCareers;
        public static Dictionary<ulong, Career_Data> DefaultCareers => _defaultCareers ??= _initialiseDefaultCareers();

        static Dictionary<ulong, Career_Data> _initialiseDefaultCareers()
        {
            return new Dictionary<ulong, Career_Data>
            {
                {
                    (ulong)CareerName.Wanderer, new Career_Data
                    (
                        careerName: CareerName.Wanderer,
                        careerDescription: "A wanderer",
                        careerBaseJobs: JobsByCareer[CareerName.Wanderer],
                        careerSpecialistJobs: new Dictionary<JobName, JobRequirement>())
                },

                {
                    (ulong)CareerName.Lumberjack, new Career_Data
                    (
                        careerName: CareerName.Lumberjack,
                        careerDescription: "A lumberjack",
                        careerBaseJobs: JobsByCareer[CareerName.Lumberjack],
                        careerSpecialistJobs: new Dictionary<JobName, JobRequirement>())
                },

                {
                    (ulong)CareerName.Smith,
                    new Career_Data(
                        careerName: CareerName.Smith,
                        careerDescription: "A smith",
                        careerBaseJobs: JobsByCareer[CareerName.Smith],
                        careerSpecialistJobs: new Dictionary<JobName, JobRequirement>())
                }
            };
        }

        static        Dictionary<CareerName, List<JobName>> _jobsByCareer;
        public static Dictionary<CareerName, List<JobName>> JobsByCareer => _jobsByCareer ??= _initialiseJobsByCareer();

        static Dictionary<CareerName, List<JobName>> _initialiseJobsByCareer()
        {
            return new Dictionary<CareerName, List<JobName>>
            {
                {
                    CareerName.Wanderer, new List<JobName>
                    {
                        JobName.Wanderer
                    }
                },
                {
                    CareerName.Lumberjack, new List<JobName>
                    {
                        JobName.Logger,
                        JobName.Sawyer,
                        JobName.Vendor
                    }
                },
                {
                    CareerName.Smith, new List<JobName>
                    {
                        JobName.Smith
                    }
                }
            };
        }
    }
}