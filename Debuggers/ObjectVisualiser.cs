using System.Collections.Generic;
using System.Linq;
using Actor;
using Managers;
using Station;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Time = UnityEngine.Time;

namespace Debuggers
{
    public class ObjectVisualiser : MonoBehaviour
    {
        static ObjectVisualiser _instance;

        public static ObjectVisualiser Instance =>
            _instance ??= GameObject.Find("Object_Visualiser").GetComponent<ObjectVisualiser>();

        GameObject _objectPanelParent;

        GameObject ObjectPanelParent => _objectPanelParent ??= _objectPanelParent =
            Manager_Game.FindTransformRecursively(transform, "ObjectPanelParent").gameObject;

        RectTransform _objectPanelParentRect;

        RectTransform ObjectPanelParentRect => _objectPanelParentRect ??= _objectPanelParentRect =
            ObjectPanelParent.GetComponent<RectTransform>();

        GameObject _objectSectionPrefab;

        GameObject ObjectSectionPrefab => _objectSectionPrefab ??= _objectSectionPrefab =
            Manager_Game.FindTransformRecursively(transform, "ObjectSectionPrefab").gameObject;

        GameObject _objectEntryPrefab;

        public GameObject ObjectEntryPrefab => _objectEntryPrefab ??= _objectEntryPrefab =
            Manager_Game.FindTransformRecursively(transform, "ObjectEntryPrefab").gameObject;

        GameObject _objectDataPrefab;

        public GameObject ObjectDataPrefab => _objectDataPrefab ??= _objectDataPrefab =
            Manager_Game.FindTransformRecursively(transform, "ObjectDataPrefab").gameObject;

        readonly Dictionary<ObjectSectionType, ObjectSection> _allObjectSections = new();

        void Start()
        {
            _togglePrefabs(false);
        }

        void _onTick()
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(ObjectPanelParentRect);

            _refreshDisplay();
        }

        const float _tickRate = 0.1f;
        float       _nextTickTime;

        public void Update()
        {
            if (!Input.GetKeyDown(KeyCode.Mouse0)) return;

            if (Camera.main is null) return;

            var ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

            List<Transform> hitTransforms = new();

            while (Physics.Raycast(ray, out var hit, 20f))
            {
                if (hit.transform is null)
                {
                    return;
                }

                if (hitTransforms.Contains(hit.transform))
                {
                    return;
                }

                hitTransforms.Add(hit.transform);

                if (_displayObject(hit.transform.gameObject))
                {
                    break;
                }

                ray.origin = hit.point + ray.direction * 0.01f;
            }

            if (Time.time < _nextTickTime) return;

            _nextTickTime = Time.time + _tickRate;

            _onTick();
        }

        GameObject _currentDisplayedObject;

        void _refreshDisplay()
        {
            if (_currentDisplayedObject is null) return;

            _displayObject(_currentDisplayedObject);
        }

        bool _displayObject(GameObject hitGO)
        {
            if (hitGO is null)
            {
                return false;
            }

            if (hitGO.TryGetComponent(out Actor_Component actor))
            {
                if (hitGO != _currentDisplayedObject)
                {
                    _resetEntries();
                    _currentDisplayedObject = hitGO;
                }

                _displayActor(actor);
                return true;
            }
            else if (hitGO.TryGetComponent(out Station_Component station))
            {
                if (hitGO != _currentDisplayedObject)
                {
                    _resetEntries();
                    _currentDisplayedObject = hitGO;
                }

                _displayStation(station);
                return true;
            }
            else
            {
                _currentDisplayedObject = null;
            }

            return false;
        }

