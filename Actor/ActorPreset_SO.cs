using System.Collections.Generic;
using System.Linq;
using Tools;
using UnityEditor;
using UnityEngine;

namespace Actor
{
    [CreateAssetMenu(fileName = "ActorDataPreset_SO", menuName = "SOList/ActorDataPreset_SO")]
    public class ActorPreset_SO : Data_SO<ActorPreset_Data>
    {
        public Data_Object<ActorPreset_Data>[] ActorDataPresets                                 => DataObjects;
        public Data_Object<ActorPreset_Data>   GetActorDataPreset(ActorDataPresetName actorDataPresetName) => GetDataObject_Master((uint)actorDataPresetName);

        public override uint GetDataObjectID(int id) => (uint)ActorDataPresets[id].DataObject.ActorDataPresetName; // Use the ActorDataPresetName

        public void PopulateDefaultActorDataPresets()
        {
            if (_defaultActorDataPresets.Count == 0)
            {
                Debug.Log("No Default Actor Data Presets Found");
            }
        }
        protected override Dictionary<uint, Data_Object<ActorPreset_Data>> _populateDefaultDataObjects()
        {
            var defaultActorDataPresets = new Dictionary<uint, ActorPreset_Data>();

            foreach (var actorDataPreset in ActorPreset_List.GetAllDefaultActorDataPresets())
            {
                defaultActorDataPresets.Add(actorDataPreset.Key, actorDataPreset.Value);
            }
            
            return _convertDictionaryToDataObject(defaultActorDataPresets);
        }

        protected override Data_Object<ActorPreset_Data> _convertToDataObject(ActorPreset_Data data)
        {
            return new Data_Object<ActorPreset_Data>(
                dataObjectID: (uint)data.ActorDataPresetName,
                dataObject: data,
                dataObjectTitle: $"{(uint)data.ActorDataPresetName}: {data.ActorDataPresetName}",
                data_Display: data.DataSO_Object(ToggleMissingDataDebugs));
        }
        
        Dictionary<uint, Data_Object<ActorPreset_Data>> _defaultActorDataPresets => DefaultDataObjects;
    }
    
     [CustomEditor(typeof(ActorPreset_SO))]
    public class AllActorDataPresets_SOEditor : Data_SOEditor<ActorPreset_Data>
    {
        public override Data_SO<ActorPreset_Data> SO => _so ??= (ActorPreset_SO)target;
    }
}
