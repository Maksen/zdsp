using Kopio.JsonContracts;
using System;
using System.Collections.Generic;
using Zealot.Common;

namespace Zealot.Repository
{
    public class LootItem
    {
        public int itemid = 0;
        public CurrencyType currencyType = CurrencyType.None;
        public int probability;
        public int weight;
        public bool ignorelv;
        public bool ignoretime;
        public int min = 1;
        public int max = 1;

        public LootItem(LootItemGroupJson json)
        {
            itemid = json.itemid;
            currencyType = json.currency;
            probability = json.probability;
            weight = json.weight;
            ignorelv = json.ignorelv;
            ignoretime = json.ignoretime;
            string[] amtArray = json.amt.Split('|');
            int arrayLength = amtArray.Length;
            if (arrayLength == 1)
                min = max = int.Parse(amtArray[0]);
            else if (arrayLength == 2)
            {
                min = int.Parse(amtArray[0]);
                max = int.Parse(amtArray[1]);
            }
        }

        public int GetAmount()
        {
            if (min < max)
                return GameUtils.RandomInt(min, max);
            return min;
        }
    }

    public class LootItemGroup
    {
        public int groupWeightTotal = 0;
        public List<LootItem> groupItems = new List<LootItem>();
        public List<LootItem> nonGroupItems = new List<LootItem>();

        public void AddItem(LootItemGroupJson json)
        {
            if (json.grouptype)
            {
                if (json.weight > 0) //no need to add weight 0 item
                {
                    groupWeightTotal += json.weight;
                    groupItems.Add(new LootItem(json));
                }
            }
            else if (json.probability != 0) //no need to add 0% item
                nonGroupItems.Add(new LootItem(json));
        }

        public List<LootItem> RandomItems()
        {
            List<LootItem> ret = new List<LootItem>();
            LootItem lootItem;
            if (groupWeightTotal > 0)
            {
                int randWeight = GameUtils.RandomInt(1, groupWeightTotal);
                for (int index = 0; index < groupItems.Count; index++)
                {
                    lootItem = groupItems[index];
                    if (randWeight <= lootItem.weight)
                    {
                        ret.Add(lootItem);
                        break;
                    }
                    randWeight -= lootItem.weight;
                }
            }
            for (int index = 0; index < nonGroupItems.Count; index++)
            {
                lootItem = nonGroupItems[index];
                if (lootItem.probability == -1) //100% 
                    ret.Add(lootItem);
                else if (GameUtils.RandomInt(1, 1000000) <= lootItem.probability)
                    ret.Add(lootItem);
            }
            return ret;
        }
    }

    public class LootLink
    {
        public LootType lootType;
        public List<int> gids = new List<int>();

        public LootLink(LootLinkJson json)
        {
            lootType = json.loottype;
            if (!string.IsNullOrEmpty(json.gids))
            {
                string[] gidArray = json.gids.Split(';');
                for (int index = 0; index < gidArray.Length; index++)
                    gids.Add(int.Parse(gidArray[index]));
            }
        }
    }

    public struct EventTime
    {
        public int start; //900
        public int end;  //1000
    }

    public class EventLootLink
    {
        public LootType lootType;
        public List<int> gids = new List<int>();
        public DateTime start;
        public DateTime end;
        public string days = "";
        public string weeks = "";
        public List<EventTime> times = new List<EventTime>();

        public EventLootLink(EventLootLinkJson json)
        {
            lootType = json.loottype;
            if (!string.IsNullOrEmpty(json.gids))
            {
                string[] gidArray = json.gids.Split(';');
                for (int index = 0; index < gidArray.Length; index++)
                    gids.Add(int.Parse(gidArray[index]));
            }
            start = DateTime.ParseExact(json.datestart, "yyyy/MM/dd HH:mm", null);
            end = DateTime.ParseExact(json.dateend, "yyyy/MM/dd HH:mm", null);
            days = json.days;
            weeks = json.weeks;
            if (!string.IsNullOrEmpty(json.times))
            {
                string[] timeArray = json.times.Split('|');
                for (int index = 0; index < timeArray.Length; index++)
                {
                    string[] startEnd = timeArray[index].Split('-');
                    string[] hourMin = startEnd[0].Split(':');
                    EventTime eventTime;
                    eventTime.start = int.Parse(hourMin[0]) * 100 + int.Parse(hourMin[1]);
                    hourMin = startEnd[1].Split(':');
                    eventTime.end = int.Parse(hourMin[0]) * 100 + int.Parse(hourMin[1]);
                    times.Add(eventTime);
                }
            } 
        }

