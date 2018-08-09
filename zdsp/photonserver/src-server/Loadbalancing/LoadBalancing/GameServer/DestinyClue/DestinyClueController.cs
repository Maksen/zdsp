using Kopio.JsonContracts;
using System;
using System.Collections.Generic;
using System.Timers;
using Zealot.Common;
using Zealot.Common.Entities;
using Zealot.Repository;
using Zealot.Server.Entities;

namespace Photon.LoadBalancing.GameServer
{
    public class ClueWeightData
    {
        public int Min;
        public int Max;
        public int Id;

        public ClueWeightData(int min, int probability, int id)
        {
            Min = min;
            Max = min + probability;
            Id = id;
        }

        public bool IsInRange(int probability)
        {
            return probability >= Min && probability < Max ? true : false;
        }
    }

    public class DestinyClueController
    {
        private Player mPlayer;

        private Dictionary<ClueCondition, Dictionary<int, List<int>>> mClueListByConditionType;
        private int mDialogueClueMinTime;
        private int mDialogueClueMaxTime;
        private int mDialogueClueProbability;
        private int mTimeClueMinTime;
        private int mTimeClueMaxTime;
        private int mTimeClueProbability;

        private Timer mDialogueClueTimer;
        private Timer mTimeClueTimer;

        private List<ActivatedClueData> mDestinyClues;
        private List<int> mUnlockMemory;
        private List<LockedClueData> mLockedClues;
        private List<int> mUnlockClues;

        private Dictionary<int, Timer> mTimeClueExpiredTimer;
        private int mMaxDestinyClueCount;

        public DestinyClueController(Player player)
        {
            mPlayer = player;
            mMaxDestinyClueCount = 30;

            mDialogueClueMinTime = GameConstantRepo.GetConstantInt("DestinyDialogueClue_Min");
            mDialogueClueMaxTime = GameConstantRepo.GetConstantInt("DestinyDialogueClue_Max");
            mDialogueClueProbability = GameConstantRepo.GetConstantInt("DestinyDialogueClue_Probability");
            mTimeClueMinTime = GameConstantRepo.GetConstantInt("DestinyTimeClue_Min");
            mTimeClueMaxTime = GameConstantRepo.GetConstantInt("DestinyTimeClue_Max");
            mTimeClueProbability = GameConstantRepo.GetConstantInt("DestinyTimeClue_Probability");

            mClueListByConditionType = new Dictionary<ClueCondition, Dictionary<int, List<int>>>();
            foreach (ClueCondition type in Enum.GetValues(typeof(ClueCondition)))
            {
                mClueListByConditionType.Add(type, new Dictionary<int, List<int>>());
            }
            GroupClueByConditionType();
        }

        private void GroupClueByConditionType()
        {
            Dictionary<int, DestinyClueJson> clueJsons = DestinyClueRepo.GetDestinyClues();
            foreach (KeyValuePair<int, DestinyClueJson> entry in clueJsons)
            {
                AddClueData(entry.Value.condition1, entry.Value.condition1id, entry.Key);
                AddClueData(entry.Value.condition2, entry.Value.condition2id, entry.Key);
                AddClueData(entry.Value.condition3, entry.Value.condition3id, entry.Key);
            }
        }

        private void AddClueData(ClueCondition type, int id, int clueid)
        {
            if (!mClueListByConditionType[type].ContainsKey(id))
            {
                mClueListByConditionType[type].Add(id, new List<int>());
            }
            mClueListByConditionType[type][id].Add(clueid);
        }

        private void StartDialogueTimer()
        {
            long duration = GameUtils.RandomInt(mDialogueClueMinTime, mDialogueClueMaxTime) * 60 * 1000;
            mDialogueClueTimer = new Timer();
            mDialogueClueTimer.Elapsed += new  ElapsedEventHandler(OnDialogueTimeEnd);
            mDialogueClueTimer.Interval = duration;
            mDialogueClueTimer.Start();
        }

        private void OnDialogueTimeEnd(object arg, ElapsedEventArgs e)
        {
            mDialogueClueTimer.Stop();
            mDialogueClueTimer = null;

            int activeresult = GameUtils.RandomInt(0, 100);
            if (activeresult < mDialogueClueProbability)
            {
                List<HeroDialogueClueJson> dialogueclues = GetAvailableDialogueClues();
                int maxweight = 0;
                List<ClueWeightData> weightDatas = ReorderDialogueClue(dialogueclues, out maxweight);
                ClueWeightData result = GetRandomClue(weightDatas, maxweight);
                if (result != null)
                {
                    HeroDialogueClueJson clueJson = null;
                    foreach (HeroDialogueClueJson dialogueclue in dialogueclues)
                    {
                        if (dialogueclue.dialogueid == result.Id)
                        {
                            clueJson = dialogueclue;
                            break;
                        }
                    }

                    if (clueJson != null)
                    {
                        ActivateDialogueClue(clueJson);
                    }
                }
            }

            StartDialogueTimer();
        }

