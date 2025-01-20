using Actor;
using UnityEngine;

namespace ActorPreset
{
    public abstract class ActorPreset_Manager
    {
        const string _actorDataPreset_SOPath = "ScriptableObjects/ActorDataPreset_SO";

        static ActorPreset_SO _actorPreset_SO;

        static  ActorPreset_SO ActorPreset_SO =>
            _actorPreset_SO ??= _getActorDataPreset_SO();

        public static ActorPreset_Data GetActorDataPreset(ActorDataPresetName actorDataPresetName) =>
            ActorPreset_SO.GetActorDataPreset(actorDataPresetName)?.Data_Object;

        static ActorPreset_SO _getActorDataPreset_SO()
        {
            var actorDataPreset_SO = Resources.Load<ActorPreset_SO>(_actorDataPreset_SOPath);

            if (actorDataPreset_SO is not null) return actorDataPreset_SO;

            Debug.LogError("ActorDataPreset_SO not found. Creating temporary ActorDataPreset_SO.");
            actorDataPreset_SO = ScriptableObject.CreateInstance<ActorPreset_SO>();

            return actorDataPreset_SO;
        }
        
        public static void ClearSOData()
        {
            ActorPreset_SO.ClearSOData();
        }
    }

    public enum ActorDataPresetName
    {
        No_Preset,
        
        Wanderer_Beginner,
        Wanderer_Novice,
        Wanderer_Apprentice,
        Wanderer_Journeyman,
        Wanderer_Expert,
        Wanderer_Master,
        
        Logger_Beginner,
        Logger_Novice,
        Logger_Apprentice,
        Logger_Journeyman,
        Logger_Expert,
        Logger_Master,
        
        Smith_Beginner,
        Smith_Novice,
        Smith_Apprentice,
        Smith_Journeyman,
        Smith_Expert,
        Smith_Master,
    }
}
