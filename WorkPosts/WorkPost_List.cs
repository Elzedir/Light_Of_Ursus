using System;
using System.Collections.Generic;
using System.Linq;
using Jobs;
using Station;
using UnityEngine;

namespace WorkPosts
{
    public abstract class WorkPost_List
    {
        static Dictionary<StationName, List<WorkPost_DefaultValue>> s_workPost_DefaultValues;

        public static Dictionary<StationName, List<WorkPost_DefaultValue>> S_WorkPosts_DefaultValues =>
            s_workPost_DefaultValues ??= _initialiseWorkPost_DefaultValues();

        public static WorkPost_DefaultValue GetWorkPost_DefaultValue(StationName stationName, ulong workPostID)
        {
            if (S_WorkPosts_DefaultValues.TryGetValue(stationName, out var positions))
            {
                return positions.FirstOrDefault(workPost => workPost.WorkPostID == workPostID);
            }

            throw new Exception($"StationName: {stationName} not found in WorkPostPositions.");
        }
        
        public static List<WorkPost_DefaultValue> GetWorkPost_DefaultValues(StationName stationName)
        {
            if (S_WorkPosts_DefaultValues.TryGetValue(stationName, out var positions))
            {
                return positions;
            }

            throw new Exception($"StationName: {stationName} not found in WorkPostPositions.");
        }

        static Dictionary<StationName, List<WorkPost_DefaultValue>> _initialiseWorkPost_DefaultValues()
        {
            return new Dictionary<StationName, List<WorkPost_DefaultValue>>
            {
                {
                    StationName.Tree, new List<WorkPost_DefaultValue>
                    {

                        new(
                            1,
                            jobName: JobName.Logger,
                            position: new Vector3(1.5f, -0.8f, 0),
                            rotation: new Quaternion(0, 0, 0, 0),
                            scale: new Vector3(1, 0.333f, 1)),

                        new(
                            2,
                            jobName: JobName.Logger,
                            position: new Vector3(0, -0.8f, 1.5f),
                            rotation: new Quaternion(0, 0, 0, 0),
                            scale: new Vector3(1, 0.333f, 1)),

                        new(
                            3,
                            jobName: JobName.Logger,
                            position: new Vector3(-1.5f, -0.8f, 0),
                            rotation: new Quaternion(0, 0, 0, 0),
                            scale: new Vector3(1, 0.333f, 1)),

                        new(
                            4,
                            jobName: JobName.Logger,
                            position: new Vector3(0, -0.8f, -1.5f),
                            rotation: new Quaternion(0, 0, 0, 0),
                            scale: new Vector3(1, 0.333f, 1))

                    }
                },
                {
                    StationName.Sawmill, new List<WorkPost_DefaultValue>
                    {

                        new(
                            1,
                            jobName: JobName.Sawyer,
                            position: new Vector3(0.75f, 0, 0),
                            rotation: new Quaternion(0, 0, 0, 0),
                            scale: new Vector3(0.333f, 1, 1)),

                        new(
                            2,
                            jobName: JobName.Sawyer,
                            position: new Vector3(0, 0, 1),
                            rotation: new Quaternion(0, 0, 0, 0),
                            scale: new Vector3(0.333f, 1, 1)),

                        new(
                            3,
                            jobName: JobName.Sawyer,
                            position: new Vector3(-0.75f, 0, 0),
                            rotation: new Quaternion(0, 0, 0, 0),
                            scale: new Vector3(0.333f, 1, 1)),

                        new(
                            4,
                            jobName: JobName.Sawyer,
                            position: new Vector3(0, 0, -1),
                            rotation: new Quaternion(0, 0, 0, 0),
                            scale: new Vector3(0.333f, 1, 1))

                    }
                },
                {
                    StationName.Log_Pile, new List<WorkPost_DefaultValue>
                    {

                        new(
                            1,
                            jobName: JobName.Hauler,
                            position: new Vector3(0.75f, 0, 0),
                            rotation: new Quaternion(0, 0, 0, 0),
                            scale: new Vector3(0.5f, 1f, 0.5f)),

                        new(
                            2,
                            jobName: JobName.Hauler,
                            position: new Vector3(0, 0, 0.75f),
                            rotation: new Quaternion(0, 0, 0, 0),
                            scale: new Vector3(0.5f, 1f, 0.5f)),

                        new(
                            3,
                            jobName: JobName.Hauler,
                            position: new Vector3(-0.75f, 0, 0),
                            rotation: new Quaternion(0, 0, 0, 0),
                            scale: new Vector3(0.5f, 1f, 0.5f)),

                        new(
                            4,
                            jobName: JobName.Hauler,
                            position: new Vector3(0, 0, -0.75f),
                            rotation: new Quaternion(0, 0, 0, 0),
                            scale: new Vector3(0.5f, 1f, 0.5f))

                    }
                },
                {
                    StationName.IdleTemp, new List<WorkPost_DefaultValue>
                    {
                        new(
                            1,
                            jobName: JobName.Idle,
                            position: new Vector3(0.75f, 0, 0),
                            rotation: new Quaternion(0, 0, 0, 0),
                            scale: new Vector3(0.5f, 1f, 0.5f)),

                        new(
                            2,
                            jobName: JobName.Idle,
                            position: new Vector3(0, 0, 0.75f),
                            rotation: new Quaternion(0, 0, 0, 0),
                            scale: new Vector3(0.5f, 1f, 0.5f)),

                        new(
                            3,
                            jobName: JobName.Idle,
                            position: new Vector3(-0.75f, 0, 0),
                            rotation: new Quaternion(0, 0, 0, 0),
                            scale: new Vector3(0.5f, 1f, 0.5f)),

                        new(
                            4,
                            jobName: JobName.Idle,
                            position: new Vector3(0, 0, -0.75f),
                            rotation: new Quaternion(0, 0, 0, 0),
                            scale: new Vector3(0.5f, 1f, 0.5f))

                    }
                }
            };
        }
    }
    
    public class WorkPost_DefaultValue
    {
        public readonly ulong WorkPostID;
        public readonly JobName JobName;
        public readonly Vector3 Position;
        public readonly Quaternion Rotation;
        public readonly Vector3 Scale;
        
        public WorkPost_DefaultValue(ulong workPostID, JobName jobName, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            WorkPostID = workPostID;
            JobName = jobName;
            Position = position;
            Rotation = rotation;
            Scale    = scale;
        }
    }
}