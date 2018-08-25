using Kopio.JsonContracts;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Zealot.Common.Datablock;
using Zealot.Repository;

namespace Zealot.Common.Entities
{
    public class RealmInfo
    {
        public int DailyEntry = 0;
        public int ExtraEntry = 0;
        public int LocalObjIdx = 0;

        public RealmInfo(int dailyEntry, int extraEntry, int localObjIdx)
        {
            DailyEntry = dailyEntry;
            ExtraEntry = extraEntry;
            LocalObjIdx = localObjIdx;
        }

        public override string ToString()
        {
            return string.Format("{0};{1}", DailyEntry, ExtraEntry);
        }

        public bool HasEntry()
        {
            return DailyEntry + ExtraEntry > 0;
        }
    }

    public class DungeonStoryInfo : RealmInfo
    {
        public int DailyExtraEntry = 0;
        public bool[] StarObjectiveCompleted; // Need to initialize this
        public int TotalStarCompleted = 0;
        public string StarRewardCollected = "";
        public Dictionary<int, bool> GetStarCollectedDict() { return starCollectedDict; }
        private Dictionary<int, bool> starCollectedDict = new Dictionary<int, bool>(); // Star number <- isCollected

        StringBuilder starRewardSb = new StringBuilder();

        public DungeonStoryInfo(int dailyEntry, int extraEntry, int dailyExtraEntry, bool starObj1Completed, bool starObj2Completed, 
                                bool starObj3Completed, bool starObj4Completed, bool starObj5Completed, bool starObj6Completed, 
                                bool starObj7Completed, bool starObj8Completed, bool starObj9Completed, string starRewardCollected, 
                                int localObjIdx) : base(dailyEntry, extraEntry, localObjIdx)
        {
            DailyExtraEntry = dailyExtraEntry;
            StarObjectiveCompleted 
                = new bool[] { starObj1Completed, starObj2Completed, starObj3Completed, starObj4Completed, starObj5Completed,
                               starObj6Completed, starObj7Completed, starObj8Completed, starObj9Completed };
            UpdateTotalStarCompleted(); // Update current stars completed

            // Init starRewardCollected
            starRewardSb.Length = 0;
            string[] starInfo = starRewardCollected.Split('|');
            int starInfoLen = starInfo.Length;          
            /*Dictionary<int, int> starRewards = RealmRepo.GetStarRewardsBySeq(localObjIdx+1);
            if(starRewards != null)
            {
                bool isFirst = true;
                foreach (int starCnt in starRewards.Keys)
                {
                    int result = 0;
                    for (int i = 0; i < starInfoLen; i+=2)
                    {
                        int tmpStarCnt = 0;
                        if (int.TryParse(starInfo[i], out tmpStarCnt) && tmpStarCnt == starCnt)
                            if (int.TryParse(starInfo[i+1], out result))
                                break;
                    }
                    if (!isFirst) starRewardSb.Append("|");
                    else isFirst = false;
                    starRewardSb.AppendFormat("{0}|{1}", starCnt, result);
                    starCollectedDict[starCnt] = (result != 0);
                }
            }*/        
            StarRewardCollected = starRewardSb.ToString();
        }

        public void UpdateTotalStarCompleted()
        {
            int len = StarObjectiveCompleted.Length;
            TotalStarCompleted = 0;
            for (int i=0; i<len; ++i)
            {
                if (StarObjectiveCompleted[i])
                    ++TotalStarCompleted;
            }
        }

        public void StarCollectedStringToDict()
        {
            string[] starInfo = StarRewardCollected.Split('|');
            int len = starInfo.Length;
            if (len >= 2)
            {
                for (int i = 0; i < len; i+=2)
                {
                    int stars = int.Parse(starInfo[i]);
                    starCollectedDict[stars] = (int.Parse(starInfo[i + 1]) != 0);
                }
            }
        }

