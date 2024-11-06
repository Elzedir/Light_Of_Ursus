using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;

public class Object_Visualiser : MonoBehaviour
{
    static Object_Visualiser _instance;
    public static Object_Visualiser Instance
    {
        get { return _instance ??= GameObject.Find("Object_Visualiser").GetComponent<Object_Visualiser>(); }
    }

    GameObject _objectPanelParent;
    public GameObject ObjectPanelParent
    {
        get { return _objectPanelParent ??= _objectPanelParent = Manager_Game.FindTransformRecursively(transform, "ObjectPanelParent").gameObject; }
    }

    Button _xButton;
    Button XButton
    {
        get { return _xButton ??= _xButton = Manager_Game.FindTransformRecursively(transform, "XPanel").GetComponent<Button>(); }
    }

    GameObject _objectSectionPrefab;
    GameObject ObjectSectionPrefab
    {
        get { return _objectSectionPrefab ??= _objectSectionPrefab = Manager_Game.FindTransformRecursively(transform, "ObjectSectionPrefab").gameObject; }
    }

    GameObject _objectEntryPrefab;
    public GameObject ObjectEntryPrefab
    {
        get { return _objectEntryPrefab ??= _objectEntryPrefab = Manager_Game.FindTransformRecursively(transform, "ObjectEntryPrefab").gameObject; }
    }

    GameObject _objectDataPrefab;
    public GameObject ObjectDataPrefab
    {
        get { return _objectDataPrefab ??= _objectDataPrefab = Manager_Game.FindTransformRecursively(transform, "ObjectDataPrefab").gameObject; }
    }

    public Dictionary<ObjectSectionType, ObjectSection> AllObjectSections = new();

    void Start()
    {
        TogglePrefabs(false);
    }

    public void Initialise()
    {
        Manager_TickRate.RegisterTickable(OnTick, TickRate.OneTenthSecond);
    }

