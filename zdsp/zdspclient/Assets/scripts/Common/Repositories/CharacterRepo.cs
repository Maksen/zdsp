using Kopio.JsonContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using Zealot.Common;

namespace Zealot.Repository
{
   public static class CharacterCreationRepo
    {
        private static Dictionary<ApperanceType, Dictionary<int, AppearanceJson>> mAppearanceIdMapByPartType;
        private static Dictionary<int, string> mFirstNameMap;
        private static Dictionary<int, string> mLastNameMap;

        static CharacterCreationRepo()
        {
            mAppearanceIdMapByPartType = new Dictionary<ApperanceType, Dictionary<int, AppearanceJson>>();
            mFirstNameMap = new Dictionary<int, string>();
            mLastNameMap = new Dictionary<int, string>();
        }

        public static void Init(GameDBRepo gameData)
        {
            foreach (KeyValuePair<int, AppearanceJson> entry in gameData.Appearance)
            {
                if (!mAppearanceIdMapByPartType.ContainsKey(entry.Value.parttype))
                {
                    mAppearanceIdMapByPartType.Add(entry.Value.parttype, new Dictionary<int, AppearanceJson>());
                }

                if (!mAppearanceIdMapByPartType[entry.Value.parttype].ContainsKey(entry.Value.partid))
                {
                    mAppearanceIdMapByPartType[entry.Value.parttype].Add(entry.Value.partid, entry.Value);
                }
            }

            foreach (KeyValuePair<int, CharacterNameJson> entry in gameData.CharacterName)
            {
                mFirstNameMap.Add(entry.Key, entry.Value.firstname);
                mLastNameMap.Add(entry.Key, entry.Value.lastname);
            }
        }

        public static Dictionary<int, AppearanceJson> GetCustomizeDatas(ApperanceType apperanceType, ApperanceGender gender, List<int> ownedlist)
        {
            Dictionary<int, AppearanceJson> result = new Dictionary<int, AppearanceJson>();
            if (mAppearanceIdMapByPartType.ContainsKey(apperanceType))
            {
                foreach (KeyValuePair<int, AppearanceJson> entry in mAppearanceIdMapByPartType[apperanceType])
                {
                    if ((entry.Value.currencytype == ApperanceCurrency.Free || ownedlist.Contains(entry.Key)) && (entry.Value.gender == gender || entry.Value.gender == ApperanceGender.All))
                    {
                        result.Add(entry.Key, entry.Value);
                    }
                }
            }

            return result;
        }

        public static AppearanceJson GetCustomizeDatas(ApperanceType apperanceType, int partid)
        {
            if (mAppearanceIdMapByPartType.ContainsKey(apperanceType))
            {
                Dictionary<int, AppearanceJson> customizedatas = mAppearanceIdMapByPartType[apperanceType];
                if (customizedatas.ContainsKey(partid))
                {
                    return customizedatas[partid];
                }
            }
            
            return null;
        }

        public static string GetMeshPathByPartId(int partid, ApperanceType apperanceType)
        {
            if (mAppearanceIdMapByPartType.ContainsKey(apperanceType))
            {
                Dictionary<int, AppearanceJson> customizedatas = mAppearanceIdMapByPartType[apperanceType];
                if (customizedatas.ContainsKey(partid))
                {
                    return customizedatas[partid].meshpath;
                }
            }

            return "";
        }

        public static string GetMaterialPathByPartId(int partid, ApperanceType apperanceType)
        {
            if (mAppearanceIdMapByPartType.ContainsKey(apperanceType))
            {
                Dictionary<int, AppearanceJson> customizedatas = mAppearanceIdMapByPartType[apperanceType];
                if (customizedatas.ContainsKey(partid))
                {
                    return customizedatas[partid].materialpath;
                }
            }

            return "";
        }

        public static string GetColorCodeByPartId(int partid, ApperanceType apperanceType)
        {
            if (mAppearanceIdMapByPartType.ContainsKey(apperanceType))
            {
                Dictionary<int, AppearanceJson> customizedatas = mAppearanceIdMapByPartType[apperanceType];
                if (customizedatas.ContainsKey(partid))
                {
                    return "#" + customizedatas[partid].color;
                }
            }

            return "";
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

        private static string GetRandomLastName()
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