        public void StarCollectedDictToString()
        {
            starRewardSb.Length = 0;
            bool isFirst = true;
            foreach (KeyValuePair<int, bool> kvp in starCollectedDict)
            {
                if (!isFirst) starRewardSb.Append("|");
                else isFirst = false;
                starRewardSb.AppendFormat("{0}|{1}", kvp.Key, kvp.Value?1:0);
            }
            StarRewardCollected = starRewardSb.ToString();
        }

        public override string ToString()
        {
            return string.Format("{0};{1};{2};{3};{4};{5};{6};{7};{8};{9};{10};{11};{12}", DailyEntry, ExtraEntry, DailyExtraEntry, 
                                 StarObjectiveCompleted[0], StarObjectiveCompleted[1], StarObjectiveCompleted[2], StarObjectiveCompleted[3], 
                                 StarObjectiveCompleted[4], StarObjectiveCompleted[5], StarObjectiveCompleted[6], StarObjectiveCompleted[7], 
                                 StarObjectiveCompleted[8], StarRewardCollected);
        }
    }

    public class RealmStats : LocalObject
    {     
        public RealmStats() : base(LOTYPE.RealmStats)
        {
            DungeonStory = new CollectionHandler<object>(40);
            DungeonStory.SetParent(this, "DungeonStory");
            dungeonStoryDict = new Dictionary<int, DungeonStoryInfo>();

            DungeonDaily = new CollectionHandler<object>(20);
            DungeonDaily.SetParent(this, "DungeonDaily");
            dungeonDailyDict = new Dictionary<int, RealmInfo>();

            DungeonSpecial = new CollectionHandler<object>(20);
            DungeonSpecial.SetParent(this, "DungeonSpecial");
            dungeonSpecialDict = new Dictionary<int, RealmInfo>();

            WorldBoss = new CollectionHandler<object>(1);
            WorldBoss.SetParent(this, "WorldBoss");
            WorldBossDict = new Dictionary<int, RealmInfo>();

            _EliteMapTime = 0;
        }

        private int _EliteMapTime;
        public int EliteMapTime
        {
            get { return _EliteMapTime; }
            set { this.OnSetAttribute("EliteMapTime", value); _EliteMapTime = value; }
        }

        public CollectionHandler<object> DungeonStory { get; set; }
        public CollectionHandler<object> DungeonDaily { get; set; }
        public CollectionHandler<object> DungeonSpecial { get; set; }
        public CollectionHandler<object> WorldBoss { get; set; }

        public Dictionary<int, DungeonStoryInfo> GetDungeonStoryDict() { return dungeonStoryDict; }
        private Dictionary<int, DungeonStoryInfo> dungeonStoryDict; //sequence <- DungeonStoryInfo

        public Dictionary<int, RealmInfo> GetDungeonDailyDict() { return dungeonDailyDict; }
        private Dictionary<int, RealmInfo> dungeonDailyDict; //sequence <- RealmInfo

        public Dictionary<int, RealmInfo> GetDungeonSpecialDict() { return dungeonSpecialDict; }
        private Dictionary<int, RealmInfo> dungeonSpecialDict; //sequence <- RealmInfo

        public Dictionary<int, RealmInfo> GetWorldBossDict() { return WorldBossDict; }
        private Dictionary<int, RealmInfo> WorldBossDict; 

