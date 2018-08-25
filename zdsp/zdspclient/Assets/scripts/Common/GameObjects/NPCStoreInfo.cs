using System.Collections;
using System.Collections.Generic;
using Kopio.JsonContracts;
using Zealot.Common;
using System.Linq;
using System;

namespace Zealot.Common
{
    public class NPCStoreInfo
    {
        public enum ItemStoreType
        {
            Normal,
            Random,
            Barter
        }

        public abstract class Item
        {
            public int StoreID;
            public int ItemListID;
            public bool Show;
            public int ItemID;
            public int Remaining;
            public int ExCount;
            public ItemStoreType Type;
            public Frequency DailyOrWeekly;
            public IInventoryItem data;

            public Item(int storeid, int itemlistid, bool show, int itemid, int remaining, int excount, ItemStoreType type, Frequency freq)
            {
                StoreID = storeid; ItemListID = itemlistid; Show = show; ItemID = itemid; Remaining = remaining; ExCount = excount; Type = type; DailyOrWeekly = freq;
            }

            public string Key() { return string.Format("{0} {1}", StoreID.ToString(), ItemListID.ToString()); }
        };

		public class BarterReq
		{
			public int StoreID, ItemListID;
			public int ReqItemID, ReqItemValue;
		};
		public class StandardItem : Item
        {
            public int ItemValue;
            public NPCStoreInfo.SoldCurrencyType SoldType;
            public int SoldValue;
            public float Discount;
            public int SortNumber;
            public DateTime StartTime;
            public DateTime EndTime;

			public List<BarterReq> required_items = new List<BarterReq>();

			public StandardItem(int storeid, int itemlistid, bool show, int itemid, ItemStoreType type,
                int itemvalue,
                NPCStoreInfo.SoldCurrencyType soldtype,
                int soldvalue,
                float discount,
                int sortnumber,
                DateTime start,
                DateTime end,
                int excount,
                Frequency dailyorweekly
                ) : base(storeid, itemlistid, show, itemid, excount, excount, type, dailyorweekly)
            {
                ItemValue = itemvalue;
                SoldType = soldtype;
                SoldValue = soldvalue;
                Discount = discount;
                SortNumber = sortnumber;
                StartTime = start;
                EndTime = end;
                ExCount = excount;
            }

            public static StandardItem GetFromBase(Item i)
            {
                if (i.Type == ItemStoreType.Normal)
                {
                    return (StandardItem)i;
                }
                else
                    return null;
            }
        };

        public class Transaction
        {
            ItemStoreType itemtype;
            public DateTime bought;
            public StandardItem solditem = null;
            public StandardItem barteritem = null;

            public int remaining;
        };

        public enum StoreType { Normal = 0, Random = 1, Barter = 2 };
        public enum SoldCurrencyType { Normal = 0, Auction = 1 };
        public enum Frequency { Unlimited = 0, Daily = 1, Weekly = 2 };

        public int StoreID;
        public string NameCT;
        public string NameEN;
        public StoreType Type;

        public Dictionary<int, StandardItem> inventory = new Dictionary<int, StandardItem>();

        public NPCStoreInfo(int id, string namect, string nameen, StoreType type)
        {
            StoreID = id; NameCT = namect; NameEN = nameen; Type = type;
        }
    };    
}
