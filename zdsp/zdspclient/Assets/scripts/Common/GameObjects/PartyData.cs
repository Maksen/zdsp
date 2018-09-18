using Newtonsoft.Json;
using Zealot.Common.Entities;
using Zealot.Repository;

namespace Zealot.Common
{
    public enum LeavePartyReason : byte
    {
        Self,
        Kick,
        Disband
    }

    public enum AutoAcceptType : byte
    {
        GuildFriends,
        All,
        Closed
    }

    public static class PartyData
    {
        public static readonly int MAX_MEMBERS = 5;
        public static readonly int MAX_REQUESTS = 20;
    }

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class PartySetting
    {
        [JsonProperty(PropertyName = "loc")]
        public int locationId { get; set; }

        [JsonProperty(PropertyName = "minlv")]
        public int minLevel { get; set; }

        [JsonProperty(PropertyName = "maxlv")]
        public int maxLevel { get; set; }

        [JsonProperty(PropertyName = "aa")]
        public AutoAcceptType autoAcceptType { get; set; }

        [JsonProperty(PropertyName = "note")]
        public string notes { get; set; }

        public PartySetting()
        {
            locationId = 0;
            minLevel = 1;
            maxLevel = CharacterLevelRepo.GetMaxLevel();
            autoAcceptType = AutoAcceptType.GuildFriends;
            notes = GUILocalizationRepo.GetLocalizedString("party_defaultNotes");
        }

        public PartySetting(int locId, int minLvl, int maxLvl, AutoAcceptType acceptType, string desc)
        {
            locationId = locId;
            minLevel = minLvl;
            maxLevel = maxLvl;
            autoAcceptType = acceptType;
            notes = desc;
        }

        public static PartySetting ToObject(string str)
        {
            return JsonConvertDefaultSetting.DeserializeObject<PartySetting>(str);
        }

        public override string ToString()
        {
            return JsonConvertDefaultSetting.SerializeObject(this);
        }

        public static bool operator ==(PartySetting ps1, PartySetting ps2)
        {
            return (ps1.locationId == ps2.locationId && ps1.minLevel == ps2.minLevel && ps1.maxLevel == ps2.maxLevel
                && ps1.autoAcceptType == ps2.autoAcceptType && ps1.notes == ps2.notes);
        }

        public static bool operator !=(PartySetting ps1, PartySetting ps2)
        {
            return !(ps1.locationId == ps2.locationId && ps1.minLevel == ps2.minLevel && ps1.maxLevel == ps2.maxLevel
                && ps1.autoAcceptType == ps2.autoAcceptType && ps1.notes == ps2.notes);
        }

        public override bool Equals(object obj)
        {
            return obj is PartySetting && this == (PartySetting)obj;
        }

        public override int GetHashCode()
        {
            return locationId.GetHashCode() ^ minLevel.GetHashCode() ^ maxLevel.GetHashCode() ^ autoAcceptType.GetHashCode() ^ notes.GetHashCode();
        }
    }

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class AvatarInfo
    {
        [JsonProperty(PropertyName = "eqt")]
        public EquipmentInventoryData equipInvData { get; set; }

        [JsonProperty(PropertyName = "job")]
        public JobType jobType { get; set; }

        [JsonProperty(PropertyName = "sex")]
        public Gender gender { get; set; }

        public AvatarInfo() {}

        public AvatarInfo(EquipmentInventoryData equipData, JobType job, Gender sex)
        {
            equipInvData = equipData;
            jobType = job;
            gender = sex;
        }
    }

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class PartyMember
    {
        [JsonProperty(PropertyName = "name")]
        public string name { get; set; }

        [JsonProperty(PropertyName = "lv")]
        public int level { get; set; }

        [JsonProperty(PropertyName = "avatar")]
        public AvatarInfo avatar { get; set; }

        [JsonProperty(PropertyName = "hp")]
        public float hp { get; set; }

        [JsonProperty(PropertyName = "mp")]
        public float mp { get; set; }

        [JsonProperty(PropertyName = "online")]
        public bool online { get; set; }

        [JsonProperty(PropertyName = "guild")]
        public string guildName { get; set; }

        [JsonProperty(PropertyName = "rejflw")]
        public bool rejectFollow { get; set; }

        [JsonProperty(PropertyName = "slot")]
        public int slotIdx { get; set; }

        [JsonProperty(PropertyName = "heroid")]
        public int heroId { get; set; }

        [JsonProperty(PropertyName = "tier")]
        public int heroTier { get; set; }

        public PartyMember() {}

        public PartyMember(string charname, int charlevel, AvatarInfo avatarInfo, float health, float mana, string guildname)
        {
            name = charname;
            level = charlevel;
            avatar = avatarInfo;
            hp = health;
            mp = mana;
            online = true;
            guildName = guildname;
            rejectFollow = false;  // default accept follow
        }

        public PartyMember(Hero hero, string owner)
        {
            name = owner + "_" + hero.HeroId;
            level = hero.Level;
            online = true;
            heroId = hero.HeroId;
            heroTier = hero.ModelTier;
        }

        public static PartyMember ToObject(string str)
        {
            JsonSerializerSettings jsonSetting = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None, DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore };
            jsonSetting.Converters.Add(new ClientInventoryItemConverter());
            return JsonConvert.DeserializeObject<PartyMember>(str, jsonSetting);
        }

