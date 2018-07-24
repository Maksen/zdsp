using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Zealot.Repository;

namespace Zealot.Common
{
    public enum GuildReturnCode : byte
    {       
        Failed,
        Success,
        GuildNotFound,
        InsufficientPlayerLevel,
        InsufficientGold,
        GuildLevelTooLow,
        TechAlreadyMax,
        GuildLeaderOnly,
        UnableToAppoint,
        NameHasForbiddenWord,
        NameHasInvalidCharacter,
        NameCharacterLimit,
        NameAlreadyExist,    
        MemberRequestSuccess,
        MemberRequestSuccessRemoveOld,
        MemberRequestExist,
        MemberRequestLimit,
        //TargetHasGuild,
        GuildMemberFull,
        IncompatibleFaction,
        CombatScoreTooLow,
        VIPLevelTooLow,
        LeaveGuildCooldown,
        LeaderLeaveGuildFailed,
        KickFailRankTooLow,
        BeKicked,
    }

    public enum GuildSearchFilter : byte
    {
        All,
        GuildId,
        GuildName
    }

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class GuildData
    {
        #region serializable properties
        [DefaultValue(true)]
        [JsonProperty(PropertyName = "iconfree")]
        public bool GuildIconFree { get; set; }

        [JsonProperty(PropertyName = "memberrequestinv")]
        public GuildMemberRequestInvData MemberRequestInvData { get; set; }

        [JsonProperty(PropertyName = "donateinv")]
        public DonateDataInv DonateInvData { get; set; }

        [JsonProperty(PropertyName = "donaterequestcdtime")]
        public Dictionary<string,DateTime> donateRequestCDTime { get; set; }
        #endregion

        public bool IsDirty;

        public GuildData()
        {
            GuildIconFree = true;
            MemberRequestInvData =  new GuildMemberRequestInvData();
            DonateInvData = new DonateDataInv();
            donateRequestCDTime = new Dictionary<string, DateTime>();
        }

        #region Json Serialization
        public string SerializeForDB(bool formatIndented = false)
        {
            JsonSerializerSettings jsonSetting = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None, DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore };

            Formatting formatting = formatIndented ? Formatting.Indented : Formatting.None;
            return JsonConvert.SerializeObject(this, formatting, jsonSetting);
        }

        public static GuildData DeserializeFromDB(string guildData)
        {
            JsonSerializerSettings jsonSetting = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None, DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore };
            return JsonConvert.DeserializeObject<GuildData>(guildData, jsonSetting);
        }
        #endregion
    }

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class MemberRequestElement
    {
        [DefaultValue("")]
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "portrait")]
        public int Portrait { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "viplvl")]
        public byte VIPLevel { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "lvl")]
        public int ProgressLevel { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "combatscore")]
        public int CombatScore { get; set; }

        [JsonProperty(PropertyName = "dt")]
        public DateTime RequestDT { get; set; }

        public MemberRequestElement(string name, int portrait, byte viplvl, int level, int combatScore, DateTime requestDT)
        {
            Name = name;
            Portrait = portrait;
            VIPLevel = viplvl;
            ProgressLevel = level;
            CombatScore = combatScore;
            RequestDT = requestDT;
        }
    }

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class GuildRequestSetting
    {
        [DefaultValue(1)]
        [JsonProperty(PropertyName = "score")]
        public int CombatScore { get; set; }

        [DefaultValue(1)]
        [JsonProperty(PropertyName = "lvl")]
        public int ProgressLvl { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "vip")]
        public byte VipLvl { get; set; }

        [JsonProperty(PropertyName = "accept")]
        public bool AutoAccept { get; set; }

        public GuildRequestSetting()
        {
            CombatScore = 1;
            ProgressLvl = GuildRepo.CreateGuildMinLevel;
            VipLvl = 0;
            AutoAccept = false;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(CombatScore);
            sb.Append(";");
            sb.Append(ProgressLvl);
            sb.Append(";");
            sb.Append(VipLvl);
            sb.Append(";");
            sb.Append(AutoAccept);
            return sb.ToString();
        }

        public static GuildRequestSetting ToObject(string str)
        {
            GuildRequestSetting settings = new GuildRequestSetting();
            string[] infos = str.Split(';');
            if (infos.Length == 4)
            {
                int idx = 0;
                settings.CombatScore = int.Parse(infos[idx++]);
                settings.ProgressLvl = int.Parse(infos[idx++]);
                settings.VipLvl = byte.Parse(infos[idx++]);
                settings.AutoAccept = bool.Parse(infos[idx++]);
            }
            return settings;
        }
    }

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class GuildMemberRequestInvData
    {
        #region serializable properties
        [JsonProperty(PropertyName = "RequestList")]
        public List<MemberRequestElement> memberRequestList = new List<MemberRequestElement>();

        [JsonProperty(PropertyName = "rqset")]
        public GuildRequestSetting requestSetting { get; set; }
        #endregion

        public GuildMemberRequestInvData()
        {
            memberRequestList = new List<MemberRequestElement>();
            requestSetting = new GuildRequestSetting();
        }
    }

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class DonateDataInv
    {
        #region serializable properties
        [JsonProperty(PropertyName = "DonateRequestDic")]
        public Dictionary<string,DonateInventory> donateRequestDic;

        [JsonProperty(PropertyName = "DonateRemainingTimes")]
        public Dictionary<string, DonateRemainingTimesInv> donateRemainingTimes;

        [JsonProperty(PropertyName = "LastUpdateTime")]
        public DateTime lastUpdateTime;
        #endregion

        public DonateDataInv()
        {
            donateRequestDic = new Dictionary<string, DonateInventory>();
            donateRemainingTimes = new Dictionary<string, DonateRemainingTimesInv>();
            lastUpdateTime = DateTime.Now;
        }
    }

    public class GuildInfo
    {
        public int guildId;
        public string guildName;
        public byte faction;
        public byte guildLevel;
        public long totalCombatScore;
        public int memberCount;
        public int maxMemberCount;
        public int rank;

        public GuildInfo(int gid, string name, byte factionType, byte level, long tcs, int memCount, int maxMemCount, int ranking)
        {
            guildId = gid;
            guildName = name;
            faction = factionType;
            guildLevel = level;
            totalCombatScore = tcs;
            memberCount = memCount;
            maxMemberCount = maxMemCount;
            rank = ranking;
        }
    }

}
