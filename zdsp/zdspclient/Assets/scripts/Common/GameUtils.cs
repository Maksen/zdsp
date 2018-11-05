using Kopio.JsonContracts;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using Zealot.Repository;
//using ExitGames.Logging;

namespace Zealot.Common
{
    public enum ClientPlatform : byte
    {
        Android = 0,
        iOS = 1,
        MyCard = 2
    }

    #region Server Type and Info
    public enum GameServerType : byte
    {
        Game = 0,  //
        Activity = 1, //
    }

    public enum ServerLoad : byte
    {
        Normal = 0,
        Busy = 1,
        Full = 2,
    }

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class ServerLine
    {
        [DefaultValue(1)]
        [JsonProperty(PropertyName = "sl")]
        public int serverLineId;

        [DefaultValue("")]
        [JsonProperty(PropertyName = "name")]
        public string displayName;

        [DefaultValue(false)]
        [JsonProperty(PropertyName = "rec")]
        public bool recommended;

        public ServerLine() { }
        public ServerLine(int serverLine, string name, bool recommended)
        {
            this.serverLineId = serverLine;
            this.displayName = name;
            this.recommended = recommended;
        }
    }

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class ServerLineList
    {
        [JsonProperty(PropertyName = "list")]
        public List<ServerLine> list = new List<ServerLine>();

        public string serializeString = "";
        public void Clear()
        {
            list.Clear();
            serializeString = "";
        }
    }

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class ServerConfig
    {
        [DefaultValue(0)]
        [JsonProperty(PropertyName = "id")]
        public int id;

        [DefaultValue("")]
        [JsonProperty(PropertyName = "addr")]
        public string ipAddr;

        [DefaultValue("")]
        [JsonProperty(PropertyName = "name")]
        public string servername;

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "type")]
        public byte servertype; //gameserver types, 0: android, 1: ios, 2:activity server as cross server

        [DefaultValue(2000)]
        [JsonProperty(PropertyName = "max")]
        public int maxplayer;

        [DefaultValue(1)]
        [JsonProperty(PropertyName = "line")]
        public int serverline;

        public string voicechat;
        public string serializeString;
        public int onlinePlayers = 0;

        public ServerConfig() { }
        public ServerConfig(int id, string ipaddr, string servername, byte servertype, 
            int maxplayer, int serverline, string voicechat)
        {
            this.id = id;
            this.ipAddr = ipaddr;
            this.servername = servername;
            this.servertype = servertype;
            this.maxplayer = maxplayer;
            this.serverline = serverline;
            this.voicechat = voicechat;
        }

        public bool IsGameServer()
        {
            return servertype == (byte)GameServerType.Game;
        }

        public ServerLoad GetServerLoad(int online)
        {
            if (online >= maxplayer)
                return ServerLoad.Full;
            else if (online >= 400)
                return ServerLoad.Busy;
            return ServerLoad.Normal;
        }

        public string Serialize()
        {
            return JsonConvertDefaultSetting.SerializeObject(this);
        }

        public static ServerConfig Deserialize(string data)
        {
            return JsonConvertDefaultSetting.DeserializeObject<ServerConfig>(data);
        }
    }

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class GMServerInfo : GMServerStatus
    {
        public string ipAddr;
        public byte servertype; //gameserver types, 0: android, 1: ios, 2:activity server as cross server
        public string name;

        public GMServerInfo(int id, string ipaddr, byte servertype, string name) : base(id)
        {
            this.ipAddr = ipaddr;
            this.servertype = servertype;
            this.name = name;
        }

        public bool IsGameServer()
        {
            return servertype == (byte)GameServerType.Game;
        }
    }

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class GMServerStatus
    {
        [JsonProperty(PropertyName = "0")]
        public int id;

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "1")]
        public int ccu;

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "2")]
        public int pcu;

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "3")]
        public int roomupdatedur;

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "4")]
        public byte cpuusage;

        [DefaultValue(false)]
        [JsonProperty(PropertyName = "5")]
        public bool online;

        public GMServerStatus() { }
        public GMServerStatus(int id)
        {
            this.id = id;
            ccu = 0;
            roomupdatedur = 50;
            cpuusage = 0;
            online = false;
        }

        public void Update(int ccu)
        {
            this.ccu = ccu;
            if (pcu < ccu)
                pcu = ccu;
        }
    }
    #endregion

    public enum RealmState : byte
    {
        Created,
        Preparation,
        Started,
        Ended
    }

    public enum GuildActivityType
    {
        DreamHouse,
        Quest,

        TotalActivity
    }

    public enum GuildQuestOperation : byte
    {
        Fetch,
        Refresh,
        Cancel,
        Accept,
        Finish,
        Fastforwad,
    }

    public enum GuildQuestOperationError : byte
    {
        General,
        NotEnoughGold, 
        QuestNotFound,
        QuestTimesNotEnough
    }

    public enum GuildQuestStatus : byte
    {
        Avialiable, 
        Accepted,
        CollectPending,
        Finished,
        Canceled,
    }

    public enum CallBackAction : byte
    {
        None,
        Interact, 
        BasicAttack, 
        ActiveSkill,      
    }

    public enum ActivityStatusBitIndex : byte
    {
        WorldBoss,
    }

    public enum DreamhouseType : byte
    {
        YiYeEnZe,
        HuaYanQiaoYu,
        QiZhenYiBao
    }

    public enum GMEventType
    {
        None,
        ItemMallUpdate,
        CompensateActivity,
        NPCShopDataUpdate
    }

    public enum CountryReportType : byte
    {
        ResourceMapKiller,
        OfficerSlayer,
        ResourceMapBossSlayer,
        YiZuZhanBossSlayer,
        DailyRefreshBaZhu,
        YiZuWinner,
        YunBiaoWinner,
        TianZiZhanWinner,
        TianZiZhanKiller,
    }

    public enum AuctionStatusBit : byte
    {
        AuctionOpen,
        NewRecord,
        CollectionAvailable
    }
    

    public enum LotteryCostType : byte
    {
        None,
        Free,
        Extra,
        Gold
    }

    public class SpecialBossStatus
    {
        public int id;
        public bool isAlive;
        public DateTime? nextSpawn;

        //client only
        public string killer = "";
        public string score = "";

        //server only;
        public string lastKiller = ""; //name-score
        public string payload = "";

        public SpecialBossStatus() { }

        public SpecialBossStatus(int id, bool isAlive, string lastKiller, string payload)
        {
            this.id = id;
            this.isAlive = isAlive;
            this.lastKiller = lastKiller;
            this.payload = payload;
        }

        public override string ToString()
        {
            if (nextSpawn.HasValue)
                return string.Format("{0}|{1}|{2}|{3}", id, isAlive, lastKiller, nextSpawn.Value.Ticks);
            return string.Format("{0}|{1}|{2}", id, isAlive, lastKiller);
        }

        public static SpecialBossStatus Deserialize(string status)
        {
            if (string.IsNullOrEmpty(status))
                return null;
            SpecialBossStatus _ret = new SpecialBossStatus();
            string[] _infos = status.Split('|');
            _ret.id = int.Parse(_infos[0]);
            _ret.isAlive = bool.Parse(_infos[1]);
            if (!string.IsNullOrEmpty(_infos[2]))
            {
                string[] _killInfo = _infos[2].Split('-');
                _ret.killer = _killInfo[0];
                _ret.score = _killInfo[1];
            }
            if (_infos.Length == 4)
                _ret.nextSpawn = new DateTime(long.Parse(_infos[3]));
            return _ret;
        }
    }

    #region GMActivity
    public enum GMActivityType : byte
    {
        //Guild
        YouMengLou = 0,
        GuildQuest = 1,
        CurrencyExchange = 2,
        HeroHouse = 3,
        Dungeon = 4,
    }

    public class GMActivityConfigData
    {
        public DateTime mStartDT;
        public DateTime mEndDT;
        public List<int> mDataList;

        public GMActivityConfigData(DateTime start, DateTime end, List<int> datalist)
        {
            mStartDT = start;
            mEndDT = end;
            mDataList = datalist;
        }
    }

    public static class GMActivityConfig
    {
        public static Dictionary<GMActivityType, List<GMActivityConfigData>> mActivityConfigByType = new Dictionary<GMActivityType, List<GMActivityConfigData>>();

        public static void CleanUp()
        {
            mActivityConfigByType.Clear();
        }

        public static GMActivityConfigData GetConfigInt(GMActivityType type, DateTime serverDt)
        {
            if (mActivityConfigByType.ContainsKey(type))
            {
                List<GMActivityConfigData> configs = mActivityConfigByType[type];
                for (int index = 0; index < configs.Count; index++)
                {
                    GMActivityConfigData config = configs[index];
                    if (config.mStartDT <= serverDt && config.mEndDT > serverDt)
                        return config;
                }
            }
            return null;
        }

        public static List<GMActivityConfigData> GetConfigIntList(GMActivityType type, DateTime serverDt)
        {
            if (mActivityConfigByType.ContainsKey(type))
            {
                List<GMActivityConfigData> configs = new List<GMActivityConfigData>(mActivityConfigByType[type]);
                configs.RemoveAll((config) => config.mStartDT > serverDt || config.mEndDT < serverDt);
                return configs;
            }
            return new List<GMActivityConfigData>() ;
        }

        public static void AddConfig(GMActivityType type, DateTime startdt, DateTime enddt, List<int> datalist)
        {
            if (!mActivityConfigByType.ContainsKey(type))
                mActivityConfigByType.Add(type, new List<GMActivityConfigData>());
            mActivityConfigByType[type].Add(new GMActivityConfigData(startdt, enddt, datalist));
        }

        public static new string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<GMActivityType, List<GMActivityConfigData>> kvp in mActivityConfigByType)
            {
                byte activitytype = (byte)kvp.Key;
                for (int index = 0; index < kvp.Value.Count; index++)
                {
                    GMActivityConfigData value = kvp.Value[index];
                    sb.Append(activitytype);
                    sb.Append("|");
                    sb.Append(value.mStartDT.ToString("yyyy/MM/dd HH:mm"));
                    sb.Append("|");
                    sb.Append(value.mEndDT.ToString("yyyy/MM/dd HH:mm"));
                    for (int x = 0; x < value.mDataList.Count; x++)
                    {
                        sb.Append("|");
                        sb.Append(value.mDataList[x]);
                    }
                    sb.Append(";");
                }
            }
            return sb.ToString();
        }
    }
    #endregion

    public static partial class GameUtils
    {
        //private static readonly ILogger log = LogManager.GetCurrentClassLogger();
        private static System.Random randomGen;

        public static int mActivityStatus = 0;
        public static bool mNewServerEventEnabled = false;
        public static int mAuctionStatus = 0;
        public const int currencyMax = 2100000000;
        public static bool IsServer = false;  

        static GameUtils()
        {
            randomGen = new System.Random();
        }

        public static System.Random GetRandomGenerator()
        {
            return randomGen;
        }

        public static bool IsBitSet(int data, int index)
        {
            return (data & 1 << index) != 0;
        }

        public static int SetBit(int data, int index)
        {
            return data | 1 << index; 
        }

        public static int UnsetBit(int data, int index)
        {
            return data & ~(1 << index);
        }

        public static int ToggleBit(int data, int index)
        {
            return data ^ 1 << index;
        }

        public static Vector3 YawToDirection(double yradians)
        {
            Vector3 vec;
            vec.x = (float)Math.Cos(yradians);
            vec.z = (float)Math.Sin(yradians);
            vec.y = 0;
            vec.Normalize();
            return vec;
        }

        public static double DirectionToYaw(Vector3 dir)
        {
            dir.Normalize();
            double angle = Math.Atan2(dir.z, dir.x);
            if (angle < 0)
                angle = angle + Math.PI * 2;

            return angle;
        }

        public static Vector3 RandomPos(Vector3 origin, float radius)
        {
            double rand = randomGen.NextDouble();
            Vector3 randomDir = YawToDirection(rand * Math.PI * 2);
            Vector3 pos = origin + randomDir * (float)(randomGen.NextDouble() * radius);
            return pos;
        }

        public static Vector3 RandomPosWithRadiusRange(Vector3 origin, float radiusMin, float radiusMax)
        {
            double rand = randomGen.NextDouble();
            Vector3 randomDir = YawToDirection(rand * Math.PI * 2);
            Vector3 pos = origin + randomDir * (radiusMin + (float)(randomGen.NextDouble() * (radiusMax - radiusMin)));
            return pos;
        }

        public static bool InRange(Vector3 posA, Vector3 posB, float radiusA, float radiusB = 1f)
        {
            //A ----------)-(---B
            Vector3 diff = posB - posA;
            float combinedRadii = radiusA + radiusB;
            return diff.sqrMagnitude <= combinedRadii * combinedRadii;
        }

        public static bool InRange2D(Vector3 posA, Vector3 posB, float radiusA, float radiusB = 0)
        {
            //A ----------)-(---B
            Vector3 diff = posB - posA;
            diff.y = 0;
            float combinedRadii = radiusA + radiusB;
            return diff.sqrMagnitude <= combinedRadii * combinedRadii;
        }

        public static Transform Search(this Transform target, string name)
        {
            if (target.name == name)
                return target;
            for (int i = 0; i < target.childCount; ++i)
            {
                var result = Search(target.GetChild(i), name);
                if (result != null)
                    return result;
            }
            return null;
        }

        public static void DebugWriteLine(string message)
        {
            System.Diagnostics.Debug.WriteLine(message);
        }

        /// <summary>
        /// better ver. of string FormatString(string str, Dictionary&lt;string,string&gt; parameters) when args count is large
        /// </summary>
        public static string FormatByName(string format, Dictionary<string, string> args,int capcityIncrease=32)
        {
            if (args.Count < 5)
                return FormatString(format, args);
            StringBuilder sb = new StringBuilder(format.Length + capcityIncrease);
            int begin = 0;
            int end = 0;
            int init = 0;
            do
            {
                begin = format.IndexOf('{', init);
                if (begin >= 0)
                {
                    end = format.IndexOf('}', begin);
                    if (end > begin)
                    {
                        sb.Append(format.Substring(init, begin - init));
                        string name = format.Substring(begin + 1, end - begin - 1);
                        string val;
                        if (args.TryGetValue(name, out val))
                            sb.Append(val);

                        init = end + 1;
                        continue;
                    }
                }
                sb.Append(format.Substring(init, format.Length - init));
                break;
            } while (init < format.Length);
            return sb.ToString();
        }

        public static string FormatString(string str, Dictionary<string, string> parameters)
        {
            foreach (KeyValuePair<string, string> entry in parameters)
            {
                str = str.Replace(string.Format("{{{0}}}", entry.Key), entry.Value);
            }
            return str;
        }

        public static Dictionary<string, string> FormatString(string str)
        {
            if (string.IsNullOrEmpty(str))
                return null;

            string[] formatString = str.Split(';');
            Dictionary<string, string> optionalparam = new Dictionary<string, string>();
            if (formatString.Length % 2 != 0)
            {
                return new Dictionary<string, string>();
            }

            for (int i = 0; i < formatString.Length; i += 2)
            {
                string key = formatString[i];
                string value = formatString[i+1];
                optionalparam.Add(key, value);
            }
            return optionalparam;
        }

        public static string FormatString(Dictionary<string, string> dic)
        {
            string res = "";
            if (dic != null)
            {
                foreach (var entry in dic)
                {
                    string newEntry = string.Format("{0};{1}", entry.Key, entry.Value);
                    if (string.IsNullOrEmpty(res))
                        res = newEntry;
                    else
                    res = string.Concat(res,";", newEntry);
                }
            }
            return res;
        }


        public static string FormatTimeString(int totalSeconds)
        {
            TimeSpan t = TimeSpan.FromSeconds(totalSeconds);
            if (t.Days > 0)
                return string.Format("{0:D2}:{1:D2}:{2:D2}:{3:D2}", t.Days, t.Hours, t.Minutes, t.Seconds);
            else if (t.Hours > 0)
                return string.Format("{0:D2}:{1:D2}:{2:D2}", t.Hours, t.Minutes, t.Seconds);
            return string.Format("{0:D2}:{1:D2}", t.Minutes, t.Seconds);
        }

        public static string FormatTimeStringTillMinute(int totalMinutes)
        {
            TimeSpan t = TimeSpan.FromMinutes(totalMinutes);
            if (t.Days > 0)
                return string.Format("{0:D2}:{1:D2}:{2:D2}", t.Days, t.Hours, t.Minutes);
            else if (t.Hours > 0)
                return string.Format("{0:D2}:{1:D2}", t.Hours, t.Minutes);
            return string.Format("{0:D2}:{1:D2}", 0, t.Minutes);
        }

        public static double Random(double min, double max)
        {
            double rand = randomGen.NextDouble();
            double range = max - min;
            return min + range * rand;
        }

        /// <summary>
        /// random int, INCLUSIVE of min and max
        /// </summary>
        public static int RandomInt(int min, int max)
        {
            if (min < max)
                return randomGen.Next(min, max + 1);
            return min;
        }

        public static long TimeToNextEvent(DateTime now, string eventformat, bool isdaily, out bool foundNext, int offset)
        {
            return isdaily ? TimeToNextEventDailyFormat(now, eventformat, out foundNext, offset) : TimeToNextEventWeeklyFormat(now, eventformat, out foundNext, offset);
        }

        public static long TimePassedFromPreviousEvent(DateTime now, string eventformat, bool isdaily, out bool foundPrevious)
        {
            return isdaily ? TimePassedFromPreviousEventDailyFormat(now, eventformat, out foundPrevious) : TimePassedFromPreviousEventWeeklyFormat(now, eventformat, out foundPrevious);
        }

        public static long TimeToNextEventDailyFormat(DateTime now, string daily, out bool foundNext, int offset)
        {
            foundNext = false;
            long timetoNextEvent = 0;
            string[] Dailys = daily.Split('|');
            int total_count = Dailys.Length;
            if (total_count == 0)
                return 0;
            for (int index = 0; index < total_count; ++index)
            {
                string[] hoursMinutes = Dailys[index].Split(':');
                if (hoursMinutes.Length >= 2)
                {
                    int hour = int.Parse(hoursMinutes[0]);
                    int minute = int.Parse(hoursMinutes[1]);
                    if (hour > now.Hour || (hour == now.Hour && minute > now.Minute))
                    {
                        DateTime nextEventTime = now.Date.AddHours(hour);
                        nextEventTime = nextEventTime.AddMinutes(minute);
                        timetoNextEvent = (long)(nextEventTime - now).TotalMilliseconds;
                        if (timetoNextEvent >= offset)
                        {
                            foundNext = true;
                            break;
                        }
                    }
                }
            }
            if (!foundNext)
            {
                string[] hoursMinutes = Dailys[0].Split(':');
                if (hoursMinutes.Length >= 2)
                {
                    int hour = int.Parse(hoursMinutes[0]);
                    int minute = int.Parse(hoursMinutes[1]);
                    DateTime nextEventTime = now.Date.AddDays(1);
                    nextEventTime = nextEventTime.AddHours(hour);
                    nextEventTime = nextEventTime.AddMinutes(minute);
                    timetoNextEvent = (long)(nextEventTime - now).TotalMilliseconds;
                    foundNext = true;
                }
            }
            return timetoNextEvent;
        }

        public static long TimeToNextEventWeeklyFormat(DateTime now, string weekly, out bool foundNext, int offset)
        {
            foundNext = false;
            long timetoNextEvent = 0;
            string[] Weeklys = weekly.Split('|');
            int total_count = Weeklys.Length;
            if (total_count == 0)
                return 0;
            for (int index = 0; index < total_count; ++index)
            {
                string[] weekHoursMinutes = Weeklys[index].Split(':');
                if (weekHoursMinutes.Length == 3)
                {
                    int week = int.Parse(weekHoursMinutes[0]);
                    int hour = int.Parse(weekHoursMinutes[1]);
                    int minute = int.Parse(weekHoursMinutes[2]);
                    int dayofweek = (int)now.DayOfWeek;
                    if (dayofweek == 0)
                        dayofweek = 7;
                    if (week > dayofweek || (week == dayofweek && hour > now.Hour) || (week == dayofweek && hour == now.Hour && minute > now.Minute))
                    {
                        DateTime nextEventTime = now.Date.AddDays(week - dayofweek);
                        nextEventTime = nextEventTime.AddHours(hour);
                        nextEventTime = nextEventTime.AddMinutes(minute);
                        timetoNextEvent = (long)(nextEventTime - now).TotalMilliseconds;
                        if (timetoNextEvent >= offset)
                        {
                            foundNext = true;
                            break;
                        }
                    }
                }
            }
            if (!foundNext)
            {
                string[] weekHoursMinutes = Weeklys[0].Split(':');
                if (weekHoursMinutes.Length == 3)
                {
                    int week = int.Parse(weekHoursMinutes[0]);
                    int hour = int.Parse(weekHoursMinutes[1]);
                    int minute = int.Parse(weekHoursMinutes[2]);
                    int dayofweek = (int)now.DayOfWeek;
                    if (dayofweek == 0)
                        dayofweek = 7;
                    DateTime nextEventTime = now.Date.AddDays(7 + week - dayofweek);
                    nextEventTime = nextEventTime.AddHours(hour);
                    nextEventTime = nextEventTime.AddMinutes(minute);
                    timetoNextEvent = (long)(nextEventTime - now).TotalMilliseconds;
                    foundNext = true;
                }
            }
            return timetoNextEvent;
        }

        public static bool IsEventOpen(DateTime now, string dateStr, int timelimit)
        {
            bool foundPrevious;
            string[] dates = dateStr.Split('|');
            string[] datesSplit = dates[0].Split(':');
            long time = 0;
            if (datesSplit.Length == 2)
            {
                time = TimePassedFromPreviousEventDailyFormat(now, dateStr, out foundPrevious);
            }
            else
            {
                time = TimePassedFromPreviousEventWeeklyFormat(now, dateStr, out foundPrevious);
            }
            return TimeSpan.FromMilliseconds(time).TotalSeconds < timelimit;
        }

        public static long TimePassedFromPreviousEventDailyFormat(DateTime now, string daily, out bool foundPrevious)
        {
            foundPrevious = false;
            long timeFromPreviousEvent = 0;
            string[] Dailys = daily.Split('|');
            int total_count = Dailys.Length;
            if (total_count == 0)
                return 0;
            for (int index = total_count - 1; index >= 0; --index)
            {
                string[] hoursMinutes = Dailys[index].Split(':');
                if (hoursMinutes.Length >= 2)
                {
                    int hour = int.Parse(hoursMinutes[0]);
                    int minute = int.Parse(hoursMinutes[1]);
                    if (hour < now.Hour || (hour == now.Hour && minute <= now.Minute))
                    {
                        DateTime previousEventTime = now.Date.AddHours(hour);
                        previousEventTime = previousEventTime.AddMinutes(minute);
                        timeFromPreviousEvent = (long)(now - previousEventTime).TotalMilliseconds;
                        foundPrevious = true;
                        break;
                    }
                }
            }
            if (!foundPrevious)
            {
                string[] hoursMinutes = Dailys[total_count - 1].Split(':');
                if (hoursMinutes.Length >= 2)
                {
                    int hour = int.Parse(hoursMinutes[0]);
                    int minute = int.Parse(hoursMinutes[1]);
                    DateTime previousEventTime = now.Date.AddDays(-1);
                    previousEventTime = previousEventTime.AddHours(hour);
                    previousEventTime = previousEventTime.AddMinutes(minute);
                    timeFromPreviousEvent = (long)(now - previousEventTime).TotalMilliseconds;
                    foundPrevious = true;
                }
            }
            return timeFromPreviousEvent;
        }

        public static long TimePassedFromPreviousEventWeeklyFormat(DateTime now, string weekly, out bool foundPrevious)
        {
            foundPrevious = false;
            long timeFromPreviousEvent = 0;
            string[] Weeklys = weekly.Split('|');
            int total_count = Weeklys.Length;
            if (total_count == 0)
                return 0;
            for (int index = total_count - 1; index >= 0; --index)
            {
                string[] weekHoursMinutes = Weeklys[index].Split(':');
                if (weekHoursMinutes.Length == 3)
                {
                    int week = int.Parse(weekHoursMinutes[0]);
                    int hour = int.Parse(weekHoursMinutes[1]);
                    int minute = int.Parse(weekHoursMinutes[2]);
                    int dayofweek = (int)now.DayOfWeek;
                    if (dayofweek == 0)
                        dayofweek = 7;
                    if (week < dayofweek || (week == dayofweek && hour < now.Hour) || (week == dayofweek && hour == now.Hour && minute <= now.Minute))
                    {
                        DateTime previousEventTime = now.Date.AddDays(week - dayofweek);
                        previousEventTime = previousEventTime.AddHours(hour);
                        previousEventTime = previousEventTime.AddMinutes(minute);
                        timeFromPreviousEvent = (long)(now - previousEventTime).TotalMilliseconds;
                        foundPrevious = true;
                        break;
                    }
                }
            }
            if (!foundPrevious)
            {
                string[] weekHoursMinutes = Weeklys[total_count-1].Split(':');
                if (weekHoursMinutes.Length == 3)
                {
                    int week = int.Parse(weekHoursMinutes[0]);
                    int hour = int.Parse(weekHoursMinutes[1]);
                    int minute = int.Parse(weekHoursMinutes[2]);
                    int dayofweek = (int)now.DayOfWeek;
                    if (dayofweek == 0)
                        dayofweek = 7;
                    DateTime previousEventTime = now.Date.AddDays(-7 + week - dayofweek);
                    previousEventTime = previousEventTime.AddHours(hour);
                    previousEventTime = previousEventTime.AddMinutes(minute);
                    timeFromPreviousEvent = (long)(now - previousEventTime).TotalMilliseconds;
                    foundPrevious = true;
                }
            }
            return timeFromPreviousEvent;
        }

        public static int GetExpandBagTime(int firstSlotTime, long pDTSlotOpenTime, int numOfSlots,DateTime now)
        {
            int minToOpenBag = GameConstantRepo.GetConstantInt("ExpandBagTime", 60);

            TimeSpan duration = new TimeSpan(now.Ticks - pDTSlotOpenTime);
            int minutes = (int)duration.TotalMinutes;
            int remainingTime = minToOpenBag - (firstSlotTime + minutes);
            if (remainingTime <= 0)
                remainingTime = 0;

            if (numOfSlots <= 0)
                return int.MaxValue;

            return (numOfSlots - 1) * minToOpenBag + remainingTime;
        }
        
        public static int GetExpandBagCost(int firstSlotTime, long pDTSlotOpenTime, int numOfSlots,DateTime now)
        {
            if (numOfSlots <= 0)
                return int.MaxValue;
            int BagExpandCost = GameConstantRepo.GetConstantInt("BagExpand", 999);
            int minToOpenBag = GameConstantRepo.GetConstantInt("ExpandBagTime", 60);

            bool isFirstFree = false;
            TimeSpan duration = new TimeSpan(now.Ticks - pDTSlotOpenTime);
            int minutes = (int)duration.TotalMinutes;
            int remainingTime = minToOpenBag - (firstSlotTime + minutes);

            if (remainingTime <= 0)
                isFirstFree = true;

            if (isFirstFree)
            {
                if (numOfSlots >= 1)
                    return (numOfSlots - 1) * BagExpandCost;
            }
            else
            {
                return numOfSlots * BagExpandCost;
            }

            return int.MaxValue;
        }

        public static string GetHyperTextTag(string linkName, string displayName)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(" <a name=\"");
            sb.Append(linkName);
            sb.Append("\">");
            sb.Append(displayName);
            sb.Append("</a>");
            return sb.ToString();
        }

        public static void SetCharacterFirstEquipments(EquipmentInventoryData equippedInventory, JobsectJson jobjson)
        {
            //List<int> mList = new List<int>();
            //mList.Add(jobjson.helmet);
            //mList.Add(jobjson.armor);
            //mList.Add(jobjson.shoe);
            //mList.Add(jobjson.gloves);
            //mList.Add(jobjson.weapon);
            //mList.Add(jobjson.decoration1);
            //mList.Add(jobjson.decoration2);

            //for (int i = 0; i < mList.Count; i++)
            //{
            //    if (mList[i] > 0)
            //    {
            //        EquipItem item = GameRepo.ItemFactory.GetEquipmentItem(mList[i]);
            //        if (item == null)
            //            continue;

            //        int slotid = (int)GameRepo.ItemFactory.GetEquipmentSlotType(item.EquipID);
            //        var equipItem = item as EquipItem;
            //        if (equipItem != null)
            //            equippedInventory.SetSlotItem(slotid, equipItem);
            //    }
            //}
        }

        public static string SerializeItemInfoList(List<ItemInfo> items)
        {
            string itemstr = "";
            if (items != null && items.Count > 0)
            {
                StringBuilder strBuild = new StringBuilder();
                for (int index = 0; index < items.Count; ++index)
                    strBuild.AppendFormat("{0}_{1};", items[index].itemId, items[index].stackCount);
                itemstr = strBuild.ToString().TrimEnd(';');
            }
            return itemstr;
        }

        public static List<ItemInfo> DeserializeItemInfoList(string iteminfos)
        {
            List<ItemInfo> itemList = new List<ItemInfo>();
            if (!string.IsNullOrEmpty(iteminfos))
            {
                string[] items = iteminfos.Split(';');
                for (int index = 0; index < items.Length; ++index)
                {
                    string[] itemid_count = items[index].Split('_');
                    if (itemid_count.Length == 2)
                        itemList.Add(new ItemInfo() { itemId = ushort.Parse(itemid_count[0]), stackCount = ushort.Parse(itemid_count[1]) });
                }
            }
            return itemList;
        }

        //for case like id1;id2 
        public static List<int> ParseStringToIntList(string data, char delimiter)
        {
            List<int> ret = new List<int>();
            if (!string.IsNullOrEmpty(data))
            {
                string[] arr = data.Split(delimiter);
                int length = arr.Length;
                for (int index = 0; index < length; ++index)
                {
                    int temp;
                    if (int.TryParse(arr[index], out temp))
                        ret.Add(temp);
                }
            }
            return ret;
        }

        public static bool IsEmptyString(string value)
        {
            if (string.IsNullOrEmpty(value) || value == " " || value == "#unlocalized#" || value == "#unnamed#")
            {
                return true;
            }
            return false;
        }
    }

    public static class JsonConvertDefaultSetting
    {
        public static JsonSerializerSettings DefaultSettings;

        static JsonConvertDefaultSetting()
        {
            DefaultSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.None,
                TypeNameHandling = TypeNameHandling.None,
                DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
                NullValueHandling = NullValueHandling.Ignore
            };
        }

        public static string SerializeObject(object value)
        {
            return JsonConvert.SerializeObject(value, DefaultSettings);
        }

        public static T DeserializeObject<T>(string value)
        {
            return JsonConvert.DeserializeObject<T>(value, DefaultSettings);
        }
    }

    public static class Pair
    {
        public static Pair<T1, T2> Create<T1, T2>(T1 item1, T2 item2)
        {
            return new Pair<T1, T2>(item1, item2);
        }
    }

    public class Pair<T1, T2> : IEquatable<Pair<T1, T2>>
    {
        private readonly T1 item1;
        private readonly T2 item2;

        public T1 Item1 { get { return item1; } }
        public T2 Item2 { get { return item2; } }

        public Pair(T1 item1, T2 item2)
        {
            this.item1 = item1;
            this.item2 = item2;
        }

        public bool Equals(Pair<T1, T2> other)
        {
            if (other == null)
                return false;

            return EqualityComparer<T1>.Default.Equals(Item1, other.Item1) &&
                   EqualityComparer<T2>.Default.Equals(Item2, other.Item2);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Pair<T1, T2>);
        }

        public override int GetHashCode()
        {
            return EqualityComparer<T1>.Default.GetHashCode(item1) * 37 +
                   EqualityComparer<T2>.Default.GetHashCode(item2);
        }
    }

    /// <summary>
    /// Simple mutable class with pair of values. If need equality comparison, please use above Pair class
    /// </summary>
    public class ValuePair<T1, T2>
    {
        public T1 Item1 { get; set; }
        public T2 Item2 { get; set; }

        public ValuePair(T1 item1, T2 item2)
        {
            Item1 = item1;
            Item2 = item2;
        }
    }
}