        void _displayActor(Actor_Component actor)
        {
            var actorFullIdentification = actor.ActorData.FullIdentification;
            var actorInventory          = actor.ActorData.InventoryData;
            var actorPriorities         = actor.DecisionMakerComponent.PriorityData.PriorityQueue;

            if (actorFullIdentification != null)
            {
                var allIdentificationData = new List<ObjectData_Data>
                {
                    new(ObjectDataType.FullIdentification,
                        $"Name: {actorFullIdentification.ActorName.GetName()}, ID: {actor.ActorData.ActorID} "),
                };

                var objectEntryData = new ObjectEntryData(
                    new ObjectEntryKey("ActorName", actorFullIdentification.ActorName.GetName(),
                        actor.ActorData.ActorID),
                    allIdentificationData);

                _updateObjectEntry(ObjectSectionType.Testing, objectEntryData);
            }
            else
            {
                Debug.LogWarning($"Actor {actor.name} does not have a valid full identification.");
            }

            if (actorInventory != null)
            {
                var allInventoryData = actorInventory.AllInventoryItems
                                                     .Select(inventoryItem =>
                                                         new ObjectData_Data(ObjectDataType.InventoryData,
                                                             $"ItemName: {inventoryItem.Value.ItemName} ItemAmount: {inventoryItem.Value.ItemAmount}"))
                                                     .ToList();

                var objectEntryData = new ObjectEntryData(
                    new ObjectEntryKey("InventoryData", actorFullIdentification?.ActorName.GetName(),
                        actor.ActorData.ActorID),
                    allInventoryData);

                _updateObjectEntry(ObjectSectionType.Testing, objectEntryData);
            }
            else
            {
                Debug.LogWarning($"Actor {actor.name} does not have a valid inventory.");
            }

            if (actorPriorities != null)
            {
                foreach (var priority in actorPriorities.PeekAll())
                {
                    var allPriorityData = new List<ObjectData_Data>
                    {
                        new(ObjectDataType.PriorityData,
                            $"PriorityID: {priority.PriorityID} PriorityValue: {priority.PriorityValue}"),
                    };

                    var objectEntryData = new ObjectEntryData(
                        new ObjectEntryKey("PriorityData", actorFullIdentification?.ActorName.GetName(),
                            actor.ActorData.ActorID),
                        allPriorityData);

                    _updateObjectEntry(ObjectSectionType.Testing, objectEntryData);
                }
            }
            else
            {
                Debug.LogWarning($"Actor {actor.name} does not have a valid priority component.");
            }
        }

        void _displayStation(Station_Component station)
        {
            var stationInventory    = station.Station_Data.InventoryData;
            var stationProgressData = station.Station_Data.StationProgressData;

            if (stationInventory != null)
            {
                var allInventoryData = stationInventory.AllInventoryItems
                                                       .Select(inventoryItem =>
                                                           new ObjectData_Data(ObjectDataType.InventoryData,
                                                               $"Name: {inventoryItem.Value.ItemName} Amount: {inventoryItem.Value.ItemAmount}"))
                                                       .ToList();

                var objectEntryData = new ObjectEntryData(
                    new ObjectEntryKey("InventoryData", station.StationName.ToString(), station.Station_Data.StationID),
                    allInventoryData);

                _updateObjectEntry(ObjectSectionType.Testing, objectEntryData);
            }
            else
            {
                Debug.LogWarning(
                    $"Station {station.Station_Data.StationID}: {station.StationName} does not have a valid inventory.");
            }

            if (stationProgressData != null)
            {
                var allProgressData = new List<ObjectData_Data>
                {
                    new(ObjectDataType.ProgressData,
                        $"CurrentProduct: {stationProgressData.CurrentProduct}"),
                    new(ObjectDataType.ProgressData,
                        $"CurrentProgress: {stationProgressData.CurrentProgress}"),
                };

                var objectEntryData = new ObjectEntryData(
                    new ObjectEntryKey("ProgressData", station.StationName.ToString(), station.Station_Data.StationID),
                    allProgressData);

                _updateObjectEntry(ObjectSectionType.Testing, objectEntryData);
            }
            else
            {
                Debug.LogWarning(
                    $"Station {station.Station_Data.StationID}: {station.StationName} does not have a valid progress data.");
            }
        }

        void _togglePrefabs(bool toggle)
        {
            ObjectSectionPrefab.SetActive(toggle);
            ObjectEntryPrefab.SetActive(toggle);
            ObjectDataPrefab.SetActive(toggle);
        }

        void _resetEntries()
        {
            for (var i = 0; i < ObjectPanelParent.transform.childCount; i++)
            {
                if (ObjectPanelParent.transform.GetChild(i).gameObject == ObjectSectionPrefab) continue;

                Destroy(ObjectPanelParent.transform.GetChild(i).gameObject);
                _allObjectSections.Clear();
            }
        }


        public void ClosePanel()
        {
            gameObject.SetActive(false);
        }

        public void OpenPanel()
        {
            gameObject.SetActive(true);
        }

