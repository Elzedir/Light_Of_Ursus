using System.Collections.Generic;
using Managers;

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
                    ownerID: 0,
                    allStationIDs: new List<uint>
                    {
                        1, 2, 3
                    },
                    allEmployeeIDs: new List<uint>(),
                    prosperityData: new ProsperityData(
                        currentProsperity: 50,
                        maxProsperity: 100,
                        baseProsperityGrowthPerDay: 1
                        ))
            }
        };
    }
}