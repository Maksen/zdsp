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

    public class PartyData
    {
        public static readonly int MAX_MEMBERS = 5;
        public static readonly int MAX_REQUESTS = 20;
    }

    public class PartySetting
    {
        public int locationId { get; set; }
        public int minLevel { get; set; }
        public int maxLevel { get; set; }
        public AutoAcceptType autoAcceptType { get; set; }
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

    public class PartyMember
    {
        public string name { get; set; }
        public int level { get; set; }
        public int portraitId { get; set; }
        public AvatarInfo avatar { get; set; }
        public float hp { get; set; }
        public float mp { get; set; }
        public bool online { get; set; }
        public string guildName { get; set; }
        public bool rejectFollow { get; set; }
        public int slotIdx { get; set; }
        public int heroId { get; set; }

        public PartyMember()
        {
        }

        public PartyMember(string charname, int charlevel, int portrait, AvatarInfo avatarInfo, float health, float mana, string guildname)
        {
            name = charname;
            level = charlevel;
            portraitId = portrait;
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
            portraitId = hero.ModelTier;
            online = true;
            heroId = hero.HeroId;
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

    public class PartyRequest
    {
        public string name { get; set; }
        public int level { get; set; }
        public JobType jobType { get; set; }
        public int portraitId { get; set; }
        public int slotIdx { get; set; }

        public PartyRequest()
        {
        }

        public PartyRequest(string charname, int charlevel, JobType job, int portrait)
        {
            name = charname;
            level = charlevel;
            jobType = job;
            portraitId = portrait;
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

        public PartyInfo()
        {
        }

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
        public bool isLeader;

        public PartyMemberInfo()
        {
        }

        public PartyMemberInfo(string charName, int lvl, int portrait, bool leader)
        {
            name = charName;
            level = lvl;
            portraitId = portrait;
            isLeader = leader;
        }
    }

    public class AvatarInfo
    {
        public EquipmentInventoryData equipInvData;
        public JobType jobType;
        public Gender gender;

        public AvatarInfo()
        {
        }

        public AvatarInfo(EquipmentInventoryData equipData, JobType job, Gender sex)
        {
            equipInvData = equipData;
            jobType = job;
            gender = sex;
        }
    }
}