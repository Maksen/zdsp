using Kopio.JsonContracts;
using System.Collections.Generic;
using Zealot.Common;

namespace Zealot.Repository
{
    public static class PortraitPathRepo
    {
        public static Dictionary<int, string> mKnifeMap;
        public static Dictionary<int, string> mSwordMap;
        public static Dictionary<int, string> mSpearMap;
        public static Dictionary<int, string> mHammerMap;
        public static Dictionary<int, string> mHeroMap;
        public static Dictionary<int, string> mGeneralMap;

        static Dictionary<int, CharPortraitType> mPortraitTypeMap;

        static PortraitPathRepo()
        {
            mKnifeMap = new Dictionary<int, string>();
            mSwordMap = new Dictionary<int, string>();
            mSpearMap = new Dictionary<int, string>();
            mHammerMap = new Dictionary<int, string>();
            mHeroMap = new Dictionary<int, string>();

            mGeneralMap = new Dictionary<int, string>();
            mPortraitTypeMap = new Dictionary<int, CharPortraitType>();
        }

        public static void Init(GameDBRepo gameData)
        {
            Dictionary<int, PortraitJson> mPortraitJson = gameData.Portrait;

            mKnifeMap.Clear();
            mSwordMap.Clear();
            mSpearMap.Clear();
            mHammerMap.Clear();
            mHeroMap.Clear();
            mGeneralMap.Clear();

            foreach (var pair in mPortraitJson)
            {
                switch (pair.Value.pclass)
                {
                    case CharPortraitType.Knife:
                        mKnifeMap.Add(pair.Value.pid, pair.Value.path);
                        break;
                    case CharPortraitType.Sword:
                        mSwordMap.Add(pair.Value.pid, pair.Value.path);
                        break;
                    case CharPortraitType.Spear:
                        mSpearMap.Add(pair.Value.pid, pair.Value.path);
                        break;
                    case CharPortraitType.Hammer:
                        mHammerMap.Add(pair.Value.pid, pair.Value.path);
                        break;
                    case CharPortraitType.Hero:
                        mHeroMap.Add(pair.Value.pid, pair.Value.path);
                        break;
                }

                mGeneralMap.Add(pair.Value.pid, pair.Value.path);
                mPortraitTypeMap.Add(pair.Value.pid, pair.Value.pclass);
            }
        }

        public static string GetPath(int portraitID)
        {
            if (mGeneralMap.ContainsKey(portraitID))
                return mGeneralMap[portraitID];

            return string.Empty;
        }

        public static string GetPath(CharPortraitType e, int portraitID)
        {
            Dictionary<int, string> dic = null;

            switch (e)
            {
                case CharPortraitType.Knife:
                    dic = mKnifeMap;
                    break;
                case CharPortraitType.Sword:
                    dic = mSwordMap;
                    break;
                case CharPortraitType.Spear:
                    dic = mSpearMap;
                    break;
                case CharPortraitType.Hammer:
                    dic = mHammerMap;
                    break;
                case CharPortraitType.Hero:
                    dic = mHeroMap;
                    break;
            }

            if (dic != null && dic.ContainsKey(portraitID))
                return dic[portraitID];

            return string.Empty;
        }
        public static CharPortraitType GetType(int portraitID)
        {
            if (mPortraitTypeMap.ContainsKey(portraitID))
                return mPortraitTypeMap[portraitID];

            return CharPortraitType.None;
        }

        public static int GetFirstDefaultPortrait(CharPortraitType e)
        {
            Dictionary<int, string> dic = null;

            switch (e)
            {
                case CharPortraitType.Knife:
                    dic = mKnifeMap;
                    break;
                case CharPortraitType.Sword:
                    dic = mSwordMap;
                    break;
                case CharPortraitType.Spear:
                    dic = mSpearMap;
                    break;
                case CharPortraitType.Hammer:
                    dic = mHammerMap;
                    break;
            }

            foreach (var elem in dic)
            {
                return elem.Key;
            }

            return -1;
        }

        public static int GetFirstDefaultPortrait(JobType e)
        {
            return 1;
        }
    }
}
