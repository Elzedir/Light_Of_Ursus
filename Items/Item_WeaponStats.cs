using System;
using System.Collections.Generic;
using System.Linq;
using Tools;

namespace Items
{
    [Serializable]
    public class Item_WeaponStats : Data_Class
    {
        public WeaponType[]  WeaponTypeArray;
        public WeaponClass[] WeaponClassArray;
        public float         MaxChargeTime;

        public Item_WeaponStats(
            WeaponType[]  weaponType    = null,
            WeaponClass[] weaponClass   = null,
            float         maxChargeTime = 0
        )
        {
            WeaponTypeArray  = weaponType  ?? new[] { WeaponType.None };
            WeaponClassArray = weaponClass ?? new[] { WeaponClass.None };
            MaxChargeTime    = maxChargeTime;
        }

        public Item_WeaponStats(Item_WeaponStats other)
        {
            WeaponTypeArray  = other.WeaponTypeArray.ToArray();
            WeaponClassArray = other.WeaponClassArray.ToArray();
            MaxChargeTime    = other.MaxChargeTime;
        }

        public override Dictionary<string, string> GetStringData()
        {
            return new Dictionary<string, string>
            {
                { "WeaponTypeArray", $"{string.Join(", ",  WeaponTypeArray)}" },
                { "WeaponClassArray", $"{string.Join(", ", WeaponClassArray)}" },
                { "MaxChargeTime", $"{MaxChargeTime}" }
            };
        }
        
        public override DataToDisplay GetDataToDisplay(bool toggleMissingDataDebugs)
        {
            _updateDataDisplay(DataToDisplay,
                title: "Weapon Stats",
                toggleMissingDataDebugs: toggleMissingDataDebugs,
                allStringData: GetStringData()
            );

            return DataToDisplay;
        }
    }
}