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
            public ItemStoreType Type;
            public IInventoryItem data;

            public Item(int storeid, int itemlistid, bool show, int itemid, ItemStoreType type)
            {
                StoreID = storeid; ItemListID = itemlistid; Show = show; ItemID = itemid; Type = type;
            }
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
            public int ExCount;
            public NPCStoreInfo.Frequency DailyOrWeekly;

            public StandardItem(int storeid, int itemlistid, bool show, int itemid, ItemStoreType type,
                int itemvalue,
                NPCStoreInfo.SoldCurrencyType soldtype,
                int soldvalue,
                float discount,
                int sortnumber,
                DateTime start,
                DateTime end,
                int excount,
                NPCStoreInfo.Frequency dailyorweekly
                ) : base(storeid, itemlistid, show, itemid, type)
            {
                ItemValue = itemvalue;
                SoldType = soldtype;
                SoldValue = soldvalue;
                Discount = discount;
                SortNumber = sortnumber;
                StartTime = start;
                EndTime = end;
                ExCount = excount;
                DailyOrWeekly = dailyorweekly;
            }
        };

        public class RandomItem : Item
        {
            public RandomItem(int storeid, int itemlistid, bool show, int itemid, ItemStoreType type) : base(storeid, itemlistid, show, itemid, type)
            {

            }
        };

        public class BarterItem : Item
        {
            public BarterItem(int storeid, int itemlistid, bool show, int itemid, ItemStoreType type) : base(storeid, itemlistid, show, itemid, type)
            {

            }
        };

        public enum StoreType { Normal = 0, Random = 1, Barter = 2 };
        public enum SoldCurrencyType { Normal = 0, Auction = 1 };
        public enum Frequency { Unlimited = 0, Daily = 1, Weekly = 2 };

        public int StoreID;
        public string NameCT;
        public string NameEN;
        public StoreType Type;

        public List<StandardItem> inventory = new List<StandardItem>();

        public NPCStoreInfo(int id, string namect, string nameen, StoreType type)
        {
            StoreID = id; NameCT = namect; NameEN = nameen; Type = type;
        }
    };    
}

