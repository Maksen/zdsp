using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using Kopio.JsonContracts;
using Zealot.Common;

namespace Zealot.Repository
{
    public static class CharacterNamingRepo
    {
        public static Dictionary<int, SurnameJson> mSurnameIdMap;
        public static Dictionary<int, MaleNameJson> mMaleNameIdMap;
        public static Dictionary<int, FemaleNameJson> mFemaleNameIdMap;

        static CharacterNamingRepo()
        {
            mSurnameIdMap = new Dictionary<int, SurnameJson>();
            mMaleNameIdMap = new Dictionary<int, MaleNameJson>();
            mFemaleNameIdMap = new Dictionary<int, FemaleNameJson>();
        }

        public static void Init(GameDBRepo gameData)
        {
            mSurnameIdMap = gameData.Surname;
            mMaleNameIdMap = gameData.MaleName;
            mFemaleNameIdMap = gameData.FemaleName;
        }

        public static string GetRandomMaleName()
        {
            Random rand = GameUtils.GetRandomGenerator();
            if (mSurnameIdMap.Count > 0 && mMaleNameIdMap.Count > 0)
            {
                string name = mSurnameIdMap.ElementAt(rand.Next(0, mSurnameIdMap.Count)).Value.surname + mMaleNameIdMap.ElementAt(rand.Next(0, mMaleNameIdMap.Count)).Value.name;
                return name;
            }
            else return "";

        }

        public static string GetRandomFemaleName()
        {
            Random rand = GameUtils.GetRandomGenerator();
            if (mSurnameIdMap.Count > 0 && mFemaleNameIdMap.Count > 0)
            {
                string name = mSurnameIdMap.ElementAt(rand.Next(0, mSurnameIdMap.Count)).Value.surname + mFemaleNameIdMap.ElementAt(rand.Next(0, mFemaleNameIdMap.Count)).Value.name;
                return name;
            }
            else return "";
        }
    }        
}
