using System;
using System.Text;
using System.Collections.Generic;
using Zealot.Common.Datablock;
using Zealot.Repository;
using Zealot.Common;
using System.Globalization;
using Kopio.JsonContracts;

namespace Zealot.Common.Entities
{
    public class GuildMemberStatsRequest
    {
        public string name = "";
        public int portrait;        
        public byte viplvl = 0;
        public int lvl = 0;
        public int combatScore = 0;
        public DateTime requestDT;
        public int localObjIdx = 0;

        public GuildMemberStatsRequest()
        {}

        public GuildMemberStatsRequest(string name, int portrait, byte viplvl, int lvl, int combatscore, DateTime requestDT, int mLocalObjIdx)
        {
            this.name = name;
            this.portrait = portrait;
            this.viplvl = viplvl;
            this.lvl = lvl;
            this.combatScore = combatscore;
            this.requestDT = requestDT;
            localObjIdx = mLocalObjIdx;
        }

        public void ToObject(string str)
        {
            string[] infos = str.Split(';');
            if (infos.Length == 6)
            {
                int idx = 0;
                name = infos[idx++];
                portrait = int.Parse(infos[idx++]);
                viplvl = byte.Parse(infos[idx++]);
                lvl = int.Parse(infos[idx++]);
                combatScore = int.Parse(infos[idx++]);
                requestDT = DateTime.ParseExact(infos[idx++], "d/M/yyyy H:m:s", CultureInfo.InvariantCulture);
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(name);
            sb.Append(";");
            sb.Append(portrait);
            sb.Append(";");
            sb.Append(viplvl);
            sb.Append(";");
            sb.Append(lvl);
            sb.Append(";");
            sb.Append(combatScore);
            sb.Append(";");
            sb.Append(requestDT.ToString("d/M/yyyy H:m:s"));
            return sb.ToString();
        }
    }

    public class GuildMemberStats
    {
        public byte rank;
        public string name = "";
        public int fundToday = 0;
        public long fundTotal = 0;
        public int combatScore = 0;
        public bool online;
        public int localObjIdx = 0;

        public GuildMemberStats()
        {}

        public GuildMemberStats(byte rank, string name, int fundToday, long fundTotal, int combatScore, bool online, int mLocalObjIdx)
        {
            this.rank = rank;
            this.name = name;
            this.fundToday = fundToday;
            this.fundTotal = fundTotal;
            this.combatScore = combatScore;
            this.online = online;
            this.localObjIdx = mLocalObjIdx;
        }

        public void ToObject(string str)
        {
            string[] infos = str.Split(';');
            if (infos.Length == 6)
            {
                int idx = 0;
                this.rank = byte.Parse(infos[idx++]);
                this.name = infos[idx++];
                this.fundToday = int.Parse(infos[idx++]);
                this.fundTotal = long.Parse(infos[idx++]);
                this.combatScore = int.Parse(infos[idx++]);
                this.online = bool.Parse(infos[idx++]);
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(rank);
            sb.Append(";");
            sb.Append(name);
            sb.Append(";");
            sb.Append(fundToday);
            sb.Append(";");
            sb.Append(fundTotal);
            sb.Append(";");
            sb.Append(combatScore);
            sb.Append(";");
            sb.Append(online);
            return sb.ToString();
        }

        public void OnNewDay()
        {
            fundToday = 0;
        }
    }

    public enum GuildHistoryType
    {
        JoinGuild,
        LeaveGuild,
        KickGuild,
        LoveBossKill,
        CaveMoney,
        CaveGoldSmall,
        CaveGoldLarge,
        TechLevelUp,
        MemberRank,
        ChangeLeader,
    }

    public class GuildHistory
    {
        public string historyStr = "";
        private int count = 0;

        public void Init(string history)
        {
            historyStr = history;
            if (!string.IsNullOrEmpty(history))
                count = history.Split(';').Length;
        }

        public void Add(GuildHistoryType type, string paramters)
        {
            string element = string.Format("{0}|{1}|{2}", (byte)type, DateTime.Now.ToString("d/M/yyyy H:m:s"), paramters);
            if (count >= 100)
            {
                historyStr = historyStr.Substring(historyStr.IndexOf(';') + 1);
                historyStr += ";" + element;
            }
            else
            {
                if (count == 0)
                    historyStr = element;
                else
                    historyStr += ";" + element;
                count++;
            }
        }
    }

    public class GuildStats : LocalObject
    {        
        private int _guildId;
        private string _name;
        private int _guildIcon;
        private bool _guildIconFree;
        private byte _guildLevel;
        private long _guildGold;
        private byte _faction;
        private int _rank; // Rank by combatscore
        private long _totalCombatScore;
        private string _requestSetting;
        private string _guildNotice;
        private string _techs;
        private int _smBossLevel;
        private int _smBossDmgDone;
        private string _smBossAttacker;
        private int _dreamHouseFavourability;
        private long _guildSecretShopTick;

        public Dictionary<GuildTechType, int> mGuildTechDict;       
        public GuildRequestSetting mRequestSetting;

