using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Tools;
using UnityEditor;
using UnityEngine;

namespace Actor
{
    [CreateAssetMenu(fileName = "Actor_SO", menuName = "SOList/Actor_SO")]
    [Serializable]
    public class Actor_SO : Data_Component_SO<Actor_Data, Actor_Component>
    {
        public Data<Actor_Data>[] Actors                      => Data;
        public Data<Actor_Data>   GetActor_Data(uint actorID) => GetData(actorID);
        public Actor_Component GetActor_Component(uint actorID) => 

        public override uint GetDataID(int id) => Actors[id].Data_Object.ActorID;

        public void UpdateActor(uint actorID, Actor_Data actor_Data) => 
            UpdateData(actorID, actor_Data);
        
        public void UpdateAllActors(Dictionary<uint, Actor_Data> allActors) => UpdateAllData(allActors);

        protected override Dictionary<uint, Data<Actor_Data>> _getDefaultData() =>
            _convertDictionaryToData(Actor_List.DefaultActors);

        protected override Dictionary<uint, Data<Actor_Data>> _getSavedData() =>
            _convertDictionaryToData(LoadSO());

        protected override Dictionary<uint, Data<Actor_Data>> _getSceneData() =>
            _convertDictionaryToData(_getSceneComponents().ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ActorData));
        

        static uint _lastUnusedActorID = 1;

        public uint GetUnusedActorID()
        {
            while (DataIndexLookup.ContainsKey(_lastUnusedActorID))
            {
                _lastUnusedActorID++;
            }

            return _lastUnusedActorID;
        }
        
        protected override Data<Actor_Data> _convertToData(Actor_Data data)
        {
            return new Data<Actor_Data>(
                dataID: data.ActorID, 
                data_Object: data,
                dataTitle: $"{data.ActorID}: {data.ActorName}",
                getData_Display: data.GetDataSO_Object);
        }
    }

    [CustomEditor(typeof(Actor_SO))]
    public class Actor_SOEditor : Data_SOEditor<Actor_Data>
    {
        public override Data_SO<Actor_Data> SO => _so ??= (Actor_SO)target;
    }
}