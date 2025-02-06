using System;
using System.Collections.Generic;
using ActorActions;
using DateAndTime;
using Inventory;
using Priorities;
using Priority;
using Tools;

namespace Actor
{
    [Serializable]
    public class Actor_Data_Identification : Priority_Class
    {
        public Actor_Data_Identification(ulong actorID, ActorName actorName, ulong actorFactionID, ulong actorCityID,
            Date actorBirthDate = null) : base(actorID, ComponentType.Actor)
        {
            ActorID = actorID;
            ActorName = actorName;
            ActorFactionID = actorFactionID;
            ActorCityID = actorCityID;
            ActorBirthDate = actorBirthDate ?? new Date(Manager_DateAndTime.GetCurrentTotalDays());
        }

        public Actor_Data_Identification(Actor_Data_Identification actorDataIdentification) : base(actorDataIdentification.ActorID,
            ComponentType.Actor)
        {
            ActorID = actorDataIdentification.ActorID;
            ActorName = new ActorName(actorDataIdentification.ActorName.Name, actorDataIdentification.ActorName.Surname);
            ActorFactionID = actorDataIdentification.ActorFactionID;
            ActorCityID = actorDataIdentification.ActorCityID;
            ActorBirthDate = new Date(actorDataIdentification.ActorBirthDate);
        }

        public override DataToDisplay GetDataToDisplay(bool toggleMissingDataDebugs)
        {
            _updateDataDisplay(DataToDisplay,
                title: "Full Identification",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: GetStringData());

            return DataToDisplay;
        }

        public override Dictionary<string, string> GetStringData() => new()
        {
            { "Actor ID", $"{ActorID}" },
            { "Actor Name", $"{ActorName.GetName()}" },
            { "ActorFaction", $"{ActorFactionID}" },
            { "Actor City ID", $"{ActorCityID}" }
        };

        public ulong ActorID;
        public ActorName ActorName;
        public ulong ActorFactionID;
        public ulong ActorCityID;
        public Date ActorBirthDate;
        public float ActorAge => ActorBirthDate.GetAge();
        public Family ActorFamily;
        public Background Background;

        public override List<ActorActionName> GetAllowedActions()
        {
            return new List<ActorActionName>();
        }
    }
    
    [Serializable]
    public class ActorName
    {
        public string Name;
        public string Surname;
        public string GetName() => $"{Name} {Surname}";
        public TitleName CurrentTitle;
        public List<TitleName> AvailableTitles;

        public void SetTitleAsCurrentTitle(TitleName titleName)
        {
            if (AvailableTitles.Contains(titleName)) CurrentTitle = titleName;
        }

        public ActorName(string name, string surname)
        {
            Name = name;
            Surname = surname;
        }
    }
    
    [Serializable]
    public class Background : Priority_Class
    {
        public Background(ulong actorID, string birthplace, Date birthdate, Family actorFamily, Dynasty actorDynasty,
            string religion) : base(actorID, ComponentType.Actor)
        {
            Birthplace = birthplace;
            Birthdate = birthdate;
            ActorFamily = actorFamily;
            ActorDynasty = actorDynasty;
            Religion = religion;
        }
        
        public override List<ActorActionName> GetAllowedActions()
        {
            return new List<ActorActionName>();
        }
        
        public override DataToDisplay GetDataToDisplay(bool toggleMissingDataDebugs)
        {
            _updateDataDisplay(DataToDisplay,
                title: "Background",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: GetStringData());

            return DataToDisplay;
        }
        
        public override Dictionary<string, string> GetStringData()
        {
            return new Dictionary<string, string>
            {
                { "Birthplace", $"{Birthplace}" },
                { "Birthdate", $"{Birthdate}" },
                { "Family", $"{ActorFamily}" },
                { "Dynasty", $"{ActorDynasty}" },
                { "Religion", $"{Religion}" }
            };
        }

        public string Birthplace;
        public Date Birthdate;
        public Family ActorFamily;
        public Dynasty ActorDynasty;
        public string Religion;
    }
}