        void _updateObjectSection(ObjectSection_Data objectSectionData)
        {
            _togglePrefabs(true);

            if (!_allObjectSections.TryGetValue(objectSectionData.ObjectSectionType, out var section))
            {
                var newObjectSection = Instantiate(ObjectSectionPrefab, ObjectPanelParent.transform)
                    .AddComponent<ObjectSection>();
                Destroy(Manager_Game.FindTransformRecursively(newObjectSection.transform, "ObjectEntryPrefab")
                                    .gameObject);
                newObjectSection.InitialiseObjectSection(new ObjectSection_Data(objectSectionData));
                _allObjectSections.Add(objectSectionData.ObjectSectionType, newObjectSection);

                _togglePrefabs(false);

                return;
            }

            section.UpdateObjectSection(objectSectionData.AllEntryData);

            _togglePrefabs(false);
        }

        void _updateObjectEntry(ObjectSectionType objectSectionType, ObjectEntryData objectEntryData)
        {
            _togglePrefabs(true);

            if (!_allObjectSections.ContainsKey(objectSectionType))
            {
                var newObjectEntryDataList = new List<ObjectEntryData> { objectEntryData };
                var newObjectSectionData   = new ObjectSection_Data(objectSectionType, newObjectEntryDataList);

                _updateObjectSection(newObjectSectionData);

                _togglePrefabs(false);

                return;
            }

            if (!_allObjectSections[objectSectionType].AllObjectEntries
                                                      .ContainsKey(objectEntryData.ObjectEntryKey.GetID()))
            {
                var newObjectEntryDataList = new List<ObjectEntryData> { objectEntryData };

                _allObjectSections[objectSectionType].UpdateObjectSection(newObjectEntryDataList);

                _togglePrefabs(false);

                return;
            }

            _allObjectSections[objectSectionType].AllObjectEntries[objectEntryData.ObjectEntryKey.GetID()]
                                                 .UpdateObjectEntry(objectEntryData.AllObjectData);

            _togglePrefabs(false);
        }

        public void UpdateObjectData(ObjectSectionType objectSectionType, ObjectEntryKey objectEntryTitle,
                                     ObjectData_Data   objectData)
        {
            _togglePrefabs(true);

            if (!_allObjectSections.TryGetValue(objectSectionType, out var section))
            {
                var newObjectDataList      = new List<ObjectData_Data> { objectData };
                var newObjectEntry         = new ObjectEntryData(objectEntryTitle, newObjectDataList);
                var newObjectEntryDataList = new List<ObjectEntryData> { newObjectEntry };
                var newObjectSectionData   = new ObjectSection_Data(objectSectionType, newObjectEntryDataList);

                _updateObjectSection(newObjectSectionData);

                _togglePrefabs(false);

                return;
            }

            if (!section.AllObjectEntries.ContainsKey(objectEntryTitle.GetID()))
            {
                var newObjectDataList = new List<ObjectData_Data> { objectData };
                var newObjectEntry    = new ObjectEntryData(objectEntryTitle, newObjectDataList);

                _updateObjectEntry(objectSectionType, newObjectEntry);

                _togglePrefabs(false);

                return;
            }

            if (!_allObjectSections[objectSectionType].AllObjectEntries[objectEntryTitle.GetID()].AllObjectData
                                                      .ContainsKey(objectData.ObjectDataType))
            {
                var newObjectDataList = new List<ObjectData_Data> { objectData };

                _allObjectSections[objectSectionType].AllObjectEntries[objectEntryTitle.GetID()]
                                                     .UpdateObjectEntry(newObjectDataList);

                _togglePrefabs(false);

                return;
            }

            _allObjectSections[objectSectionType].AllObjectEntries[objectEntryTitle.GetID()]
                                                 .AllObjectData[objectData.ObjectDataType].ObjectValue =
                objectData.ObjectValue;

            _togglePrefabs(false);
        }

        public void RemoveObjectSection(ObjectSectionType objectSectionType)
        {
            if (!_allObjectSections.TryGetValue(objectSectionType, out var section))
            {
                Debug.LogWarning($"Object Section {objectSectionType} does not exist.");
                return;
            }

            Destroy(section.gameObject);
            _allObjectSections.Remove(objectSectionType);
        }
    }
}