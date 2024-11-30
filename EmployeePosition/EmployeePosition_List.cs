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
        
        static Dictionary<uint, EmployeePosition_Master> _logger()
        {
            return new Dictionary<uint, EmployeePosition_Master>
            {
                {
                    (uint)EmployeePositionName.Logger, new EmployeePosition_Master
                    (
                        EmployeePositionName.Logger,
                        new ActorGenerationParameters_Master(
                            
                            )
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
                        EmployeePositionName.Sawyer, new Actor_Data(
                            careerName: CareerName.Lumberjack,
                            initialRecipes: new List<RecipeName>(
                                new[] { RecipeName.Plank }),
                            initialVocations: new List<ActorVocation>(
                                new[] { new ActorVocation(VocationName.Sawying, 1000) })
                        )
                    )

                }
            };
        }

        static Dictionary<uint, EmployeePosition_Master> _smith()
        {
            return new Dictionary<uint, EmployeePosition_Master>()
            {
                {
                    (uint)EmployeePositionName.Smith, new EmployeePosition_Master
                    (EmployeePositionName.Smith, new ActorGenerationParameters_Master(
                            careerName: CareerName.Smith,
                            initialRecipes: new List<RecipeName>(
                                new[] { RecipeName.None }),
                            initialVocations: new List<ActorVocation>(
                                new[] { new ActorVocation(VocationName.Smithing, 1000) })
                        )
                    )
                }
            };
        }
    }
}
