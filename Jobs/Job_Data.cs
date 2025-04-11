using System.Collections.Generic;
using System.Linq;
using ActorActions;
using Actors;
using Station;
using Tools;

namespace Jobs
{
    public class Job_Data : Data_Class
    {
        public readonly ulong ID;
        public readonly ulong StationID;
        
        public ulong ActorID;

        Actor_Data _actor;

        Station_Component _station;
        
        public readonly JobName              JobName;
        public readonly string               JobDescription;
        public readonly List<ActorActionName> JobActions;
        
        public bool            IsWorkerMovingToJob;
        public bool IsBeingOperated;
        
        public ActivityPeriod ActivityPeriod;
        
        public Actor_Data Actor_Data => _actor ??= Actor_Manager.GetActor_Data(ActorID);
        public Station_Component Station => _station ??= Station_Manager.GetStation_Component(StationID);

        public Job_Data(JobName jobName, string jobDescription, List<ActorActionName> jobActions)
        {
            JobName        = jobName;
            JobDescription = jobDescription;
            JobActions       = jobActions;
        }

        public override Dictionary<string, string> GetStringData()
        {
            return new Dictionary<string, string>
            {
                { "Job ID", $"{ID}" },
                { "Job Name", $"{JobName}" },
                { "Station ID", $"{StationID}" },
                { "Worker ID", $"{ActorID}" },
                { "Is Worker Moving To WorkPost", $"{IsWorkerMovingToJob}" },
                { "Job Description", JobDescription }
            };
        }

        public override DataToDisplay GetDataToDisplay(bool toggleMissingDataDebugs)
        {
            _updateDataDisplay(DataToDisplay,
                title: "Base Job Data",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: GetStringData());
            
            _updateDataDisplay(DataToDisplay,
                title: "Job Tasks",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: JobActions.ToDictionary(jobTask => $"{(ulong)jobTask}", jobTask => $"{jobTask}"));

            return DataToDisplay;
        }
    }
}