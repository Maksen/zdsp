#define SCENEASSETS
using System;
using System.Text;
using System.Collections.Generic;
using Kopio.JsonContracts;
using Zealot.Common;

namespace Zealot.Repository
{
    public static class GUILocalizationRepo
    {
        public static Dictionary<int, GUILocalizedStringJson> mLocalizedStrIdMap;
        public static Dictionary<int, SystemMessageJson> mSysMsgIdMap;

        public static Dictionary<string, string> mLocalizedStrMap;
        public static Dictionary<string, int> mIdRefNameMap;
        
        public static string localizedTimeDay = "";  // 天
        public static string localizedTimeHour = ""; // 小時  for time countdown
        public static string localizedTimeMin = "";  // 分
        public static string localizedTimeSec = "";  // 秒
        public static string localizedTimeHr = "";   // 時  for Datetime display

        public static string localizedNum1 = "";
        public static string localizedNum2 = "";
        public static string localizedNum3 = "";
        public static string localizedNum4 = "";
        public static string localizedNum5 = "";
        public static string localizedNum6 = "";
        public static string localizedDay = "";  // 日
        public static string localizedWeek = ""; // 週
        public static string localizedMonth = "";
        public static string localizedYear = "";

        public static string colon = "";

        static GUILocalizationRepo()
        {
            mLocalizedStrIdMap = new Dictionary<int, GUILocalizedStringJson>();
            mSysMsgIdMap = new Dictionary<int, SystemMessageJson>();

            mLocalizedStrMap = new Dictionary<string, string>();
            mIdRefNameMap = new Dictionary<string, int>();
        }

        public static void Init(GameDBRepo gameData)
        {
            mLocalizedStrIdMap = gameData.GUILocalizedString;
            mSysMsgIdMap = gameData.SystemMessage;

            mLocalizedStrMap.Clear();
            mIdRefNameMap.Clear();

            foreach (KeyValuePair<int, GUILocalizedStringJson> entry in gameData.GUILocalizedString)
            {
                mLocalizedStrMap.Add(entry.Value.name, entry.Value.localizedval);
            }

            foreach (KeyValuePair<int, SystemMessageJson> entry in gameData.SystemMessage)
            {
                mIdRefNameMap.Add(entry.Value.name, entry.Value.id);
            }
            // Init localized time text
            localizedTimeDay = GetLocalizedString("time_day");
            localizedTimeHour = GetLocalizedString("time_hour");
            localizedTimeMin = GetLocalizedString("time_minute");
            localizedTimeSec = GetLocalizedString("time_second");
            localizedTimeHr = GetLocalizedString("time_hr");

            // Init localized numbers
            localizedNum1 = GetLocalizedString("com_One");
            localizedNum2 = GetLocalizedString("com_Two");
            localizedNum3 = GetLocalizedString("com_Three");
            localizedNum4 = GetLocalizedString("com_Four");
            localizedNum5 = GetLocalizedString("com_Five");
            localizedNum6 = GetLocalizedString("com_Six");
            localizedDay = GetLocalizedString("com_Day");
            localizedWeek = GetLocalizedString("com_Week");
            localizedMonth = GetLocalizedString("com_Month");
            localizedYear = GetLocalizedString("com_Year");

            colon = GetLocalizedString("com_colon");
        }

        public static string GetLocalizedString(string guiname)
        {
            string result = "";
            if(mLocalizedStrMap.TryGetValue(guiname, out result))
                return result;
            else
                return "#" + guiname; // when someone see this, pls add entry into guilocalization
        }

        public static string GetLocalizedString(string guiname, Dictionary<string, string> param)
        {
            string result = GetLocalizedString(guiname);
            return GameUtils.FormatString(result, param);
        }

        public static SystemMessageJson GetSysMsgById(int id)
        {
            SystemMessageJson result = null;
            mSysMsgIdMap.TryGetValue(id, out result);
            return result;
        }

        public static int GetSysMsgIdByName(string name)
        {
            int result = 0;
            mIdRefNameMap.TryGetValue(name, out result);
            return result;
        }

        public static string GetLocalizedSysMsgById(int id, Dictionary<string, string> param)
        {
            SystemMessageJson sysmsgjson = null;
            if(mSysMsgIdMap.TryGetValue(id, out sysmsgjson) == true)
            {
                if (param != null)
                {
                    return GameUtils.FormatString(sysmsgjson.localizedval, param);
                }
                return sysmsgjson.localizedval;
            }
            return string.Format("Cannot find id = {0}", id);
        }

        public static string GetLocalizedSysMsgByName(string name, Dictionary<string, string> param = null)
        {
            int id = GetSysMsgIdByName(name);
            if (id <= 0)
                return string.Format("#{0}", name);
            return GetLocalizedSysMsgById(id, param);
        }

        public static string GetLocalizedNumber(int number)
        {
            switch (number)
            {
                case 1: return localizedNum1;
                case 2: return localizedNum2;
                case 3: return localizedNum3;
                case 4: return localizedNum4;
                case 5: return localizedNum5;
                case 6: return localizedNum6;
                default: return "";
            }
        }