        public void ResetOnNewDay()
        {
            foreach (KeyValuePair<int, DungeonStoryInfo> entry in dungeonStoryDict)
            {
                DungeonStoryInfo info = entry.Value;
                int seq = info.LocalObjIdx+1;
                DungeonJson storyJson = null;
                Dictionary<DungeonDifficulty, DungeonJson> dungeonStoryBySeq = RealmRepo.GetDungeonStoryBySeq(seq);       
                if (dungeonStoryBySeq != null && dungeonStoryBySeq.TryGetValue(DungeonDifficulty.Easy, out storyJson))
                {
                    info.DailyEntry = storyJson.entrylimit;
                    info.DailyExtraEntry = 0;
                    DungeonStory[info.LocalObjIdx] = info.ToString();
                }
                else
                    DungeonStory[info.LocalObjIdx] = null;
            }
            /*foreach (KeyValuePair<int, RealmInfo> entry in dungeonDailyDict)
            {
                RealmInfo info = entry.Value;
                int seq = info.LocalObjIdx+1;
                List<DungeonDailySpecialJson> dailyJsonList = RealmRepo.GetDungeonDailyBySeq(seq);
                if (dailyJsonList != null && dailyJsonList.Count > 0)
                {
                    info.DailyEntry = dailyJsonList[0].dailyentry;
                    DungeonDaily[info.LocalObjIdx] = info.ToString();
                }
                else
                    DungeonDaily[info.LocalObjIdx] = null;
            }
            foreach (KeyValuePair<int, RealmInfo> entry in dungeonSpecialDict)
            {
                RealmInfo info = entry.Value;
                int seq = info.LocalObjIdx+1;
                List<DungeonDailySpecialJson> specialJsonDict = RealmRepo.GetDungeonSpecialBySeq(seq);
                if (specialJsonDict != null && specialJsonDict.Count > 0)
                {
                    info.DailyEntry = specialJsonDict[0].dailyentry;
                    DungeonSpecial[info.LocalObjIdx] = info.ToString();
                }
                else
                    DungeonSpecial[info.LocalObjIdx] = null;
            }

            foreach (KeyValuePair<int, RealmInfo> entry in WorldBossDict)
            {
                RealmInfo info = entry.Value;
                ActivityWorldBossJson worldBossJson = RealmRepo.mActivityWorldBoss;
                if (worldBossJson != null)
                {
                    info.DailyEntry = worldBossJson.dailyentry;
                    WorldBoss[info.LocalObjIdx] = info.ToString();
                }
                else
                    WorldBoss[info.LocalObjIdx] = null;
            }*/

            EliteMapTime = 0;
        }

        public void AddExtraEntry(RealmType realmType, int seq, int amt, DungeonType dungeonType=DungeonType.Daily)
        {
            switch(realmType)
            {
                case RealmType.Dungeon:
                    DungeonStoryInfo dungeonStoryInfo = GetDungeonStoryDict()[seq];
                    dungeonStoryInfo.ExtraEntry += amt;
                    DungeonStory[dungeonStoryInfo.LocalObjIdx] = dungeonStoryInfo.ToString();
                    break;
                //case RealmType.DungeonDailySpecial:
                //    RealmInfo realmInfo = (dungeonType == DungeonType.Daily) 
                //        ? GetDungeonDailyDict()[seq] : GetDungeonSpecialDict()[seq];
                //    realmInfo.ExtraEntry += amt;
                //    CollectionHandler<object> list = (dungeonType == DungeonType.Daily) ? DungeonDaily : DungeonSpecial;
                //    list[realmInfo.LocalObjIdx] = realmInfo.ToString();
                //    break;
            }
        }

        public int GetDungeonStoryCompletedCount(DungeonDifficulty difficulty)
        {
            int storyCompleted = 0, diffIdx = (int)difficulty*3;
            Dictionary<int, DungeonStoryInfo>.ValueCollection dungeonStoryVals = dungeonStoryDict.Values;           
            foreach (DungeonStoryInfo info in dungeonStoryVals)
            {
                bool atLeastOne = false;
                for (int i=0; i<3; ++i)
                {
                    if (info.StarObjectiveCompleted[diffIdx+i])
                    {
                        atLeastOne = true;
                        break;
                    }
                }
                if (atLeastOne)
                    ++storyCompleted;
            }
            return storyCompleted;
        }

        public int GetTotalStarsCompleted()
        {
            return dungeonStoryDict.Values.Sum(x => x.TotalStarCompleted);
        }

