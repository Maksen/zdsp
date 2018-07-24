using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Kopio.JsonContracts;
using System.ComponentModel;
using Zealot.Common;
using Zealot.Repository;

namespace Zealot.Common
{
    public class SevenDaysTask
    {
        private int                     _id;
        private NewServerActivityType   _type;
        private int                     _progress;
        private int                     _count1;
        private int                     _count2;
        private bool                    _isCollected;

        public SevenDaysTask(int id, NewServerActivityType type, int progress, int count1, int count2 = 0)
        {
            _id             = id;
            _type           = type;
            _progress       = progress;
            _count1         = count1;
            _count2         = count2;
            _isCollected    = false;
        }

        public void Update(int progress)
        {
            if(IsComplete())
            {
                return;
            }

            if(_type != NewServerActivityType.Equipupgrade_n && 
                _type != NewServerActivityType.Herolevel_n && 
                _type != NewServerActivityType.Heroskilltotal_n)
            {
                if(_count2 > 0)
                {
                    if(_type == NewServerActivityType.Herosquality_n)
                    {
                        if(progress == Count2())
                        {
                            ++_progress;
                        }
                    }
                    else
                    {
                        if(progress >= Count2())
                        {
                            ++_progress;
                        }
                    }
                }
                else
                {
                    if(_type == NewServerActivityType.Herofighting_n && progress > _progress)
                    {
                        _progress = progress;
                    }
                    else
                    {
                        _progress = progress;
                    }
                }
            }
            else
            {
                _progress = progress;
            }

            if(IsComplete())
            {
                _progress = _count1;
            }
        }

        public void Update()
        {
            if (IsComplete())
            {
                return;
            }

            ++_progress;
        }

        public void SetProgress(int progress)
        {
            if(_type != NewServerActivityType.Militantrank)
            {
                _progress = progress > _count1 ? _count1 : progress;
            }
            else
            {
                _progress = progress;
            }
        }

        public int ID()
        {
            return _id;
        }

        public bool IsComplete()
        {
            bool isComplete = false;
            if(_type == NewServerActivityType.Militantrank)
            {
                isComplete = _progress <= _count1;
            }
            else
            {
                isComplete = _progress >= _count1;
            }

            return isComplete;
        }

        public NewServerActivityType Type()
        {
            return _type;
        }

        public int Progress()
        {
            return _progress;
        }

        public int Count1()
        {
            return _count1;
        }

        public int Count2()
        {
            return _type == NewServerActivityType.Herosquality_n ? _count2 - 1 : _count2;
        }

        public int RawCount2()
        {
            return _count2;
        }

        public void SetCollected(bool isCollected)
        {
            _isCollected = isCollected;
        }

        public void Collect()
        {
            _isCollected = true;
        }

        public bool IsCollected()
        {
            return _isCollected;
        }

        public void Reset()
        {
            _progress = _type == NewServerActivityType.Militantrank ? 500 : 0;
            _isCollected = false;
        }

