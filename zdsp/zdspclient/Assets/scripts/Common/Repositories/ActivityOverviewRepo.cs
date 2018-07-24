using Kopio.JsonContracts;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Zealot.Repository
{
    public class ActivityOverviewInfo
    {
        public ActivityOverviewJson json;
        public DateTime startdate;
        public DateTime enddate;
        public DateTime starttime;
        public DateTime endtime;

        public ActivityOverviewInfo(ActivityOverviewJson json)
        {
            this.json = json;
            startdate = DateTime.ParseExact(json.startdate, "yyyyMMdd", CultureInfo.InvariantCulture);
            enddate = DateTime.ParseExact(json.enddate, "yyyyMMdd", CultureInfo.InvariantCulture);
            starttime = DateTime.ParseExact(json.starttime, "HHmm", CultureInfo.InvariantCulture);
            endtime = DateTime.ParseExact(json.endtime, "HHmm", CultureInfo.InvariantCulture);
        }
    }

    public static class ActivityOverviewRepo
    {
        public static Dictionary<int, ActivityOverviewInfo> mIdMap;

        static ActivityOverviewRepo()
        {
            mIdMap = new Dictionary<int, ActivityOverviewInfo>();
        }

        public static void Init(GameDBRepo gameData)
        {
            mIdMap.Clear();
            foreach (var kvp in gameData.ActivityOverview)
                mIdMap.Add(kvp.Key, new ActivityOverviewInfo(kvp.Value));
        }

        public static List<ActivityOverviewInfo> GetByWeekAndDate(DateTime serverNow, int day, bool today, int reqlvl)
        {
            int nowMinutesInDay = serverNow.Hour * 60 + serverNow.Minute;
            List<ActivityOverviewInfo> orderedList =  mIdMap.Values.OrderBy(x => x.endtime).ThenBy(x => x.starttime)
                .Where(x => x.json.reqlvl <= reqlvl 
                && x.startdate.Date <= serverNow && x.enddate.Date.AddDays(1) >= serverNow
                && (x.json.week.Contains('8') || x.json.week.Contains(day.ToString()))).ToList();
            if (today)
            {
                List<ActivityOverviewInfo> finalList = new List<ActivityOverviewInfo>();
                List<ActivityOverviewInfo> expiryList = new List<ActivityOverviewInfo>();
                List<ActivityOverviewInfo> notOpenList = new List<ActivityOverviewInfo>();
                for (int index = 0; index < orderedList.Count; index++)
                {
                    ActivityOverviewInfo info = orderedList[index];
                    int activityEndMinutesInDay = info.endtime.Hour * 60 + info.endtime.Minute;
                    if (activityEndMinutesInDay <= nowMinutesInDay) //expiry
                        expiryList.Add(orderedList[index]);
                    else
                    {
                        int activityStartMinutesInDay = info.starttime.Hour * 60 + info.starttime.Minute;
                        int difftostart = activityStartMinutesInDay - nowMinutesInDay;
                        if (difftostart <= 0)
                            finalList.Add(info);
                        else
                            notOpenList.Add(info);
                    }
                }
                finalList.AddRange(notOpenList.OrderBy(x => x.starttime).ToList());
                finalList.AddRange(expiryList);
                return finalList;
            }
            else
                return orderedList;
        }
    }
}


