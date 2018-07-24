using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Kopio.JsonContracts;
using System;
using System.Linq;
using Zealot.Common;

namespace Zealot.Repository
{
    public class DiscountItem
    {
        public int mItemid;
        public int mStackcount;

        public DiscountItem(int itemid, int stackcount)
        {
            mItemid = itemid;
            mStackcount = stackcount;
        }
    }

    public class SevenDaysTaskData
    {
        public string mLocalizedName;
        public int mCount1;
        public int mCount2;
        public int mRewardId;

        public SevenDaysTaskData()
        {
            mLocalizedName = "";
            mCount1 = 0;
            mCount2 = 0;
            mRewardId = 0;
        }

        public SevenDaysTaskData(NewServerActivityJson taskJson)
        {
            mLocalizedName = SevenDaysRepo.GetFormattedTaskName(taskJson);
            mCount1 = taskJson.count1;
            mCount2 = taskJson.count2;
            mRewardId = taskJson.rewardlist;
        }
    }

    public class SevenDaysTaskDataList
    {
        Dictionary<string, SevenDaysTaskData> _taskList;

        public SevenDaysTaskDataList()
        {
            _taskList = new Dictionary<string, SevenDaysTaskData>();
        }

        public void Insert(NewServerActivityJson taskJson)
        {
            SevenDaysTaskData task = new SevenDaysTaskData(taskJson);
            string taskName = SevenDaysRepo.GetFormattedTaskName(taskJson);

            if(!_taskList.ContainsKey(taskName))
            {
                _taskList.Add(taskName, task);
            }
        }

        public SevenDaysTaskData GetTask(string taskname)
        {
            if(_taskList.ContainsKey(taskname))
            {
                return _taskList[taskname];
            }

            return null;
        }

        public Dictionary<string, SevenDaysTaskData> GetList()
        {
            return _taskList;
        }
    }

    public enum SevenDaysSystem
    {
        SevenDaysTask,
        Restriction
    }

    public class SevenDaysRepo
    {
        // Restriction
        private static Dictionary<Days, RestrictionJson>                                _daysRestrictionJsonMap;        // Which Day -> Json Mapping

        // NewServerActivitySub
        private static Dictionary<int, NewServerActivitySubJson>                        _newSeverActvtySubIdJsonMap;    // ID -> Json Mapping

        // NewServerActivity
        private static Dictionary<Days, List<int>>                                      _daysSubCatIdMap;               // Days -> List of Sub Category Mapping
        private static Dictionary<Days, Dictionary<int, List<NewServerActivityJson>>>   _daysNewSeverActvtyJsonMap;     // Days -> Sub Category -> Name -> Json Mapping
        private static Dictionary<Days, Dictionary<int, SevenDaysTaskDataList>>         _daysNewSeverActTaskListMap;    // Days -> Sub Category -> Task Name -> Task Mapping

        static SevenDaysRepo()
        {
            // Restriction
            _daysRestrictionJsonMap = new Dictionary<Days, RestrictionJson>();

            // NewServerActivitySub
            _newSeverActvtySubIdJsonMap = new Dictionary<int, NewServerActivitySubJson>();

            // NewServerActivity
            _daysSubCatIdMap = new Dictionary<Days, List<int>>();
            _daysNewSeverActvtyJsonMap = new Dictionary<Days, Dictionary<int, List<NewServerActivityJson>>>();
            _daysNewSeverActTaskListMap = new Dictionary<Days, Dictionary<int, SevenDaysTaskDataList>>();
        }

        public static void Init(GameDBRepo gameData)
        {
            InitRestriction(gameData.Restriction);
            InitNewServerActivitySub(gameData.NewServerActivitySub);
            InitNewServerActivity(gameData.NewServerActivity);
        }

        private static void InitRestriction(Dictionary<int, RestrictionJson> restrictionData)
        {
            foreach(KeyValuePair<int, RestrictionJson> entry in restrictionData)
            {
                Days day = entry.Value.category;
                if(!_daysRestrictionJsonMap.ContainsKey(day))
                {
                    _daysRestrictionJsonMap.Add(day, entry.Value);
                }
            }
        }

