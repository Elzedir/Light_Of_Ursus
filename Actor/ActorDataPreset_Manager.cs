using UnityEditor;
using UnityEngine;

namespace Actor
{
    public abstract class ActorDataPreset_Manager
    {
        const string _actorDataPresetSOPath = "ScriptableObjects/ActorDataPreset_SO";

        static ActorDataPreset_SO _actorDataPreset_SO;

        static  ActorDataPreset_SO ActorDataPreset_SO =>
            _actorDataPreset_SO ??= _getOrCreateAllActorDataPresetSO();

        public static Actor_Data GetActorDataPreset(ActorDataPresetName actorDataPresetName) =>
            ActorDataPreset_SO.GetActorDataPreset(actorDataPresetName);

        static ActorDataPreset_SO _getOrCreateAllActorDataPresetSO()
        {
            var actorDataPresetSO = Resources.Load<ActorDataPreset_SO>(_actorDataPresetSOPath);

            if (actorDataPresetSO is not null) return actorDataPresetSO;

            actorDataPresetSO = ScriptableObject.CreateInstance<ActorDataPreset_SO>();
            AssetDatabase.CreateAsset(actorDataPresetSO, $"Assets/Resources/{_actorDataPresetSOPath}");
            AssetDatabase.SaveAssets();

            return actorDataPresetSO;
        }
    }

    public enum ActorDataPresetName
    {
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
    }
}