        /*public int GetEliteMapDailyTimeLeft(int progressLevel)
        {
            int dailyTime = GetEliteMapDailyTimeTotal(progressLevel);
            int left = dailyTime - EliteMapTime;
            return left >= 0 ? left : 0;
        }

        public int GetEliteMapDailyTimeTotal(int progressLevel)
        {
            EliteMapJson eliteMapJson = RealmRepo.GetEliteMapByPlayerLevel(progressLevel);
            if (eliteMapJson != null)
               return eliteMapJson.dailytime;
            return 0;
        }*/
    }

    public class DungeonObjectiveStats : LocalObject
    {
        public CollectionHandler<object> StarObjectivesProgress { get; set; }
        //private List<RealmObjectiveJson> starObjectives = null;

        public DungeonObjectiveStats() : base(LOTYPE.DungeonObjectiveStats)
        {
            StarObjectivesProgress = new CollectionHandler<object>(3);
            StarObjectivesProgress.SetParent(this, "StarObjectivesProgress");
            StarObjectivesProgress.SetNotifyParent(false);
            int count = StarObjectivesProgress.Count;
            for (int i=0; i<count; ++i)
                StarObjectivesProgress[i] = 1;
            StarObjectivesProgress.SetNotifyParent(true);
        }

        public void InitStarObjectives(int objective1, int objective2, int objective3)
        {
            //starObjectives = new List<RealmObjectiveJson>();
            //starObjectives.Add(RealmRepo.GetRealmObjectiveById(objective1));
            //starObjectives.Add(RealmRepo.GetRealmObjectiveById(objective2));
            //starObjectives.Add(RealmRepo.GetRealmObjectiveById(objective3));

            //int objectivesCnt = starObjectives.Count;
            //for (int i=0; i<objectivesCnt; ++i)
            //    StarObjectivesProgress[i] = 0;
        }

        public void OnObjectiveMonsterKill(int npcId)
        {
            /*if (starObjectives == null)
                return;
            for (int i = 0; i < 3; ++i)
            {
                RealmObjectiveJson starObjective = starObjectives[i];
                if (starObjective == null)
                    continue;
                else if (starObjective.realmobjectivetype == RealmObjectiveType.NPCKills)
                {
                    int progress = (int)StarObjectivesProgress[i];
                    if (npcId == starObjective.monsterid && progress < starObjective.monsterkillcnt)
                        StarObjectivesProgress[i] = progress+1;
                }
            }*/
        }

        public void OnObjectiveTimeNoDeath(long elapsedTime, int deathCount)
        {
            /*if (starObjectives == null)
                return;
            for (int i = 0; i < 3; ++i)
            {
                RealmObjectiveJson starObjective = starObjectives[i];
                if (starObjective == null)
                    continue;
                else if (starObjective.realmobjectivetype == RealmObjectiveType.TimeWithinNoDeath)
                {
                    if (deathCount == 0 && (int)StarObjectivesProgress[i] != 1)
                        StarObjectivesProgress[i] = 1;
                    else if (deathCount > 0 && (int)StarObjectivesProgress[i] != 0)
                    {
                        if (elapsedTime < (long)starObjective.timelimit*1000 || starObjective.timelimit == 0)
                            StarObjectivesProgress[i] = 0;
                    }
                }
            }*/
        }

        public void OnObjectiveTimeComplete(long elapsedTime, bool success)
        {
            /*if (starObjectives == null)
                return;
            for (int i = 0; i < 3; ++i)
            {
                RealmObjectiveJson starObjective = starObjectives[i];
                if (starObjective == null)
                    continue;
                else if (starObjective.realmobjectivetype == RealmObjectiveType.TimeWithinComplete)
                {
                    if (elapsedTime <= (long)starObjective.timelimit*1000 && success)
                        StarObjectivesProgress[i] = 1;
                }
            }*/
        }

