using System.Collections.Generic;
using Actor;
using Careers;
using Recipe;
using UnityEngine;

namespace EmployeePosition
{
    public abstract class EmployeePosition_List
    {
        public static Dictionary<uint, EmployeePosition_Data> GetAllDefaultEmployeePositions()
        {
            var allEmployeePositions = new Dictionary<uint, EmployeePosition_Data>();

            foreach (var logger in _logger())
            {
                allEmployeePositions.Add(logger.Key, logger.Value);
            }

            foreach (var sawyer in _sawyer())
            {
                allEmployeePositions.Add(sawyer.Key, sawyer.Value);
            }

            foreach (var smith in _smith())
            {
                allEmployeePositions.Add(smith.Key, smith.Value);
            }

            return allEmployeePositions;
        }

        static Dictionary<uint, EmployeePosition_Data> _logger()
        {
            return new Dictionary<uint, EmployeePosition_Data>
            {
                {
                    (uint)EmployeePositionName.Logger, new EmployeePosition_Data(
                        employeePositionName: EmployeePositionName.Logger,
                        employeeDataPreset: ActorDataPresetName.Lumberjack_Journeyman,
                        requiredCareer: CareerName.Lumberjack,
                        requiredVocations: new Dictionary<VocationName, float>
                        {
                            { VocationName.Logging, 1000 }
                        },
                        requiredRecipes: new List<RecipeName> { RecipeName.Log })
                }
            };
        }

        static Dictionary<uint, EmployeePosition_Data> _sawyer()
        {
            return new Dictionary<uint, EmployeePosition_Data>
            {
                {
                    (uint)EmployeePositionName.Sawyer, new EmployeePosition_Data
                    (
                        employeePositionName: EmployeePositionName.Sawyer,
                        employeeDataPreset: ActorDataPresetName.Lumberjack_Journeyman,
                        requiredCareer: CareerName.Lumberjack,
                        requiredVocations: new Dictionary<VocationName, float>
                        {
                            { VocationName.Sawying, 1000 }
                        },
                        requiredRecipes: new List<RecipeName> { RecipeName.Plank }
                    )
                }
            };
        }

        static Dictionary<uint, EmployeePosition_Data> _smith()
        {
            return new Dictionary<uint, EmployeePosition_Data>
            {
                {
                    (uint)EmployeePositionName.Smith, new EmployeePosition_Data
                    (
                        employeePositionName: EmployeePositionName.Smith,
                        employeeDataPreset: ActorDataPresetName.Smith_Journeyman,
                        requiredCareer: CareerName.Smith,
                        requiredVocations: new Dictionary<VocationName, float>
                        {
                            { VocationName.Smithing, 1000 }
                        },
                        requiredRecipes: new List<RecipeName> { RecipeName.Iron_Ingot }
                    )
                }
            };
        }
    }
}