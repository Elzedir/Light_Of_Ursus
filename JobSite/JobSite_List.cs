using System.Collections.Generic;

namespace JobSite
{
    public abstract class JobSite_List
    {
        public static readonly Dictionary<uint, JobSite_Data> DefaultJobSites = new()
        {
            {
                1, new JobSite_Data(
                    jobSiteID: 1,
                    jobSiteName: JobSiteName.Lumber_Yard,
                    jobsiteFactionID: 0,
                    cityID: 1,
                    jobsiteDescription: "JobSite 1 Description",
                    ownerID: 0)
            }
        };
    }
}