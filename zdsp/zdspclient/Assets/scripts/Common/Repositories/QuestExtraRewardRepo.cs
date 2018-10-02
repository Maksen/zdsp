using UnityEngine;
using Kopio.JsonContracts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Zealot.Common;

namespace Zealot.Repository
{
    public class QETaskData
    {
        private int             _taskId;
        private string          _taskName;
        private QuestExtraType  _taskType;
        private string          _taskObjective;
        private int             _taskCount;
        private string          _taskIconPath;
        private int             _reqGoldToDone;
        private int             _rewardActivePts;
        private CurrencyType    _currencyType;
        private int             _rewardCurrency;
        private int             _rewardItem;
        private LinkUIType      _linkUI;

        public QETaskData(int taskId, string taskName, QuestExtraType taskType, string taskObjective, int taskCount, string taskIconPath,
            int reqGoldToDone, int rewardActivePts, int lockgoldReward, int moneyReward, int vipExpReward, int rewardItem, LinkUIType linkUI)
        {
            _taskId             = taskId;
            _taskName           = taskName;
            _taskType           = taskType;
            _taskObjective      = taskObjective;
            _taskCount          = taskCount;
            _taskIconPath       = taskIconPath;
            _reqGoldToDone      = reqGoldToDone;
            _rewardActivePts    = rewardActivePts;
            SetCurrencyReward(lockgoldReward, moneyReward, vipExpReward);
            _rewardItem         = rewardItem;
            _linkUI             = linkUI;
        }

        public int TaskID()
        {
            return _taskId;
        }

        public string TaskName()
        {
            return _taskName;
        }

        public QuestExtraType TaskType()
        {
            return _taskType;
        }

        public string TaskObjective()
        {
            return _taskObjective;
        }

        public int TaskCount()
        {
            return _taskCount;
        }

        public string TaskIconPath()
        {
            return _taskIconPath;
        }

        public int GoldToDone()
        {
            return _reqGoldToDone;
        }

        public int RewardPts()
        {
            return _rewardActivePts;
        }

        public CurrencyType RewardCurrencyType()
        {
            return _currencyType;
        }

        public int RewardCurrency()
        {
            return _rewardCurrency;
        }

        public int RewardItem()
        {
            return _rewardItem;
        }

        public LinkUIType LinkUI()
        {
            return _linkUI;
        }

        private void SetCurrencyReward(int goldReward, int moneyReward, int vipExpReward)
        {

            if ((goldReward != 0 && moneyReward != 0 && vipExpReward != 0) ||
                (goldReward != 0 && moneyReward != 0 && vipExpReward == 0) ||
                (goldReward != 0 && moneyReward == 0 && vipExpReward != 0) ||
                (goldReward == 0 && moneyReward != 0 && vipExpReward != 0) ||
                (goldReward == 0 && moneyReward == 0 && vipExpReward == 0))
            {
                // No currencies or more than 1 currency reward entered!
                // This shouldn't happen!
                _currencyType = CurrencyType.None;
                _rewardCurrency = -1;
            }
            else if (goldReward != 0)
            {
                _currencyType = CurrencyType.LockGold;
                _rewardCurrency = goldReward;
            }
            else if (moneyReward != 0)
            {
                _currencyType = CurrencyType.Money;
                _rewardCurrency = moneyReward;
            }
        }
    }

    public class QETaskDataList
    {
        private List<QETaskData> _dataList;

        public QETaskDataList()
        {
            _dataList = new List<QETaskData>();
        }

        public bool Contains(QETaskData taskData)
        {
            QETaskData res = _dataList.Find(o => o.TaskID() == taskData.TaskID());

            return res != null;
        }

        public bool Contains(int taskId)
        {
            QETaskData res = _dataList.Find(o => o.TaskID() == taskId);

            return res != null;
        }

        public bool ContainsByIdx(int index)
        {
            if (index < 0 || index >= _dataList.Count)
            {
                return false;
            }

            return true;
        }

        public void Add(QETaskData taskData)
        {
            if(_dataList == null)
            {
                _dataList = new List<QETaskData>();
            }

            if(!Contains(taskData))
            {
                _dataList.Add(taskData);
            }

            _dataList = _dataList.OrderBy(o => o.TaskID()).ToList();
        }

        public QETaskData Get(int taskId)
        {
            if(!Contains(taskId))
            {
                return null;
            }

            return _dataList.Find(o => o.TaskID() == taskId);
        }

        public QETaskData GetByIdx(int index)
        {
            if (!ContainsByIdx(index))
            {
                return null;
            }

            return _dataList[index];
        }

        public int GetIdxByTaskID(int taskId)
        {
            if (!Contains(taskId))
            {
                return -1;
            }

            return _dataList.FindIndex(o => o.TaskID() == taskId);
        }

        public int Count()
        {
            return _dataList.Count;
        }
    }

    public class QERewardData
    {
        private int _boxId;
        private int _reqPoints;
        private int _rewardList;

