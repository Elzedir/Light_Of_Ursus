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

        [SerializeField] uint           _currentWorkerID;
        Actor_Component        _currentWorker;

        public Actor_Component CurrentWorker => _currentWorker ??=
            _currentWorkerID != 0
                ? Actor_Manager.GetActor_Component(_currentWorkerID)
                : null;
        
        public bool            IsWorkerMovingToWorkPost;

        public WorkPost_Data(uint workPostID, uint stationID, uint currentWorkerID = 0)
        {
            WorkPostID      = workPostID;
            _stationID      = stationID;
            _currentWorkerID = currentWorkerID;
        }

        public void AddWorkerToWorkPost(Actor_Component worker)
        {
            if (_currentWorkerID != 0)
            {
                RemoveCurrentWorkerFromWorkPost();
                Debug.Log($"WorkPost: {WorkPostID} replaced Worker: {_currentWorkerID} with new Worker {worker.ActorID}");
            }
            
            IsWorkerMovingToWorkPost = false;
            _currentWorkerID         = worker.ActorID;
        }

        public void RemoveCurrentWorkerFromWorkPost()
        {
            if (_currentWorkerID == 0)
            {
                Debug.Log($"WorkPost does not have current Worker.");
                return;
            }

            CurrentWorker.ActorData.CareerData.StopCurrentJob();
            _currentWorkerID         = 0;
            _currentWorker           = null;
            IsWorkerMovingToWorkPost = false;
        }
        
        protected override Data_Display _getDataSO_Object(bool toggleMissingDataDebugs, ref Data_Display dataSO_Object)
        {
            var dataObjects = dataSO_Object == null
                ? new Dictionary<string, Data_Display>()
                : new Dictionary<string, Data_Display>(dataSO_Object.SubData);

            try
            {
                dataObjects["Base WorkPost Data"] = new Data_Display(
                    title: "Base WorkPost Data",
                    dataDisplayType: DataDisplayType.Item,
                    dataSO_Object: dataSO_Object,
                    data: new Dictionary<string, string>
                    {
                        {"WorkPost ID", $"{WorkPostID}"},
                        {"Station ID", $"{_stationID}"},
                        {"Current Worker ID", $"{_currentWorkerID}"},
                        {"Is Worker Moving To WorkPost", $"{IsWorkerMovingToWorkPost}"}
                    });
            }
            catch
            {
                Debug.LogError("Error: Base WorkPost Data not found.");
            }

            return dataSO_Object = new Data_Display(
                title: "Base WorkPost Data",
                dataDisplayType: DataDisplayType.CheckBoxList,
                dataSO_Object: dataSO_Object,
                subData: dataObjects);
        }
    }
    
    public class WorkPost_TransformValue
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Scale;
        
        public WorkPost_TransformValue(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            Position = position;
            Rotation = rotation;
            Scale    = scale;
        }
    }
}
