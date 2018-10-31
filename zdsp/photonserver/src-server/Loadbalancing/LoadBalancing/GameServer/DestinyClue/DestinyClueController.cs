using Kopio.JsonContracts;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Timers;
using Zealot.Common;
using Zealot.Common.Entities;
using Zealot.Repository;
using Zealot.Server.Entities;
using Zealot.Server.Rules;

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
        private GameClientPeer mSlot;

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
        private List<int> mUnlockTimeClues;

        private Dictionary<int, Timer> mTimeClueExpiredTimer;
        private int mMaxDestinyClueCount;

        private bool bFeatureUnlock = false;
        private bool bLocked = false;
        private List<int> mClueForDelete = new List<int>();
        private List<ActivatedClueData> mClueForAdd = new List<ActivatedClueData>();
        private bool bInit = false;

        public DestinyClueController(GameClientPeer slot)
        {
            mSlot = slot;
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
            if (!bFeatureUnlock)
            {
                return;
            }

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
                if (mSlot.mPlayer.PlayerSynStats.Level >= entry.Value.lvl && entry.Value.isuse)
                {
                    result.Add(entry.Value);
                }
            }
            return result;
        }

        private List<ClueWeightData> ReorderDialogueClue(List<HeroDialogueClueJson> dialogueclues, out int maxweight)
        {
            List<HeroDialogueClueJson> cluelist = dialogueclues.CloneJson();
            List<HeroDialogueClueJson> weightlist = new List<HeroDialogueClueJson>();
            List<ClueWeightData> weightdata = new List<ClueWeightData>();

            while (cluelist.Count > 0)
            {
                int result = RandomClueOrder(cluelist.Count - 1);
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
            DateTime dateTime = DateTime.Now;
            string date = string.Format("{0:yyyy}.{0:MM}.{0:dd}", dateTime);
            string time = string.Format("{0:HH}:{0:mm}", dateTime);
            ActivatedClueData clueData = new ActivatedClueData(clueJson.dialogueid, ClueType.Dialogue, date, time, ClueStatus.New);
            if (bLocked)
            {
                mClueForAdd.Add(clueData);
            }
            else
            {
                bLocked = true;
                mDestinyClues.Add(clueData);
                SyncDestinyClueStats();
            }
            

            
        }

        private void StartTimeClueTimer()
        {
            if (!bFeatureUnlock)
            {
                return;
            }

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
            List<TimeClueJson> cluelist = timeclues.CloneJson();
            List<TimeClueJson> weightlist = new List<TimeClueJson>();
            List<ClueWeightData> weightdata = new List<ClueWeightData>();

            while (cluelist.Count > 0)
            {
                int result = RandomClueOrder(cluelist.Count - 1);
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
            DateTime dateTime = DateTime.Now;
            string date = string.Format("{0:yyyy}.{0:MM}.{0:dd}", dateTime);
            string time = string.Format("{0:HH}:{0:mm}", dateTime);
            ActivatedClueData clueData = new ActivatedClueData(clueJson.dialogueid, ClueType.Time, date, time, ClueStatus.New);
            AddTimeClueExpiredTimer(clueData);
            if (!mUnlockTimeClues.Contains(clueData.ClueId))
            {
                mUnlockTimeClues.Add(clueData.ClueId);
                SyncUnlockTimeCluesStats();
            }
            if (bLocked)
            {
                mClueForAdd.Add(clueData);
            }
            else
            {
                bLocked = true;
                mDestinyClues.Add(clueData);
                SyncDestinyClueStats();
            }

            SyncDestinyClueStats();
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

        public void InitFromData(DestinyClueInventory clueinv, int level)
        {
            if (bInit)
            {
                return;
            }

            mDestinyClues = clueinv == null ? new List<ActivatedClueData>() : clueinv.DeserializedActivatedClues();
            mUnlockMemory = clueinv == null ? new List<int>() : clueinv.DeserializedUnlockMemory();
            mLockedClues = clueinv == null ? new List<LockedClueData>() : clueinv.DeserializedLockedClues();
            mUnlockClues = clueinv == null ? new List<int>() : clueinv.DeserializedUnlockClues();
            mUnlockTimeClues = clueinv == null ? new List<int>() : clueinv.DeserializedUnlockTimeClues();

            StartDialogueTimer();
            StartTimeClueTimer();

            InitTimeClueExpiredTimer();

            if (level >= 2)
            {
                bFeatureUnlock = true;
            }

            bInit = true;
        }

        public void InitDestinyClueStats(ref DestinyClueSynStats destinyClueStats)
        {
            destinyClueStats.destinyClues = SyncDestinyClues();
            destinyClueStats.unlockMemory = SyncUnlockMemory();
            destinyClueStats.unlockClues = SyncUnlockClues();
            destinyClueStats.unlockTimeClues = SyncUnlockTimeClues();
        }

        private void InitTimeClueExpiredTimer()
        {
            mTimeClueExpiredTimer = new Dictionary<int, Timer>();
            List<int> idlist = new List<int>();
            foreach(ActivatedClueData cluedata in mDestinyClues)
            {
                if (cluedata.ClueType == (byte)ClueType.Time)
                {
                    if (!AddTimeClueExpiredTimer(cluedata))
                    {
                        idlist.Add(cluedata.ClueId);
                    }
                }
            }

            foreach (int clueid in idlist)
            {
                ActivatedClueData clueData = GetClue(clueid, ClueType.Time);
                if (clueData != null)
                {
                    mDestinyClues.Remove(clueData);
                    if (mUnlockTimeClues.Contains(clueData.ClueId))
                    {
                        mUnlockTimeClues.Remove(clueData.ClueId);
                    }
                }
            }
        }

        private void OnClueExpired(int clueid)
        {
            if (mTimeClueExpiredTimer.ContainsKey(clueid))
            {
                Timer timer = mTimeClueExpiredTimer[clueid];
                timer.Stop();
                mTimeClueExpiredTimer.Remove(clueid);
                mClueForDelete.Add(clueid);
                if (!bLocked)
                {
                    bLocked = true;
                    DeleteClue();
                    SyncDestinyClueStats();
                    SyncUnlockTimeCluesStats();
                }
            }
        }

        private bool AddTimeClueExpiredTimer(ActivatedClueData clueData)
        {
            TimeClueJson clueJson = DestinyClueRepo.GetTimeClueById(clueData.ClueId);
            if (clueJson != null)
            {
                DateTime endDT = DateTime.ParseExact(clueData.ActivatedDate, "yyyy.MM.dd", CultureInfo.InvariantCulture);
                endDT = DateTime.ParseExact(clueData.ActivatedTime, "HH:mm", CultureInfo.InvariantCulture);
                endDT = endDT.AddMinutes(clueJson.time);
                double duration = (endDT - DateTime.Now).TotalMilliseconds;
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
                    return true;
                }
            }
            return false;
        }

        private void DeleteAddClueAfterUpdate()
        {
            bLocked = true;
            foreach (ActivatedClueData cluedata in mClueForAdd)
            {
                mDestinyClues.Add(cluedata);
            }

            DeleteOldClue();
            DeleteClue();

            mClueForAdd = new List<ActivatedClueData>();
            mClueForDelete = new List<int>();
            SyncDestinyClueStats();
            SyncUnlockTimeCluesStats();
        }

        private void DeleteOldClue()
        {
            while(mDestinyClues.Count > mMaxDestinyClueCount)
            {
                mDestinyClues.RemoveAt(0);
            }
        }

        private void DeleteClue()
        {
            mDestinyClues = mDestinyClues.Where(o => o != null).ToList();

            foreach (int clueid in mClueForDelete)
            {
                ActivatedClueData clueData = GetClue(clueid, ClueType.Time);
                if (clueData != null)
                {
                    mDestinyClues.Remove(clueData);
                    if (mUnlockTimeClues.Contains(clueData.ClueId))
                    {
                        mUnlockTimeClues.Remove(clueData.ClueId);
                    }
                }
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

        private ActivatedClueData GetClue(int clueid, ClueType type)
        {
            foreach (ActivatedClueData cluedata in mDestinyClues)
            {
                if (cluedata.ClueType == (byte)type && cluedata.ClueId == clueid)
                {
                    return cluedata;
                }
            }
            return null;
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
                        if (clueJson.condition1 == ClueCondition.Item)
                        {
                            if (mSlot.mPlayer.Slot.mInventory.HasItem((ushort)clueJson.condition1id, 1))
                            {
                                clueData.Condition1Status = true;
                            }
                            else
                            {
                                clueData.Condition1Status = false;
                            }
                        }
                        else
                        {
                            clueData.Condition1Status = true;
                        }
                    }
                    if ((clueJson.condition2 == condition && clueJson.condition2id == conditionid) || clueJson.condition2 == ClueCondition.None)
                    {
                        if (clueJson.condition2 == ClueCondition.Item)
                        {
                            if (mSlot.mPlayer.Slot.mInventory.HasItem((ushort)clueJson.condition2id, 1))
                            {
                                clueData.Condition2Status = true;
                            }
                            else
                            {
                                clueData.Condition2Status = false;
                            }
                        }
                        else
                        {
                            clueData.Condition2Status = true;
                        }
                    }
                    if ((clueJson.condition3 == condition && clueJson.condition3id == conditionid) || clueJson.condition3 == ClueCondition.None)
                    {
                        if (clueJson.condition3 == ClueCondition.Item)
                        {
                            if (mSlot.mPlayer.Slot.mInventory.HasItem((ushort)clueJson.condition3id, 1))
                            {
                                clueData.Condition3Status = true;
                            }
                            else
                            {
                                clueData.Condition3Status = false;
                            }
                        }
                        else
                        {
                            clueData.Condition3Status = true;
                        }
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
                SyncUnlockCluesStats();
                if (bLocked)
                {
                    SyncDestinyClueStats();
                }
            }
        }

        private int ActivateDestinyClue(int clueid)
        {
            if (!IsClueAlreadyUnlock(clueid))
            {
                DestinyClueJson clueJson = DestinyClueRepo.GetDestinyClueById(clueid);
                if (clueJson != null)
                {
                    DateTime dateTime = DateTime.Now;
                    string date = string.Format("{0:yyyy}.{0:MM}.{0:dd}", dateTime);
                    string time = string.Format("{0:HH}:{0:mm}", dateTime);
                    ActivatedClueData clueData = new ActivatedClueData(clueJson.clueid, ClueType.Normal, date, time, ClueStatus.New);
                    if (bLocked)
                    {
                        mClueForAdd.Add(clueData);
                    }
                    else
                    {
                        mDestinyClues.Add(clueData);
                    }
                    mUnlockClues.Add(clueid);
                    return clueJson.heroid;
                }
            }
            return -1;
        }

        public bool IsClueAlreadyUnlock(int clueid)
        {
            return mUnlockClues.Contains(clueid);
        }

        public bool IsTimeClueAlreadyUnlock(int clueid)
        {
            return mUnlockTimeClues.Contains(clueid);
        }

        private string SyncDestinyClues()
        {
            string result = JsonConvertDefaultSetting.SerializeObject(mDestinyClues.CloneJson());
            bLocked = false;
            return result;
        }

        private string SyncUnlockMemory()
        {
            return JsonConvertDefaultSetting.SerializeObject(mUnlockMemory);
        }

        private string SyncUnlockClues()
        {
            return JsonConvertDefaultSetting.SerializeObject(mUnlockClues);
        }

        private string SyncUnlockTimeClues()
        {
            return JsonConvertDefaultSetting.SerializeObject(mUnlockTimeClues);
        }

        private void SyncDestinyClueStats()
        {
            mSlot.mPlayer.DestinyClueStats.destinyClues = SyncDestinyClues();
            DeleteAddClueAfterUpdate();
        }

        private void SyncUnlockMemoryStats()
        {
            mSlot.mPlayer.DestinyClueStats.unlockMemory = SyncUnlockMemory();
        }

        private void SyncUnlockCluesStats()
        {
            mSlot.mPlayer.DestinyClueStats.unlockClues = SyncUnlockClues();
        }

        private void SyncUnlockTimeCluesStats()
        {
            mSlot.mPlayer.DestinyClueStats.unlockTimeClues = SyncUnlockTimeClues();
        }

        public void ReadClue(int clueid, byte type)
        {
            ActivatedClueData clueData = GetClue(clueid, (ClueType)type);
            if (clueData != null && clueData.Status == (byte)ClueStatus.New)
            {
                bLocked = true;
                clueData.Status = (byte)ClueStatus.Read;
                SyncDestinyClueStats();
            }
        }

        public bool CollectDestinyClueReward(int clueid)
        {
            ActivatedClueData clueData = GetClue(clueid, ClueType.Dialogue);
            if (clueData != null && clueData.Status != (byte)ClueStatus.Collected)
            {
                HeroDialogueClueJson clueJson = DestinyClueRepo.GetHeroDialogueClueById(clueid);
                if (clueJson != null)
                {
                    bool isfull = false;
                    GameRules.GiveReward_CheckBagSlot(mSlot.mPlayer, new List<int>() { clueJson.reward }, out isfull, true, true, "DestinyClue_Reward");
                    if (!isfull)
                    {
                        bLocked = true;
                        clueData.Status = (byte)ClueStatus.Collected;
                        SyncDestinyClueStats();
                        return true;
                    }
                }
            }
            return false;
        }

        public void SaveDestinyClueInventory(DestinyClueInventory destinyClueInventory)
        {
            destinyClueInventory.SerializedActivatedClues(mDestinyClues);
            destinyClueInventory.SerializedUnlockMemory(mUnlockMemory);
            destinyClueInventory.SerializedLockedClues(mLockedClues);
            destinyClueInventory.SerializedUnlockClues(mUnlockClues);
            destinyClueInventory.SerializedUnlockTimeClues(mUnlockTimeClues);
        }

        public void Dispose()
        {
            if (mDialogueClueTimer != null)
            {
                mDialogueClueTimer.Stop();
                mDialogueClueTimer = null;
            }

            if (mTimeClueTimer != null)
            {
                mTimeClueTimer.Stop();
                mTimeClueTimer = null;
            }

            if (mTimeClueExpiredTimer != null)
            {
                foreach(KeyValuePair<int, Timer> entry in mTimeClueExpiredTimer)
                {
                    entry.Value.Stop();
                }
            }
            mTimeClueExpiredTimer = new Dictionary<int, Timer>();
        }

        public void Unlock(int level)
        {
            if (level >= 2 && !bFeatureUnlock)
            {
                bFeatureUnlock = true;
                HeroDialogueClueJson clueJson = DestinyClueRepo.GetHeroDialogueClueById(GameConstantRepo.GetConstantInt("DestinyTutorialId"));
                if (clueJson != null)
                {
                    ActivateDialogueClue(clueJson);
                }
                StartDialogueTimer();
                StartTimeClueTimer();
            }
        }
    }
}
