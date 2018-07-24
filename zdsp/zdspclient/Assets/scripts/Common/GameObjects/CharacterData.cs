using System;
using System.ComponentModel;
using System.Collections.Generic;
using Newtonsoft.Json;
using Zealot.Repository;
using Zealot.Common.Entities;

namespace Zealot.Common
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class CharacterData
    {
        #region serializable properties

        [DefaultValue("")]
        [JsonProperty(PropertyName = "Name")]
        public string Name { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "Gender")]
        public byte Gender { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "job")]
        public byte JobSect { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "faction")]
        public byte Faction { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "fnkill")]
        public int FactionKill { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "fndie")]
        public int FactionDeath { get; set; }

        [JsonProperty(PropertyName = "training")]
        public bool TrainingRealmDone { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "lvl")]
        public int ProgressLevel { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "equipscore")]
        public int EquipScore { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "health")]
        public int Health { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "exp")]
        public int Experience { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "levelid")]
        public int lastlevelid { get; set; }

        [DefaultValue("")]
        [JsonProperty(PropertyName = "roomguid")]
        public string roomguid { get; set; }

        [JsonProperty(PropertyName = "lastpos")]
        public float[] lastpos { get; set; }

        [JsonProperty(PropertyName = "lastdir")]
        public float[] lastdirection { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "revive")]
        public int FreeReviveOnSpot { get; set; }

        [JsonProperty(PropertyName = "newday")]
        public DateTime NewDayDts { get; set; }

        [JsonProperty(PropertyName = "ihotbar")]
        public string ItemHotBarData { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "guildid")]
        public int GuildId { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "guildrank")]
        public byte GuildRank { get; set; }

        [DefaultValue("")]
        [JsonProperty(PropertyName = "gshopbuycnt")]
        public string GuildShopBuyCount { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "gsmbossentry")]
        public int GuildSMBossEntry { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "gsmbossextra")]
        public int GuildSMBossExtraEntry { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "gbossrealm")]
        public int GuildBossRewardRealm { get; set; }

        [JsonProperty(PropertyName = "newguildweek")]
        public DateTime NewGuildWeekDt { get; set; }

        [JsonProperty(PropertyName = "gleavecdtick")]
        public long LeaveGuildCDEndTick { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "guilddhused")]
        public byte GuildDreamHouseUsed { get; set; }

        [DefaultValue("")]
        [JsonProperty(PropertyName = "guilddhcollected")]
        public string GuildDreamHouseCollected { get; set; }

        [JsonProperty(PropertyName = "lstfrlottdrw")]
        public long LastFreeLotteryRoll { get; set; }

        [DefaultValue("")]
        [JsonProperty(PropertyName = "botsetting")]
        public string BotSetting { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "PortraitID")]
        public int portraitID { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "currencyExchangeTime")]
        public byte CurrencyExchangeTime { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "MountID")]
        public int MountID { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "recFacReward")]
        public bool GetRecommendedFactionReward { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "firstbuyflag")]
        public int FirstBuyFlag { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "firstbuycollected")]
        public int FirstBuyCollected { get; set; }
        #endregion

        [JsonProperty(PropertyName = "currency")]
        public CurrencyInventoryData CurrencyInventory { get; set; }

        [JsonProperty(PropertyName = "bag")]
        public ItemInventoryData ItemInventory { get; set; }

        [JsonProperty(PropertyName = "eqinv")]
        public EquipmentInventoryData EquipmentInventory { get; set; }

        [JsonProperty(PropertyName = "realminv")]
        public RealmInventoryData RealmInventory { get; set; }

        [JsonProperty(PropertyName = "questinv")]
        public QuestInventoryData QuestInventory { get; set; }

        [JsonProperty(PropertyName = "sideeffectinv")]
        public SideEffectInventoryData SideEffectInventory { get; set; }

        [JsonProperty(PropertyName = "skillInv")]
        public SkillInventoryData SkillInventory { get; set; }

        [JsonProperty(PropertyName = "itemkind")]
        public ItemKindData ItemKindInv { get; set; }

        [JsonProperty(PropertyName = "arenainv")]
        public ArenaInventoryData ArenaInventory { get; set; }

        [JsonProperty(PropertyName = "mailInv")]
        public MailInventoryData MailInventory { get; set; }

        [JsonProperty(PropertyName = "mallInv")]
        public ItemMallInventoryData ItemMallInventory { get; set; }

        [JsonProperty(PropertyName = "welfareinv")]
        public WelfareInventoryData WelfareInventory { get; set; }

        [JsonProperty(PropertyName = "sevendaysinv")]
        public SevenDaysInvData SevenDaysInventory { get; set; }

        [JsonProperty(PropertyName = "questextrarewardsinv")]
        public QuestExtraRewardsInvData QuestExtraRewardsInventory { get; set; }

        [JsonProperty(PropertyName = "offlineexpinv2")]
        public OfflineExpInventory2 OfflineExpInv2 { get; set; }

        [JsonProperty(PropertyName = "lotteryinv")]
        public LotteryInventoryData LotteryInventory { get; set; }

        [JsonProperty(PropertyName = "HeroInvData")]
        public HeroInvData HeroInventory { get; set; }

        [JsonProperty(PropertyName = "storeData")]
        public StoreData StoreData { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "RandomBox")]
        public long RandomBoxTimeTick { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "costbuffid")]
        public int costbuffid { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "costbuffgold")]
        public int costbuffgold { get; set; }

        [JsonProperty(PropertyName = "GuildQuests")]
        public GuildQuestInventory GuildQuests { get; set; }

        [JsonProperty(PropertyName = "PrizeGuaranteeData")]
        public PrizeGuaranteeData PrizeGuaranteeData { get; set; }

        [DefaultValue("")]
        [JsonProperty(PropertyName = "achievements")]
        public string Achievements { get; set; }

        [JsonProperty(PropertyName = "CombatStats")]
        public InspectCombatStats InspectCombatStats;

        [JsonProperty(PropertyName = "ExchangeShop")]
        public ExchangeShopInventory ExchangeShopInv;

        [JsonProperty(PropertyName = "PortraitData")]
        public PortraitData PortraitData;

        [JsonProperty(PropertyName = "CharacterInfoData")]
        public CharacterInfoData CharInfoData;

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "tutorialreddot")]
        public int tutorialreddot { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "battime")]
        public int BattleTime { get; set; }

        public CharacterData()
        {
            CurrencyInventory = new CurrencyInventoryData();
            ItemInventory = new ItemInventoryData();
            EquipmentInventory = new EquipmentInventoryData();
            RealmInventory = new RealmInventoryData();
            QuestInventory = new QuestInventoryData();
            SideEffectInventory = new SideEffectInventoryData();
            SkillInventory = new SkillInventoryData();
            ItemKindInv = new ItemKindData();
            ArenaInventory = new ArenaInventoryData();
            MailInventory = new MailInventoryData();
            ItemMallInventory = new ItemMallInventoryData();
            WelfareInventory = new WelfareInventoryData();
            SevenDaysInventory = new SevenDaysInvData();
            QuestExtraRewardsInventory = new QuestExtraRewardsInvData();
            OfflineExpInv2 = new OfflineExpInventory2();
            LotteryInventory = new LotteryInventoryData();
            StoreData = new StoreData();
            GuildQuests = new GuildQuestInventory();
            PrizeGuaranteeData = new PrizeGuaranteeData();
            InspectCombatStats = new InspectCombatStats();
            ExchangeShopInv = new ExchangeShopInventory();
            PortraitData = new PortraitData();
            CharInfoData = new CharacterInfoData();
            HeroInventory = new HeroInvData();
        }

        /// <summary>
        /// Sets default data when creating new character
        /// </summary>
        public void InitDefault(JobType jobsect)
        {
            ItemInventory.InitDefault();
            EquipmentInventory.InitDefault();
            //ItemKindInv.InitDefault();
            RestoreDefaultSkill();
            //WelfareInventory.InitDefault();
            //SevenDaysInventory.InitDefault();
            //QuestExtraRewardsInventory.InitDefault();
            //LotteryInventory.InitDefault();
            //PrizeGuaranteeData.InitDefault();
            //GuildQuests.InitDefault();
            //ExchangeShopInv.InitDefault();
            //PortraitData.InitDefault((JobType)jobsect);
            //StoreData.InitDefault();
        }

        public void ValidateDefault()
        {
            if (EquipmentInventory.Fashions.Count == 0)
                EquipmentInventory.InitDefault();
        }

        public void RestoreDefaultSkill()
        {
            SkillInventory.InitDefault(JobSectRepo.GetJobByType((JobType)JobSect));
        }

        public void ResetOnNewDay()
        {
            NewDayDts = DateTime.Today;
            CurrencyExchangeTime = 0;
            GuildSMBossEntry = 2;
            GuildDreamHouseUsed = 0;
            GuildDreamHouseCollected = "";
            ArenaInventory.Entries = 0;
            FreeReviveOnSpot = 0;
            // Remove all expired item
            ExchangeShopInv.NewDayReset();
            CurrencyInventory.GuildFundToday = 0;
        }

        public void ClearGuild()
        {
            GuildId = 0;
            GuildRank = (byte)GuildRankType.Member;
            CurrencyInventory.GuildFundToday = 0;
            CurrencyInventory.GuildFundTotal = 0;
        }

        #region Json Serialization
        public string SerializeForDB(bool formatIndented = false)
        {
            return SerializeForDB(this, formatIndented);
        }

        public static string SerializeForDB(object data, bool formatIndented = false)
        {
            JsonSerializerSettings jsonSetting = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None, DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore };

            Formatting formatting = formatIndented ? Formatting.Indented : Formatting.None;
            return JsonConvert.SerializeObject(data, formatting, jsonSetting);
        }

        public static CharacterData DeserializeFromDB(string charData)
        {
            CharacterData data = DeserializeFromDB<CharacterData>(charData);
            data.ValidateDefault();
            return data;
        }

        public static T DeserializeFromDB<T>(string charData)
        {
            JsonSerializerSettings jsonSetting = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None, DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore };
            jsonSetting.Converters.Add(new DBInventoryItemConverter());
            return JsonConvert.DeserializeObject<T>(charData, jsonSetting);
        }

        public string SerializeForClient(bool formatIndented = false)
        {
            return SerializeForClient(this, formatIndented);
        }

        public static string SerializeForClient(object data, bool formatIndented = false)
        {
            JsonSerializerSettings jsonSetting = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None, DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore };
            jsonSetting.Converters.Add(new ClientInventoryItemConverter());
            Formatting formatting = formatIndented ? Formatting.Indented : Formatting.None;
            return JsonConvert.SerializeObject(data, formatting, jsonSetting);
        }

        public static T DeserializeFromClient<T>(string charData)
        {
            JsonSerializerSettings jsonSetting = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None, DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore };
            jsonSetting.Converters.Add(new ClientInventoryItemConverter());
            return JsonConvert.DeserializeObject<T>(charData, jsonSetting);
        }
        #endregion
    }

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class CharacterCreationDataBase
    {
        [DefaultValue("")]
        [JsonProperty(PropertyName = "Name")]
        public string Name { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "job")]
        public byte JobSect { get; set; }

        [JsonProperty(PropertyName = "eqinv")]
        public EquipmentInventoryData EquipmentInventory { get; set; }

        public CharacterCreationDataBase()
        {
            EquipmentInventory = new EquipmentInventoryData();
        }

        /// <summary>
        /// Sets default data when creating new character
        /// </summary>
        public void InitDefault(JobType jobsect)
        {
            EquipmentInventory.InitDefault();
        }

        public string SerializeForCharCreation(bool formatIndented = false)
        {
            JsonSerializerSettings jsonSetting = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None, DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore };
            jsonSetting.Converters.Add(new ClientInventoryItemConverter());
            Formatting formatting = formatIndented ? Formatting.Indented : Formatting.None;
            return JsonConvert.SerializeObject(this, formatting, jsonSetting);
        }
    }

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class CharacterCreationData : CharacterCreationDataBase
    {
        #region serializable properties
        [DefaultValue(0)]
        [JsonProperty(PropertyName = "lvl")]
        public int ProgressLevel { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "equipscore")]
        public int EquipScore { get; set; }

        [DefaultValue(1)]
        [JsonProperty(PropertyName = "talt")]
        public byte TalentType { get; set; }
        #endregion

        public CharacterCreationData() : base()
        {
            TalentType = 1;
        }

        public static CharacterCreationData DeserializeForCharCreation(string charData)
        {
            JsonSerializerSettings jsonSetting = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None, DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore };
            jsonSetting.Converters.Add(new ClientInventoryItemConverter());
            return JsonConvert.DeserializeObject<CharacterCreationData>(charData, jsonSetting);
        }
    }

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class GetCharactersList
    {
        [JsonProperty(PropertyName = "chars")]
        public List<CharacterCreationData> CharList { get; set; }

        public GetCharactersList()
        {
            CharList = new List<CharacterCreationData>();
        }

        public string Serialize(bool formatIndented = false)
        {
            JsonSerializerSettings jsonSetting = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None, DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore };
            jsonSetting.Converters.Add(new ClientInventoryItemConverter());
            Formatting formatting = formatIndented ? Formatting.Indented : Formatting.None;
            return JsonConvert.SerializeObject(this, formatting, jsonSetting);
        }

        public static GetCharactersList Deserialize(string charlist)
        {
            JsonSerializerSettings jsonSetting = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None, DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore };
            jsonSetting.Converters.Add(new ClientInventoryItemConverter());
            return JsonConvert.DeserializeObject<GetCharactersList>(charlist, jsonSetting);
        }
    }

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class InspectCombatStats
    {
        [DefaultValue(0)]
        [JsonProperty(PropertyName = "hp")]
        public int Health { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "armor")]
        public int Armor { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "cri")]
        public int Critical { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "cocri")]
        public int CoCritical { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "cridmg")]
        public int CriticalDamage { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "eva")]
        public int Evasion { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "acc")]
        public int Accuracy { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "tal")]
        public int Talent { get; set; }

        public void Update(LocalCombatStats combatstats)
        {
            Health = combatstats.HealthMax;
            Armor = combatstats.Armor;
            Critical = combatstats.Critical;
            CoCritical = combatstats.CoCritical;
            CriticalDamage = combatstats.CriticalDamage;
            Evasion = combatstats.Evasion;
            Accuracy = combatstats.Accuracy;
        }
    }

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class CharacterInspectData : CharacterCreationDataBase
    {
        #region serializable properties
        [DefaultValue(1)]
        [JsonProperty(PropertyName = "lvl")]
        public int ProgressLevel { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "faction")]
        public byte Faction { get; set; }

        [DefaultValue("")]
        [JsonProperty(PropertyName = "guild")]
        public string Guild { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "vip")]
        public byte VIP { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "score")]
        public int CombatScore { get; set; }

        [JsonProperty(PropertyName = "stats")]
        public InspectCombatStats InspectCombatStats { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "exp")]
        public int Experience { get; set; }
        #endregion

        public CharacterInspectData() : base()
        {
            InspectCombatStats = new InspectCombatStats();
        }

        public static CharacterInspectData Deserialize(string data)
        {
            JsonSerializerSettings jsonSetting = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None, DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore };
            jsonSetting.Converters.Add(new ClientInventoryItemConverter());
            return JsonConvert.DeserializeObject<CharacterInspectData>(data, jsonSetting);
        }
    }
}
