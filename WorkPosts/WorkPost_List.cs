using System.Collections.Generic;
using Station;
using UnityEngine;

namespace WorkPosts
{
    public abstract class WorkPost_List
    {
        public static Dictionary<uint, WorkPost_TransformValue> GetWorkPlace_TransformValues(StationName stationName)
        {
            if (_workPlacePositions.TryGetValue(stationName, out var positions))
            {
                return positions;
            }
            
            Debug.Log($"StationName: {stationName} not found in WorkPlacePositions.");
            return null;
        }
        
        static readonly Dictionary<StationName, Dictionary<uint, WorkPost_TransformValue>> _workPlacePositions = new()
        {
            {
                StationName.Tree, new Dictionary<uint, WorkPost_TransformValue>
                {
                    {
                        1, new WorkPost_TransformValue(
                            new Vector3(1.5f, -0.8f, 0),
                            new Quaternion(0, 0, 0, 0),
                            new Vector3(1, 0.333f, 1))
                    },
                    {
                        2, new WorkPost_TransformValue(
                            new Vector3(0, -0.8f, 1.5f),
                            new Quaternion(0, 0, 0, 0),
                            new Vector3(1, 0.333f, 1))
                    },
                    {
                        3, new WorkPost_TransformValue(
                            new Vector3(-1.5f, -0.8f, 0),
                            new Quaternion(0, 0, 0, 0),
                            new Vector3(1, 0.333f, 1))
                    },
                    {
                        4, new WorkPost_TransformValue(
                            new Vector3(0, -0.8f, -1.5f),
                            new Quaternion(0, 0, 0, 0),
                            new Vector3(1, 0.333f, 1))
                    }
                }
            },
            {
                StationName.Sawmill, new Dictionary<uint, WorkPost_TransformValue>
                {
                    {
                        1, new WorkPost_TransformValue(
                            new Vector3(0.75f, 0, 0),
                            new Quaternion(0, 0, 0, 0),
                            new Vector3(0.333f, 1, 1))
                    },
                    {
                        2, new WorkPost_TransformValue(
                            new Vector3(0, 0, 1),
                            new Quaternion(0, 0, 0, 0),
                            new Vector3(0.333f, 1, 1))
                    },
                    {
                        3, new WorkPost_TransformValue(
                            new Vector3(-0.75f, 0, 0),
                            new Quaternion(0, 0, 0, 0),
                            new Vector3(0.333f, 1, 1))
                    },
                    {
                        4, new WorkPost_TransformValue(
                            new Vector3(0, 0, -1),
                            new Quaternion(0, 0, 0, 0),
                            new Vector3(0.333f, 1, 1))
                    }
                }
            },
            {
                StationName.Log_Pile, new Dictionary<uint, WorkPost_TransformValue>
                {
                    {
                        1, new WorkPost_TransformValue(
                            new Vector3(0.75f, 0, 0),
                            new Quaternion(0, 0, 0, 0),
                            new Vector3(0.5f, 1f, 0.5f))
                    },
                    {
                        2, new WorkPost_TransformValue(
                            new Vector3(0, 0, 0.75f),
                            new Quaternion(0, 0, 0, 0),
                            new Vector3(0.5f, 1f, 0.5f))
                    },
                    {
                        3, new WorkPost_TransformValue(
                            new Vector3(-0.75f, 0, 0),
                            new Quaternion(0, 0, 0, 0),
                            new Vector3(0.5f, 1f, 0.5f))
                    },
                    {
                        4, new WorkPost_TransformValue(
                            new Vector3(0, 0, -0.75f),
                            new Quaternion(0, 0, 0, 0),
                            new Vector3(0.5f, 1f, 0.5f))
                    }
                }
            }
        };
    }
}