        private List<HeroDialogueClueJson> GetAvailableDialogueClues()
        {
            List<HeroDialogueClueJson> result = new List<HeroDialogueClueJson>();
            Dictionary<int, HeroDialogueClueJson> dialogueclues = DestinyClueRepo.GetHeroDialogueClues();
            foreach(KeyValuePair<int, HeroDialogueClueJson> entry in dialogueclues)
            {
                if (mPlayer.PlayerSynStats.Level >= entry.Value.lvl && entry.Value.isuse)
                {
                    result.Add(entry.Value);
                }
            }
            return result;
        }

        private List<ClueWeightData> ReorderDialogueClue(List<HeroDialogueClueJson> dialogueclues, out int maxweight)
        {
            List<HeroDialogueClueJson> cluelist = dialogueclues;
            List<HeroDialogueClueJson> weightlist = new List<HeroDialogueClueJson>();
            List<ClueWeightData> weightdata = new List<ClueWeightData>();

            while (cluelist.Count > 0)
            {
                int result = RandomClueOrder(cluelist.Count);
                HeroDialogueClueJson clue = cluelist[result];
                weightlist.Add(clue);
                cluelist.RemoveAt(result);
            }

            maxweight = 0;
            foreach (HeroDialogueClueJson clue in weightlist)
            {
                weightdata.Add(new ClueWeightData(maxweight, clue.probability, clue.dialogueid));
                maxweight += clue.probability;
            }
            return weightdata;
        }

        private void ActivateDialogueClue(HeroDialogueClueJson clueJson)
        {
            long currenttime = mPlayer.GetSynchronizedTime();
            DateTime dateTime = mPlayer.GetSynchronizedServerDT();
            string date = string.Format("{0:yyyy}.{0:MM}.{0:dd}", dateTime);
            string time = string.Format("{0:HH}:{0:mm}", dateTime);
            ActivatedClueData clueData = new ActivatedClueData(clueJson.dialogueid, ClueType.Dialogue, date, time, currenttime, ClueStatus.New);
            mDestinyClues.Add(clueData);

            DeleteOldClue();
        }

        private void StartTimeClueTimer()
        {
            long duration = GameUtils.RandomInt(mTimeClueMinTime, mTimeClueMaxTime) * 60 * 1000;
            mTimeClueTimer = new Timer();
            mTimeClueTimer.Elapsed += new ElapsedEventHandler(OnTimeClueTimeEnd);
            mTimeClueTimer.Interval = duration;
            mTimeClueTimer.Start();
        }

        private void OnTimeClueTimeEnd(object arg, ElapsedEventArgs e)
        {
            mTimeClueTimer.Stop();
            mTimeClueTimer = null;

            int activeresult = GameUtils.RandomInt(0, 100);
            if (activeresult < mTimeClueProbability)
            {
                List<TimeClueJson> timeclues = GetAvailableTimeClues();
                int maxweight = 0;
                List<ClueWeightData> weightDatas = ReorderTimeClue(timeclues, out maxweight);
                ClueWeightData result = GetRandomClue(weightDatas, maxweight);
                if (result != null)
                {
                    TimeClueJson clueJson = null;
                    foreach (TimeClueJson timeclue in timeclues)
                    {
                        if (timeclue.dialogueid == result.Id)
                        {
                            clueJson = timeclue;
                            break;
                        }
                    }

                    if (clueJson != null)
                    {
                        ActivateTimeClue(clueJson);
                    }
                }
            }

            StartTimeClueTimer();
        }

        private List<TimeClueJson> GetAvailableTimeClues()
        {
            List<TimeClueJson> result = new List<TimeClueJson>();
            Dictionary<int, TimeClueJson> timeclues = DestinyClueRepo.GetTimeClues();
            foreach (KeyValuePair<int, TimeClueJson> entry in timeclues)
            {
                if (entry.Value.isuse)
                {
                    result.Add(entry.Value);
                }
            }
            return result;
        }

        private List<ClueWeightData> ReorderTimeClue(List<TimeClueJson> timeclues, out int maxweight)
        {
            List<TimeClueJson> cluelist = timeclues;
            List<TimeClueJson> weightlist = new List<TimeClueJson>();
            List<ClueWeightData> weightdata = new List<ClueWeightData>();

            while (cluelist.Count > 0)
            {
                int result = RandomClueOrder(cluelist.Count);
                TimeClueJson clue = cluelist[result];
                weightlist.Add(clue);
                cluelist.RemoveAt(result);
            }

            maxweight = 0;
            foreach (TimeClueJson clue in weightlist)
            {
                weightdata.Add(new ClueWeightData(maxweight, clue.probability, clue.dialogueid));
                maxweight += clue.probability;
            }
            return weightdata;
        }