        public static string GetLocalizedTimeString(double totalSeconds)
        {
            if (totalSeconds < 0)
                return "0" + localizedTimeSec;
            TimeSpan t = TimeSpan.FromSeconds(totalSeconds);
            StringBuilder sb = new StringBuilder();
            if (t.Days > 0)
            {
                sb.Append("{0}");
                sb.Append(localizedTimeDay);
                sb.Append("{1:D1}");
                sb.Append(localizedTimeHour);
                return string.Format(sb.ToString(), t.Days, t.Hours);
            }
            else if (t.Hours > 0)
            {
                sb.Append("{0}");
                sb.Append(localizedTimeHour);
                sb.Append("{1:D1}");
                sb.Append(localizedTimeMin);
                return string.Format(sb.ToString(), t.Hours, t.Minutes);
            }
            else if (t.Minutes > 0)
            {
                sb.Append("{0}");
                sb.Append(localizedTimeMin);
                sb.Append("{1:D1}");
                sb.Append(localizedTimeSec);
                return string.Format(sb.ToString(), t.Minutes, t.Seconds);
            }
            else
            {
                sb.Append("{0}");
                sb.Append(localizedTimeSec);
                return string.Format(sb.ToString(), t.Seconds);
            }
        }

        public static string GetLocalizedTimeStringToMinute(double totalSeconds)
        {
            if (totalSeconds <= 0)
                return "0" + localizedTimeHour + "0" + localizedTimeMin;
            TimeSpan t = TimeSpan.FromSeconds(totalSeconds);
            StringBuilder sb = new StringBuilder();
            if (t.Days > 0)
            {
                sb.Append("{0}");
                sb.Append(localizedTimeDay);
                sb.Append("{1:D1}");
                sb.Append(localizedTimeHour);
                return string.Format(sb.ToString(), t.Days, t.Hours);
            }
            else if (t.Hours > 0)
            {
                sb.Append("{0}");
                sb.Append(localizedTimeHour);
                sb.Append("{1:D1}");
                sb.Append(localizedTimeMin);
                return string.Format(sb.ToString(), t.Hours, t.Minutes);
            }
            else if (t.Minutes > 0)
            {
                sb.Append("0");
                sb.Append(localizedTimeHour);
                sb.Append("{0}");
                sb.Append(localizedTimeMin);
                return string.Format(sb.ToString(), t.Minutes);
            }
            else
            {
                sb.Append("0");
                sb.Append(localizedTimeHour);
                sb.Append("1");
                sb.Append(localizedTimeMin);
                return string.Format(sb.ToString());
            }
        }

        public static string GetShortLocalizedTimeString(double totalSeconds)
        {
            if (totalSeconds< 0)
                return "0" + localizedTimeSec;
            TimeSpan t = TimeSpan.FromSeconds(totalSeconds);
            StringBuilder sb = new StringBuilder();

            if (t.Days > 0)
            {
                sb.Append("{0}");
                sb.Append(localizedTimeDay);
                if (t.Hours > 0)
                {
                    sb.Append("{1}");
                    sb.Append(localizedTimeHour);
                    return string.Format(sb.ToString(), t.Days, t.Hours);
                }
                return string.Format(sb.ToString(), t.Days);
            }
            else if (t.Hours > 0)
            {
                sb.Append("{0}");
                sb.Append(localizedTimeHour);
                if (t.Minutes > 0)
                {
                    sb.Append("{1}");
                    sb.Append(localizedTimeMin);
                    return string.Format(sb.ToString(), t.Hours, t.Minutes);
                }
                return string.Format(sb.ToString(), t.Hours);
            }
            if (t.Minutes > 0)
            {
                sb.Append("{0}");
                sb.Append(localizedTimeMin);
                if (t.Seconds > 0)
                {
                    sb.Append("{1}");
                    sb.Append(localizedTimeSec);
                    return string.Format(sb.ToString(), t.Minutes, t.Seconds);
                }
                return string.Format(sb.ToString(), t.Minutes);
            }
            else
            {
                sb.Append("{0}");
                sb.Append(localizedTimeSec);
                return string.Format(sb.ToString(), t.Seconds);
            }
        }

