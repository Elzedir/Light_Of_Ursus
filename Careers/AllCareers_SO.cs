using System.Collections.Generic;
using ScriptableObjects;

namespace Careers
{
    public class AllCareers_SO : Base_SO<Career_Master>
    {
        public Career_Master[] Careers                           => Objects;
        public Career_Master   GetCareer_Master(CareerName careerName) => GetObject_Master((uint)careerName);

        public override uint GetObjectID(int id) => (uint)Careers[id].CareerName;

        public override Dictionary<uint, Career_Master> PopulateDefaultObjects()
        {
            var defaultJobs = new Dictionary<uint, Career_Master>();

            foreach (var item in List_Career.GetAllDefaultCareers())
            {
                defaultJobs.Add(item.Key, item.Value);
            }

            return defaultJobs;
        }
        
        public Dictionary<uint, Career_Master> DefaultJobs => DefaultObjects;
    }
}
