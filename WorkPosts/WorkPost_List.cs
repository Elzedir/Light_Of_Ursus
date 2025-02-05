using System.Collections.Generic;
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
        
        public static List<WorkPost_DefaultValue> GetWorkPlace_DefaultValues(StationName stationName)
        {
            if (S_WorkPosts_DefaultValues.TryGetValue(stationName, out var positions))
            {
                return positions;
            }

            Debug.Log($"StationName: {stationName} not found in WorkPlacePositions.");
            return null;
        }

        static Dictionary<StationName, List<WorkPost_DefaultValue>> _initialiseWorkPost_DefaultValues()
        {
            return new Dictionary<StationName, List<WorkPost_DefaultValue>>
            {
                {
                    StationName.Tree, new List<WorkPost_DefaultValue>
                    {

                        new(
                            jobName: JobName.Logger,
                            position: new Vector3(1.5f, -0.8f, 0),
                            rotation: new Quaternion(0, 0, 0, 0),
                            scale: new Vector3(1, 0.333f, 1)),

                        new(
                            jobName: JobName.Logger,
                            position: new Vector3(0, -0.8f, 1.5f),
                            rotation: new Quaternion(0, 0, 0, 0),
                            scale: new Vector3(1, 0.333f, 1)),

                        new(
                            jobName: JobName.Logger,
                            position: new Vector3(-1.5f, -0.8f, 0),
                            rotation: new Quaternion(0, 0, 0, 0),
                            scale: new Vector3(1, 0.333f, 1)),

                        new(
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
                            jobName: JobName.Sawyer,
                            position: new Vector3(0.75f, 0, 0),
                            rotation: new Quaternion(0, 0, 0, 0),
                            scale: new Vector3(0.333f, 1, 1)),

                        new(
                            jobName: JobName.Sawyer,
                            position: new Vector3(0, 0, 1),
                            rotation: new Quaternion(0, 0, 0, 0),
                            scale: new Vector3(0.333f, 1, 1)),

                        new(
                            jobName: JobName.Sawyer,
                            position: new Vector3(-0.75f, 0, 0),
                            rotation: new Quaternion(0, 0, 0, 0),
                            scale: new Vector3(0.333f, 1, 1)),

                        new(
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
                            jobName: JobName.Sawyer,
                            position: new Vector3(0.75f, 0, 0),
                            rotation: new Quaternion(0, 0, 0, 0),
                            scale: new Vector3(0.5f, 1f, 0.5f)),

                        new(
                            jobName: JobName.Sawyer,
                            position: new Vector3(0, 0, 0.75f),
                            rotation: new Quaternion(0, 0, 0, 0),
                            scale: new Vector3(0.5f, 1f, 0.5f)),

                        new(
                            jobName: JobName.Sawyer,
                            position: new Vector3(-0.75f, 0, 0),
                            rotation: new Quaternion(0, 0, 0, 0),
                            scale: new Vector3(0.5f, 1f, 0.5f)),

                        new(
                            jobName: JobName.Sawyer,
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
        //* Change JobName to AllPossibleJobs
        public JobName JobName;
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Scale;
        
        public WorkPost_DefaultValue(JobName jobName, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            JobName = jobName;
            Position = position;
            Rotation = rotation;
            Scale    = scale;
        }
    }
}