        public QERewardData(int boxId, int reqPoints, int rewardListId)
        {
            _boxId = boxId;
            _reqPoints = reqPoints;
            _rewardList = rewardListId;
        }

        public int BoxID()
        {
            return _boxId;
        }

        public int ReqPoints()
        {
            return _reqPoints;
        }

        public int RewardList()
        {
            return _rewardList;
        }
    }

    public class QERewardDataList
    {
        private List<QERewardData> _dataList;

        public QERewardDataList()
        {
            _dataList = new List<QERewardData>();
        }

        public bool Contains(QERewardData rewardData)
        {
            QERewardData res = _dataList.Find(o => o.BoxID() == rewardData.BoxID());

            return res != null;
        }

        public bool Contains(int boxId)
        {
            QERewardData res = _dataList.Find(o => o.BoxID() == boxId);

            return res != null;
        }

        public bool ContainsByIdx(int index)
        {
            if(index < 0 || index >= _dataList.Count)
            {
                return false;
            }

            return true;
        }

        public void Add(QERewardData rewardData)
        {
            if (_dataList == null)
            {
                _dataList = new List<QERewardData>();
            }

            if (!Contains(rewardData))
            {
                _dataList.Add(rewardData);
            }

            _dataList = _dataList.OrderBy(o => o.BoxID()).ToList();
        }

        public QERewardData Get(int boxId)
        {
            if (!Contains(boxId))
            {
                return null;
            }

            return _dataList.Find(o => o.BoxID() == boxId);
        }

        public QERewardData GetByIdx(int index)
        {
            if(!ContainsByIdx(index))
            {
                return null;
            }

            return _dataList[index];
        }

        public int GetIdxByBoxNum(int boxNum)
        {
            if (!Contains(boxNum))
            {
                return -1;
            }

            return _dataList.FindIndex(o => o.BoxID() == boxNum);
        }

        public int Count()
        {
            return _dataList.Count;
        }
    }

    public enum QERTaskGetType
    {
        ActivePoints,
        Money,
        VIPXP,
        LockGold,
        Item
    }

    public class QuestExtraRewardsRepo
    {
        // Quest Extra Task
        public static Dictionary<int, QuestExtraTaskJson> _idTaskMap;
        public static QETaskDataList _questExtraTaskList;

        // Quest Extra Reward
        public static Dictionary<int, QuestExtraRewardJson> _idRewardMap;
        public static QERewardDataList _questExtraRewardList;

        static QuestExtraRewardsRepo()
        {
            _idTaskMap = new Dictionary<int, QuestExtraTaskJson>();
            _questExtraTaskList = new QETaskDataList();

            _idRewardMap = new Dictionary<int, QuestExtraRewardJson>();
            _questExtraRewardList = new QERewardDataList();
        }

        public static void Init(GameDBRepo gameData)
        {
            InitQuestExtraTask(gameData.QuestExtraTask);
            InitQuestExtraReward(gameData.QuestExtraReward);
        }

        private static void InitQuestExtraTask(Dictionary<int, QuestExtraTaskJson> questExtraTaskData)
        {
            _idTaskMap = questExtraTaskData;

            foreach(KeyValuePair<int, QuestExtraTaskJson> entry in questExtraTaskData)
            {
                int             taskId              = entry.Key;
                string          taskName            = entry.Value.questname;
                QuestExtraType  taskType            = entry.Value.questtype;
                string          taskObjective       = entry.Value.questobj;
                int             taskCount           = entry.Value.questcount;
                string          taskIconPath        = entry.Value.iconpath;
                int             goldToDone          = entry.Value.reqgold;
                RewardListJson  rewardList          = RewardListRepo.GetRewardById(entry.Value.rewardlist);
                int             activePts           = 0;
                int             goldReward          = 0;
                int             moneyReward         = 0;
                int             vipReward           = 0;
                int             rewardItem          = 0;
                LinkUIType      linkUI              = entry.Value.linkui;

                QETaskData taskData = new QETaskData(taskId, taskName, taskType, taskObjective, taskCount, taskIconPath, 
                    goldToDone, activePts, goldReward, moneyReward, vipReward, rewardItem, linkUI);
                _questExtraTaskList.Add(taskData);
            }
        }

        public static QETaskDataList GetTaskList()
        {
            return _questExtraTaskList;
        }

        private static void InitQuestExtraReward(Dictionary<int, QuestExtraRewardJson> questExtraRewardData)
        {
            _idRewardMap = questExtraRewardData;

            foreach(KeyValuePair<int, QuestExtraRewardJson> entry in questExtraRewardData)
            {
                int boxId = entry.Value.boxnum;
                int reqPts = entry.Value.reqpts;
                QERewardData rewardData = new QERewardData(boxId, reqPts, entry.Value.rewardlist);
                _questExtraRewardList.Add(rewardData);
            }
        }

        public static QERewardDataList GetRewardList()
        {
            return _questExtraRewardList;
        }
    }
}