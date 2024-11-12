using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Actors;
using UnityEditor;
using UnityEngine;

public enum EmployeePosition
{
    None,

    Owner,

    Shopkeeper,

    Logger,

    Sawyer,

    Smith,

    Hauler,
}

[CreateAssetMenu(fileName = "AllEmployeeTypes_SO", menuName = "SOList/AllEmployeeTypes_SO")]
[Serializable]
public class AllEmployeeTypes_SO : ScriptableObject
{
    public List<Employee_Master> AllEmployeeTypes = new();

    public void LoadData(SaveData saveData)
    {
        //AllEmployeeTypes = saveData.SavedEmployeeData.AllEmployeeTypes;
    }

    public void ClearEmployeeData() => AllEmployeeTypes.Clear();

    public void RemoveAndAddDefaultEmployeeTypes()
    {

        _removeAndAddEmployeeType(EmployeePosition.Owner);
        _removeAndAddEmployeeType(EmployeePosition.Shopkeeper);

        _removeAndAddEmployeeType(EmployeePosition.Logger);
        var logger = AllEmployeeTypes.FirstOrDefault(e => e.EmployeePosition == EmployeePosition.Logger);
        logger.ActorGenerationParameters.SetInitialRecipes(new List<RecipeName> { RecipeName.Log });
        logger.ActorGenerationParameters.SetInitialVocations(new List<ActorVocation> 
        { 
            new ActorVocation(VocationName.Logging, 1000)
        });
        
        _removeAndAddEmployeeType(EmployeePosition.Sawyer);

        var sawyer = AllEmployeeTypes.FirstOrDefault(e => e.EmployeePosition == EmployeePosition.Sawyer);
        sawyer.ActorGenerationParameters.SetInitialRecipes(new List<RecipeName> { RecipeName.Plank });
        sawyer.ActorGenerationParameters.SetInitialVocations(new List<ActorVocation> 
        { 
            new ActorVocation(VocationName.Sawying, 1000)
        });

        _removeAndAddEmployeeType(EmployeePosition.Smith);

        var smith = AllEmployeeTypes.FirstOrDefault(e => e.EmployeePosition == EmployeePosition.Smith);
        smith.ActorGenerationParameters.SetInitialRecipes(new List<RecipeName> {  });
        smith.ActorGenerationParameters.SetInitialVocations(new List<ActorVocation> 
        { 
            new ActorVocation(VocationName.Smithing, 1000)
        });

        _removeAndAddEmployeeType(EmployeePosition.Hauler);
    }

    void _removeAndAddEmployeeType(EmployeePosition employeePosition)
    {
        var employeeType = AllEmployeeTypes.FirstOrDefault(e => e.EmployeePosition == EmployeePosition.None);

        if (employeeType != null)
        {
            AllEmployeeTypes.Remove(employeeType);
        }

        AllEmployeeTypes.Add(new Employee_Master(
            employeePosition,
            new ActorGenerationParameters()
            ));
    }

    public Employee_Master GetEmployeeType(EmployeePosition employeePosition)
    {
        var employeeType = AllEmployeeTypes.FirstOrDefault(e => e.EmployeePosition == employeePosition);

        if (employeeType == null)
        {
            RemoveAndAddDefaultEmployeeTypes();

            employeeType = AllEmployeeTypes.FirstOrDefault(e => e.EmployeePosition == employeePosition);

            if (employeeType == null)
            {
                Debug.Log($"EmployeeType not found for {employeePosition}.");
            }
        }

        return employeeType;
    }
}

public class AllEmployeeTypesManager
{
    static AllEmployeeTypes_SO _allEmployeeTypes_SO;
    public static AllEmployeeTypes_SO AllEmployeeTypes_SO { get { return _allEmployeeTypes_SO ??= Resources.Load<AllEmployeeTypes_SO>("ScriptableObjects/AllEmployeeTypes_SO"); } }
}

[Serializable]
public class Employee_Master
{
    public EmployeePosition EmployeePosition;
    public ActorGenerationParameters ActorGenerationParameters;

    public Employee_Master(EmployeePosition employeePosition, ActorGenerationParameters actorGenerationParameters)
    {
        EmployeePosition = employeePosition;
        ActorGenerationParameters = actorGenerationParameters;
    }
}

[CustomPropertyDrawer(typeof(Employee_Master))]
public class Employee_Master_Drawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var employeePositionProp = property.FindPropertyRelative("EmployeePosition");

        label.text = $"{(EmployeePosition)employeePositionProp.enumValueIndex}";

        EditorGUI.PropertyField(position, property, label, true);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true);
    }
}