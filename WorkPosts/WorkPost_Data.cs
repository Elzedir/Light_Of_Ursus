using System;
using Actor;
using Station;
using Tools;
using UnityEngine;

namespace WorkPost
{
    [Serializable]
    public class WorkPost_Data : Data_Class
    {
        public uint             WorkPostID;
        public                                           uint             StationID;
        Station_Component        _station;
        public Station_Component Station => _station ??= Station_Manager.GetStation_Component(StationID);
        
        [SerializeField] uint           _currentWorkerID;
        Actor_Component        _currentWorker;
        public Actor_Component CurrentWorker => _currentWorker ??= Actor_Manager.GetActor_Component(_currentWorkerID);
        public bool           HasWorker()   => _currentWorkerID != 0;
        public bool           IsWorkerMovingToWorkPost;

        public WorkPost_Data(uint workPostID, uint stationID)
        {
            WorkPostID   = workPostID;
            StationID         = stationID;
            _currentWorkerID = 0;
        }

        public void AddWorkerToWorkPost(uint WorkerID)
        {
            if (_currentWorkerID != 0)
            {
                RemoveCurrentWorkerFromWorkPost();
                Debug.Log($"WorkPost: {WorkPostID} replaced Worker: {_currentWorkerID} with new Worker {WorkerID}");
            }
            
            IsWorkerMovingToWorkPost = false;
            _currentWorkerID              = WorkerID;
        }

        public void RemoveCurrentWorkerFromWorkPost()
        {
            if (_currentWorkerID == 0)
            {
                Debug.Log($"WorkPost does not have current Worker.");
                return;
            }

            CurrentWorker.ActorData.CareerData.StopCurrentJob();
            _currentWorkerID              = 0;
            _currentWorker                = null;
            IsWorkerMovingToWorkPost = false;
        }
    }
}
