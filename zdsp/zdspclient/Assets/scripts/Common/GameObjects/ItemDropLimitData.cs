using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Zealot.Repository;
using Zealot.Common.Entities;
using Kopio.JsonContracts;

namespace Zealot.Common
{
    public class ItemLimit
    {
        int mItemID;
        int mDailyLimit = -1;
        int mWeeklyLimit = -1;

        public int ItemID
        {
            get { return mItemID; }
        }
        public int DailyLimit
        {
            get { return mDailyLimit; }
            set { mDailyLimit = value; }
        }
        public int WeeklyLimit
        {
            get { return mWeeklyLimit; }
            set { mWeeklyLimit = value; }
        }

        public ItemLimit(int _itemid, int _dailylimit, int _weeklylimit)
        {
            mItemID = _itemid;
            mDailyLimit = _dailylimit;
            mWeeklyLimit = _weeklylimit;
        }
        public bool Expend()
        {
            if (mWeeklyLimit == 0 || mDailyLimit == 0)
                return false;

            if (mWeeklyLimit >= 0)
                mWeeklyLimit--;
            if (mDailyLimit >= 0)
                mDailyLimit--;
            return true;
        }
    }

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class ItemUseDropLimitData
    {
        [JsonProperty(PropertyName = "droplimitlst")]
        public List<ItemLimit> Droplimitlst = new List<ItemLimit>();
        [JsonProperty(PropertyName = "uselimitlst")]
        public List<ItemLimit> Uselimitlst = new List<ItemLimit>();

        public bool DropItem(IInventoryItem item)
        {
            if (item == null)
                return false;
            if (item.JsonObject.weeklygetlimit <= 0 && item.JsonObject.dailygetlimit <= 0)
                return true;

            ItemLimit dl = Droplimitlst.Find(i => i.ItemID == item.ItemID);
            if (dl == null)
            {
                dl = new ItemLimit(item.ItemID, item.JsonObject.dailygetlimit, item.JsonObject.weeklygetlimit);
                Droplimitlst.Add(dl);
            }

            return dl.Expend();
        }

        public bool UseItem(IInventoryItem item)
        {
            if (item == null)
                return false;
            if (item.JsonObject.weeklyuselimit <= 0 && item.JsonObject.dailyuselimit <= 0)
                return true;

            ItemLimit ul = Uselimitlst.Find(i => i.ItemID == item.ItemID);
            if (ul == null)
            {
                ul = new ItemLimit(item.ItemID, item.JsonObject.dailyuselimit, item.JsonObject.weeklyuselimit);
                Uselimitlst.Add(ul);
            }

            return ul.Expend();
        }

        public void ResetNewDay()
        {
            Droplimitlst.RemoveAll(x => x.WeeklyLimit < 0);
            for (int i = 0; i < Droplimitlst.Count; ++i)
            {
                ItemBaseJson item = GameRepo.ItemFactory.GetItemById(Droplimitlst[i].ItemID);
                Droplimitlst[i].DailyLimit = item.dailygetlimit;
            }

            Uselimitlst.RemoveAll(x => x.WeeklyLimit < 0);
            for (int i = 0; i < Uselimitlst.Count; ++i)
            {
                ItemBaseJson item = GameRepo.ItemFactory.GetItemById(Uselimitlst[i].ItemID);
                Uselimitlst[i].DailyLimit = item.dailyuselimit;
            }
        }

        public void ResetNewWeek()
        {
            Droplimitlst.Clear();
            Uselimitlst.Clear();
        }
    }
}