        public static string GetLocalizedTimeString(int totalSeconds, int length)
        {
            //length = 1 show seconds only
            //length = 2 show minutes and seconds;
            //length = 3 show hours, minutes, seconds; 
            //length = 4 show days, hours, minutes, seconds; 
            TimeSpan t = TimeSpan.FromSeconds(totalSeconds);
            StringBuilder sb = new StringBuilder();
            sb.Append("{0:D2}");
            switch (length)
            {
                case 4:
                    sb.Append(localizedTimeDay);
                    sb.Append("{1:D2}");
                    sb.Append(localizedTimeHour);
                    sb.Append("{2:D2}");
                    sb.Append(localizedTimeMin);
                    sb.Append("{3:D2}");
                    sb.Append(localizedTimeSec);
                    return string.Format(sb.ToString(), t.Days, t.Hours, t.Minutes, t.Seconds);
                case 3:
                    sb.Append(localizedTimeHour);
                    sb.Append("{1:D2}");
                    sb.Append(localizedTimeMin);
                    sb.Append("{2:D2}");
                    sb.Append(localizedTimeSec);
                    return string.Format(sb.ToString(), t.Hours, t.Minutes, t.Seconds);
                case 2:
                    sb.Append(localizedTimeMin);
                    sb.Append("{1:D2}");
                    sb.Append(localizedTimeSec);
                    return string.Format(sb.ToString(), t.Minutes, t.Seconds);
                default:
                    sb.Append(localizedTimeSec);
                    return string.Format(sb.ToString(), t.Seconds);
            }
        }

        public static string GetLocalizedSingleUnitTimeString(double totalSeconds)
        {
            string timeStr = "";

            int minute = 60;
            int hour = 3600;
            int day = 86400;

            TimeSpan t = TimeSpan.FromSeconds(totalSeconds);
            StringBuilder sb = new StringBuilder();

            // Seconds
            if (totalSeconds <= minute)
            {
                sb.Append("{0}");
                sb.Append(localizedTimeSec);
                timeStr = string.Format(sb.ToString(), t.Seconds);
            }
            // Minutes
            else if(totalSeconds <= hour)
            {
                sb.Append("{0}");
                sb.Append(localizedTimeMin);
                timeStr = string.Format(sb.ToString(), t.Minutes);
            }
            // Hours
            else if(totalSeconds <= day)
            {
                sb.Append("{0}");
                sb.Append(localizedTimeHr);
                timeStr = string.Format(sb.ToString(), t.Hours);
            }
            // Days
            else
            {
                sb.Append("{0}");
                sb.Append(localizedTimeDay);
                timeStr = string.Format(sb.ToString(), t.Days);
            }

            return timeStr;
        }

        public static string GetLocalizedMultiUnitTimeString(double totalSeconds)
        {
            string timeStr = "";

            int minute = 60;
            int hour = 3600;
            int day = 86400;

            TimeSpan t = TimeSpan.FromSeconds(totalSeconds);
            StringBuilder sb = new StringBuilder();

            // Seconds
            if (totalSeconds <= minute)
            {
                sb.Append("{0}");
                sb.Append(localizedTimeSec);
                timeStr = string.Format(sb.ToString(), t.Seconds);
            }
            // Minutes
            else if (totalSeconds <= hour)
            {
                sb.Append("{0}");
                sb.Append(localizedTimeMin);
                sb.Append("{1}");
                sb.Append(localizedTimeSec);
                timeStr = string.Format(sb.ToString(), t.Minutes, t.Seconds);
            }
            // Hours
            else if (totalSeconds <= day)
            {
                sb.Append("{0}");
                sb.Append(localizedTimeHr);
                sb.Append("{1}");
                sb.Append(localizedTimeMin);
                sb.Append("{2}");
                sb.Append(localizedTimeSec);
                timeStr = string.Format(sb.ToString(), t.Hours, t.Minutes, t.Seconds);
            }
            // Days
            else
            {
                sb.Append("{0}");
                sb.Append(localizedTimeDay);
                sb.Append("{1}");
                sb.Append(localizedTimeHr);
                sb.Append("{2}");
                sb.Append(localizedTimeMin);
                timeStr = string.Format(sb.ToString(), t.Days, t.Hours, t.Minutes);
            }

            return timeStr;
        }

        /// <summary>
        /// display = 1 show date and time,
        /// display = 2 show date only,
        /// display = 3 show time only
        /// </summary>
        public static string GetLocalizedDateTime(DateTime dt, int display = 1)
        {
            StringBuilder sb = new StringBuilder();
            switch (display)
            {
                case 1:
                    sb.Append("{0}");
                    sb.Append(localizedYear);
                    sb.Append("{1:D2}");
                    sb.Append(localizedMonth);
                    sb.Append("{2:D2}");
                    sb.Append(localizedDay);
                    sb.Append("{3:D2}");
                    sb.Append(localizedTimeHr);
                    sb.Append("{4:D2}");
                    sb.Append(localizedTimeMin);
                    return string.Format(sb.ToString(), dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute);
                case 2:
                    sb.Append("{0}");
                    sb.Append(localizedYear);
                    sb.Append("{1:D2}");
                    sb.Append(localizedMonth);
                    sb.Append("{2:D2}");
                    sb.Append(localizedDay);
                    return string.Format(sb.ToString(), dt.Year, dt.Month, dt.Day);
                default:
                    sb.Append("{0:D2}");
                    sb.Append(localizedTimeHr);
                    sb.Append("{1:D2}");
                    sb.Append(localizedTimeMin);
                    return string.Format(sb.ToString(), dt.Hour, dt.Minute);
            } 
        }
    }
}