        //public void OnObjectiveHeroUseOnly(SkillSynStats skillStats)
        //{
        //    if (starObjectives == null)
        //        return;
        //    for (int i = 0; i < 3; ++i)
        //    {
        //        RealmObjectiveJson starObjective = starObjectives[i];
        //        if (starObjective == null)
        //            continue;
        //        else if (starObjective.realmobjectivetype == RealmObjectiveType.HeroUseOnly)
        //        {
        //            HeroCardJson hero = HeroRepo.GetHeroCardBySkillGroupId(skillStats.RedHeroCardSkillAttackSId);
        //            if (hero != null && hero.id == starObjective.hero)
        //            {
        //                StarObjectivesProgress[i] = 1;
        //                continue;
        //            }
        //            hero = HeroRepo.GetHeroCardBySkillGroupId(skillStats.RedHeroCardSubskillId);
        //            if (hero != null && hero.id == starObjective.hero)
        //            {
        //                StarObjectivesProgress[i] = 1;
        //                continue;
        //            }
        //            hero = HeroRepo.GetHeroCardBySkillGroupId(skillStats.GreenHeroCardSkillAttackSId);
        //            if (hero != null && hero.id == starObjective.hero)
        //            {
        //                StarObjectivesProgress[i] = 1;
        //                continue;
        //            }
        //            hero = HeroRepo.GetHeroCardBySkillGroupId(skillStats.GreenHeroCardSubskillId);
        //            if (hero != null && hero.id == starObjective.hero)
        //            {
        //                StarObjectivesProgress[i] = 1;
        //                continue;
        //            }
        //            hero = HeroRepo.GetHeroCardBySkillGroupId(skillStats.BlueHeroCardSkillAttackSId);
        //            if (hero != null && hero.id == starObjective.hero)
        //            {
        //                StarObjectivesProgress[i] = 1;
        //                continue;
        //            }
        //            hero = HeroRepo.GetHeroCardBySkillGroupId(skillStats.BlueHeroCardSubskillId);
        //            if (hero != null && hero.id == starObjective.hero)
        //            {
        //                StarObjectivesProgress[i] = 1;
        //                continue;
        //            }
        //        }                 
        //    }
        //}

        //public void OnObjectiveHeroCannotUse(SkillSynStats skillStats)
        //{
        //    if (starObjectives == null)
        //        return;
        //    for (int i = 0; i < 3; ++i)
        //    {
        //        RealmObjectiveJson starObjective = starObjectives[i];
        //        if (starObjective == null)
        //            continue;
        //        else if (starObjective.realmobjectivetype == RealmObjectiveType.HeroCannotUse)
        //        {
        //            StarObjectivesProgress[i] = 1;
        //            HeroCardJson hero = HeroRepo.GetHeroCardBySkillGroupId(skillStats.RedHeroCardSkillAttackSId);
        //            if (hero != null && hero.id == starObjective.hero)
        //            {
        //                StarObjectivesProgress[i] = 0;
        //                continue;
        //            }
        //            hero = HeroRepo.GetHeroCardBySkillGroupId(skillStats.RedHeroCardSubskillId);
        //            if (hero != null && hero.id == starObjective.hero)
        //            {
        //                StarObjectivesProgress[i] = 0;
        //                continue;
        //            }
        //            hero = HeroRepo.GetHeroCardBySkillGroupId(skillStats.GreenHeroCardSkillAttackSId);
        //            if (hero != null && hero.id == starObjective.hero)
        //            {
        //                StarObjectivesProgress[i] = 0;
        //                continue;
        //            }
        //            hero = HeroRepo.GetHeroCardBySkillGroupId(skillStats.GreenHeroCardSubskillId);
        //            if (hero != null && hero.id == starObjective.hero)
        //            {
        //                StarObjectivesProgress[i] = 0;
        //                continue;
        //            }
        //            hero = HeroRepo.GetHeroCardBySkillGroupId(skillStats.BlueHeroCardSkillAttackSId);
        //            if (hero != null && hero.id == starObjective.hero)
        //            {
        //                StarObjectivesProgress[i] = 0;
        //                continue;
        //            }
        //            hero = HeroRepo.GetHeroCardBySkillGroupId(skillStats.BlueHeroCardSubskillId);
        //            if (hero != null && hero.id == starObjective.hero)
        //            {
        //                StarObjectivesProgress[i] = 0;
        //                continue;
        //            }
        //        }
        //    }
        //}

