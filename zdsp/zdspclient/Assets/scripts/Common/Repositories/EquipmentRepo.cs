using System.Collections.Generic;
using Kopio.JsonContracts;
using System;
using System.Linq;
using Zealot.Common;

namespace Zealot.Repository
{

    public static class EquipmentRepo
    {


        static EquipmentRepo()
        {

        }

        public static void Init(GameDBRepo gameData)
        {

        }

        public static WeaponType GetWeaponTypeByPartType(PartsType partType)
        {
            switch (partType)
            {
                case PartsType.Sword:
                    return WeaponType.Sword;
                case PartsType.Blade:
                    return WeaponType.Blade;
                case PartsType.Lance:
                    return WeaponType.Lance;
                case PartsType.Hammer:
                    return WeaponType.Hammer;
                case PartsType.Fan:
                    return WeaponType.Fan;
                case PartsType.Xbow:
                    return WeaponType.Xbow;
                case PartsType.Dagger:
                    return WeaponType.Dagger;
                case PartsType.Sanxian:
                    return WeaponType.Sanxian;
                default:
                    return WeaponType.Any;
            }
        }
    }
}
