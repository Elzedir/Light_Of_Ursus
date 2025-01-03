using System.Collections.Generic;
using Managers;

namespace JobSite
{
    public abstract class JobSite_List
    {
        static Dictionary<uint, JobSite_Data> _defaultJobSites;
        public static Dictionary<uint, JobSite_Data> DefaultJobSites => _defaultJobSites ??= _initialiseDefaultJobSites();
        
        static Dictionary<uint, JobSite_Data> _initialiseDefaultJobSites()
        {
            return new Dictionary<uint, JobSite_Data>
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
}