        //public void OnObjectiveHeroTypeCannotUse(SkillSynStats skillStats)
        //{
        //    if (starObjectives == null)
        //        return;
        //    for (int i = 0; i < 3; ++i)
        //    {
        //        RealmObjectiveJson starObjective = starObjectives[i];
        //        if (starObjective == null)
        //            continue;
        //        else if (starObjective.realmobjectivetype == RealmObjectiveType.HeroTypeCannotUse)
        //        {
        //            int mainID = 0;
        //            switch (starObjective.herotype)
        //            {
        //                case HeroType.Red:   mainID = skillStats.RedHeroCardSkillAttackSId; break;
        //                case HeroType.Green: mainID = skillStats.GreenHeroCardSkillAttackSId; break;
        //                case HeroType.Blue:  mainID = skillStats.BlueHeroCardSkillAttackSId; break;
        //                case HeroType.White: mainID = skillStats.JobskillAttackSId; break;
        //            }
        //            if (mainID == 0)
        //                StarObjectivesProgress[i] = 1;
        //        }
        //    }
        //}

        public void OnObjectiveSkillCannotUseAny(SkillSynStats skillStats)
        {
            /*if (starObjectives == null)
                return;
            for (int i = 0; i < 3; ++i)
            {
                RealmObjectiveJson starObjective = starObjectives[i];
                if (starObjective == null)
                    continue;
                else if (starObjective.realmobjectivetype == RealmObjectiveType.SkillCannotUseAny)
                {
                    //if (skillStats.RedHeroCardSkillAttackSId == 0 && skillStats.GreenHeroCardSkillAttackSId == 0 &&
                     //   skillStats.BlueHeroCardSkillAttackSId == 0 && skillStats.JobskillAttackSId == 0)
                     //       StarObjectivesProgress[i] = 1; 
                }
            }*/
        }

        //public void OnObjectiveHeroQualityUseOnly(SkillSynStats skillStats)
        //{
        //    if (starObjectives == null)
        //        return;
        //    for (int i = 0; i < 3; ++i)
        //    {
        //        RealmObjectiveJson starObjective = starObjectives[i];
        //        if (starObjective == null)
        //            continue;
        //        else if (starObjective.realmobjectivetype == RealmObjectiveType.HeroQualityUseOnly)
        //        {
        //            if (skillStats.RedHeroCardSkillAttackSId != 0 || skillStats.GreenHeroCardSkillAttackSId != 0 ||
        //                skillStats.BlueHeroCardSkillAttackSId != 0)
        //            {
        //                StarObjectivesProgress[i] = 1;
        //                HeroCardJson hero = HeroRepo.GetHeroCardBySkillGroupId(skillStats.RedHeroCardSkillAttackSId);
        //                if (hero != null && hero.heroquality != starObjective.heroquality)
        //                {
        //                    StarObjectivesProgress[i] = 0;
        //                    continue;
        //                }
        //                hero = HeroRepo.GetHeroCardBySkillGroupId(skillStats.RedHeroCardSubskillId);
        //                if (hero != null && hero.heroquality != starObjective.heroquality)
        //                {
        //                    StarObjectivesProgress[i] = 0;
        //                    continue;
        //                }
        //                hero = HeroRepo.GetHeroCardBySkillGroupId(skillStats.GreenHeroCardSkillAttackSId);
        //                if (hero != null && hero.heroquality != starObjective.heroquality)
        //                {
        //                    StarObjectivesProgress[i] = 0;
        //                    continue;
        //                }
        //                hero = HeroRepo.GetHeroCardBySkillGroupId(skillStats.GreenHeroCardSubskillId);
        //                if (hero != null && hero.heroquality != starObjective.heroquality)
        //                {
        //                    StarObjectivesProgress[i] = 0;
        //                    continue;
        //                }
        //                hero = HeroRepo.GetHeroCardBySkillGroupId(skillStats.BlueHeroCardSkillAttackSId);
        //                if (hero != null && hero.heroquality != starObjective.heroquality)
        //                {
        //                    StarObjectivesProgress[i] = 0;
        //                    continue;
        //                }
        //                hero = HeroRepo.GetHeroCardBySkillGroupId(skillStats.BlueHeroCardSubskillId);
        //                if (hero != null && hero.heroquality != starObjective.heroquality)
        //                {
        //                    StarObjectivesProgress[i] = 0;
        //                    continue;
        //                }
        //            }
        //        }
        //    }
        //}