        public bool IsInEvent(DateTime now)
        {
            if (now < start || now >= end)
                return false;

            if (days == "0")
            {
                if (weeks != "0")
                {
                    int dayofweek = (int)now.DayOfWeek;
                    if (dayofweek == 0)
                        dayofweek = 7;
                    if (!weeks.Contains("[" + dayofweek + "]"))
                        return false;
                }
            }
            else
            {
                int dayofmonth = now.Day;
                if (!days.Contains("[" + dayofmonth + "]"))
                    return false;
            }
            if (times.Count > 0)
            {
                int nowTime = now.Hour * 100 + now.Minute;
                for (int index = 0; index < times.Count; index++)
                {
                    if (nowTime >= times[index].start && nowTime < times[index].end)
                        return true;
                }
                return false;
            }
            return true;
        }
    }

    public static class LootRepo
    {
        public static Dictionary<LootCorrectionType, LootCorrectionJson> mLootCorrection = new Dictionary<LootCorrectionType, LootCorrectionJson>();
        public static Dictionary<int, LootItemGroup> mLootItemGroup = new Dictionary<int, LootItemGroup>();
        public static Dictionary<int, LootLink> mLootLinks = new Dictionary<int, LootLink>();
        public static Dictionary<int, EventLootLink> mEventLootLinks = new Dictionary<int, EventLootLink>();
        public static Dictionary<int, LimitedItemJson> mLimitedItems = new Dictionary<int, LimitedItemJson>();

        public static void Init(GameDBRepo gameData)
        {
            mLootCorrection.Clear();
            mLootItemGroup.Clear();
            mLootLinks.Clear();
            mEventLootLinks.Clear();
            mLimitedItems.Clear();

            foreach (var kvp in gameData.LootCorrection)
                mLootCorrection.Add(kvp.Value.type, kvp.Value);
            int gid = 0;
            foreach (var kvp in gameData.LootItemGroup)
            {
                gid = kvp.Value.gid;
                if (!mLootItemGroup.ContainsKey(gid))
                    mLootItemGroup.Add(gid, new LootItemGroup());
                mLootItemGroup[gid].AddItem(kvp.Value);
            }
            foreach (var kvp in gameData.LootLink)
                mLootLinks[kvp.Key] = new LootLink(kvp.Value);
            foreach (var kvp in gameData.EventLootLink)
                mEventLootLinks[kvp.Key] = new EventLootLink(kvp.Value);
            foreach (var kvp in gameData.LimitedItem)
                mLimitedItems[kvp.Value.itemid] = kvp.Value;
        }   
        
        public static LootCorrectionJson GetLootCorrection(int leveldiff)
        {
            LootCorrectionType type = LootCorrectionType.Minus10;
            if (leveldiff >= -10)
                return null;
            else if (leveldiff >= -20)
                type = LootCorrectionType.Minus1120;
            else if (leveldiff >= -30)
                type = LootCorrectionType.Minus2130;
            else if (leveldiff >= -40)
                type = LootCorrectionType.Minus3140;
            else if (leveldiff >= -50)
                type = LootCorrectionType.Minus4150;
            else
                type = LootCorrectionType.Minus51;
            LootCorrectionJson json;
            mLootCorrection.TryGetValue(type, out json);
            return json;
        }

        public static List<LootItem> RandomItems(List<int> grpIds)
        {
            List<LootItem> lootItems = new List<LootItem>();
            for (int index = 0; index < grpIds.Count; index++)
                lootItems.AddRange(RandomItems(grpIds[index]));
            return lootItems;
        }

        public static List<LootItem> RandomItems(int gid)
        {
            LootItemGroup lootItemGroup;
            if (mLootItemGroup.TryGetValue(gid, out lootItemGroup))
                return lootItemGroup.RandomItems();
            return new List<LootItem>();
        }

        public static LootLink GetLootLink(int linkid)
        {
            LootLink lootLink;
            mLootLinks.TryGetValue(linkid, out lootLink);
            return lootLink;
        }

        public static EventLootLink GetEventLootLink(int linkid)
        {
            EventLootLink eventLootLink;
            mEventLootLinks.TryGetValue(linkid, out eventLootLink);
            return eventLootLink;
        }

        public static LimitedItemJson GetLimitedItemInfo(int itemid)
        {
            LimitedItemJson json;
            mLimitedItems.TryGetValue(itemid, out json);
            return json;
        }
    }
}


