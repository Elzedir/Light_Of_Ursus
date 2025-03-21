using System.Collections.Generic;
using Jobs;
using Managers;
using Priorities;
using Station;

namespace JobSites
{
    public abstract class JobSite_List
    {
        static Dictionary<ulong, JobSite_Data> s_defaultJobSites;
        public static Dictionary<ulong, JobSite_Data> S_DefaultJobSites => s_defaultJobSites ??= _initialiseDefaultJobSites();
        
        static Dictionary<ulong, JobSite_Data> _initialiseDefaultJobSites()
        {
            return new Dictionary<ulong, JobSite_Data>
            {
                {
                    //* Find another way to initialise Jobs. 
                    1, new JobSite_Data(
                        jobSiteID: 1,
                        jobSiteFactionID: 0,
                        cityID: 1,
                        ownerID: 0,
                        jobSiteName: JobSiteName.Lumber_Yard,
                        productionData: new ProductionData(1),
                        prosperityData: new ProsperityData(
                            currentProsperity: 50,
                            maxProsperity: 100,
                            baseProsperityGrowthPerDay: 1),
                        priorityData: new Priority_Data_JobSite(1))
                }
            };
        }
    }
}