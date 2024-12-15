using System;
using System.Linq;

namespace Items
{
    [Serializable]
    public class Item_WeaponStats
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
    }
}