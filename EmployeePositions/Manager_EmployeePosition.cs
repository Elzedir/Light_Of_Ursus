using System;
using Actors;
using UnityEditor;
using UnityEngine;

namespace EmployeePositions
{
    public abstract class Manager_EmployeePosition : MonoBehaviour
    {
        const string _allEmployeePositionsSOPath = "ScriptableObjects/AllEmployeePositions_SO";

        static AllEmployeePositions_SO _allEmployeePositions;

        static AllEmployeePositions_SO AllEmployeePositions =>
            _allEmployeePositions ??= _getOrCreateAllEmployeePositionsSO();

        public static EmployeePosition_Master GetEmployeePosition_Master(EmployeePositionName employeePositionName) =>
            AllEmployeePositions.GetEmployeePosition_Master(employeePositionName);

        public static void PopulateAllEmployeePositions()
        {
            AllEmployeePositions.PopulateDefaultEmployeePositions();
            // Then populate custom EmployeePositions.
        }

        static AllEmployeePositions_SO _getOrCreateAllEmployeePositionsSO()
        {
            var allEmployeePositionsSO = Resources.Load<AllEmployeePositions_SO>(_allEmployeePositionsSOPath);

            if (allEmployeePositionsSO is not null) return allEmployeePositionsSO;

            allEmployeePositionsSO = ScriptableObject.CreateInstance<AllEmployeePositions_SO>();
            AssetDatabase.CreateAsset(allEmployeePositionsSO, $"Assets/Resources/{_allEmployeePositionsSOPath}");
            AssetDatabase.SaveAssets();

            return allEmployeePositionsSO;
        }
    }

    [Serializable]
    public class EmployeePosition_Master
    {
        public readonly EmployeePositionName      EmployeePositionName;
        public readonly ActorGenerationParameters ActorGenerationParameters;

        public EmployeePosition_Master(EmployeePositionName      employeePositionName,
                                       ActorGenerationParameters actorGenerationParameters)
        {
            EmployeePositionName      = employeePositionName;
            ActorGenerationParameters = actorGenerationParameters;
        }
    }

    public enum EmployeePositionName
    {
        Intern,

        Owner,

        Shopkeeper,

        Logger,

        Sawyer,

        Smith,

        Hauler,
    }
}