        public GuildStats() : base(LOTYPE.GuildStats)
        {
            // Synced
            members = new CollectionHandler<object>(GuildRepo.MAX_MEMBERS);
            members.SetParent(this, "members");
            memberStatsDict = new Dictionary<string, GuildMemberStats>();
            memberRequests = new CollectionHandler<object>(GuildRepo.MAX_REQUEST);
            memberRequests.SetParent(this, "memberRequests");
            memberRequestsDict = new Dictionary<string, GuildMemberStatsRequest>();
            memberDonateInv = new DonateDataInv();
            donateRequestCDTime = new Dictionary<string, DateTime>();
            _totalCombatScore = 0;
            _rank = 0;

            // Not Synced            
            mGuildTechDict = new Dictionary<GuildTechType, int>();
            mRequestSetting = new GuildRequestSetting();
        }

        #region Synced objects
        public int guildId
        {
            get { return _guildId; }
            set { this.OnSetAttribute("guildId", value); _guildId = value; }
        }

        public string name
        {
            get { return _name; }
            set { this.OnSetAttribute("name", value); _name = value; }
        }

        public int guildIcon
        {
            get { return _guildIcon; }
            set { this.OnSetAttribute("guildIcon", value); _guildIcon = value; }
        }

        public bool guildIconFree
        {
            get { return _guildIconFree; }
            set { this.OnSetAttribute("guildIconFree", value); _guildIconFree = value; }
        }

        public byte guildLevel
        {
            get { return _guildLevel; }
            set { this.OnSetAttribute("guildLevel", value); _guildLevel = value; }
        }

        public long guildGold
        {
            get { return _guildGold; }
            set { this.OnSetAttribute("guildGold", value); _guildGold = value; }
        }

        public byte faction
        {
            get { return _faction; }
            set { this.OnSetAttribute("faction", value); _faction = value; }
        }

        public int rank
        {
            get { return _rank; }
            set { this.OnSetAttribute("rank", value); _rank = value; }
        }

        public long totalCombatScore
        {
            get { return _totalCombatScore; }
            set { this.OnSetAttribute("totalCombatScore", value); _totalCombatScore = value; }
        }

        public string requestSetting
        {
            get { return _requestSetting; }
            set { this.OnSetAttribute("requestSetting", value); _requestSetting = value; }
        }

        public string guildNotice
        {
            get { return _guildNotice; }
            set { this.OnSetAttribute("guildNotice", value); _guildNotice = value; }
        }

        public string techs
        {
            get { return _techs; }
            set { this.OnSetAttribute("techs", value); _techs = value; }
        }

        public int SMBossLevel
        {
            get { return _smBossLevel; }
            set { this.OnSetAttribute("SMBossLevel", value); _smBossLevel = value; }
        }

        public int SMBossDmgDone
        {
            get { return _smBossDmgDone; }
            set { this.OnSetAttribute("SMBossDmgDone", value); _smBossDmgDone = value; }
        }

        public string SMBossAttacker
        {
            get { return _smBossAttacker; }
            set { this.OnSetAttribute("SMBossAttacker", value); _smBossAttacker = value; }
        }

        public int DreamHouseFavourability
        {
            get { return _dreamHouseFavourability; }
            set { this.OnSetAttribute("DreamHouseFavourability", value); _dreamHouseFavourability = value; }
        }

        public long guildSecretShopTick
        {
            get { return _guildSecretShopTick; }
            set { this.OnSetAttribute("guildSecretShopTick", value); _guildSecretShopTick = value; }
        }

        public CollectionHandler<object> members { get; set; } // Store member info in string

        public CollectionHandler<object> memberRequests { get; set; } // Store member request info in string

        public DonateDataInv memberDonateInv { get; set; }

        public Dictionary<string,DateTime> donateRequestCDTime { get; set; }
        #endregion

        public Dictionary<string, GuildMemberStats> GetMemberStatsDict() { return memberStatsDict; }
        protected Dictionary<string, GuildMemberStats> memberStatsDict; // Member name <- GuildMemberStats

        public Dictionary<string, GuildMemberStatsRequest> GetMemberRequestsDict() { return memberRequestsDict; }
        protected Dictionary<string, GuildMemberStatsRequest> memberRequestsDict; // Member name <- GuildMemberStatsRequest
      
        public bool IsMemberFull()
        {
            return memberStatsDict.Count >= GuildRepo.GetGuildTechByTypeAndLevel(GuildTechType.Level, guildLevel).stats;
        }

        public bool IsRequestFull()
        {
            return memberRequestsDict.Count >= memberRequests.Count;
        }

        public float GetGuildTechStats(GuildTechType type)
        {
            if (mGuildTechDict.ContainsKey(type))
            {
                GuildTechLevelJson techLevelJson = GuildRepo.GetGuildTechByTypeAndLevel(type, mGuildTechDict[type]);
                if (techLevelJson != null)
                    return techLevelJson.stats;
            }              
            return 0;
        }
    }
}