        private static void InitNewServerActivitySub(Dictionary<int, NewServerActivitySubJson> newServerActivitySubData)
        {
            foreach (KeyValuePair<int, NewServerActivitySubJson> entry in newServerActivitySubData)
            {
                int id = entry.Value.id;
                if (!_newSeverActvtySubIdJsonMap.ContainsKey(id))
                {
                    _newSeverActvtySubIdJsonMap.Add(id, entry.Value);
                }
            }
        }

        private static void InitNewServerActivity(Dictionary<int, NewServerActivityJson> newServerActivityData)
        {
            foreach (KeyValuePair<int, NewServerActivityJson> entry in newServerActivityData)
            {
                Days day = entry.Value.maincategory;
                if (!_daysNewSeverActvtyJsonMap.ContainsKey(day))
                {
                    Dictionary<int, List<NewServerActivityJson>> subCatJsonMap = new Dictionary<int, List<NewServerActivityJson>>();
                    _daysNewSeverActvtyJsonMap.Add(day, subCatJsonMap);
                }

                if (!_daysNewSeverActTaskListMap.ContainsKey(day))
                {
                    Dictionary<int, SevenDaysTaskDataList> subCatTaskListMap = new Dictionary<int, SevenDaysTaskDataList>();
                    _daysNewSeverActTaskListMap.Add(day, subCatTaskListMap);
                }

                if (!_daysSubCatIdMap.ContainsKey(day))
                {
                    List<int> subCatList = new List<int>();
                    _daysSubCatIdMap.Add(day, subCatList);
                }

                int subCat = entry.Value.subcategory;
                if(!_daysNewSeverActvtyJsonMap[day].ContainsKey(subCat))
                {
                    List<NewServerActivityJson> jsonList = new List<NewServerActivityJson>();
                    _daysNewSeverActvtyJsonMap[day].Add(subCat, jsonList);
                }
                
                if(!_daysNewSeverActTaskListMap[day].ContainsKey(subCat))
                {
                    SevenDaysTaskDataList sevenDaysTaskList = new SevenDaysTaskDataList();
                    _daysNewSeverActTaskListMap[day].Add(subCat, sevenDaysTaskList);
                }

                _daysNewSeverActvtyJsonMap[day][subCat].Add(entry.Value);

                _daysNewSeverActTaskListMap[day][subCat].Insert(entry.Value);

                if(!_daysSubCatIdMap[day].Contains(subCat))
                {
                    _daysSubCatIdMap[day].Add(subCat);
                }
            }
        }

        public static int GetRestrictionId(Days day)
        {
            if (_daysRestrictionJsonMap.ContainsKey(day))
            {
                return _daysRestrictionJsonMap[day].id;
            }

            return -1;
        }

        public static IInventoryItem GetDiscountItem(Days day)
        {
            if(_daysRestrictionJsonMap.ContainsKey(day))
            {
                //DiscountItem discountItem = new DiscountItem(_daysRestrictionJsonMap[day].itemid, _daysRestrictionJsonMap[day].itemcount);
                IInventoryItem discountItem = GameRepo.ItemFactory.GetInventoryItem(_daysRestrictionJsonMap[day].itemid);
                if(discountItem != null)
                {
                    discountItem.StackCount = (ushort)_daysRestrictionJsonMap[day].itemcount;
                }

                return discountItem;
            }

            return null;
        }

        public static int GetOriginalPrice(Days day)
        {
            if (_daysRestrictionJsonMap.ContainsKey(day))
            {
                return _daysRestrictionJsonMap[day].displaypoints;
            }

            return -1;
        }

        public static int GetDiscountPrice(Days day)
        {
            if (_daysRestrictionJsonMap.ContainsKey(day))
            {
                return _daysRestrictionJsonMap[day].xenjopoints;
            }

            return -1;
        }

        public static int GetBuyLimit(Days day)
        {
            if (_daysRestrictionJsonMap.ContainsKey(day))
            {
                return _daysRestrictionJsonMap[day].limitcount;
            }

            return -1;
        }

