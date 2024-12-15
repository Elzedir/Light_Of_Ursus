using System.Collections.Generic;
using Jobs;
using UnityEngine;

namespace Career
{
    public abstract class Career_List
    {
        public static Dictionary<uint, Career_Data> GetAllDefaultCareers()
        {
            var allCareers = new Dictionary<uint, Career_Data>();

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
        
        static Dictionary<uint, Career_Data> _wanderer()
        {
            return new Dictionary<uint, Career_Data>
            {
                {
                    (uint)CareerName.Wanderer, new Career_Data
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

        static Dictionary<uint, Career_Data> _lumberjack()
        {
            return new Dictionary<uint, Career_Data>
            {
                {
                    (uint)CareerName.Lumberjack, new Career_Data
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

        static Dictionary<uint, Career_Data> _smith()
        {
            return new Dictionary<uint, Career_Data>()
            {
                {
                    (uint)CareerName.Smith,
                    new Career_Data(
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
