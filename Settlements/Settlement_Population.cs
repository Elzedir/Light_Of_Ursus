using System;
using System.Collections.Generic;
using System.Linq;
using Tools;

namespace Settlements
{
    [Serializable]
    public class Settlement_Population : Data_Class
    {
        public float CurrentPopulation;
        public float MaxPopulation;
        public float ExpectedPopulation;
        public SerializableHashSet<ulong> AllCitizenIDs = new();

        public Settlement_Population(Settlement_Population data)
        {
            CurrentPopulation = data.CurrentPopulation;
            MaxPopulation = data.MaxPopulation;
            ExpectedPopulation = data.ExpectedPopulation;
            
            foreach (var citizenID in data.AllCitizenIDs)
            {
                AllCitizenIDs.Add(citizenID);
            }
        }

        public Settlement_Population(List<ulong> allCitizenIDList, float maxPopulation, float expectedPopulation)
        {
            foreach (var citizenID in allCitizenIDList)
            {
                AllCitizenIDs.Add(citizenID);
            }

            CurrentPopulation = allCitizenIDList.Count;
            MaxPopulation = maxPopulation;
            ExpectedPopulation = expectedPopulation;
        }

        public void OnProgressDay()
        {
            
        }

        public override DataToDisplay GetDataToDisplay(bool toggleMissingDataDebugs)
        {
            _updateDataDisplay(DataToDisplay,
                title: "Population Data",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: GetStringData());

            _updateDataDisplay(DataToDisplay,
                title: "Citizen IDs",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: AllCitizenIDs.ToDictionary(citizenID => $"{citizenID}", citizenID => $"{citizenID}"));

            return DataToDisplay;
        }

        public override Dictionary<string, string> GetStringData()
        {
            return new Dictionary<string, string>
            {
                { "Current Population", $"{CurrentPopulation}" },
                { "Max Population", $"{MaxPopulation}" },
                { "Expected Population", $"{ExpectedPopulation}" }
            };
        }
    }
}