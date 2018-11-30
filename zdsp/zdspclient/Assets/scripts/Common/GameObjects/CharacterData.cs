using System;
using System.ComponentModel;
using System.Collections.Generic;
using Newtonsoft.Json;
using Zealot.Common.Entities;
using Zealot.Repository;

namespace Zealot.Common
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class CharacterData
    {
        #region serializable properties

        [DefaultValue("")]
        [JsonProperty(PropertyName = "Name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "Gender")]
        public byte Gender { get; set; }

        [JsonProperty(PropertyName = "job")]
        public byte JobSect { get; set; }

        [JsonProperty(PropertyName = "faction")]
        public byte Faction { get; set; }

        [JsonProperty(PropertyName = "fnkill")]
        public int FactionKill { get; set; }

        [JsonProperty(PropertyName = "fndie")]
        public int FactionDeath { get; set; }

        [JsonProperty(PropertyName = "lvl")]
        public int ProgressLevel { get; set; }

        [JsonProperty(PropertyName = "equipscore")]
        public int EquipScore { get; set; }

        [JsonProperty(PropertyName = "health")]
        public int Health { get; set; }

        [JsonProperty(PropertyName = "mana")]
        public int Mana { get; set; }

        [JsonProperty(PropertyName = "exp")]
        public int Experience { get; set; }

        [DefaultValue("")]
        [JsonProperty(PropertyName = "removechardt")]
        public string RemoveCharDT { get; set; }

        [JsonProperty(PropertyName = "tutorialdone")]
        public bool IsTutorialRealmDone { get; set; }

        [DefaultValue("")]
        [JsonProperty(PropertyName = "roomguid")]
        public string RoomGuid { get; set; }

        [JsonProperty(PropertyName = "lastlevelid")]
        public int LastLevelId { get; set; }

        [JsonProperty(PropertyName = "lastlevelpos")]
        public float[] LastLevelPos { get; set; }

        [JsonProperty(PropertyName = "lastworldid")]
        public int LastWorldId { get; set; }

        [JsonProperty(PropertyName = "lastworldpos")]
        public float[] LastWorldPos { get; set; }

        [JsonProperty(PropertyName = "lastdir")]
        public float[] LastDirection { get; set; }

        [JsonProperty(PropertyName = "revive")]
        public int FreeReviveOnSpot { get; set; }

        [JsonProperty(PropertyName = "newday")]
        public DateTime NewDayDts { get; set; }

        [DefaultValue("")]
        [JsonProperty(PropertyName = "ihotbar")]
        public string ItemHotBarData { get; set; }

        [JsonProperty(PropertyName = "guildid")]
        public int GuildId { get; set; }

        [JsonProperty(PropertyName = "guildrank")]
        public byte GuildRank { get; set; }

        [DefaultValue("")]
        [JsonProperty(PropertyName = "gshopbuycnt")]
        public string GuildShopBuyCount { get; set; }

        [JsonProperty(PropertyName = "gsmbossentry")]
        public int GuildSMBossEntry { get; set; }

        [JsonProperty(PropertyName = "gsmbossextra")]
        public int GuildSMBossExtraEntry { get; set; }

        [JsonProperty(PropertyName = "gbossrealm")]
        public int GuildBossRewardRealm { get; set; }

        [JsonProperty(PropertyName = "newguildweek")]
        public DateTime NewGuildWeekDt { get; set; }

        [JsonProperty(PropertyName = "gleavecdtick")]
        public long LeaveGuildCDEndTick { get; set; }

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

        [JsonProperty(PropertyName = "PortraitID")]
        public int portraitID { get; set; }

        [JsonProperty(PropertyName = "currencyExchangeTime")]
        public byte CurrencyExchangeTime { get; set; }

        [JsonProperty(PropertyName = "MountID")]
        public int MountID { get; set; }

        [JsonProperty(PropertyName = "firstbuyflag")]
        public int FirstBuyFlag { get; set; }

        [JsonProperty(PropertyName = "firstbuycollected")]
        public int FirstBuyCollected { get; set; }

        [JsonProperty(PropertyName = "TutorialStatus")]
        public int TutorialStatus { get; set; }

        [JsonProperty(PropertyName = "randombox")]
        public long RandomBoxTimeTick { get; set; }

        [JsonProperty(PropertyName = "costbuffid")]
        public int costbuffid { get; set; }

        [JsonProperty(PropertyName = "costbuffgold")]
        public int costbuffgold { get; set; }

        [JsonProperty(PropertyName = "tutorialreddot")]
        public int tutorialreddot { get; set; }

        [JsonProperty(PropertyName = "battletime")]
        public int BattleTime { get; set; } //max 900 minutes * 60seconds = 54000 seconds.
        #endregion

        [JsonProperty(PropertyName = "currency")]
        public CurrencyInventoryData CurrencyInventory { get; set; }

        [JsonProperty(PropertyName = "bag")]
        public ItemInventoryData ItemInventory { get; set; }

        [JsonProperty(PropertyName = "eqinv")]
        public EquipmentInventoryData EquipmentInventory { get; set; }

        [JsonProperty(PropertyName = "realminv")]
        public RealmInventoryData RealmInventory { get; set; }

        //[JsonProperty(PropertyName = "chatinv")]
        //public ChatInventoryData ChatInventory { get; set; }

        [JsonProperty(PropertyName = "clueinv")]
        public DestinyClueInventory ClueInventory { get; set; }

        [JsonProperty(PropertyName = "questinv")]
        public QuestInventoryData QuestInventory { get; set; }

        [JsonProperty(PropertyName = "donateinv")]
        public DonateInventoryData DonateInventory { get; set; }

        [JsonProperty(PropertyName = "sideeffectinv")]
        public SideEffectInventoryData SideEffectInventory { get; set; }

        [JsonProperty(PropertyName = "skillInv")]
        public SkillInventoryData SkillInventory { get; set; }

        [JsonProperty(PropertyName = "itemsortinv")]
        public ItemSortInventoryData ItemSortInventory { get; set; }

        [JsonProperty(PropertyName = "arenainv")]
        public ArenaInventoryData ArenaInventory { get; set; }

        [JsonProperty(PropertyName = "mailinv")]
        public MailInventoryData MailInventory { get; set; }

        [JsonProperty(PropertyName = "mallinv")]
        public ItemMallInventoryData ItemMallInventory { get; set; }

        [JsonProperty(PropertyName = "welfareinv")]
        public WelfareInventoryData WelfareInventory { get; set; }

        [JsonProperty(PropertyName = "sevendaysinv")]
        public SevenDaysInvData SevenDaysInventory { get; set; }

        [JsonProperty(PropertyName = "questextrarewardsinv")]
        public QuestExtraRewardsInvData QuestExtraRewardsInventory { get; set; }

        [JsonProperty(PropertyName = "dnainv")]
        public DNAInvData DNAInventory { get; set; }

        [JsonProperty(PropertyName = "powerupinv")]
        public PowerUpInventoryData PowerUpInventory { get; set; }

        [JsonProperty(PropertyName = "equipmentcraftinv")]
        public EquipmentCraftInventoryData EquipmentCraftInventory { get; set; }

        [JsonProperty(PropertyName = "equipfusioninv")]
        public EquipFusionInventoryData EquipFusionInventory { get; set; }

        [JsonProperty(PropertyName = "offlineexpinv2")]
        public OfflineExpInventory2 OfflineExpInv2 { get; set; }

        [JsonProperty(PropertyName = "lotteryinv")]
        public LotteryInventoryData LotteryInventory { get; set; }

        [JsonProperty(PropertyName = "heroinvdata")]
        public HeroInvData HeroInventory { get; set; }

        [JsonProperty(PropertyName = "storeinv")]
        public StoreInventoryData StoreInventory { get; set; }

        [JsonProperty(PropertyName = "guildquests")]
        public GuildQuestInventory GuildQuests { get; set; }

        [JsonProperty(PropertyName = "prizeguarantee")]
        public PrizeGuaranteeData PrizeGuaranteeData { get; set; }

        [JsonProperty(PropertyName = "combatstats")]
        public InspectCombatStats InspectCombatStats;

        [JsonProperty(PropertyName = "exchangeshop")]
        public ExchangeShopInventory ExchangeShopInv;

        [JsonProperty(PropertyName = "portraitdata")]
        public PortraitData PortraitData;

        [JsonProperty(PropertyName = "characterinfodata")]
        public CharacterInfoData CharInfoData;

        [JsonProperty(PropertyName = "itemlimitdata")]
        public ItemUseDropLimitData ItemLimitData { get; set; }

        public CharacterData()
        {
            CurrencyInventory = new CurrencyInventoryData();
            ItemInventory = new ItemInventoryData();
            EquipmentInventory = new EquipmentInventoryData();
            EquipmentCraftInventory = new EquipmentCraftInventoryData();
            EquipFusionInventory = new EquipFusionInventoryData();
            RealmInventory = new RealmInventoryData();
            //ChatInventory = new ChatInventoryData();
            QuestInventory = new QuestInventoryData();
            ClueInventory = new DestinyClueInventory();
            DonateInventory = new DonateInventoryData();
            SideEffectInventory = new SideEffectInventoryData();
            SkillInventory = new SkillInventoryData();
            ItemSortInventory = new ItemSortInventoryData();
            ArenaInventory = new ArenaInventoryData();
            MailInventory = new MailInventoryData();
            ItemMallInventory = new ItemMallInventoryData();
            WelfareInventory = new WelfareInventoryData();
            SevenDaysInventory = new SevenDaysInvData();
            QuestExtraRewardsInventory = new QuestExtraRewardsInvData();
            DNAInventory = new DNAInvData();
            OfflineExpInv2 = new OfflineExpInventory2();
            LotteryInventory = new LotteryInventoryData();
            StoreInventory = new StoreInventoryData();
            GuildQuests = new GuildQuestInventory();
            PrizeGuaranteeData = new PrizeGuaranteeData();
            InspectCombatStats = new InspectCombatStats();
            ExchangeShopInv = new ExchangeShopInventory();
            PortraitData = new PortraitData();
            CharInfoData = new CharacterInfoData();
            HeroInventory = new HeroInvData();
            PowerUpInventory = new PowerUpInventoryData();
            ItemLimitData = new ItemUseDropLimitData();
        }

        /// <summary>
        /// Sets default data when creating new character
        /// </summary>
        public void InitDefault(JobType jobsect)
        {
            ItemInventory.InitDefault();
            EquipmentInventory.InitDefault();
            //ItemSortInventory.InitDefault();
            RestoreDefaultSkill();
            //WelfareInventory.InitDefault();
            //SevenDaysInventory.InitDefault();
            //QuestExtraRewardsInventory.InitDefault();
            DNAInventory.InitDefault();
            //LotteryInventory.InitDefault();
            //PrizeGuaranteeData.InitDefault();
            //GuildQuests.InitDefault();
            //ExchangeShopInv.InitDefault();
            //PortraitData.InitDefault((JobType)jobsect);
            //StoreData.InitDefault();
            PowerUpInventory.InitDefault();
            EquipmentCraftInventory.InitDefault();
            EquipFusionInventory.InitDefault();
            CharInfoData.InitDefault();
        }

        public void ValidateDefault()
        {
            ItemInventory.ValidateDefault();
            EquipmentInventory.ValidateDefault();
        }

        public void RestoreDefaultSkill()
        {
            SkillInventory.InitDefault(JobSectRepo.GetJobByType((JobType)JobSect));
        }

        private void BattleTimeResetOnNewDay()
        {
            if (BattleTime <= 0) //300 minutes
                BattleTime = 18000;
            else
            {
                BattleTime += 18000;
                if (BattleTime > 54000)
                    BattleTime = 54000;
            }
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
            BattleTimeResetOnNewDay();
            ItemLimitData.ResetNewDay();
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
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "job")]
        public byte JobSect { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "gender")]
        public byte Gender { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "lastlevelid")]
        public int LastLevelId { get; set; }

        [DefaultValue("")]
        [JsonProperty(PropertyName = "removechardt")]
        public string RemoveCharDT { get; set; }

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
        #endregion

        public CharacterCreationData() : base()
        {           
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
