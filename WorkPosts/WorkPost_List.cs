using System.Collections.Generic;
using Station;
using UnityEngine;

namespace WorkPosts
{
    public abstract class WorkPost_List
    {
        public static List<WorkPost_TransformValue> GetWorkPlace_DefaultValues(StationName stationName)
        {
            if (_workPlacePositions.TryGetValue(stationName, out var positions))
            {
                return positions;
            }

            Debug.Log($"StationName: {stationName} not found in WorkPlacePositions.");
            return null;
        }

        static readonly Dictionary<StationName, List<WorkPost_TransformValue>> _workPlacePositions = new()
        {
            {
                StationName.Tree, new List<WorkPost_TransformValue>
                {

                    new(
                        position: new Vector3(1.5f, -0.8f, 0),
                        rotation: new Quaternion(0, 0, 0, 0),
                        scale: new Vector3(1, 0.333f, 1)),

                    new(
                        position: new Vector3(0, -0.8f, 1.5f),
                        rotation: new Quaternion(0, 0, 0, 0),
                        scale: new Vector3(1, 0.333f, 1)),

                    new(
                        position: new Vector3(-1.5f, -0.8f, 0),
                        rotation: new Quaternion(0, 0, 0, 0),
                        scale: new Vector3(1, 0.333f, 1)),

                    new(
                        position: new Vector3(0, -0.8f, -1.5f),
                        rotation: new Quaternion(0, 0, 0, 0),
                        scale: new Vector3(1, 0.333f, 1))

                }
            },
            {
                StationName.Sawmill, new List<WorkPost_TransformValue>
                {

                    new(
                        position: new Vector3(0.75f, 0, 0),
                        rotation: new Quaternion(0, 0, 0, 0),
                        scale: new Vector3(0.333f, 1, 1)),

                    new(
                        position: new Vector3(0, 0, 1),
                        rotation: new Quaternion(0, 0, 0, 0),
                        scale: new Vector3(0.333f, 1, 1)),

                    new(
                        position: new Vector3(-0.75f, 0, 0),
                        rotation: new Quaternion(0, 0, 0, 0),
                        scale: new Vector3(0.333f, 1, 1)),

                    new(
                        position: new Vector3(0, 0, -1),
                        rotation: new Quaternion(0, 0, 0, 0),
                        scale: new Vector3(0.333f, 1, 1))

                }
            },
            {
                StationName.Log_Pile, new List<WorkPost_TransformValue>
                {

                    new(
                        position: new Vector3(0.75f, 0, 0),
                        rotation: new Quaternion(0, 0, 0, 0),
                        scale: new Vector3(0.5f, 1f, 0.5f)),

                    new(
                        position: new Vector3(0, 0, 0.75f),
                        rotation: new Quaternion(0, 0, 0, 0),
                        scale: new Vector3(0.5f, 1f, 0.5f)),

                    new(
                        position: new Vector3(-0.75f, 0, 0),
                        rotation: new Quaternion(0, 0, 0, 0),
                        scale: new Vector3(0.5f, 1f, 0.5f)),

                    new(
                        position: new Vector3(0, 0, -0.75f),
                        rotation: new Quaternion(0, 0, 0, 0),
                        scale: new Vector3(0.5f, 1f, 0.5f))

                }
            }
        };
    }
}