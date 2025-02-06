using System;
using System.Collections.Generic;
using System.Linq;
using ActorActions;
using Inventory;
using Priorities;
using Priority;
using Recipes;
using Tools;
using UnityEngine;

namespace Actor
{
    [Serializable]
    public class Actor_Data_Vocation : Priority_Class
    {
        public Actor_Data_Vocation(ulong actorID, Dictionary<VocationName, ActorVocation> actorVocations = null) : base(
            actorID,
            ComponentType.Actor)
        {
            ActorVocations = actorVocations ?? new Dictionary<VocationName, ActorVocation>();
        }

        public Actor_Data_Vocation(Actor_Data_Vocation actorDataVocation) : base(actorDataVocation.ActorReference.ActorID,
            ComponentType.Actor)
        {
            ActorVocations = new Dictionary<VocationName, ActorVocation>(actorDataVocation.ActorVocations);
        }
        
        public override List<ActorActionName> GetAllowedActions()
        {
            //* Able to perform actions based on vocations. For example, advanced metalworking or smithing, etc.
            return new List<ActorActionName>();
        }
        
        public override DataToDisplay GetDataToDisplay(bool toggleMissingDataDebugs)
        {
            _updateDataDisplay(DataToDisplay,
                title: "Vocation Data",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: GetStringData());

            return DataToDisplay;
        }

        public override Dictionary<string, string> GetStringData()
        {
            return ActorVocations.Values.ToDictionary(vocation => $"{vocation.VocationName}:",
                vocation => $"{vocation.VocationExperience}");
        }

        public ComponentReference_Actor ActorReference => Reference as ComponentReference_Actor;

        public Dictionary<VocationName, ActorVocation> ActorVocations;
        public void SetVocations(Dictionary<VocationName, ActorVocation> vocations) => ActorVocations = vocations;

        public void AddVocation(VocationName vocationName, float vocationExperience)
        {
            if (ActorVocations.ContainsKey(vocationName))
            {
                Debug.Log($"Vocation: {vocationName} already exists in Vocations.");
                return;
            }

            ActorVocations.Add(vocationName, new ActorVocation(vocationName, vocationExperience));
        }

        public void RemoveVocation(VocationName vocationName)
        {
            if (!ActorVocations.ContainsKey(vocationName))
            {
                Debug.Log($"Vocation: {vocationName} does not exist in Vocations.");
                return;
            }

            ActorVocations.Remove(vocationName);
        }

        public void ChangeVocationExperience(VocationName vocationName, float experienceChange)
        {
            if (!ActorVocations.TryGetValue(vocationName, out var vocation))
            {
                Debug.Log($"Vocation: {vocationName} does not exist in Vocations.");
                return;
            }

            vocation.VocationExperience += experienceChange;
        }

        public float GetVocationExperience(VocationName vocationName)
        {
            if (ActorVocations.TryGetValue(vocationName, out var vocation)) return vocation.VocationExperience;

            Debug.LogError($"Vocation: {vocationName} does not exist in Vocations.");
            return -1;

        }

        public float GetProgress(VocationRequirement vocationRequirement)
        {
            var currentExperience = GetVocationExperience(vocationRequirement.VocationName);

            if (currentExperience < vocationRequirement.MinimumVocationExperience)
                return 0;

            var progress = (currentExperience - vocationRequirement.ExpectedVocationExperience) /
                            Math.Max(currentExperience, 1);

            if (progress < 0) return 1 / Math.Abs(progress);

            return progress;
        }
    }
    
    [Serializable]
    public class ActorVocation
    {
        public VocationName VocationName;
        public VocationTitle VocationTitle;
        public float VocationExperience;

        public ActorVocation(VocationName vocationName, float vocationExperience)
        {
            VocationName = vocationName;
            VocationExperience = vocationExperience;

            // Implement later
            //VocationTitle = Manager_Vocation.GetVocation(vocationName).GetVocationTitle(vocationExperience);
        }
    }
}