        public static List<int> GetSubCategoryIDs()
        {
            List<int> subCatIDList = new List<int>();

            foreach(KeyValuePair<int, NewServerActivitySubJson> kvp in _newSeverActvtySubIdJsonMap)
            {
                subCatIDList.Add(kvp.Value.id);
            }

            return subCatIDList;
        }

        public static List<int> GetSubCategoryIDsByDay(Days day)
        {
            if(_daysSubCatIdMap.ContainsKey(day))
            {
                return _daysSubCatIdMap[day];
            }

            return null;
        }

        public static NewServerActivitySubJson GetSubCategoryByID(int subcatId)
        {
            if(_newSeverActvtySubIdJsonMap.ContainsKey(subcatId))
            {
                return _newSeverActvtySubIdJsonMap[subcatId];
            }

            return null;
        }
        
        public static int GetSubCategoryElementByID(Days day, int subcatId)
        {
            List<int> subCatList = GetSubCategoryIDsByDay(day);
            
            return subCatList.FindIndex(o => o == subcatId);
        }

        public static List<NewServerActivityJson> GetNewServerActivityList(Days day, int subCat)
        {
            if(_daysNewSeverActvtyJsonMap.ContainsKey(day))
            {
                List<int> realSubCatList = GetSubCategoryIDsByDay(day);
                if(subCat < 0 || subCat >= realSubCatList.Count)
                {
                    return null;
                }

                int realSubCat = realSubCatList[subCat];
                if(_daysNewSeverActvtyJsonMap[day].ContainsKey(realSubCat))
                {
                    return _daysNewSeverActvtyJsonMap[day][realSubCat];
                }
            }

            return null;
        }

        public static List<string> GetNewServerTaskNameList(Days day, int subCat)
        {
            if (_daysNewSeverActvtyJsonMap.ContainsKey(day))
            {
                List<int> realSubCatList = GetSubCategoryIDsByDay(day);
                if(subCat < 0 || subCat >= realSubCatList.Count)
                {
                    return null;
                }

                int realSubCat = realSubCatList[subCat];
                if (_daysNewSeverActvtyJsonMap[day].ContainsKey(realSubCat))
                {
                    List<string> taskNameList = new List<string>();
                    List<NewServerActivityJson> taskJsonList = _daysNewSeverActvtyJsonMap[day][realSubCat];

                    for(int i = 0; i < taskJsonList.Count; ++i)
                    {
                        string taskName = GetFormattedTaskName(taskJsonList[i]);
                        taskNameList.Add(taskName);
                    }

                    return taskNameList;
                }
            }

            return null;
        }

        public static int GetNewServerActivityTasksCount(Days day, int subCat)
        {
            if (_daysNewSeverActvtyJsonMap.ContainsKey(day))
            {
                List<int> realSubCatList = GetSubCategoryIDsByDay(day);
                if (subCat < 0 || subCat >= realSubCatList.Count)
                {
                    return 0;
                }

                int realSubCat = realSubCatList[subCat];
                if (_daysNewSeverActvtyJsonMap[day].ContainsKey(realSubCat))
                {
                    return _daysNewSeverActvtyJsonMap[day][realSubCat].Count;
                }
            }

            return 0;
        }

        public static SevenDaysTaskDataList GetNewServerActivityTasksList(Days day, int subCat)
        {
            if(_daysNewSeverActTaskListMap.ContainsKey(day))
            {
                List<int> realSubCatList = GetSubCategoryIDsByDay(day);
                if (subCat < 0 || subCat >= realSubCatList.Count)
                {
                    return null;
                }

                int realSubCat = realSubCatList[subCat];
                if (_daysNewSeverActTaskListMap[day].ContainsKey(realSubCat))
                {
                    return _daysNewSeverActTaskListMap[day][realSubCat];
                }
            }

            return null;
        }

        public static string GetFormattedTaskName(NewServerActivityJson taskJson)
        {
            string taskName = taskJson.description;
            Dictionary<string, string> param = new Dictionary<string, string>();
            if (taskJson.count1 > 0)
            {
                param.Add("count1", taskJson.count1.ToString());
            }
            if (taskJson.count2 > 0)
            {
                param.Add("count2", taskJson.count2.ToString());
            }

            return GameUtils.FormatString(taskName, param);
        }
    }
}