        public void ResetCollected()
        {
            _isCollected = false;
        }
    }

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class SevenDaysInvTask
    {
        [DefaultValue("")]
        [JsonProperty(PropertyName = "taskname")]
        public string taskName { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "taskval1")]
        public int currentValue1 { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "taskval2")]
        public int currentValue2 { get; set; }
    }

    public enum BuyDiscItmCode
    {
        BuyFailed,
        BuySuccess
    }

    public class BuyDiscItmRetval
    {
        public BuyDiscItmCode mBuyCode;
        public string mBuyStr;

        public BuyDiscItmRetval(BuyDiscItmCode code, string buyStr)
        {
            mBuyCode = code;
            mBuyStr = buyStr;
        }
    }

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public partial class SevenDaysInvData
    {
        //private bool IsEventRunning;

        //private bool IsEventPeriod;

        //private bool IsCollectionPeriod;

        // Permanent
        //[JsonProperty(PropertyName = "sevenDaysEventStart")]
        public long SevenDaysEventStart;

        // Permanent
        [JsonProperty(PropertyName = "discountItemBoughtNums")]
        public string DiscountItemBoughtNums;

        // Lasts until player logs out
        List<SevenDaysTask> TaskProgressList;

        // Permanent
        [JsonProperty(PropertyName = "taskProgress")]
        public string TaskProgress;

        // Permanent
        [JsonProperty(PropertyName = "taskCollected")]
        public string TaskCollected;

        // Task reward collection
        // Permanent
        [JsonProperty(PropertyName = "dayOneTasksCollected")]
        public List<int> DayOneTasksCollected;

        // Permanent
        [JsonProperty(PropertyName = "dayOneTasksAllCollected")]
        public bool DayOneTasksAllCollected;

        // Permanent
        [JsonProperty(PropertyName = "dayTwoTasksCollected")]
        public List<int> DayTwoTasksCollected;

        // Permanent
        [JsonProperty(PropertyName = "dayTwoTasksAllCollected")]
        public bool DayTwoTasksAllCollected;

        // Permanent
        [JsonProperty(PropertyName = "dayThreeTasksCollected")]
        public List<int> DayThreeTasksCollected;

        // Permanent
        [JsonProperty(PropertyName = "dayThreeTasksAllCollected")]
        public bool DayThreeTasksAllCollected;

        // Permanent
        [JsonProperty(PropertyName = "dayFourTasksCollected")]
        public List<int> DayFourTasksCollected;

        // Permanent
        [JsonProperty(PropertyName = "dayFourTasksAllCollected")]
        public bool DayFourTasksAllCollected;

        // Permanent
        [JsonProperty(PropertyName = "dayFiveTasksCollected")]
        public List<int> DayFiveTasksCollected;

        // Permanent
        [JsonProperty(PropertyName = "dayFiveTasksAllCollected")]
        public bool DayFiveTasksAllCollected;

        // Permanent
        [JsonProperty(PropertyName = "daySixTasksCollected")]
        public List<int> DaySixTasksCollected;

        // Permanent
        [JsonProperty(PropertyName = "daySixTasksAllCollected")]
        public bool DaySixTasksAllCollected;

        // Permanent
        [JsonProperty(PropertyName = "daySevenTasksCollected")]
        public List<int> DaySevenTasksCollected;

        // Permanent
        [JsonProperty(PropertyName = "daySevenTasksAllCollected")]
        public bool DaySevenTasksAllCollected;

        // Task completion
        // Permanent
        [JsonProperty(PropertyName = "dayOneTasksCompleted")]
        public List<int> DayOneTasksCompleted;

        // Permanent
        [JsonProperty(PropertyName = "dayTwoTasksCompleted")]
        public List<int> DayTwoTasksCompleted;

        // Permanent
        [JsonProperty(PropertyName = "dayThreeTasksCompleted")]
        public List<int> DayThreeTasksCompleted;

        // Permanent
        [JsonProperty(PropertyName = "dayFourTasksCompleted")]
        public List<int> DayFourTasksCompleted;

        // Permanent
        [JsonProperty(PropertyName = "dayFiveTasksCompleted")]
        public List<int> DayFiveTasksCompleted;

        // Permanent
        [JsonProperty(PropertyName = "daySixTasksCompleted")]
        public List<int> DaySixTasksCompleted;

        // Permanent
        [JsonProperty(PropertyName = "daySevenTasksCompleted")]
        public List<int> DaySevenTasksCompleted;

        public const int MAX_SUBCAT = 5;
        public const int MAX_TASKS_PER_SUBCAT = 10;
        public const int MAX_TASKS_PER_DAY = 30;

        public SevenDaysInvData()
        {
            DiscountItemBoughtNums = "0|0|0|0|0|0|0";
            InitTaskProgressData();
        }

        public void InitDefault()
        {
            // Task reward collection
            if(DayOneTasksCollected == null || DayOneTasksCollected.Count == 0)
            {
                DayOneTasksCollected = new List<int>(new int[MAX_SUBCAT]);
            }
            DayOneTasksAllCollected = false;

            if(DayTwoTasksCollected == null || DayTwoTasksCollected.Count == 0)
            {
                DayTwoTasksCollected = new List<int>(new int[MAX_SUBCAT]);
            }
            DayTwoTasksAllCollected = false;

            if(DayThreeTasksCollected == null || DayThreeTasksCollected.Count == 0)
            {
                DayThreeTasksCollected = new List<int>(new int[MAX_SUBCAT]);
            }
            DayThreeTasksAllCollected = false;

            if(DayFourTasksCollected == null || DayFourTasksCollected.Count == 0)
            {
                DayFourTasksCollected = new List<int>(new int[MAX_SUBCAT]);
            }
            DayFourTasksAllCollected = false;

            if(DayFiveTasksCollected == null || DayFiveTasksCollected.Count == 0)
            {
                DayFiveTasksCollected = new List<int>(new int[MAX_SUBCAT]);
            }
            DayFiveTasksAllCollected = false;

            if(DaySixTasksCollected == null || DaySixTasksCollected.Count == 0)
            {
                DaySixTasksCollected = new List<int>(new int[MAX_SUBCAT]);
            }
            DaySixTasksAllCollected = false;

            if(DaySevenTasksCollected == null || DaySevenTasksCollected.Count == 0)
            {
                DaySevenTasksCollected = new List<int>(new int[MAX_SUBCAT]);
            }
            DaySevenTasksAllCollected = false;

            // Task completion
            if (DayOneTasksCompleted == null || DayOneTasksCompleted.Count == 0)
            {
                DayOneTasksCompleted = new List<int>(new int[MAX_SUBCAT]);
            }

            if (DayTwoTasksCompleted == null || DayTwoTasksCompleted.Count == 0)
            {
                DayTwoTasksCompleted = new List<int>(new int[MAX_SUBCAT]);
            }

            if (DayThreeTasksCompleted == null || DayThreeTasksCompleted.Count == 0)
            {
                DayThreeTasksCompleted = new List<int>(new int[MAX_SUBCAT]);
            }

            if (DayFourTasksCompleted == null || DayFourTasksCompleted.Count == 0)
            {
                DayFourTasksCompleted = new List<int>(new int[MAX_SUBCAT]);
            }

            if (DayFiveTasksCompleted == null || DayFiveTasksCompleted.Count == 0)
            {
                DayFiveTasksCompleted = new List<int>(new int[MAX_SUBCAT]);
            }

            if (DaySixTasksCompleted == null || DaySixTasksCompleted.Count == 0)
            {
                DaySixTasksCompleted = new List<int>(new int[MAX_SUBCAT]);
            }

            if (DaySevenTasksCompleted == null || DaySevenTasksCompleted.Count == 0)
            {
                DaySevenTasksCompleted = new List<int>(new int[MAX_SUBCAT]);
            }

            DiscountItemBoughtNums = "0|0|0|0|0|0|0";
            InitTaskProgressData();
        }
    }
}