        private void ActivateTimeClue(TimeClueJson clueJson)
        {
            long currenttime = mPlayer.GetSynchronizedTime();
            DateTime dateTime = mPlayer.GetSynchronizedServerDT();
            string date = string.Format("{0:yyyy}.{0:MM}.{0:dd}", dateTime);
            string time = string.Format("{0:HH}:{0:mm}", dateTime);
            ActivatedClueData clueData = new ActivatedClueData(clueJson.dialogueid, ClueType.Time, date, time, currenttime, ClueStatus.New);
            mDestinyClues.Add(clueData);
            AddTimeClueExpiredTimer(clueData);

            DeleteOldClue();
        }

        private ClueWeightData GetRandomClue(List<ClueWeightData> weightDatas, int maxweight)
        {
            int probability = GameUtils.RandomInt(0, maxweight);
            foreach (ClueWeightData data in weightDatas)
            {
                if (data.IsInRange(probability))
                {
                    return data;
                }
            }
            return null;
        }

        private int RandomClueOrder(int max)
        {
            return GameUtils.RandomInt(0, max);
        }

        public void InitFromData(DestinyClueInventory clueinv)
        {
            mDestinyClues = clueinv == null ? new List<ActivatedClueData>() : clueinv.DeserializedActivatedClues();
            mUnlockMemory = clueinv == null ? new List<int>() : clueinv.DeserializedUnlockMemory();
            mLockedClues = clueinv == null ? new List<LockedClueData>() : clueinv.DeserializedLockedClues();
            mUnlockClues = clueinv == null ? new List<int>() : clueinv.DeserializedUnlockClues();

            ActivateDestinyClue(1);

            StartDialogueTimer();
            StartTimeClueTimer();

            InitTimeClueExpiredTimer();
        }

        public void InitDestinyClueStats(ref DestinyClueSynStats destinyClueStats)
        {
            destinyClueStats.destinyClues = SyncDestinyClues();
            destinyClueStats.unlockMemory = SyncUnlockMemory();
        }

        private void InitTimeClueExpiredTimer()
        {
            mTimeClueExpiredTimer = new Dictionary<int, Timer>();
            foreach(ActivatedClueData cluedata in mDestinyClues)
            {
                if (cluedata.ClueType == (byte)ClueType.Time)
                {
                    AddTimeClueExpiredTimer(cluedata);
                }
            }
        }

        private void OnClueExpired(int clueid)
        {
            Timer timer = mTimeClueExpiredTimer[clueid];
            timer.Stop();
            mTimeClueExpiredTimer.Remove(clueid);
            DeleteClueById(clueid, ClueType.Time);
        }

        private void AddTimeClueExpiredTimer(ActivatedClueData clueData)
        {
            TimeClueJson clueJson = DestinyClueRepo.GetTimeClueById(clueData.ClueId);
            if (clueJson != null)
            {
                long currenttime = mPlayer.GetSynchronizedTime();
                long endtime = clueData.ActivatedDateTime + (clueJson.time * 60 * 1000);
                long duration = endtime - currenttime;
                int id = clueData.ClueId;
                if (duration > 0)
                {
                    Timer timer = new Timer();
                    timer.Elapsed += delegate { OnClueExpired(id); };
                    timer.Interval = duration;
                    timer.Start();
                    if (!mTimeClueExpiredTimer.ContainsKey(id))
                    {
                        mTimeClueExpiredTimer.Add(id, timer);
                    }
                    else
                    {
                        mTimeClueExpiredTimer[id] = timer;
                    }
                }
            }
        }

        private void DeleteOldClue()
        {
            while(mDestinyClues.Count > mMaxDestinyClueCount)
            {
                mDestinyClues.RemoveAt(0);
            }

            SyncDestinyClueStats();
        }

        private void DeleteClueById(int clueid, ClueType type)
        {
            ActivatedClueData clueData = null;
            foreach (ActivatedClueData cluedata in mDestinyClues)
            {
                if (cluedata.ClueType == (byte)type && cluedata.ClueId == clueid)
                {
                    clueData = cluedata;
                }
            }

            if (clueData != null)
            {
                mDestinyClues.Remove(clueData);
            }
        }

        private bool IsClueTriggered(int clueid, ClueType type)
        {
            foreach (ActivatedClueData cluedata in mDestinyClues)
            {
                if (cluedata.ClueType == (byte)type && cluedata.ClueId == clueid)
                {
                    return true;
                }
            }
            return false;
        }

        private LockedClueData GetLockedClueData(int clueid)
        {
            foreach(LockedClueData cluedata in mLockedClues)
            {
                if (cluedata.ClueId == clueid)
                {
                    return cluedata;
                }
            }
            return null;
        }

