using System;

namespace Initialisation
{
    public abstract class Manager_Initialisation
    {
        public static event Action OnInitialiseManagerFaction;

        public static event Action OnInitialiseManagerRegion;
        public static event Action OnInitialiseManagerCity;
        public static event Action OnInitialiseManagerJobsite;
        public static event Action OnInitialiseManagerStation;
        public static event Action OnInitialiseManagerOperatingArea;
        public static event Action OnInitialiseManagerOrder;

        public static event Action OnInitialiseManagerActor;
        public static event Action OnInitialiseActorData;
        public static event Action OnInitialiseActors;

        public static event Action OnInitialiseRegions;
        public static event Action OnInitialiseCities;
        public static event Action OnInitialiseJobsites;
        public static event Action OnInitialiseStations;

        public static void InitialiseFactions()
        {
            OnInitialiseManagerFaction?.Invoke();
        }

        public static void InitialiseManagers() 
        {
            OnInitialiseManagerRegion?.Invoke();
            OnInitialiseManagerCity?.Invoke();
            OnInitialiseManagerJobsite?.Invoke();
            OnInitialiseManagerStation?.Invoke();
            OnInitialiseManagerOperatingArea?.Invoke();
            OnInitialiseManagerOrder?.Invoke();
        }

        public static void InitialiseActors()
        {
            OnInitialiseManagerActor?.Invoke();
            OnInitialiseActorData?.Invoke();
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

        public static void InitialiseJobsites()
        {
            OnInitialiseJobsites?.Invoke();
        }

        public static void InitialiseStations()
        {
            OnInitialiseStations?.Invoke();
        }
    }
}
