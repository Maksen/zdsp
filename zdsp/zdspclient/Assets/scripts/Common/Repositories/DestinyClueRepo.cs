using System;
using System.Collections.Generic;
using System.Linq;
using Kopio.JsonContracts;
using Zealot.Common;

namespace Zealot.Repository
{
    public static class DestinyClueRepo
    {
        public static Dictionary<int, DestinyClueJson> mDestinyClueMap;
        public static Dictionary<int, HeroMemoryJson> mHeroMemoryMap;
        public static Dictionary<int, HeroDialogueClueJson> mHeroDialogueClueMap;
        public static Dictionary<int, TimeClueJson> mTimeClueMap;

        static DestinyClueRepo()
        {
            mDestinyClueMap = new Dictionary<int, DestinyClueJson>();
            mHeroMemoryMap = new Dictionary<int, HeroMemoryJson>();
            mHeroDialogueClueMap = new Dictionary<int, HeroDialogueClueJson>();
            mTimeClueMap = new Dictionary<int, TimeClueJson>();
        }

        public static void Init(GameDBRepo gameData)
        {
            mDestinyClueMap.Clear();
            mHeroMemoryMap.Clear();
            mHeroDialogueClueMap.Clear();
            mTimeClueMap.Clear();

            foreach(KeyValuePair<int, DestinyClueJson> entry in gameData.DestinyClue)
            {
                mDestinyClueMap.Add(entry.Value.clueid, entry.Value);
            }

            foreach (KeyValuePair<int, HeroMemoryJson> entry in gameData.HeroMemory)
            {
                mHeroMemoryMap.Add(entry.Value.heroid, entry.Value);
            }

            foreach (KeyValuePair<int, HeroDialogueClueJson> entry in gameData.HeroDialogueClue)
            {
                mHeroDialogueClueMap.Add(entry.Value.dialogueid, entry.Value);
            }

            foreach (KeyValuePair<int, TimeClueJson> entry in gameData.TimeClue)
            {
                mTimeClueMap.Add(entry.Value.dialogueid, entry.Value);
            }
        }

        public static Dictionary<int, DestinyClueJson> GetDestinyClues()
        {
            return mDestinyClueMap;
        }

        public static DestinyClueJson GetDestinyClueById(int clueid)
        {
            if (mDestinyClueMap.ContainsKey(clueid))
            {
                return mDestinyClueMap[clueid];
            }
            return null;
        }

        public static Dictionary<int, HeroMemoryJson> GetHeroMemories()
        {
            return mHeroMemoryMap;
        }

        public static HeroMemoryJson GetHeroMemoryById(int memoryid)
        {
            if (mHeroMemoryMap.ContainsKey(memoryid))
            {
                return mHeroMemoryMap[memoryid];
            }
            return null;
        }

        public static Dictionary<int, HeroDialogueClueJson> GetHeroDialogueClues()
        {
            return mHeroDialogueClueMap;
        }

        public static HeroDialogueClueJson GetHeroDialogueClueById(int clueid)
        {
            if (mHeroDialogueClueMap.ContainsKey(clueid))
            {
                return mHeroDialogueClueMap[clueid];
            }
            return null;
        }

        public static Dictionary<int, TimeClueJson> GetTimeClues()
        {
            return mTimeClueMap;
        }

        public static TimeClueJson GetTimeClueById(int clueid)
        {
            if (mTimeClueMap.ContainsKey(clueid))
            {
                return mTimeClueMap[clueid];
            }
            return null;
        }
    }
}
