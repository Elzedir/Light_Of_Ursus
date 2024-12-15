using UnityEngine;

namespace EmployeePosition
{
    public abstract class EmployeePosition_Manager : MonoBehaviour
    {
        const string _employeePosition_SOPath = "ScriptableObjects/EmployeePosition_SO";

        static EmployeePosition_SO _employeePositions;

        static EmployeePosition_SO EmployeePositions =>
            _employeePositions ??= _getEmployeePosition_SO();

        public static EmployeePosition_Data GetEmployeePosition_Master(EmployeePositionName employeePositionName) =>
            EmployeePositions.GetEmployeePosition_Master(employeePositionName).DataObject;

        public static void PopulateAllEmployeePositions()
        {
            EmployeePositions.PopulateDefaultEmployeePositions();
            // Then populate custom EmployeePositions.
        }

        static EmployeePosition_SO _getEmployeePosition_SO()
        {
            var employeePosition_SO = Resources.Load<EmployeePosition_SO>(_employeePosition_SOPath);

            if (employeePosition_SO is not null) return employeePosition_SO;

            Debug.LogError("EmployeePosition_SO not found. Creating temporary EmployeePosition_SO.");
            employeePosition_SO = ScriptableObject.CreateInstance<EmployeePosition_SO>();

            return employeePosition_SO;
        }
        
        public static void ClearSOData()
        {
            EmployeePositions.ClearSOData();
        }
    }
}