        public override string ToString()
        {
            JsonSerializerSettings jsonSetting = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None, DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore };
            jsonSetting.Converters.Add(new ClientInventoryItemConverter());
            return JsonConvert.SerializeObject(this, Formatting.None, jsonSetting);
        }

        public string GetName()
        {
            if (IsHero())
                return HeroRepo.GetHeroById(heroId).localizedname;
            else
                return name;
        }

        public string GetPortraitPath()
        {
            if (IsHero())
                return HeroRepo.GetHeroById(heroId).smallportraitpath;
            else
                return JobSectRepo.GetJobPortraitPath(avatar.jobType);
        }

        public bool IsHero()
        {
            return heroId > 0;
        }

        public string GetHeroOwner()
        {
            if (IsHero())
            {
                string[] namestr = name.Split('_');
                return namestr[0];
            }
            else
                return "";
        }
    }

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class PartyRequest
    {
        [JsonProperty(PropertyName = "name")]
        public string name { get; set; }

        [JsonProperty(PropertyName = "lv")]
        public int level { get; set; }

        [JsonProperty(PropertyName = "job")]
        public JobType jobType { get; set; }

        [JsonProperty(PropertyName = "slot")]
        public int slotIdx { get; set; }

        public PartyRequest() {}

        public PartyRequest(string charname, int charlevel, JobType job)
        {
            name = charname;
            level = charlevel;
            jobType = job;
        }

        public static PartyRequest ToObject(string str)
        {
            return JsonConvertDefaultSetting.DeserializeObject<PartyRequest>(str);
        }

        public override string ToString()
        {
            return JsonConvertDefaultSetting.SerializeObject(this);
        }
    }

    // Party data for UI display
    public class PartyInfo
    {
        public int partyId;
        public int locationId;
        public int minLevel;
        public int maxLevel;
        public string leader;
        public int memberCount;
        public string notes;
        public bool isAutoAccept;

        public PartyInfo() {}

        public PartyInfo(PartyStats party)
        {
            partyId = party.partyId;
            locationId = party.mPartySetting.locationId;
            minLevel = party.mPartySetting.minLevel;
            maxLevel = party.mPartySetting.maxLevel;
            leader = party.leader;
            memberCount = party.MemberCount();
            notes = party.mPartySetting.notes;
            isAutoAccept = party.mPartySetting.autoAcceptType == AutoAcceptType.All;
        }
    }

    // Member data for UI display
    public class PartyMemberInfo
    {
        public string name;
        public int level;
        public int portraitId;

        public PartyMemberInfo() {}

        public PartyMemberInfo(string charName, int lvl, int portrait)
        {
            name = charName;
            level = lvl;
            portraitId = portrait;
        }
    }
}