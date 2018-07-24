using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Zealot.Repository;

namespace Zealot.Common
{
    //public struct PortraitInfo
    //{
    //    public bool mUnlocked;
    //    public bool mIsNew;

    //    public PortraitInfo(bool unlock, bool isNew)
    //    { mUnlocked = unlock; mIsNew = isNew; }
    //}

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class PortraitData
    {
        //<portrait id, info>
        [JsonProperty(PropertyName = "portstats2")]
        public Dictionary<int, bool> mInfoDic = null;

        public void InitDefault(Zealot.Common.JobType jt)
        {
            //Create the save data if it doesnt exist, and clear all entry
            if (mInfoDic == null)
                mInfoDic = new Dictionary<int, bool>();
            mInfoDic.Clear();

            UpdateClassPortrait(jt);
        }

        public void SaveToPortraitData(string portraitDataStatsString)
        {
            mInfoDic.Clear();
            mInfoDic = JsonConvert.DeserializeObject<Dictionary<int, bool>>(portraitDataStatsString);
        }

        /// <summary>
        /// Unlock all class (non-hero) specific portraits
        /// </summary>
        /// <param name="job"></param>
        public void UpdateClassPortrait(Zealot.Common.JobType job)
        {
            Dictionary<int, string> heroMap = PortraitPathRepo.mHeroMap;
            Dictionary<int, string> classMap = GetClassMap(job);

            //Do nothing if invalid class map
            if (classMap == null)
                return;

            //Remove class portrait, if they dont exist in kopio data
            var portKeyLst = new List<int>(mInfoDic.Keys);
            for (int i = 0; i < portKeyLst.Count; ++i)
            {
                if (!classMap.ContainsKey(portKeyLst[i]) && !heroMap.ContainsKey(portKeyLst[i]))
                    mInfoDic.Remove(portKeyLst[i]);
            }

            //Add class portrait as old portrait, skip if they already existed
            foreach (var p in classMap)
            {
                if (mInfoDic.ContainsKey(p.Key))
                    continue;

                mInfoDic.Add(p.Key, false);
            }
        }

        public string GetJsonString()
        {
            string jsonStr = JsonConvert.SerializeObject(mInfoDic);

            return jsonStr;
        }

        public bool hasNewPortrait()
        {
            if (mInfoDic.Count == 0)
                return false;

            foreach (var info in mInfoDic)
            {
                if (info.Value)
                    return true;
            }

            return false;
        }

        private Dictionary<int, string> GetClassMap(Zealot.Common.JobType job)
        {
            Dictionary<int, string> classMap = null;
            switch (job)
            {
                case JobType.Warrior:
                    classMap = PortraitPathRepo.mKnifeMap;
                    break;
                case JobType.Soldier:
                    classMap = PortraitPathRepo.mSwordMap;
                    break;
                case JobType.Tactician:
                    classMap = PortraitPathRepo.mSpearMap;
                    break;
                case JobType.Killer:
                    classMap = PortraitPathRepo.mHammerMap;
                    break;
            }

            return classMap;
        }
    }
}