        public void OnObjectiveRealmComplete(bool success)
        {
            /*if (starObjectives == null)
                return;
            for (int i = 0; i < 3; ++i)
            {
                RealmObjectiveJson starObjective = starObjectives[i];
                if (starObjective == null)
                    continue;
                else if (starObjective.realmobjectivetype == RealmObjectiveType.RealmComplete)
                {
                    if(success)
                        StarObjectivesProgress[i] = 1;
                }
            }*/
        }

        public string UpdatePlayerStarObjectivesProgress(RealmStats realmStats, DungeonJson dungeonJson, bool updateStats)
        {
            /*if (starObjectives == null)
                return "";

            Dictionary<int, DungeonStoryInfo> dungeonStoryInfoDict = realmStats.GetDungeonStoryDict();
            DungeonStoryInfo dungeonStoryInfo = dungeonStoryInfoDict[dungeonJson.sequence];
            int diff = (int)dungeonJson.difficulty*3;
            StringBuilder sb = new StringBuilder();
            bool isFirst = true;
            for (int i = 0; i < 3; ++i)
            {
                RealmObjectiveJson starObjective = starObjectives[i];
                if (starObjective == null)
                    continue;
                int idx = i+diff;
                if (!dungeonStoryInfo.StarObjectiveCompleted[idx])
                {
                    dungeonStoryInfo.StarObjectiveCompleted[idx] = (starObjective.realmobjectivetype == RealmObjectiveType.NPCKills) 
                        ? (int)StarObjectivesProgress[i] >= starObjective.monsterkillcnt
                        : (int)StarObjectivesProgress[i] == 1;
                }
                if (!isFirst)
                    sb.Append(';');
                else isFirst = false;
                sb.Append(dungeonStoryInfo.StarObjectiveCompleted[idx]);
            }
            if (updateStats)
            {
                realmStats.DungeonStory[dungeonStoryInfo.LocalObjIdx] = dungeonStoryInfo.ToString();
                dungeonStoryInfo.UpdateTotalStarCompleted(); // Update current stars completed
            }
            return sb.ToString();*/
            return "";
        }

        /* Star objective planning
         * 
         * TimeWithinNoDeath - ok
         * - Check for death and time exceed
         * 
         * TimeWithinComplete - ok
         * - Check during OnMissionComplete and time exceed
         * 
         * HeroUseOnly
         * - true, until change hero other than this, need initial check
         * 
         * HeroCannotUse
         * - true, until used cannot use hero, need initial check
         * 
         * HeroTypeCannotUse
         * - true, until used hero with this type, need initial check
         * 
         * SkillCannotUseAny
         * - true, until skill is used
         * 
         * HeroQualityUseOnly
         * - true, until hero quality other than this is used
         * 
         * NPCKills - ok
         * Check for NPCkills
         * 
         * RealmComplete - ok
         * - Check during OnMissionComplete success
         */
    }
}
