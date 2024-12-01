using System.Collections.Generic;
using ScriptableObjects;
using UnityEngine;

namespace Actor
{
    public class ActorDataPreset_SO : Base_SO<Actor_Data>
    {
        public Actor_Data[] ActorDataPresets                                 => Objects;
        public Actor_Data   GetActorDataPreset(ActorDataPresetName careerName) => GetObject_Master((uint)careerName);

        public override uint GetObjectID(int id) => (uint)ActorDataPresets[id].FullIdentification.ActorDataPresetName; // Use the ActorDataPresetName

        public void PopulateDefaultActorDataPresets()
        {
            if (_defaultActorDataPresets.Count == 0)
            {
                Debug.Log("No Default Actor Data Presets Found");
            }
        }
        protected override Dictionary<uint, Actor_Data> _populateDefaultObjects()
        {
            var defaultActorDataPresets = new Dictionary<uint, Actor_Data>();

            foreach (var actorDataPreset in ActorDataPreset_List.GetAllDefaultActorDataPresets())
            {
                defaultActorDataPresets.Add(actorDataPreset.Key, actorDataPreset.Value);
            }

            return defaultActorDataPresets;
        }
        
        Dictionary<uint, Actor_Data> _defaultActorDataPresets => DefaultObjects;
    }
}
