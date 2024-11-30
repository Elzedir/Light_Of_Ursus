using System;
using Actor;
using UnityEditor;
using UnityEngine;

namespace EmployeePosition
{
    public abstract class EmployeePosition_Manager : MonoBehaviour
    {
        const string _allEmployeePositionsSOPath = "ScriptableObjects/AllEmployeePositions_SO";

        static EmployeePosition_SO _EmployeePositions;

        static EmployeePosition_SO EmployeePositions =>
            _EmployeePositions ??= _getOrCreateAllEmployeePositionsSO();

        public static EmployeePosition_Master GetEmployeePosition_Master(EmployeePositionName employeePositionName) =>
            EmployeePositions.GetEmployeePosition_Master(employeePositionName);

        public static void PopulateAllEmployeePositions()
        {
            EmployeePositions.PopulateDefaultEmployeePositions();
            // Then populate custom EmployeePositions.
        }

        static EmployeePosition_SO _getOrCreateAllEmployeePositionsSO()
        {
            var allEmployeePositionsSO = Resources.Load<EmployeePosition_SO>(_allEmployeePositionsSOPath);

            if (allEmployeePositionsSO is not null) return allEmployeePositionsSO;

            allEmployeePositionsSO = ScriptableObject.CreateInstance<EmployeePosition_SO>();
            AssetDatabase.CreateAsset(allEmployeePositionsSO, $"Assets/Resources/{_allEmployeePositionsSOPath}");
            AssetDatabase.SaveAssets();

            return allEmployeePositionsSO;
        }
    }

    [Serializable]
    public class EmployeePosition_Master
    {
        public readonly EmployeePositionName EmployeePositionName;
        public readonly ActorDataPresetName  EmployeeDataPreset;

        public EmployeePosition_Master(EmployeePositionName employeePositionName,
                                       ActorDataPresetName  employeeDataPreset)
        {
            EmployeePositionName = employeePositionName;
            EmployeeDataPreset   = employeeDataPreset;
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