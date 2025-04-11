using System;
using System.Collections.Generic;
using ActorActions;
using Station;
using UnityEngine;

namespace Jobs
{
    public abstract class Job_List
    {
        static Dictionary<ulong, Job_Data> s_defaultJobs;
        public static Dictionary<ulong, Job_Data> S_DefaultJobs => s_defaultJobs ??= _initialiseDefaultJobs();
        
        public static Job_Data GetJob_Data(JobName jobName)
        {
            if (S_DefaultJobs.TryGetValue((ulong)jobName, out var jobData))
            {
                return jobData;
            }

            throw new Exception($"Job ID: {jobName} not found in Job List.");
        }
        
        static Dictionary<ulong, Job_Data> _initialiseDefaultJobs()
        {
            return new Dictionary<ulong, Job_Data>
            {
                {
                    (ulong)JobName.Idle, new Job_Data(
                        jobName: JobName.Idle,
                        jobDescription: "An idler",
                        jobActions: new List<ActorActionName>
                        {
                            ActorActionName.Idle,
                        })
                },
                {
                    (ulong)JobName.Logger, new Job_Data(
                        jobName: JobName.Logger,
                        jobDescription: "A logger",
                        jobActions: new List<ActorActionName>
                        {
                            ActorActionName.Chop_Wood
                        })
                },
                {
                    (ulong)JobName.Sawyer, new Job_Data(
                        jobName: JobName.Sawyer,
                        jobDescription: "A sawyer",
                        jobActions: new List<ActorActionName>
                        {
                            ActorActionName.Process_Logs
                        })
                },
                {
                    (ulong)JobName.Hauler, new Job_Data(
                        jobName: JobName.Hauler,
                        jobDescription: "A hauler",
                        jobActions: new List<ActorActionName>
                        {
                            ActorActionName.Haul
                        })
                },
                {
                    (ulong)JobName.Vendor, new Job_Data(
                        jobName: JobName.Vendor,
                        jobDescription: "A vendor",
                        jobActions: new List<ActorActionName>
                        {
                            ActorActionName.Stand_At_Counter,
                            ActorActionName.Restock_Shelves
                        })
                },
                {
                    (ulong)JobName.Smith, new Job_Data(
                        jobName: JobName.Smith,
                        jobDescription: "Smith something",
                        jobActions: new List<ActorActionName>
                        {
                            ActorActionName.Beat_Metal
                        })
                }
            };
        }
        
        static Dictionary<StationName, List<Job_Prefabs>> s_station_JobPrefabs;

        public static Dictionary<StationName, List<Job_Prefabs>> S_Station_JobPrefabs =>
            s_station_JobPrefabs ??= _initialiseStation_JobPrefabs();
        
        public static List<Job_Prefabs> GetStation_JobPrefabs(StationName stationName)
        {
            if (S_Station_JobPrefabs.TryGetValue(stationName, out var positions))
            {
                return positions;
            }

            throw new Exception($"StationName: {stationName} not found in JobPrefabs.");
        }

        static Dictionary<StationName, List<Job_Prefabs>> _initialiseStation_JobPrefabs()
        {
            return new Dictionary<StationName, List<Job_Prefabs>>
            {
                {
                    StationName.Tree, new List<Job_Prefabs>
                    {

                        new(
                            name: JobName.Logger,
                            position: new Vector3(1.5f, -0.8f, 0),
                            rotation: new Quaternion(0, 0, 0, 0),
                            scale: new Vector3(1, 0.333f, 1)),

                        new(
                            name: JobName.Logger,
                            position: new Vector3(0, -0.8f, 1.5f),
                            rotation: new Quaternion(0, 0, 0, 0),
                            scale: new Vector3(1, 0.333f, 1)),

                        new(
                            name: JobName.Logger,
                            position: new Vector3(-1.5f, -0.8f, 0),
                            rotation: new Quaternion(0, 0, 0, 0),
                            scale: new Vector3(1, 0.333f, 1)),

                        new(
                            name: JobName.Logger,
                            position: new Vector3(0, -0.8f, -1.5f),
                            rotation: new Quaternion(0, 0, 0, 0),
                            scale: new Vector3(1, 0.333f, 1))

                    }
                },
                {
                    StationName.Sawmill, new List<Job_Prefabs>
                    {

                        new(
                            name: JobName.Sawyer,
                            position: new Vector3(0.75f, 0, 0),
                            rotation: new Quaternion(0, 0, 0, 0),
                            scale: new Vector3(0.333f, 1, 1)),

                        new(
                            name: JobName.Sawyer,
                            position: new Vector3(0, 0, 1),
                            rotation: new Quaternion(0, 0, 0, 0),
                            scale: new Vector3(0.333f, 1, 1)),

                        new(
                            name: JobName.Sawyer,
                            position: new Vector3(-0.75f, 0, 0),
                            rotation: new Quaternion(0, 0, 0, 0),
                            scale: new Vector3(0.333f, 1, 1)),

                        new(
                            name: JobName.Sawyer,
                            position: new Vector3(0, 0, -1),
                            rotation: new Quaternion(0, 0, 0, 0),
                            scale: new Vector3(0.333f, 1, 1))

                    }
                },
                {
                    StationName.Log_Pile, new List<Job_Prefabs>
                    {

                        new(
                            name: JobName.Hauler,
                            position: new Vector3(0.75f, 0, 0),
                            rotation: new Quaternion(0, 0, 0, 0),
                            scale: new Vector3(0.5f, 1f, 0.5f)),

                        new(
                            name: JobName.Hauler,
                            position: new Vector3(0, 0, 0.75f),
                            rotation: new Quaternion(0, 0, 0, 0),
                            scale: new Vector3(0.5f, 1f, 0.5f)),

                        new(
                            name: JobName.Hauler,
                            position: new Vector3(-0.75f, 0, 0),
                            rotation: new Quaternion(0, 0, 0, 0),
                            scale: new Vector3(0.5f, 1f, 0.5f)),

                        new(
                            name: JobName.Hauler,
                            position: new Vector3(0, 0, -0.75f),
                            rotation: new Quaternion(0, 0, 0, 0),
                            scale: new Vector3(0.5f, 1f, 0.5f))

                    }
                },
                {
                    StationName.IdleTemp, new List<Job_Prefabs>
                    {
                        new(
                            name: JobName.Idle,
                            position: new Vector3(0.75f, 0, 0),
                            rotation: new Quaternion(0, 0, 0, 0),
                            scale: new Vector3(0.5f, 1f, 0.5f)),

                        new(
                            name: JobName.Idle,
                            position: new Vector3(0, 0, 0.75f),
                            rotation: new Quaternion(0, 0, 0, 0),
                            scale: new Vector3(0.5f, 1f, 0.5f)),

                        new(
                            name: JobName.Idle,
                            position: new Vector3(-0.75f, 0, 0),
                            rotation: new Quaternion(0, 0, 0, 0),
                            scale: new Vector3(0.5f, 1f, 0.5f)),

                        new(
                            name: JobName.Idle,
                            position: new Vector3(0, 0, -0.75f),
                            rotation: new Quaternion(0, 0, 0, 0),
                            scale: new Vector3(0.5f, 1f, 0.5f))

                    }
                }
            };
        }
    }
    
    public class Job_Prefabs
    {
        public readonly JobName Name;
        public readonly Vector3 Position;
        public readonly Quaternion Rotation;
        public readonly Vector3 Scale;
        
        public Job_Prefabs(JobName name, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            Name = name;
            Position = position;
            Rotation = rotation;
            Scale    = scale;
        }
    }
}
