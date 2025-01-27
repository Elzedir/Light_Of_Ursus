using System.Collections.Generic;
using Tools;
using UnityEditor;
using UnityEngine;

namespace ActorPresets
{
    [CreateAssetMenu(fileName = "ActorDataPreset_SO", menuName = "SOList/ActorDataPreset_SO")]
    public class ActorPreset_SO : Data_SO<ActorPreset_Data>
    {
        public Data<ActorPreset_Data>[] ActorDataPresets => Data;
        public Data<ActorPreset_Data>   GetActorDataPreset(ActorDataPresetName actorDataPresetName) => GetData((ulong)actorDataPresetName);
        
        protected override Dictionary<ulong, Data<ActorPreset_Data>> _getDefaultData()
        {
            return _convertDictionaryToData(ActorPreset_List.DefaultActorDataPresets);
        }
        
        protected override Data<ActorPreset_Data> _convertToData(ActorPreset_Data data)
        {
            return new Data<ActorPreset_Data>(
                dataID: (ulong)data.ActorDataPresetName,
                data_Object: data,
                dataTitle: $"{(ulong)data.ActorDataPresetName}: {data.ActorDataPresetName}",
                getDataToDisplay: data.GetDataToDisplay);
        }
    }
    
     [CustomEditor(typeof(ActorPreset_SO))]
    public class AllActorDataPresets_SOEditor : Data_SOEditor<ActorPreset_Data>
    {
        public override Data_SO<ActorPreset_Data> SO => _so ??= (ActorPreset_SO)target;
    }
}
