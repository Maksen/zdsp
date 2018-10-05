using Kopio.JsonContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using Zealot.Common;

namespace Zealot.Repository
{
   public static class CharacterNamingRepo
    {
        public static Dictionary<int, CharacterNameJson> mCharacterNameIdMap;
        private static Dictionary<int, string> mFirstNameMap;
        private static Dictionary<int, string> mLastNameMap;

        static CharacterNamingRepo()
        {
            mFirstNameMap = new Dictionary<int, string>();
            mLastNameMap = new Dictionary<int, string>();
        }

        public static void Init(GameDBRepo gameData)
        {
            foreach(KeyValuePair<int, CharacterNameJson> entry in gameData.CharacterName)
            {
                mFirstNameMap.Add(entry.Key, entry.Value.firstname);
                mLastNameMap.Add(entry.Key, entry.Value.lastname);
            }
        }

        public static string GetRandomName()
        {
            string firstname = GetRandomFirstName();
            string lastname = GetRandomLastName();
            return firstname + lastname;
        }

        private static string GetRandomFirstName()
        {
            Random rand = GameUtils.GetRandomGenerator();
            if (mFirstNameMap.Count > 0)
            {
                return mFirstNameMap.ElementAt(rand.Next(0, mFirstNameMap.Count)).Value;
            }

            return "";
        }

        public static string GetRandomLastName()
        {
            Random rand = GameUtils.GetRandomGenerator();
            if (mLastNameMap.Count > 0)
            {
                return mLastNameMap.ElementAt(rand.Next(0, mLastNameMap.Count)).Value;
            }

            return "";
        }
    }
}
