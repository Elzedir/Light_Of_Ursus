using UnityEngine;

namespace Careers
{
    public abstract class Career_Manager
    {
        const string  _career_SOPath = "ScriptableObjects/Career_SO";
        
        static Career_SO _careerSO;
        static Career_SO Career_SO => _careerSO ??= _getCareer_SO();

        public static Career_Data GetCareer_Master(CareerName careerName) => Career_SO.GetCareer_Data(careerName).Data_Object;
        
        static Career_SO _getCareer_SO()
        {
            var career_SO = Resources.Load<Career_SO>(_career_SOPath);
            
            if (career_SO is not null) return career_SO;
            
            Debug.LogError("Career_SO not found. Creating temporary Career_SO.");
            career_SO = ScriptableObject.CreateInstance<Career_SO>();
            
            return career_SO;
        }
        
        public static void ClearSOData()
        {
            Career_SO.ClearSOData();
        }
    }

    public enum CareerName 
    {
        None,
        
        Wanderer,
        
        Lumberjack,
        Smith
    }
}