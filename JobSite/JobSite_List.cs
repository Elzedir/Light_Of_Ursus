using System.Collections.Generic;
using Managers;

namespace JobSite
{
    public abstract class JobSite_List
    {
        static Dictionary<ulong, JobSite_Data> _defaultJobSites;
        public static Dictionary<ulong, JobSite_Data> DefaultJobSites => _defaultJobSites ??= _initialiseDefaultJobSites();
        
        static Dictionary<ulong, JobSite_Data> _initialiseDefaultJobSites()
        {
            return new Dictionary<ulong, JobSite_Data>
            {
                {
                    1, new JobSite_Data(
                        jobSiteID: 1,
                        jobSiteName: JobSiteName.Lumber_Yard,
                        jobSiteFactionID: 0,
                        cityID: 1,
                        jobSiteDescription: "JobSite 1 Description",
                        ownerID: 0,
                        allStationIDs: new List<ulong>
                        {
                            1, 2, 3
                        },
                        allEmployeeIDs: new List<ulong>(),
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