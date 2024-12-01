using System.Collections.Generic;
using Actor;
using Career;
using Recipes;
using UnityEngine;

namespace EmployeePosition
{
    public class EmployeePosition_List : MonoBehaviour
    {
        public static Dictionary<uint, EmployeePosition_Master> GetAllDefaultEmployeePositions()
        {
            var allEmployeePositions = new Dictionary<uint, EmployeePosition_Master>();

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

        public static EmployeePosition_Requirements GetEmployeeMinimumRequirements(EmployeePositionName employeePositionName)
        {
            if (_employeeMinimumRequirements.TryGetValue(employeePositionName, out var employeeMinimumRequirements))
            {
                return employeeMinimumRequirements;
            }

            Debug.LogError($"Employee Position {employeePositionName} not found in EmployeePosition_List");
            return null;
        }

        static Dictionary<uint, EmployeePosition_Master> _logger()
        {
            return new Dictionary<uint, EmployeePosition_Master>
            {
                {
                    (uint)EmployeePositionName.Logger, new EmployeePosition_Master(
                        EmployeePositionName.Logger,
                        ActorDataPresetName.Lumberjack_Journeyman
                    )
                }
            };
        }

        static Dictionary<uint, EmployeePosition_Master> _sawyer()
        {
            return new Dictionary<uint, EmployeePosition_Master>
            {
                {
                    (uint)EmployeePositionName.Sawyer, new EmployeePosition_Master
                    (
                        EmployeePositionName.Sawyer,
                        ActorDataPresetName.Lumberjack_Journeyman

                    )
                }
            };
        }

        static Dictionary<uint, EmployeePosition_Master> _smith()
        {
            return new Dictionary<uint, EmployeePosition_Master>
            {
                {
                    (uint)EmployeePositionName.Smith, new EmployeePosition_Master
                    (
                        EmployeePositionName.Smith,
                        ActorDataPresetName.Smith_Journeyman
                    )
                }
            };
        }

        static readonly Dictionary<EmployeePositionName, EmployeePosition_Requirements> _employeeMinimumRequirements =
            new()
            {
                {
                    EmployeePositionName.Logger, new EmployeePosition_Requirements(
                        EmployeePositionName.Logger,
                        new CareerRequirements(CareerName.Lumberjack),
                        new CraftingRequirements(new List<RecipeName> { RecipeName.Log }),
                        new VocationRequirements(new Dictionary<VocationName, float>
                        {
                            { VocationName.Logging, 1000 }
                        })
                    )
                },
                {
                    EmployeePositionName.Sawyer, new EmployeePosition_Requirements(
                        EmployeePositionName.Sawyer,
                        new CareerRequirements(CareerName.Lumberjack),
                        new CraftingRequirements(new List<RecipeName> { RecipeName.Plank }),
                        new VocationRequirements(new Dictionary<VocationName, float>
                        {
                            { VocationName.Sawying, 1000 }
                        })
                    )
                },
                {
                    EmployeePositionName.Smith, new EmployeePosition_Requirements(
                        EmployeePositionName.Smith,
                        new CareerRequirements(CareerName.Smith),
                        new CraftingRequirements(new List<RecipeName> { RecipeName.Iron_Ingot }),
                        new VocationRequirements(new Dictionary<VocationName, float>
                        {
                            { VocationName.Smithing, 1000 }
                        })
                    )
                }
            };
    }
}