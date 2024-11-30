using System.Collections.Generic;
using Jobs;
using UnityEngine;

namespace Careers
{
    public class Career_List : MonoBehaviour
    {
        public static Dictionary<uint, Career_Master> GetAllDefaultCareers()
        {
            var allCareers = new Dictionary<uint, Career_Master>();

            foreach (var wanderer in _wanderer())
            {
                allCareers.Add(wanderer.Key, wanderer.Value);
            }
            
            foreach (var lumberjack in _lumberjack())
            {
                allCareers.Add(lumberjack.Key, lumberjack.Value);
            }

            foreach (var smith in _smith())
            {
                allCareers.Add(smith.Key, smith.Value);
            }
            
            return allCareers;
        }
        
        // Put a priority List in the tasks so you can check which tasks to do.
        
        static Dictionary<uint, Career_Master> _wanderer()
        {
            return new Dictionary<uint, Career_Master>
            {
                {
                    (uint)CareerName.Wanderer, new Career_Master
                    (
                        careerName: CareerName.Wanderer,
                        careerDescription: "A wanderer",
                        new HashSet<JobName>()
                        {
                            JobName.Wanderer
                        }
                    )

                }
            };
        }

        static Dictionary<uint, Career_Master> _lumberjack()
        {
            return new Dictionary<uint, Career_Master>
            {
                {
                    (uint)CareerName.Lumberjack, new Career_Master
                    (
                        careerName: CareerName.Lumberjack,
                        careerDescription: "A lumberjack",
                        new HashSet<JobName>
                        {
                            JobName.Logger,
                            JobName.Sawmiller,
                            JobName.Hauler,
                            JobName.Vendor,
                        }
                    )

                }
            };
        }

        static Dictionary<uint, Career_Master> _smith()
        {
            return new Dictionary<uint, Career_Master>()
            {
                {
                    (uint)CareerName.Smith,
                    new Career_Master(
                        careerName: CareerName.Smith,
                        careerDescription: "A smith",
                        new HashSet<JobName>
                        {
                            JobName.Smith
                        })
                }
            };
        }
    }
}
