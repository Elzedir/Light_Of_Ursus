using System;
using System.Collections.Generic;
using Actor;
using Station;
using Tools;
using UnityEngine;
using UnityEngine.Serialization;

namespace WorkPosts
{
    [Serializable]
    public class WorkPost_Data : Data_Class
    {
        public           uint    WorkPostID;
        [SerializeField] uint    _stationID;
        Station_Component        _station;
        public Station_Component Station => _station ??= Station_Manager.GetStation_Component(_stationID);

        public uint           CurrentWorkerID;
        Actor_Component        _currentWorker;
        public Actor_Component CurrentWorker => _currentWorker ??= Actor_Manager.GetActor_Component(CurrentWorkerID);
        public bool            HasWorker()   => CurrentWorkerID != 0;
        public bool            IsWorkerMovingToWorkPost;

        public WorkPost_Data(uint workPostID, uint stationID, uint currentWorkerID = 0)
        {
            WorkPostID      = workPostID;
            _stationID      = stationID;
            CurrentWorkerID = currentWorkerID;
        }

        public void AddWorkerToWorkPost(uint workerID)
        {
            if (CurrentWorkerID != 0)
            {
                RemoveCurrentWorkerFromWorkPost();
                Debug.Log($"WorkPost: {WorkPostID} replaced Worker: {CurrentWorkerID} with new Worker {workerID}");
            }
            
            IsWorkerMovingToWorkPost = false;
            CurrentWorkerID              = workerID;
        }

        public void RemoveCurrentWorkerFromWorkPost()
        {
            if (CurrentWorkerID == 0)
            {
                Debug.Log($"WorkPost does not have current Worker.");
                return;
            }

            CurrentWorker.ActorData.CareerData.StopCurrentJob();
            CurrentWorkerID              = 0;
            _currentWorker                = null;
            IsWorkerMovingToWorkPost = false;
        }
        
        protected override Data_Display _getDataSO_Object(bool toggleMissingDataDebugs)
        {
            var dataObjects = new List<Data_Display>();

            try
            {
                dataObjects.Add(new Data_Display(
                    title: "Base WorkPost Data",
                    dataDisplayType: DataDisplayType.Item,
                    data: new List<string>
                    {
                        $"WorkPost ID: {WorkPostID}",
                        $"StationID: {_stationID}",
                        $"CurrentWorkedID: {CurrentWorkerID}",
                        $"ISWorkingMovingTowardsWorkPost: {IsWorkerMovingToWorkPost}"
                    }));
            }
            catch
            {
                Debug.LogError("Error: Base WorkPost Data not found.");
            }

            return new Data_Display(
                title: "Base WorkPost Data",
                dataDisplayType: DataDisplayType.CheckBoxList,
                subData: dataObjects);
        }
    }
    
    public class WorkPost_Position
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Scale;
        
        public WorkPost_Position(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            Position = position;
            Rotation = rotation;
            Scale    = scale;
        }
    }
}