    public void OnTick()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(ObjectPanelParent.GetComponent<RectTransform>());
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()), out var hit))
            {
                _displayObject(hit.transform.gameObject);
            }
        }
    }

    void _displayObject(GameObject hitObject)
    {
        if (hitObject == null)
        {
            return;
        }

        if (hitObject.TryGetComponent(out ActorComponent actor))
        {
            _displayActor(actor);
            return;
        }
        else if (hitObject.TryGetComponent(out StationComponent station))
        {
            _displayStation(station);
        }

        else
        {
            Debug.LogWarning($"Object {hitObject.name} does not have a valid component.");
        }
    }

    void _displayActor(ActorComponent actor)
    {
        var fullIdentification = actor.ActorData.FullIdentification;

        if (fullIdentification != null)
        {
            var allIdentificationData = new List<ObjectData_Data>
            {
                new ObjectData_Data(ObjectDataType.FullIdentification, $"Name: {fullIdentification.ActorName.GetName()}, ID: {actor.ActorData.ActorID} "),

            };
            
            UpdateObjectEntry(objectSectionData);
        }
        else
        {
            Debug.LogWarning($"Actor {actor.name} does not have a valid full identification.");
        }
        var objectData = new ObjectData_Data(ObjectDataType.StationType, hitObject.name);

        var objectEntryData = new ObjectEntry_Data(new ObjectEntryKey(hitObject.name, hitObject.GetInstanceID().ToString()));

        var objectSectionData = new ObjectSection_Data(ObjectSectionType.Testing, );
        
        objectEntryData.AllObjectData.Add(objectData.ObjectDataType, objectData);
        objectSectionData.AllEntryData.Add(objectEntryData);

        UpdateObjectSection(objectSectionData);
    }

    void _displayStation(StationComponent station)
    {
        var objectData = new ObjectData_Data(ObjectDataType.StationType, hitObject.name);

        var objectEntryData = new ObjectEntry_Data(new ObjectEntryKey(hitObject.name, hitObject.GetInstanceID().ToString()));

        var objectSectionData = new ObjectSection_Data(ObjectSectionType.Testing, );
        
        objectEntryData.AllObjectData.Add(objectData.ObjectDataType, objectData);
        objectSectionData.AllEntryData.Add(objectEntryData);

        UpdateObjectSection(objectSectionData);
    }

    public void TogglePrefabs(bool toggle)
    {
        ObjectSectionPrefab.SetActive(toggle);
        ObjectEntryPrefab.SetActive(toggle);
        ObjectDataPrefab.SetActive(toggle);
    }

    public void ClosePanel()
    {
        gameObject.SetActive(false);
    }

    public void OpenPanel()
    {
        gameObject.SetActive(true);
    }

    public void UpdateObjectSection(ObjectSection_Data objectSectionData)
    {
        TogglePrefabs(true);

        if (!AllObjectSections.ContainsKey(objectSectionData.ObjectSectionType))
        {
            var newObjectSection = Instantiate(ObjectSectionPrefab, ObjectPanelParent.transform).AddComponent<ObjectSection>();
            Destroy(Manager_Game.FindTransformRecursively(newObjectSection.transform, "ObjectEntryPrefab").gameObject);
            newObjectSection.InitialiseObjectSection(new ObjectSection_Data(objectSectionData));
            AllObjectSections.Add(objectSectionData.ObjectSectionType, newObjectSection);
            
            TogglePrefabs(false);
            
            return;
        }

        AllObjectSections[objectSectionData.ObjectSectionType].UpdateObjectSection(objectSectionData.AllEntryData);

        TogglePrefabs(false);
    }

    public void UpdateObjectEntry(ObjectSectionType objectSectionType, ObjectEntry_Data objectEntryData)
    {
        TogglePrefabs(true);

        if (!AllObjectSections.ContainsKey(objectSectionType))
        {
            var newObjectEntryDataList = new List<ObjectEntry_Data> { objectEntryData };
            var newObjectSectionData = new ObjectSection_Data(objectSectionType, newObjectEntryDataList);

            UpdateObjectSection(newObjectSectionData);

            TogglePrefabs(false);

            return;
        }

        if (!AllObjectSections[objectSectionType].AllObjectEntries.ContainsKey(objectEntryData.ObjectEntryKey.GetID()))
        {
            var newObjectEntryDataList = new List<ObjectEntry_Data> { objectEntryData };

            AllObjectSections[objectSectionType].UpdateObjectSection(newObjectEntryDataList);

            TogglePrefabs(false);

            return;
        }

        AllObjectSections[objectSectionType].AllObjectEntries[objectEntryData.ObjectEntryKey.GetID()].UpdateObjectEntry(objectEntryData.AllObjectData);

        TogglePrefabs(false);
    }

    public void UpdateObjectData(ObjectSectionType ObjectSectionType, ObjectEntryKey ObjectEntryTitle, ObjectData_Data ObjectData)
    {
        TogglePrefabs(true);

        if (!AllObjectSections.ContainsKey(ObjectSectionType))
        {
            var newObjectDataList = new List<ObjectData_Data> { ObjectData };
            var newObjectEntry = new ObjectEntry_Data(ObjectEntryTitle, newObjectDataList);
            var newObjectEntryDataList = new List<ObjectEntry_Data> { newObjectEntry };
            var newObjectSectionData = new ObjectSection_Data(ObjectSectionType, newObjectEntryDataList);

            UpdateObjectSection(newObjectSectionData);

            TogglePrefabs(false);

            return;
        }

        if (!AllObjectSections[ObjectSectionType].AllObjectEntries.ContainsKey(ObjectEntryTitle.GetID()))
        {
            var newObjectDataList = new List<ObjectData_Data> { ObjectData };
            var newObjectEntry = new ObjectEntry_Data(ObjectEntryTitle, newObjectDataList);

            UpdateObjectEntry(ObjectSectionType, newObjectEntry);

            TogglePrefabs(false);

            return;
        }

        if (!AllObjectSections[ObjectSectionType].AllObjectEntries[ObjectEntryTitle.GetID()].AllObjectData.ContainsKey(ObjectData.ObjectDataType))
        {
            var newObjectDataList = new List<ObjectData_Data> { ObjectData };

            AllObjectSections[ObjectSectionType].AllObjectEntries[ObjectEntryTitle.GetID()].UpdateObjectEntry(newObjectDataList);

            TogglePrefabs(false);

            return;
        }

        AllObjectSections[ObjectSectionType].AllObjectEntries[ObjectEntryTitle.GetID()].AllObjectData[ObjectData.ObjectDataType].ObjectValue = ObjectData.ObjectValue;

        TogglePrefabs(false);
    }

    public void RemoveObjectSection(ObjectSectionType ObjectSectionType)
    {
        if (!AllObjectSections.ContainsKey(ObjectSectionType))
        {
            Debug.LogWarning($"Object Section {ObjectSectionType} does not exist.");
            return;
        }

        Destroy(AllObjectSections[ObjectSectionType].gameObject);
        AllObjectSections.Remove(ObjectSectionType);
    }
}