        private void AddLockedClueData(LockedClueData clueData)
        {
            int index = mLockedClues.FindIndex(o => o.ClueId == clueData.ClueId);
            if (index == -1)
            {
                mLockedClues.Add(clueData);
            }
            else
            {
                mLockedClues[index] = clueData;
            }
        }

        public void TriggerClueCondition(ClueCondition condition, int conditionid)
        {
            List<int> cluelist = null;
            Dictionary<int, List<int>> conditionlist = mClueListByConditionType[condition];
            foreach(KeyValuePair<int,List<int>> entry in conditionlist)
            {
                if (entry.Key == conditionid)
                {
                    cluelist = entry.Value;
                    break;
                }
            }

            List<int> availablelist = new List<int>();
            if (cluelist != null)
            {
                foreach(int clueid in cluelist)
                {
                    bool istriggered = IsClueTriggered(clueid, ClueType.Normal);
                    bool isunlocked = IsClueAlreadyUnlock(clueid);

                    if (!istriggered && !isunlocked)
                    {
                        availablelist.Add(clueid);
                    }
                }
            }

            foreach(int clueid in availablelist)
            {
                LockedClueData clueData = GetLockedClueData(clueid);
                if (clueData == null)
                {
                    clueData = new LockedClueData(clueid);
                }

                DestinyClueJson clueJson = DestinyClueRepo.GetDestinyClueById(clueid);
                if (clueJson != null)
                {
                    if ((clueJson.condition1 == condition && clueJson.condition1id == conditionid) || clueJson.condition1 == ClueCondition.None)
                    {
                        clueData.Condition1Status = true;
                    }
                    if ((clueJson.condition2 == condition && clueJson.condition2id == conditionid) || clueJson.condition2 == ClueCondition.None)
                    {
                        clueData.Condition2Status = true;
                    }
                    if ((clueJson.condition3 == condition && clueJson.condition3id == conditionid) || clueJson.condition3 == ClueCondition.None)
                    {
                        clueData.Condition3Status = true;
                    }
                    AddLockedClueData(clueData);
                }
            }

            ActiveClueByCondition();
        }

        private void ActiveClueByCondition()
        {
            List<int> cluelist = new List<int>();
            for(int i=0;i< mLockedClues.Count;i++)
            {
                LockedClueData clueData = mLockedClues[i];
                if (clueData.Condition1Status && clueData.Condition2Status && clueData.Condition3Status)
                {
                    cluelist.Add(i);
                }
            }

            List<int> unlockmemory = new List<int>();
            foreach(int index in cluelist)
            {
                int memoryid = ActivateDestinyClue(mLockedClues[index].ClueId);
                if (memoryid != -1)
                {
                    unlockmemory.Add(memoryid);
                }
                mLockedClues.RemoveAt(index);
            }

            if (unlockmemory.Count > 0)
            {
                foreach(int memoryid in unlockmemory)
                {
                    if (!mUnlockMemory.Contains(memoryid))
                    {
                        mUnlockMemory.Add(memoryid);
                    }
                }
                SyncUnlockMemoryStats();
            }

            DeleteOldClue();
        }

        private int ActivateDestinyClue(int clueid)
        {
            if (!IsClueAlreadyUnlock(clueid))
            {
                DestinyClueJson clueJson = DestinyClueRepo.GetDestinyClueById(clueid);
                if (clueJson != null)
                {
                    long currenttime = mPlayer.GetSynchronizedTime();
                    DateTime dateTime = mPlayer.GetSynchronizedServerDT();
                    string date = string.Format("{0:yyyy}.{0:MM}.{0:dd}", dateTime);
                    string time = string.Format("{0:HH}:{0:mm}", dateTime);
                    ActivatedClueData clueData = new ActivatedClueData(clueJson.clueid, ClueType.Normal, date, time, currenttime, ClueStatus.New);
                    mDestinyClues.Add(clueData);
                    mUnlockClues.Add(clueid);
                    return clueJson.heroid;
                }
            }
            return -1;
        }

        private bool IsClueAlreadyUnlock(int clueid)
        {
            return mUnlockClues.Contains(clueid);
        }

        private string SyncDestinyClues()
        {
            return JsonConvertDefaultSetting.SerializeObject(mDestinyClues);
        }

        private string SyncUnlockMemory()
        {
            return JsonConvertDefaultSetting.SerializeObject(mUnlockMemory);
        }

        private void SyncDestinyClueStats()
        {
            mPlayer.DestinyClueStats.destinyClues = SyncDestinyClues();
        }

        private void SyncUnlockMemoryStats()
        {
            mPlayer.DestinyClueStats.unlockMemory = SyncUnlockMemory();
        }
    }
}
