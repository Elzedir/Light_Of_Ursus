using UnityEditor;
using UnityEngine;

namespace Actor
{
    public abstract class ActorDataPreset_Manager
    {
        const string _actorDataPreset_SOPath = "ScriptableObjects/ActorDataPreset_SO";

        static ActorDataPreset_SO _actorDataPreset_SO;

        static  ActorDataPreset_SO ActorDataPreset_SO =>
            _actorDataPreset_SO ??= _getActorDataPreset_SO();

        public static Actor_Data GetActorDataPreset(ActorDataPresetName actorDataPresetName) =>
            ActorDataPreset_SO.GetActorDataPreset(actorDataPresetName);
        
        public static void PopulateAllActorDataPresets()
        {
            ActorDataPreset_SO.PopulateDefaultActorDataPresets();
            // Then populate custom actor data presets.
        }

        static ActorDataPreset_SO _getActorDataPreset_SO()
        {
            var actorDataPreset_SO = Resources.Load<ActorDataPreset_SO>(_actorDataPreset_SOPath);

            if (actorDataPreset_SO is not null) return actorDataPreset_SO;

            Debug.LogError("ActorDataPreset_SO not found. Creating temporary ActorDataPreset_SO.");
            actorDataPreset_SO = ScriptableObject.CreateInstance<ActorDataPreset_SO>();

            return actorDataPreset_SO;
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
        
        Lumberjack_Beginner,
        Lumberjack_Novice,
        Lumberjack_Apprentice,
        Lumberjack_Journeyman,
        Lumberjack_Expert,
        Lumberjack_Master,
        
        Smith_Beginner,
        Smith_Novice,
        Smith_Apprentice,
        Smith_Journeyman,
        Smith_Expert,
        Smith_Master,
    }
}
