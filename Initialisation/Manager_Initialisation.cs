using System;

namespace Initialisation
{
    public abstract class Manager_Initialisation
    {
        public static event Action OnInitialiseManagerFaction;
        public static event Action OnInitialiseManagerActor;
        
        public static event Action OnInitialiseManagerRegion;
        public static event Action OnInitialiseManagerCity;
        public static event Action OnInitialiseManagerJobSite;
        public static event Action OnInitialiseManagerStation;
        public static event Action OnInitialiseManagerOrder;

        public static event Action OnInitialiseFactions;
        public static event Action OnInitialiseActors;

        public static event Action OnInitialiseRegions;
        public static event Action OnInitialiseCities;
        public static event Action OnInitialiseJobSites;
        public static event Action OnInitialiseStations;

        public static void InitialiseManagers() 
        {
            OnInitialiseManagerFaction?.Invoke();
            OnInitialiseManagerActor?.Invoke();
            
            OnInitialiseManagerRegion?.Invoke();
            OnInitialiseManagerCity?.Invoke();
            OnInitialiseManagerJobSite?.Invoke();
            OnInitialiseManagerStation?.Invoke();
            OnInitialiseManagerOrder?.Invoke();
        }
        
        public static void InitialiseFactions()
        {
            OnInitialiseFactions?.Invoke();
        }

        public static void InitialiseActors()
        {
            OnInitialiseActors?.Invoke();
        }

        public static void InitialiseRegions()
        {
            OnInitialiseRegions?.Invoke();   
        }
        
        public static void InitialiseCities()
        {
            OnInitialiseCities?.Invoke();
        }

        public static void InitialiseJobSites()
        {
            OnInitialiseJobSites?.Invoke();
        }

        public static void InitialiseStations()
        {
            OnInitialiseStations?.Invoke();
        }
    }
}
