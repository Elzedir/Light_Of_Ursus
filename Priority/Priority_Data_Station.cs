namespace Priority
{
    public class PriorityComponent_Station// : PriorityComponent
    {
        // protected override List<uint> _canPeek(List<uint> priorityIDs)
        // {
        //     var allowedPriorities = new List<uint>();
        //
        //     foreach (var priorityID in priorityIDs)
        //     {
        //         if (priorityID is (uint)JobTaskName.Idle)
        //         {
        //             Debug.LogError(
        //                 $"ActorActionName: {(ActorActionName)priorityID} not allowed in PeekHighestSpecificPriority.");
        //             continue;
        //         }
        //
        //         allowedPriorities.Add(priorityID);
        //     }
        //
        //     return allowedPriorities;
        // }
        //
        // protected override PriorityQueue _createPriorityQueue(uint priorityQueueID)
        // {
        //     return null;
        // }
        //
        // protected override PriorityElement _createPriorityElement(uint priorityQueueID, uint priorityID,
        //                                                           Dictionary<PriorityParameterName, object>
        //                                                               priorityParameters)
        // {
        //     return new PriorityElement(PriorityType.JobTask, priorityQueueID, priorityID,
        //         priorityParameters ?? Manager_JobTask.GetDefaultTaskParameters((JobTaskName)priorityID));
        // }
        //
        // protected override void _regeneratePriority(uint priorityQueueID, uint priorityID)
        // {
        //     if (!_priorityExists(priorityQueueID, priorityID, out var existingPriorityParameters)) return;
        //
        //     // We need a get all parameters function to fill the priority again. Or maybe just complete the action and then recalculate the
        //     // priorities rather than clearing them.
        //
        //     var newPriorities = ;
        //
        //     AllPriorities[priorityQueueID].Update(priorityID, newPriorities);
        // }
        //
        // protected override ObservableDictionary<uint, PriorityQueue> _getRelevantPriorityQueues(
        //     PriorityState priorityState)
        // {
        //     return AllPriorities;
        // }
        //
        // public PriorityComponent_Station(uint stationID)
        // {
        //     _stationReferences = new ComponentReference_Station(stationID);
        // }
        //
        // readonly ComponentReference_Station _stationReferences;
        //
        // public uint      StationID => _stationReferences.StationID;
        // StationComponent _station  => _stationReferences.Station;
        //
        // protected override Dictionary<DataChanged, List<PriorityToChange>> _prioritiesToChange { get; } = new()
        // {
        //
        // };
    }
}
