using System.Collections.Generic;
using System.Linq;
using Tools;
using UnityEditor;
using UnityEngine;

namespace Actor
{
    [CreateAssetMenu(fileName = "ActorDataPreset_SO", menuName = "SOList/ActorDataPreset_SO")]
    public class ActorDataPreset_SO : Data_SO<Actor_Data>
    {
        public Data_Object<Actor_Data>[] ActorDataPresets                                 => DataObjects;
        public Data_Object<Actor_Data>   GetActorDataPreset(ActorDataPresetName actorDataPresetName) => GetDataObject_Master((uint)actorDataPresetName);

        public override uint GetDataObjectID(int id) => (uint)ActorDataPresets[id].DataObject.ActorDataPresetName; // Use the ActorDataPresetName

        public void PopulateDefaultActorDataPresets()
        {
            if (_defaultActorDataPresets.Count == 0)
            {
                Debug.Log("No Default Actor Data Presets Found");
            }
        }
        protected override Dictionary<uint, Data_Object<Actor_Data>> _populateDefaultDataObjects()
        {
            var defaultActorDataPresets = new Dictionary<uint, Actor_Data>();

            foreach (var actorDataPreset in ActorDataPreset_List.GetAllDefaultActorDataPresets())
            {
                defaultActorDataPresets.Add(actorDataPreset.Key, actorDataPreset.Value);
            }
            
            return _convertDictionaryToDataObject(defaultActorDataPresets);
        }

        protected override Data_Object<Actor_Data> _convertToDataObject(Actor_Data data)
        {
            return new Data_Object<Actor_Data>(
                dataObjectID: data.ActorID,
                dataObject: data,
                dataObjectTitle: $"{data.ActorName}{data.ActorName}",
                dataSO_Object: data.DataSO_Object);
        }
        
        Dictionary<uint, Data_Object<Actor_Data>> _defaultActorDataPresets => DefaultDataObjects;
    }
    
     [CustomEditor(typeof(ActorDataPreset_SO))]
    public class AllActorDataPresets_SOEditor : Data_SOEditor<Actor_Data>
    {
        public override Data_SO<Actor_Data> SO => _so ??= (ActorDataPreset_SO)target;
    }
}
