using System.Collections.Generic;
using DataPersistence;
using Tools;
using UnityEditor;
using UnityEngine;

namespace Actor
{
    [CreateAssetMenu(fileName = "ActorDataPreset_SO", menuName = "SOList/ActorDataPreset_SO")]
    public class ActorPreset_SO : Data_SO<ActorPreset_Data>
    {
        public Data<ActorPreset_Data>[] ActorDataPresets => Data;
        public Data<ActorPreset_Data>   GetActorDataPreset(ActorDataPresetName actorDataPresetName) => GetData((uint)actorDataPresetName);

        public override uint GetDataID(int id) => (uint)ActorDataPresets[id].Data_Object.ActorDataPresetName; // Use the ActorDataPresetName
        
        protected override Dictionary<uint, Data<ActorPreset_Data>> _getDefaultData()
        {
            return _convertDictionaryToData(ActorPreset_List.DefaultActorDataPresets);
        }
        
        protected override Data<ActorPreset_Data> _convertToData(ActorPreset_Data dataObject)
        {
            return new Data<ActorPreset_Data>(
                dataID: (uint)dataObject.ActorDataPresetName,
                data_Object: dataObject,
                dataTitle: $"{(uint)dataObject.ActorDataPresetName}: {dataObject.ActorDataPresetName}",
                getData_Display: dataObject.GetData_Display);
        }
    }
    
     [CustomEditor(typeof(ActorPreset_SO))]
    public class AllActorDataPresets_SOEditor : Data_SOEditor<ActorPreset_Data>
    {
        public override Data_SO<ActorPreset_Data> SO => _so ??= (ActorPreset_SO)target;
    }
}
