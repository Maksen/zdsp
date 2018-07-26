/*
***********************************************************************
** AUTO GENERATED FILE, DO NOT TOUCH, IF YOU DO YOU'LL BURN IN HELL
************************************************************************
*/

using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Zealot.Common
{
    public enum GuildTechType {Level = 0, Shop = 1, Quest = 2, Cave = 3, Love = 4, HeroHouse = 5, Health = 6, Attack = 7, Armor = 8, Accuracy = 9, Evasion = 10, Critical = 11, CoCritical = 12, CriticalDamage = 13, CoCriticalDamage = 14};
    public enum GuildRankType {Leader = 0, Officer = 1, Member = 2};
    public enum GuildQuestType {White = 0, Green = 1, Blue = 2, Purple = 3, Golden = 4};
    public enum HeroRarity {Rare = 0, Epic = 1, Celestial = 2, Legendary = 3};
    public enum HeroInterestType {Random = 0, MartialArts = 1, Metallurgy = 2, Debate = 3, StrategicAttack = 4, Alcohol = 5, Heroism = 6, Lust = 7, Musical = 8, Greed = 9, Business = 10, Eloquence = 11, Negotiation = 12, TotalNum = 13};
    public enum HeroBondType {None = 0, HeroLevel = 1, HeroSkill = 2};
    public enum TerrainType {Tavern = 0, TeaHouse = 1, Town = 2, Forest = 3, Inn = 4, Brothel = 5, Market = 6, Academy = 7, Villa = 8};
    public enum ChestRequirementType {None = 0, HeroID = 1, HeroInterest = 2, HeroTrust = 3};
    public enum ItemOriginType {UI = 0, Monster = 1, Shop = 2};
    public enum RealmType {RealmWorld = 0, DungeonStory = 1, DungeonDailySpecial = 2};
    public enum MapType {City = 0, Wilderness = 1, Dungeon = 2, Activity = 3, Others = 4};
    public enum DungeonDifficulty {Easy = 0, Hard = 1, Hell = 2};
    public enum LevelPVPType {Peace = 0, FreeForAll = 2, Guild = 4, Faction = 5};
    public enum DungeonType {Daily = 0, Special = 1};
    public enum RealmObjectiveType {TimeWithinNoDeath = 0, TimeWithinComplete = 1, HeroUseOnly = 2, HeroCannotUse = 3, HeroTypeCannotUse = 4, SkillCannotUseAny = 5, HeroQualityUseOnly = 6, NPCKills = 7, RealmComplete = 8};
    public enum ForbiddenUsesType {Item = 0, SideEffect = 1, Skill = 2};
    public enum LootCorrectionType {Minus10 = 1, Minus1120 = 2, Minus2130 = 3, Minus3140 = 4, Minus4150 = 5, Minus51 = 6};
    public enum LootType {Normal = 0, Share = 1, Boss = 2, BigBoss = 3, LastHit = 4, Explore = 5};
    public enum LootSourceType {Monster = 0, Chest = 1, Both = 2};
    public enum LimitItemCycle {Day = 0, Week = 1, Month = 2, Forever = 3};
    public enum BossCategory {BIGBOSS = 1, BOSS = 2};
    public enum BossSpawnType {SpawnDaily = 1, SpawnWeekly = 2, SpawnDuration = 3, Event = 4};
    public enum NPCType {Combat = 0, Static = 1};
    public enum MonsterClass {Normal = 0, Mini = 1, Boss = 2};
    public enum LocationType {Nearby = 0, Dungeon = 1, EndlessTower = 2, Rift = 3, History = 4, PVP = 5};
    public enum QuestExtraType {StoryDungeon = 0, DailyDungeon = 1, HeroLvlUpgrade = 2, HeroSkillUpgrade = 3, HeroesHouseBefriend = 4, HeroHouseFight = 5, EquipmentUpgrade = 6, TalentUpgrade = 7, GuildYoumeng = 8, GuildSMBoss = 9, GuildQuest = 10, GuildWishingPoolBroadcast = 11, GuildWishingPoolDonate = 12, ArenaTimes = 13, PvpTimes = 14, AlchemyTimes = 15, PetLevel = 16, LotteryTimes = 17, WorldBossTimes = 18, GoldSpent = 19, RandomBoxTimes = 20, TowerTimes = 21, TreasureUpgrade = 22, PetHouseGet = 23, OfflineExpGet = 24, TimeCityFight = 25, PetEngraving = 26, NUM_TYPES = 27};
    public enum RewardType {All = 0, Random = 1, Weight = 2};
    public enum Days {One = 0, Two = 1, Three = 2, Four = 3, Five = 4, Six = 5, Seven = 6, NUM_DAYS = 7};
    public enum NewServerActivityType {Level = 0, Points = 1, Normalchapter = 2, Elitechapter = 3, Hellchapter = 4, ChapterStars = 5, Herototal = 7, Herolevel_n = 8, Equipupgrade_n = 9, Heroskilltotal_n = 10, Playerfighting = 11, Herofighting_n = 12, Playertalent = 13, Militantrank = 14, Herosquality_n = 15};
    public enum AISkillCondition {SelfHpUp = 0, SelfHpDown = 1, SelfHpInterval = 2, TargetHpUp = 3, TargetHpDown = 4, Engage = 5, InrangePlayer = 6, TargetNegSkill = 7, TargetNegSE = 8, None = 9};
    public enum NPCLootRule {LootByLasthit = 0, LootByDamage = 1};
    public enum LootDropType {SelectOne = 0, SelectAll = 1};
    public enum Gender {Male = 0, Female = 1};
    public enum WeaponType {Any = 0, Sword = 1, Blade = 2, Lance = 3, Hammer = 4, Fan = 5, Xbow = 6, Dagger = 7, Sanxian = 8};
    public enum JobType {Newbie = 0, Warrior = 1, Soldier = 2, Tactician = 3, Killer = 4, Samurai = 5, Swordsman = 6, Lieutenant = 7, SpecialForce = 8, Alchemist = 9, QigongMaster = 10, Assassin = 11, Shadow = 12, BladeMaster = 13, SwordMaster = 14, General = 15, Commando = 16, Strategist = 17, Schemer = 18, Executioner = 19, Slaughter = 20};
    public enum CharPortraitType {None = 0, Knife = 1, Sword = 2, Spear = 3, Hammer = 4, Hero = 5};
    public enum FactionType {None = 0, Dragon = 1, Tiger = 2, Pig = 3, Leopard = 4};
    public enum CurrencyType {None = 0, Money = 1, GuildContribution = 2, GuildGold = 3, Gold = 4, LockGold = 5, LotteryTicket = 6, HonorValue = 7, BattleCoin = 8, Exp = 9, VIP = 10};
    public enum EquipmentType {Weapon = 0, Armor = 8, Accessory = 12};
    public enum PartsType {Sword = 0, Blade = 1, Lance = 2, Hammer = 3, Fan = 4, Xbow = 5, Dagger = 6, Sanxian = 7, Helm = 101, Body = 102, Wing = 103, Boots = 104, Bathrobe = 105, Ring = 201, Jewelry = 202, Accessory = 203};
    public enum MainWeaponAttribute {Str = 0, Dex = 1, Int = 2};
    public enum FashionSuitType {Head = 0, Weapon = 1, Body = 2, Back = 3};
    public enum EquipmentAttributeType {Health = 0, Attack = 1, Armor = 2, CriticalDamage = 3, CocriticalDamage = 4, Evasion = 5, Accuracy = 6};
    public enum ActorStatsType {None = 0, Attack = 1, Armor = 2, Critical = 3, Cocritical = 4, CriticalDamage = 5, CocriticalDamage = 6, Evasion = 7, Accuracy = 8};
    public enum AbilityType {Attack = 0, Armor = 1, Accuracy = 2, Evasion = 3, AmplifyDamage = 4, AbsorbDamage = 5, MaxHealth = 6, CriticalPerfect = 7, LevelAttack = 8, LevelArmor = 9, Health = 10, HealthPct = 11, AttackPct = 12, ArmorPct = 13, AccuracyPct = 14, EvasionPct = 15, SkillDamage = 16, SkillDamageReduce = 17, Exdamage = 18, Critical = 19, CriticalDouble = 20};
    public enum AttackType {Physical = 0, Magic = 1};
    public enum RespawnType {Spot_City_CountdownSafeZoon = 0, Spot_City_CountdownCity = 1, City_CountdownCity = 2, SafeZoneCost_CountdownSafeZoon = 3, SafeZoon_CountdownSafeZoon = 4, CountdownSafeZoon = 5, SpotCost_CountdownSafeZoon = 6, None = 7};
    public enum CraftingType {Strengthen = 0, Gem = 1, Other = 2};
    public enum SkillType {BasicAttack = 0, Active = 1, Passive = 2};
    public enum SkillClass {Normal = 0, Special = 1};
    public enum SkillBehaviour {Self = 0, Target = 1, Ground = 2};
    public enum TargetType {Friendly = 0, Enemy = 1, Party = 2};
    public enum HitType {Basic = 0, Definite = 1};
    public enum Threatzone {Single = 0, DegreeArc120 = 1, DegreeArc360 = 2, LongStream = 3};
    public enum CriticalType {Normal = 0, None = 1, Critical = 2};
    public enum EffectType {Damage_NoElementDamage = 0, Damage_MetalDamage = 1, Damage_WoodDamage = 2, Damage_EarthDamage = 3, Damage_WaterDamage = 4, Damage_FireDamage = 5, Damage_DamageBasedOnWeaponElement = 6, Damage_PureDamage = 7, Stats_Strength = 50, Stats_Agility = 51, Stats_Dexterity = 52, Stats_Constitution = 53, Stats_Intelligence = 54, Stats_AttackSpeed = 55, Stats_AttackSpeed_Debuff = 56, Stats_CastSpeed = 57, Stats_CastSpeed_Debuff = 58, Stats_MoveSpeed = 59, Stats_MoveSpeed_Debuff = 60, Stats_ExpBonus = 61, Stats_MaxHealth = 62, Stats_HealthRegen = 63, Stats_MaxMana = 64, Stats_ManaRegen = 65, Stats_EnergyShield = 66, Stats_IgnoreArmor = 67, Stats_ChangeEleToNone = 68, Stats_ChangeEleToMetal = 69, Stats_ChangeEleToWood = 70, Stats_ChangeEleToEarth = 71, Stats_ChangeEleToWater = 72, Stats_ChangeEleToFire = 73, Stats_HeavyStand = 74, Stats_SkillCostReduce = 75, Stats_SkillAffectEnhance = 76, Stats_HealingPoint = 77, Stats_HealingPoint_Debuff = 78, Stats_Effect = 79, Stats_Effect_Debuff = 80, Stats_HealingIncome = 81, Stats_HealingIncome_Debuff = 82, Rejuvenate_HealthPotion = 100, Rejuvenate_ManaPotion = 101, Rejuvenate_Healing = 102, StatsAttack_WeaponAttack = 110, StatsAttack_WeaponAttack_Debuff = 111, StatsAttack_AttackPower = 112, StatsAttack_AttackPower_Debuff = 113, StatsAttack_Accuracy = 114, StatsAttack_Accuracy_Debuff = 115, StatsAttack_Critical = 116, StatsAttack_Critical_Debuff = 117, StatsAttack_CriticalDamage = 118, StatsAttack_CriticalDamage_Debuff = 119, StatsAttack_IncSmashDamage = 120, StatsAttack_IncSmashDamage_Debuff = 121, StatsAttack_IncSliceDamage = 122, StatsAttack_IncSliceDamage_Debuff = 123, StatsAttack_IncPierceDamage = 124, StatsAttack_IncPierceDamage_Debuff = 125, StatsAttack_IncEleNoneDamage = 126, StatsAttack_IncEleNoneDamageDebuff = 127, StatsAttack_IncEleMetalDamage = 128, StatsAttack_IncEleMetalDamage_Debuff = 129, StatsAttack_IncEleWoodDamage = 130, StatsAttack_IncEleWoodDamage_Debuff = 131, StatsAttack_IncEleEarthDamage = 132, StatsAttack_IncEleEarthDamage_Debuff = 133, StatsAttack_IncEleWaterDamage = 134, StatsAttack_IncEleWaterDamage_Debuff = 135, StatsAttack_IncEleFireDamage = 136, StatsAttack_IncEleFireDamage_Debuff = 137, StatsAttack_VSHumanDamage = 138, StatsAttack_VSHumanDamage_Debuff = 139, StatsAttack_VSZombieDamage = 140, StatsAttack_VSZombieDamage_Debuff = 141, StatsAttack_VSVampireDamage = 142, StatsAttack_VSVampireDamage_Debuff = 143, StatsAttack_VSAnimalDamage = 144, StatsAttack_VSAnimalDamage_Debuff = 145, StatsAttack_VSPlantDamage = 146, StatsAttack_VSPlantDamage_Debuff = 147, StatsAttack_VSEleNoneDamage = 148, StatsAttack_VSEleNoneDamage_Deduff = 149, StatsAttack_VSEleMetalDamage = 150, StatsAttack_VSEleMetalDamage_Debuff = 151, StatsAttack_VSEleWoodDamage = 152, StatsAttack_VsEleWoodDamage_Debuff = 153, StatsAttack_VSEleEarthDamage = 154, StatsAttack_VSEleEarthDamage_Debuff = 155, StatsAttack_VSEleWaterDamage = 156, StatsAttack_VSEleWaterDamage_Debuff = 157, StatsAttack_VSEleFireDamage = 158, StatsAttack_VSEleFireDamage_Debuff = 159, StatsAttack_VSBossDamage = 160, StatsAttack_IncFinalDamage = 161, StatsDefence_Armor = 200, StatsDefence_Armor_Debuff = 201, StatsDefence_Block = 202, StatsDefence_Block_Debuff = 203, StatsDefence_BlockValue = 204, StatsDefence_BlockValue_Debuff = 205, StatsDefence_Evasion = 206, StatsDefence_Evasion_Debuff = 207, StatsDefence_CoCritical = 208, StatsDefence_CoCritical_Debuff = 209, StatsDefence_IncSmashDefence = 210, StatsDefence_IncSmashDefence_Debuff = 211, StatsDefence_IncSliceDefence = 212, StatsDefence_IncSliceDefence_Debuff = 213, StatsDefence_IncPierceDefence = 214, StatsDefence_IncPierceDefence_Debuff = 215, StatsDefence_IncEleNoneDefence = 216, StatsDefence_IncEleNoneDefence_Debuff = 217, StatsDefence_IncEleMetalDefence = 218, StatsDefence_IncEleMetalDefence_Debuff = 219, StatsDefence_IncEleWoodDefence = 220, StatsDefence_IncEleWoodDefence_Debuff = 221, StatsDefence_IncEleEarthDefence = 222, StatsDefence_IncEleEarthDefence_Debuff = 223, StatsDefence_IncEleWaterDefence = 224, StatsDefence_IncEleWaterDefence_Debuff = 225, StatsDefence_IncEleFireDefence = 226, StatsDefence_IncEleFireDefence_Debuff = 227, StatsDefence_VSHumanDefence = 228, StatsDefence_VSHumanDefence_Debuff = 229, StatsDefence_VSZombieDefence = 230, StatsDefence_VSZombieDefence_Debuff = 231, StatsDefence_VSVampireDefence = 232, StatsDefence_VsVampireDefence_Debuff = 233, StatsDefence_VSAnimalDefence = 234, StatsDefence_VSAnimalDefence_Debuff = 235, StatsDefence_VSPlantDefence = 236, StatsDefence_VSPlantDefence_Debuff = 237, StatsDefence_DecreaseFinalDamage = 238, StatsDefence_AmplifyDamage = 239, StatsDefence_AmplifyDamage_Debuff = 240, Control_Stun = 300, Control_Root = 301, Control_Fear = 302, Control_Silence = 303, Control_Taunt = 304, Immune_AllDamage = 350, Immune_AllDebuff = 351, Immune_AllImmune = 352, Immune_Stun = 353, Immune_Root = 354, Immune_Fear = 355, Immune_Silence = 356, Immune_Taunt = 357, Remove_AllControl = 400, Remove_Stun = 401, Remove_Root = 402, Remove_Fear = 403, Remove_Silence = 404, Remove_RandomBuff = 405, Remove_RandomDebuff = 406, Stealth_Stealth = 420, Stealth_DetectStealth = 421, Trigger_OnNormalAttack = 450, Enhance_IncRepeatSE = 460, Enhance_IncSkillAffect = 461};
    public enum InteractiveType {OpenTalent = 0, OpenGem = 1};
    public enum ItemRarity {Common = 0, Uncommon = 1, Rare = 2, Epic = 3, Celestial = 4, Legendary = 5};
    public enum ItemType {Invalid = 0, PotionFood = 1, Material = 2, LuckyPick = 3, Henshin = 4, Features = 5, Equipment = 6, DNA = 7, Relic = 8, QuestItem = 9, MercenaryItem = 10, InstanceItem = 11, PetItem = 12};
    public enum BagType {Any = 0, Equipment = 1, Consumable = 2, Material = 3, DNA = 4, Gem = 5};
    public enum MaterialType {Exchange = 0, UpgradeItem = 1, Special = 2, Token = 3};
    public enum ToolTipType {Common = 0, Event = 1, Consumables = 2};
    public enum GCDType {Non_GDC = 0, HP_Recover = 1, MP_Recover = 2, Hybrid_Recover = 3};
    public enum DNAType {Locked = 0, Head = 1, Shoulders = 2, Chest = 3, Arms = 4, Body = 5, Legs = 6, Feet = 7};
    public enum RelicType {None = 0, Head = 1, Shoulders = 2, Body = 3, Arms = 4, Shoes = 5, Accessory = 6, Weapon = 7, SecondaryArm = 8};
    public enum HeroItemType {Shard = 0, Gift = 1, Onigiri = 2};
    public enum ESERequireType {Require_Upgrade = 0, Other = 1};
    public enum GItemSubType {None = 0, EquipUpgrade = 1, HeroItem = 2};
    public enum DialogType {Common = 0, MainQuest = 1, NationQuest = 2, Warehouse = 3};
    public enum DirtyWordType {Chat = 0, GameName = 1, BothChatName = 2};
    public enum DailyEventCategoryType {None = 0, BindGold = 1, BindGem = 2, Crystal = 3, SP = 4, Achievement = 5, Honor = 6, Prestige = 7, Exp = 8, Equipment = 9, GemStone = 10};
    public enum DailyEventType {None = 0, JinQianGuan = 1, ResourceMap = 3, TianMoTa = 4, PartyRealm = 5, Arena = 6, YiZuZhan = 7, GuZhanChang = 8, FeiLongGuBao = 9, WorldBoss = 10, QianKunShengDian = 11, MainLineRealm = 12, DailyQuest = 13, ActivityQuest = 14, NationQuest = 15};
    public enum PowerUpType {Title = 0, Rank = 1, Equipment = 2, Armor = 4, skills = 5, Secrets = 6};
    public enum LotteryShopType {Weapon = 0, Armor = 1, Item = 2};
    public enum StoreType {None = 0, Android = 1, IOS = 2};
    public enum LinkUIType {None = 0, Equipment_Upgrade = 1, Equipment_Reform = 2, Equipment_Socket = 3, DNA = 4, Shop = 5, Skill = 6, Realm = 7, GoTopUp = 8, Achievement = 9};
    public enum ItemMallCategory {None = 0, Strengthen = 1, Treasure = 2, Prestige  = 3, Hot = 4, Potion = 5, Fashion = 6, Vehicle = 7, Pet = 8, WeaponFashion = 9};
    public enum ItemMallShoppingType {None = 0, Tortoise = 1, Phoenix = 2, Tiger = 3, Dragon = 4, Unbind = 5, Bind = 6};
    public enum PushNotificationType {None = 0, Monday = 1, Tuesday = 2, Wednesday = 3, Thursday = 4, Friday = 5, Saturday = 6, Sunday = 7, Everyday = 8};
    public enum PrizeGuaranteeType {Chest = 0, Activity = 1};
    public enum NotificationType {None = 0, WorldBoss = 1};
    public enum UIStoreLinkType {NotApplicable = 0, MoneyStore = 1, GuildStore = 2, Lottery = 3, WuLing = 4, WuMen = 5};
    public enum NPCStore {Normal = 0, Random = 1, Barter = 2};
    public enum PositionType {Background = 0, Patten = 1, Frame = 2};
    public enum ReturnType {ReturnCity = 0, ReturnSafeZone = 1};
    public enum QuestType {Main = 0, Destiny = 1, Sub = 2, Guild = 3, Signboard = 4, Event = 5};
    public enum QuestRepeatType {NoRpeat = 0, AlwaysRepeat = 1, OncePerDay = 2, OncePerWeek = 3};
    public enum QuestTriggerType {NPC = 0, Item = 1, Level = 2, Interact = 3, Signboard = 4, Hero = 5, SensorChip = 6};
    public enum QuestDestinyType {Qing = 0, Wei = 1, Zhao = 2, Yan = 3, Qi = 4, Chu = 5, Han = 6};
    public enum QuestObjectiveType {Kill = 0, Talk = 1, RealmComplete = 2, PercentageKill = 3, Choice = 4, Interact = 5, MultipleObj = 6, Empty = 7, QuickTalk = 8};
    public enum QuestEventType {Cutscene = 0, Playmaker = 1, Monster = 2, Teleport = 3, SideEffect = 4, Companion = 5, Outfit = 6, NPC = 7};
    public enum EventTimingType {StartQuest = 0, CompleteObjective = 1, CompleteQuest = 2};
    public enum QuestInteractiveType {StartQuest = 0, EndQuest = 1};
    public enum QuestRequirementType {Level = 0, Item = 1, Equipment = 2, Hero = 3, Title = 4, SideEffect = 5, Companian = 6, NPC = 7, Outfit = 8, Job = 9};
    public enum QuestSelectionActionType {Non = 0, Job = 1, ObjectiveGroup = 2, SubmitObjective = 3};
    public enum StaticNPCInteractType {Talk = 0, Area = 1, Target = 2};
    public enum AttackStyle {Slice = 0, Pierce = 1, Smash = 2, God = 3, Normal = 4};
    public enum Race {Human = 0, Zombie = 1, Vampire = 2, Animal = 3, Plant = 4};
    public enum Element {None = 0, Metal = 1, Wood = 2, Earth = 3, Water = 4, Fire = 5};

}

namespace Kopio.JsonContracts
{
    using Zealot.Common;
    using System.Reflection;

    [AttributeUsage(AttributeTargets.Property)]
    public class AssetDataAttribute : Attribute
    {
        public AssetDataAttribute(String AssetType_in)
        {
            this.assetType = AssetType_in;
        }
        protected String assetType;
        public String AssetType
        {
            get
            {
                return this.assetType;
            }
        }
    }
    [JsonObject(MemberSerialization.OptIn)]
    public class LinkUIJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public int uiid { get; set; } 
        
        [JsonProperty("2")]
        public string localizedname { get; set; } 
        
        [JsonProperty("3")]
        public LinkUIType uitype { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            uiid = (int)vals["uiid"];
            localizedname = (string)vals["localizedname"];
            uitype = (LinkUIType)vals["uitype"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class ItemOriginJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public string name { get; set; } 
        
        [JsonProperty("2")]
        public ItemOriginType origintype { get; set; } 
        
        [JsonProperty("3")]
        public string param { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            name = (string)vals["name"];
            origintype = (ItemOriginType)vals["origintype"];
            param = (string)vals["param"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class ItemSortJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public int itemsortid { get; set; } 
        
        [JsonProperty("2")]
        public string name { get; set; } 
        
        [JsonProperty("3")]
        public string localizedname { get; set; } 
        
        [JsonProperty("4")]
        public int sortorder { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            itemsortid = (int)vals["itemsortid"];
            name = (string)vals["name"];
            localizedname = (string)vals["localizedname"];
            sortorder = (int)vals["sortorder"];
        }
    }
    
    public class ItemBaseJson
    {
        public virtual int id { get; set; } 
        
        public virtual ItemType itemtype { get { return ItemType.Invalid; } } 
        
        public virtual int itemid { get; set; } 
        
        public virtual string name { get; set; } 
        
        public virtual string localizedname { get; set; } 
        
        public virtual string description { get; set; } 
        
        public virtual BagType bagtype { get; set; } 
        
        public virtual ItemRarity rarity { get; set; } 
        
        public virtual int requirelvl { get; set; } 
        
        public virtual int casttime { get; set; } 
        
        public virtual int sellprice { get; set; } 
        
        public virtual string auction { get; set; } 
        
        //type ItemSortJson
        public virtual int itemsort { get; set; } 
        
        public virtual bool deposit { get; set; } 
        
        public virtual string origin { get; set; } 
        
        public virtual int dailyuselimit { get; set; } 
        
        public virtual int weeklyuselimit { get; set; } 
        
        public virtual int dailygetlimit { get; set; } 
        
        public virtual int weeklygetlimit { get; set; } 
        
        public virtual string iconspritepath { get; set; } 
        
        public virtual bool log { get; set; } 
        
        public virtual bool tickermessage { get; set; } 
        
        public virtual bool generateuid { get; set; } 
        
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class PotionFoodJson : ItemBaseJson
    {
        [JsonProperty("0")]
        public override int id { get; set; } 
        
        [JsonProperty("1")]
        public override int itemid { get; set; } 
        
        [JsonProperty("2")]
        public override string name { get; set; } 
        
        [JsonProperty("3")]
        public override string localizedname { get; set; } 
        
        [JsonProperty("4")]
        public override string description { get; set; } 
        
        [JsonProperty("5")]
        public override BagType bagtype { get; set; } 
        
        [JsonProperty("6")]
        public override ItemRarity rarity { get; set; } 
        
        [JsonProperty("7")]
        public override int requirelvl { get; set; } 
        
        [JsonProperty("8")]
        public override int casttime { get; set; } 
        
        [JsonProperty("9")]
        public override int sellprice { get; set; } 
        
        [JsonProperty("10")]
        public override string auction { get; set; } 
        
        //type ItemSortJson
        [JsonProperty("11")]
        public override int itemsort { get; set; } 
        
        [JsonProperty("12")]
        public override bool deposit { get; set; } 
        
        [JsonProperty("13")]
        public override string origin { get; set; } 
        
        [JsonProperty("14")]
        public override int dailyuselimit { get; set; } 
        
        [JsonProperty("15")]
        public override int weeklyuselimit { get; set; } 
        
        [JsonProperty("16")]
        public override int dailygetlimit { get; set; } 
        
        [JsonProperty("17")]
        public override int weeklygetlimit { get; set; } 
        
        [AssetData("sprite")]
        [JsonProperty("18")]
        public override string iconspritepath { get; set; } 
        
        [JsonProperty("19")]
        public override bool log { get; set; } 
        
        [JsonProperty("20")]
        public override bool tickermessage { get; set; } 
        
        [JsonProperty("21")]
        public override bool generateuid { get; set; } 
        
        public override ItemType itemtype { get { return ItemType.PotionFood; } } 
        
        [JsonProperty("22")]
        public float cdtime { get; set; } 
        
        [JsonProperty("23")]
        public float gcdtime { get; set; } 
        
        [JsonProperty("24")]
        public GCDType gcdtype { get; set; } 
        
        [JsonProperty("25")]
        public string segrpid { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            itemid = (int)vals["itemid"];
            name = (string)vals["name"];
            localizedname = (string)vals["localizedname"];
            description = (string)vals["description"];
            bagtype = (BagType)vals["bagtype"];
            rarity = (ItemRarity)vals["rarity"];
            requirelvl = (int)vals["requirelvl"];
            casttime = (int)vals["casttime"];
            sellprice = (int)vals["sellprice"];
            auction = (string)vals["auction"];
            itemsort = (int)vals["itemsort"];
            deposit = (bool)vals["deposit"];
            origin = (string)vals["origin"];
            dailyuselimit = (int)vals["dailyuselimit"];
            weeklyuselimit = (int)vals["weeklyuselimit"];
            dailygetlimit = (int)vals["dailygetlimit"];
            weeklygetlimit = (int)vals["weeklygetlimit"];
            iconspritepath = (string)vals["iconspritepath"];
            log = (bool)vals["log"];
            tickermessage = (bool)vals["tickermessage"];
            generateuid = (bool)vals["generateuid"];
            cdtime = Convert.ToSingle((double)vals["cdtime"]);
            gcdtime = Convert.ToSingle((double)vals["gcdtime"]);
            gcdtype = (GCDType)vals["gcdtype"];
            segrpid = (string)vals["segrpid"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class MaterialJson : ItemBaseJson
    {
        [JsonProperty("0")]
        public override int id { get; set; } 
        
        [JsonProperty("1")]
        public override int itemid { get; set; } 
        
        [JsonProperty("2")]
        public override string name { get; set; } 
        
        [JsonProperty("3")]
        public override string localizedname { get; set; } 
        
        [JsonProperty("4")]
        public override string description { get; set; } 
        
        [JsonProperty("5")]
        public override BagType bagtype { get; set; } 
        
        [JsonProperty("6")]
        public override ItemRarity rarity { get; set; } 
        
        [JsonProperty("7")]
        public override int requirelvl { get; set; } 
        
        [JsonProperty("8")]
        public override int casttime { get; set; } 
        
        [JsonProperty("9")]
        public override int sellprice { get; set; } 
        
        [JsonProperty("10")]
        public override string auction { get; set; } 
        
        //type ItemSortJson
        [JsonProperty("11")]
        public override int itemsort { get; set; } 
        
        [JsonProperty("12")]
        public override bool deposit { get; set; } 
        
        [JsonProperty("13")]
        public override string origin { get; set; } 
        
        [JsonProperty("14")]
        public override int dailyuselimit { get; set; } 
        
        [JsonProperty("15")]
        public override int weeklyuselimit { get; set; } 
        
        [JsonProperty("16")]
        public override int dailygetlimit { get; set; } 
        
        [JsonProperty("17")]
        public override int weeklygetlimit { get; set; } 
        
        [AssetData("sprite")]
        [JsonProperty("18")]
        public override string iconspritepath { get; set; } 
        
        [JsonProperty("19")]
        public override bool log { get; set; } 
        
        [JsonProperty("20")]
        public override bool tickermessage { get; set; } 
        
        [JsonProperty("21")]
        public override bool generateuid { get; set; } 
        
        public override ItemType itemtype { get { return ItemType.Material; } } 
        
        [JsonProperty("22")]
        public ToolTipType tooltiptype { get; set; } 
        
        [JsonProperty("23")]
        public MaterialType mattype { get; set; } 
        
        [JsonProperty("24")]
        public int uiid { get; set; } 
        
        [JsonProperty("25")]
        public int reqval { get; set; } 
        
        [JsonProperty("26")]
        public int exchitemid { get; set; } 
        
        [JsonProperty("27")]
        public float upgitemrate { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            itemid = (int)vals["itemid"];
            name = (string)vals["name"];
            localizedname = (string)vals["localizedname"];
            description = (string)vals["description"];
            bagtype = (BagType)vals["bagtype"];
            rarity = (ItemRarity)vals["rarity"];
            requirelvl = (int)vals["requirelvl"];
            casttime = (int)vals["casttime"];
            sellprice = (int)vals["sellprice"];
            auction = (string)vals["auction"];
            itemsort = (int)vals["itemsort"];
            deposit = (bool)vals["deposit"];
            origin = (string)vals["origin"];
            dailyuselimit = (int)vals["dailyuselimit"];
            weeklyuselimit = (int)vals["weeklyuselimit"];
            dailygetlimit = (int)vals["dailygetlimit"];
            weeklygetlimit = (int)vals["weeklygetlimit"];
            iconspritepath = (string)vals["iconspritepath"];
            log = (bool)vals["log"];
            tickermessage = (bool)vals["tickermessage"];
            generateuid = (bool)vals["generateuid"];
            tooltiptype = (ToolTipType)vals["tooltiptype"];
            mattype = (MaterialType)vals["mattype"];
            uiid = (int)vals["uiid"];
            reqval = (int)vals["reqval"];
            exchitemid = (int)vals["exchitemid"];
            upgitemrate = Convert.ToSingle((double)vals["upgitemrate"]);
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class LuckyPickJson : ItemBaseJson
    {
        [JsonProperty("0")]
        public override int id { get; set; } 
        
        [JsonProperty("1")]
        public override int itemid { get; set; } 
        
        [JsonProperty("2")]
        public override string name { get; set; } 
        
        [JsonProperty("3")]
        public override string localizedname { get; set; } 
        
        [JsonProperty("4")]
        public override string description { get; set; } 
        
        [JsonProperty("5")]
        public override BagType bagtype { get; set; } 
        
        [JsonProperty("6")]
        public override ItemRarity rarity { get; set; } 
        
        [JsonProperty("7")]
        public override int requirelvl { get; set; } 
        
        [JsonProperty("8")]
        public override int casttime { get; set; } 
        
        [JsonProperty("9")]
        public override int sellprice { get; set; } 
        
        [JsonProperty("10")]
        public override string auction { get; set; } 
        
        //type ItemSortJson
        [JsonProperty("11")]
        public override int itemsort { get; set; } 
        
        [JsonProperty("12")]
        public override bool deposit { get; set; } 
        
        [JsonProperty("13")]
        public override string origin { get; set; } 
        
        [JsonProperty("14")]
        public override int dailyuselimit { get; set; } 
        
        [JsonProperty("15")]
        public override int weeklyuselimit { get; set; } 
        
        [JsonProperty("16")]
        public override int dailygetlimit { get; set; } 
        
        [JsonProperty("17")]
        public override int weeklygetlimit { get; set; } 
        
        [AssetData("sprite")]
        [JsonProperty("18")]
        public override string iconspritepath { get; set; } 
        
        [JsonProperty("19")]
        public override bool log { get; set; } 
        
        [JsonProperty("20")]
        public override bool tickermessage { get; set; } 
        
        [JsonProperty("21")]
        public override bool generateuid { get; set; } 
        
        public override ItemType itemtype { get { return ItemType.LuckyPick; } } 
        
        [JsonProperty("22")]
        public string luckypickgroup { get; set; } 
        
        [JsonProperty("23")]
        public string currency { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            itemid = (int)vals["itemid"];
            name = (string)vals["name"];
            localizedname = (string)vals["localizedname"];
            description = (string)vals["description"];
            bagtype = (BagType)vals["bagtype"];
            rarity = (ItemRarity)vals["rarity"];
            requirelvl = (int)vals["requirelvl"];
            casttime = (int)vals["casttime"];
            sellprice = (int)vals["sellprice"];
            auction = (string)vals["auction"];
            itemsort = (int)vals["itemsort"];
            deposit = (bool)vals["deposit"];
            origin = (string)vals["origin"];
            dailyuselimit = (int)vals["dailyuselimit"];
            weeklyuselimit = (int)vals["weeklyuselimit"];
            dailygetlimit = (int)vals["dailygetlimit"];
            weeklygetlimit = (int)vals["weeklygetlimit"];
            iconspritepath = (string)vals["iconspritepath"];
            log = (bool)vals["log"];
            tickermessage = (bool)vals["tickermessage"];
            generateuid = (bool)vals["generateuid"];
            luckypickgroup = (string)vals["luckypickgroup"];
            currency = (string)vals["currency"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class HenshinJson : ItemBaseJson
    {
        [JsonProperty("0")]
        public override int id { get; set; } 
        
        [JsonProperty("1")]
        public override int itemid { get; set; } 
        
        [JsonProperty("2")]
        public override string name { get; set; } 
        
        [JsonProperty("3")]
        public override string localizedname { get; set; } 
        
        [JsonProperty("4")]
        public override string description { get; set; } 
        
        [JsonProperty("5")]
        public override BagType bagtype { get; set; } 
        
        [JsonProperty("6")]
        public override ItemRarity rarity { get; set; } 
        
        [JsonProperty("7")]
        public override int requirelvl { get; set; } 
        
        [JsonProperty("8")]
        public override int casttime { get; set; } 
        
        [JsonProperty("9")]
        public override int sellprice { get; set; } 
        
        [JsonProperty("10")]
        public override string auction { get; set; } 
        
        //type ItemSortJson
        [JsonProperty("11")]
        public override int itemsort { get; set; } 
        
        [JsonProperty("12")]
        public override bool deposit { get; set; } 
        
        [JsonProperty("13")]
        public override string origin { get; set; } 
        
        [JsonProperty("14")]
        public override int dailyuselimit { get; set; } 
        
        [JsonProperty("15")]
        public override int weeklyuselimit { get; set; } 
        
        [JsonProperty("16")]
        public override int dailygetlimit { get; set; } 
        
        [JsonProperty("17")]
        public override int weeklygetlimit { get; set; } 
        
        [AssetData("sprite")]
        [JsonProperty("18")]
        public override string iconspritepath { get; set; } 
        
        [JsonProperty("19")]
        public override bool log { get; set; } 
        
        [JsonProperty("20")]
        public override bool tickermessage { get; set; } 
        
        [JsonProperty("21")]
        public override bool generateuid { get; set; } 
        
        public override ItemType itemtype { get { return ItemType.Henshin; } } 
        
        [JsonProperty("22")]
        public string henshinname { get; set; } 
        
        [JsonProperty("23")]
        public string sideeffect { get; set; } 
        
        [AssetData("sprite")]
        [JsonProperty("24")]
        public string iconpath { get; set; } 
        
        [AssetData("prefab")]
        [JsonProperty("25")]
        public string prefabpath { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            itemid = (int)vals["itemid"];
            name = (string)vals["name"];
            localizedname = (string)vals["localizedname"];
            description = (string)vals["description"];
            bagtype = (BagType)vals["bagtype"];
            rarity = (ItemRarity)vals["rarity"];
            requirelvl = (int)vals["requirelvl"];
            casttime = (int)vals["casttime"];
            sellprice = (int)vals["sellprice"];
            auction = (string)vals["auction"];
            itemsort = (int)vals["itemsort"];
            deposit = (bool)vals["deposit"];
            origin = (string)vals["origin"];
            dailyuselimit = (int)vals["dailyuselimit"];
            weeklyuselimit = (int)vals["weeklyuselimit"];
            dailygetlimit = (int)vals["dailygetlimit"];
            weeklygetlimit = (int)vals["weeklygetlimit"];
            iconspritepath = (string)vals["iconspritepath"];
            log = (bool)vals["log"];
            tickermessage = (bool)vals["tickermessage"];
            generateuid = (bool)vals["generateuid"];
            henshinname = (string)vals["henshinname"];
            sideeffect = (string)vals["sideeffect"];
            iconpath = (string)vals["iconpath"];
            prefabpath = (string)vals["prefabpath"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class FeaturesJson : ItemBaseJson
    {
        [JsonProperty("0")]
        public override int id { get; set; } 
        
        [JsonProperty("1")]
        public override int itemid { get; set; } 
        
        [JsonProperty("2")]
        public override string name { get; set; } 
        
        [JsonProperty("3")]
        public override string localizedname { get; set; } 
        
        [JsonProperty("4")]
        public override string description { get; set; } 
        
        [JsonProperty("5")]
        public override BagType bagtype { get; set; } 
        
        [JsonProperty("6")]
        public override ItemRarity rarity { get; set; } 
        
        [JsonProperty("7")]
        public override int requirelvl { get; set; } 
        
        [JsonProperty("8")]
        public override int casttime { get; set; } 
        
        [JsonProperty("9")]
        public override int sellprice { get; set; } 
        
        [JsonProperty("10")]
        public override string auction { get; set; } 
        
        //type ItemSortJson
        [JsonProperty("11")]
        public override int itemsort { get; set; } 
        
        [JsonProperty("12")]
        public override bool deposit { get; set; } 
        
        [JsonProperty("13")]
        public override string origin { get; set; } 
        
        [JsonProperty("14")]
        public override int dailyuselimit { get; set; } 
        
        [JsonProperty("15")]
        public override int weeklyuselimit { get; set; } 
        
        [JsonProperty("16")]
        public override int dailygetlimit { get; set; } 
        
        [JsonProperty("17")]
        public override int weeklygetlimit { get; set; } 
        
        [AssetData("sprite")]
        [JsonProperty("18")]
        public override string iconspritepath { get; set; } 
        
        [JsonProperty("19")]
        public override bool log { get; set; } 
        
        [JsonProperty("20")]
        public override bool tickermessage { get; set; } 
        
        [JsonProperty("21")]
        public override bool generateuid { get; set; } 
        
        public override ItemType itemtype { get { return ItemType.Features; } } 
        
        [JsonProperty("22")]
        public int uiid { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            itemid = (int)vals["itemid"];
            name = (string)vals["name"];
            localizedname = (string)vals["localizedname"];
            description = (string)vals["description"];
            bagtype = (BagType)vals["bagtype"];
            rarity = (ItemRarity)vals["rarity"];
            requirelvl = (int)vals["requirelvl"];
            casttime = (int)vals["casttime"];
            sellprice = (int)vals["sellprice"];
            auction = (string)vals["auction"];
            itemsort = (int)vals["itemsort"];
            deposit = (bool)vals["deposit"];
            origin = (string)vals["origin"];
            dailyuselimit = (int)vals["dailyuselimit"];
            weeklyuselimit = (int)vals["weeklyuselimit"];
            dailygetlimit = (int)vals["dailygetlimit"];
            weeklygetlimit = (int)vals["weeklygetlimit"];
            iconspritepath = (string)vals["iconspritepath"];
            log = (bool)vals["log"];
            tickermessage = (bool)vals["tickermessage"];
            generateuid = (bool)vals["generateuid"];
            uiid = (int)vals["uiid"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class EquipmentJson : ItemBaseJson
    {
        [JsonProperty("0")]
        public override int id { get; set; } 
        
        [JsonProperty("1")]
        public override int itemid { get; set; } 
        
        [JsonProperty("2")]
        public override string name { get; set; } 
        
        [JsonProperty("3")]
        public override string localizedname { get; set; } 
        
        [JsonProperty("4")]
        public override string description { get; set; } 
        
        [JsonProperty("5")]
        public override BagType bagtype { get; set; } 
        
        [JsonProperty("6")]
        public override ItemRarity rarity { get; set; } 
        
        [JsonProperty("7")]
        public override int requirelvl { get; set; } 
        
        [JsonProperty("8")]
        public override int casttime { get; set; } 
        
        [JsonProperty("9")]
        public override int sellprice { get; set; } 
        
        [JsonProperty("10")]
        public override string auction { get; set; } 
        
        //type ItemSortJson
        [JsonProperty("11")]
        public override int itemsort { get; set; } 
        
        [JsonProperty("12")]
        public override bool deposit { get; set; } 
        
        [JsonProperty("13")]
        public override string origin { get; set; } 
        
        [JsonProperty("14")]
        public override int dailyuselimit { get; set; } 
        
        [JsonProperty("15")]
        public override int weeklyuselimit { get; set; } 
        
        [JsonProperty("16")]
        public override int dailygetlimit { get; set; } 
        
        [JsonProperty("17")]
        public override int weeklygetlimit { get; set; } 
        
        [AssetData("sprite")]
        [JsonProperty("18")]
        public override string iconspritepath { get; set; } 
        
        [JsonProperty("19")]
        public override bool log { get; set; } 
        
        [JsonProperty("20")]
        public override bool tickermessage { get; set; } 
        
        [JsonProperty("21")]
        public override bool generateuid { get; set; } 
        
        public override ItemType itemtype { get { return ItemType.Equipment; } } 
        
        [JsonProperty("22")]
        public EquipmentType equiptype { get; set; } 
        
        [JsonProperty("23")]
        public PartsType partstype { get; set; } 
        
        [JsonProperty("24")]
        public bool fashionsuit { get; set; } 
        
        [JsonProperty("25")]
        public MainWeaponAttribute mainwepattrib { get; set; } 
        
        [JsonProperty("26")]
        public string equipclass { get; set; } 
        
        [JsonProperty("27")]
        public string basese { get; set; } 
        
        [JsonProperty("28")]
        public string extrase { get; set; } 
        
        [JsonProperty("29")]
        public int upgradelimit { get; set; } 
        
        [JsonProperty("30")]
        public bool enchant { get; set; } 
        
        [JsonProperty("31")]
        public string socketspace { get; set; } 
        
        [JsonProperty("32")]
        public string evolvegrp { get; set; } 
        
        [JsonProperty("33")]
        public int evolvechange { get; set; } 
        
        [AssetData("prefab")]
        [JsonProperty("34")]
        public string prefabpath { get; set; } 
        
        [AssetData("prefab")]
        [JsonProperty("35")]
        public string femaleprefabpath { get; set; } 
        
        [AssetData("mesh")]
        [JsonProperty("36")]
        public string malemeshpath { get; set; } 
        
        [AssetData("material")]
        [JsonProperty("37")]
        public string malematerialpath { get; set; } 
        
        [AssetData("mesh")]
        [JsonProperty("38")]
        public string femalemeshpath { get; set; } 
        
        [AssetData("material")]
        [JsonProperty("39")]
        public string femalematerialpath { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            itemid = (int)vals["itemid"];
            name = (string)vals["name"];
            localizedname = (string)vals["localizedname"];
            description = (string)vals["description"];
            bagtype = (BagType)vals["bagtype"];
            rarity = (ItemRarity)vals["rarity"];
            requirelvl = (int)vals["requirelvl"];
            casttime = (int)vals["casttime"];
            sellprice = (int)vals["sellprice"];
            auction = (string)vals["auction"];
            itemsort = (int)vals["itemsort"];
            deposit = (bool)vals["deposit"];
            origin = (string)vals["origin"];
            dailyuselimit = (int)vals["dailyuselimit"];
            weeklyuselimit = (int)vals["weeklyuselimit"];
            dailygetlimit = (int)vals["dailygetlimit"];
            weeklygetlimit = (int)vals["weeklygetlimit"];
            iconspritepath = (string)vals["iconspritepath"];
            log = (bool)vals["log"];
            tickermessage = (bool)vals["tickermessage"];
            generateuid = (bool)vals["generateuid"];
            equiptype = (EquipmentType)vals["equiptype"];
            partstype = (PartsType)vals["partstype"];
            fashionsuit = (bool)vals["fashionsuit"];
            mainwepattrib = (MainWeaponAttribute)vals["mainwepattrib"];
            equipclass = (string)vals["equipclass"];
            basese = (string)vals["basese"];
            extrase = (string)vals["extrase"];
            upgradelimit = (int)vals["upgradelimit"];
            enchant = (bool)vals["enchant"];
            socketspace = (string)vals["socketspace"];
            evolvegrp = (string)vals["evolvegrp"];
            evolvechange = (int)vals["evolvechange"];
            prefabpath = (string)vals["prefabpath"];
            femaleprefabpath = (string)vals["femaleprefabpath"];
            malemeshpath = (string)vals["malemeshpath"];
            malematerialpath = (string)vals["malematerialpath"];
            femalemeshpath = (string)vals["femalemeshpath"];
            femalematerialpath = (string)vals["femalematerialpath"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class DNAJson : ItemBaseJson
    {
        [JsonProperty("0")]
        public override int id { get; set; } 
        
        [JsonProperty("1")]
        public override int itemid { get; set; } 
        
        [JsonProperty("2")]
        public override string name { get; set; } 
        
        [JsonProperty("3")]
        public override string localizedname { get; set; } 
        
        [JsonProperty("4")]
        public override string description { get; set; } 
        
        [JsonProperty("5")]
        public override BagType bagtype { get; set; } 
        
        [JsonProperty("6")]
        public override ItemRarity rarity { get; set; } 
        
        [JsonProperty("7")]
        public override int requirelvl { get; set; } 
        
        [JsonProperty("8")]
        public override int casttime { get; set; } 
        
        [JsonProperty("9")]
        public override int sellprice { get; set; } 
        
        [JsonProperty("10")]
        public override string auction { get; set; } 
        
        //type ItemSortJson
        [JsonProperty("11")]
        public override int itemsort { get; set; } 
        
        [JsonProperty("12")]
        public override bool deposit { get; set; } 
        
        [JsonProperty("13")]
        public override string origin { get; set; } 
        
        [JsonProperty("14")]
        public override int dailyuselimit { get; set; } 
        
        [JsonProperty("15")]
        public override int weeklyuselimit { get; set; } 
        
        [JsonProperty("16")]
        public override int dailygetlimit { get; set; } 
        
        [JsonProperty("17")]
        public override int weeklygetlimit { get; set; } 
        
        [AssetData("sprite")]
        [JsonProperty("18")]
        public override string iconspritepath { get; set; } 
        
        [JsonProperty("19")]
        public override bool log { get; set; } 
        
        [JsonProperty("20")]
        public override bool tickermessage { get; set; } 
        
        [JsonProperty("21")]
        public override bool generateuid { get; set; } 
        
        public override ItemType itemtype { get { return ItemType.DNA; } } 
        
        [JsonProperty("22")]
        public DNAType dnatype { get; set; } 
        
        [JsonProperty("23")]
        public int exp { get; set; } 
        
        [JsonProperty("24")]
        public string evotype { get; set; } 
        
        [JsonProperty("25")]
        public int maxlevel { get; set; } 
        
        [JsonProperty("26")]
        public int maxstage { get; set; } 
        
        [JsonProperty("27")]
        public string postive { get; set; } 
        
        [JsonProperty("28")]
        public string negative { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            itemid = (int)vals["itemid"];
            name = (string)vals["name"];
            localizedname = (string)vals["localizedname"];
            description = (string)vals["description"];
            bagtype = (BagType)vals["bagtype"];
            rarity = (ItemRarity)vals["rarity"];
            requirelvl = (int)vals["requirelvl"];
            casttime = (int)vals["casttime"];
            sellprice = (int)vals["sellprice"];
            auction = (string)vals["auction"];
            itemsort = (int)vals["itemsort"];
            deposit = (bool)vals["deposit"];
            origin = (string)vals["origin"];
            dailyuselimit = (int)vals["dailyuselimit"];
            weeklyuselimit = (int)vals["weeklyuselimit"];
            dailygetlimit = (int)vals["dailygetlimit"];
            weeklygetlimit = (int)vals["weeklygetlimit"];
            iconspritepath = (string)vals["iconspritepath"];
            log = (bool)vals["log"];
            tickermessage = (bool)vals["tickermessage"];
            generateuid = (bool)vals["generateuid"];
            dnatype = (DNAType)vals["dnatype"];
            exp = (int)vals["exp"];
            evotype = (string)vals["evotype"];
            maxlevel = (int)vals["maxlevel"];
            maxstage = (int)vals["maxstage"];
            postive = (string)vals["postive"];
            negative = (string)vals["negative"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class RelicJson : ItemBaseJson
    {
        [JsonProperty("0")]
        public override int id { get; set; } 
        
        [JsonProperty("1")]
        public override int itemid { get; set; } 
        
        [JsonProperty("2")]
        public override string name { get; set; } 
        
        [JsonProperty("3")]
        public override string localizedname { get; set; } 
        
        [JsonProperty("4")]
        public override string description { get; set; } 
        
        [JsonProperty("5")]
        public override BagType bagtype { get; set; } 
        
        [JsonProperty("6")]
        public override ItemRarity rarity { get; set; } 
        
        [JsonProperty("7")]
        public override int requirelvl { get; set; } 
        
        [JsonProperty("8")]
        public override int casttime { get; set; } 
        
        [JsonProperty("9")]
        public override int sellprice { get; set; } 
        
        [JsonProperty("10")]
        public override string auction { get; set; } 
        
        //type ItemSortJson
        [JsonProperty("11")]
        public override int itemsort { get; set; } 
        
        [JsonProperty("12")]
        public override bool deposit { get; set; } 
        
        [JsonProperty("13")]
        public override string origin { get; set; } 
        
        [JsonProperty("14")]
        public override int dailyuselimit { get; set; } 
        
        [JsonProperty("15")]
        public override int weeklyuselimit { get; set; } 
        
        [JsonProperty("16")]
        public override int dailygetlimit { get; set; } 
        
        [JsonProperty("17")]
        public override int weeklygetlimit { get; set; } 
        
        [AssetData("sprite")]
        [JsonProperty("18")]
        public override string iconspritepath { get; set; } 
        
        [JsonProperty("19")]
        public override bool log { get; set; } 
        
        [JsonProperty("20")]
        public override bool tickermessage { get; set; } 
        
        [JsonProperty("21")]
        public override bool generateuid { get; set; } 
        
        public override ItemType itemtype { get { return ItemType.Relic; } } 
        
        [JsonProperty("22")]
        public RelicType relictype { get; set; } 
        
        [JsonProperty("23")]
        public string sockability { get; set; } 
        
        [JsonProperty("24")]
        public string collectability { get; set; } 
        
        [JsonProperty("25")]
        public bool recycle { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            itemid = (int)vals["itemid"];
            name = (string)vals["name"];
            localizedname = (string)vals["localizedname"];
            description = (string)vals["description"];
            bagtype = (BagType)vals["bagtype"];
            rarity = (ItemRarity)vals["rarity"];
            requirelvl = (int)vals["requirelvl"];
            casttime = (int)vals["casttime"];
            sellprice = (int)vals["sellprice"];
            auction = (string)vals["auction"];
            itemsort = (int)vals["itemsort"];
            deposit = (bool)vals["deposit"];
            origin = (string)vals["origin"];
            dailyuselimit = (int)vals["dailyuselimit"];
            weeklyuselimit = (int)vals["weeklyuselimit"];
            dailygetlimit = (int)vals["dailygetlimit"];
            weeklygetlimit = (int)vals["weeklygetlimit"];
            iconspritepath = (string)vals["iconspritepath"];
            log = (bool)vals["log"];
            tickermessage = (bool)vals["tickermessage"];
            generateuid = (bool)vals["generateuid"];
            relictype = (RelicType)vals["relictype"];
            sockability = (string)vals["sockability"];
            collectability = (string)vals["collectability"];
            recycle = (bool)vals["recycle"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class QuestItemJson : ItemBaseJson
    {
        [JsonProperty("0")]
        public override int id { get; set; } 
        
        [JsonProperty("1")]
        public override int itemid { get; set; } 
        
        [JsonProperty("2")]
        public override string name { get; set; } 
        
        [JsonProperty("3")]
        public override string localizedname { get; set; } 
        
        [JsonProperty("4")]
        public override string description { get; set; } 
        
        [JsonProperty("5")]
        public override BagType bagtype { get; set; } 
        
        [JsonProperty("6")]
        public override ItemRarity rarity { get; set; } 
        
        [JsonProperty("7")]
        public override int requirelvl { get; set; } 
        
        [JsonProperty("8")]
        public override int casttime { get; set; } 
        
        [JsonProperty("9")]
        public override int sellprice { get; set; } 
        
        [JsonProperty("10")]
        public override string auction { get; set; } 
        
        //type ItemSortJson
        [JsonProperty("11")]
        public override int itemsort { get; set; } 
        
        [JsonProperty("12")]
        public override bool deposit { get; set; } 
        
        [JsonProperty("13")]
        public override string origin { get; set; } 
        
        [JsonProperty("14")]
        public override int dailyuselimit { get; set; } 
        
        [JsonProperty("15")]
        public override int weeklyuselimit { get; set; } 
        
        [JsonProperty("16")]
        public override int dailygetlimit { get; set; } 
        
        [JsonProperty("17")]
        public override int weeklygetlimit { get; set; } 
        
        [AssetData("sprite")]
        [JsonProperty("18")]
        public override string iconspritepath { get; set; } 
        
        [JsonProperty("19")]
        public override bool log { get; set; } 
        
        [JsonProperty("20")]
        public override bool tickermessage { get; set; } 
        
        [JsonProperty("21")]
        public override bool generateuid { get; set; } 
        
        public override ItemType itemtype { get { return ItemType.QuestItem; } } 
        
        [JsonProperty("22")]
        public int questid { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            itemid = (int)vals["itemid"];
            name = (string)vals["name"];
            localizedname = (string)vals["localizedname"];
            description = (string)vals["description"];
            bagtype = (BagType)vals["bagtype"];
            rarity = (ItemRarity)vals["rarity"];
            requirelvl = (int)vals["requirelvl"];
            casttime = (int)vals["casttime"];
            sellprice = (int)vals["sellprice"];
            auction = (string)vals["auction"];
            itemsort = (int)vals["itemsort"];
            deposit = (bool)vals["deposit"];
            origin = (string)vals["origin"];
            dailyuselimit = (int)vals["dailyuselimit"];
            weeklyuselimit = (int)vals["weeklyuselimit"];
            dailygetlimit = (int)vals["dailygetlimit"];
            weeklygetlimit = (int)vals["weeklygetlimit"];
            iconspritepath = (string)vals["iconspritepath"];
            log = (bool)vals["log"];
            tickermessage = (bool)vals["tickermessage"];
            generateuid = (bool)vals["generateuid"];
            questid = (int)vals["questid"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class HeroItemJson : ItemBaseJson
    {
        [JsonProperty("0")]
        public override int id { get; set; } 
        
        [JsonProperty("1")]
        public override int itemid { get; set; } 
        
        [JsonProperty("2")]
        public override string name { get; set; } 
        
        [JsonProperty("3")]
        public override string localizedname { get; set; } 
        
        [JsonProperty("4")]
        public override string description { get; set; } 
        
        [JsonProperty("5")]
        public override BagType bagtype { get; set; } 
        
        [JsonProperty("6")]
        public override ItemRarity rarity { get; set; } 
        
        [JsonProperty("7")]
        public override int requirelvl { get; set; } 
        
        [JsonProperty("8")]
        public override int casttime { get; set; } 
        
        [JsonProperty("9")]
        public override int sellprice { get; set; } 
        
        [JsonProperty("10")]
        public override string auction { get; set; } 
        
        //type ItemSortJson
        [JsonProperty("11")]
        public override int itemsort { get; set; } 
        
        [JsonProperty("12")]
        public override bool deposit { get; set; } 
        
        [JsonProperty("13")]
        public override string origin { get; set; } 
        
        [JsonProperty("14")]
        public override int dailyuselimit { get; set; } 
        
        [JsonProperty("15")]
        public override int weeklyuselimit { get; set; } 
        
        [JsonProperty("16")]
        public override int dailygetlimit { get; set; } 
        
        [JsonProperty("17")]
        public override int weeklygetlimit { get; set; } 
        
        [AssetData("sprite")]
        [JsonProperty("18")]
        public override string iconspritepath { get; set; } 
        
        [JsonProperty("19")]
        public override bool log { get; set; } 
        
        [JsonProperty("20")]
        public override bool tickermessage { get; set; } 
        
        [JsonProperty("21")]
        public override bool generateuid { get; set; } 
        
        public override ItemType itemtype { get { return ItemType.MercenaryItem; } } 
        
        [JsonProperty("22")]
        public HeroItemType heroitemtype { get; set; } 
        
        [JsonProperty("23")]
        public string heroid { get; set; } 
        
        [JsonProperty("24")]
        public int ischangelike { get; set; } 
        
        [JsonProperty("25")]
        public int liketype { get; set; } 
        
        [JsonProperty("26")]
        public int giftexp { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            itemid = (int)vals["itemid"];
            name = (string)vals["name"];
            localizedname = (string)vals["localizedname"];
            description = (string)vals["description"];
            bagtype = (BagType)vals["bagtype"];
            rarity = (ItemRarity)vals["rarity"];
            requirelvl = (int)vals["requirelvl"];
            casttime = (int)vals["casttime"];
            sellprice = (int)vals["sellprice"];
            auction = (string)vals["auction"];
            itemsort = (int)vals["itemsort"];
            deposit = (bool)vals["deposit"];
            origin = (string)vals["origin"];
            dailyuselimit = (int)vals["dailyuselimit"];
            weeklyuselimit = (int)vals["weeklyuselimit"];
            dailygetlimit = (int)vals["dailygetlimit"];
            weeklygetlimit = (int)vals["weeklygetlimit"];
            iconspritepath = (string)vals["iconspritepath"];
            log = (bool)vals["log"];
            tickermessage = (bool)vals["tickermessage"];
            generateuid = (bool)vals["generateuid"];
            heroitemtype = (HeroItemType)vals["heroitemtype"];
            heroid = (string)vals["heroid"];
            ischangelike = (int)vals["ischangelike"];
            liketype = (int)vals["liketype"];
            giftexp = (int)vals["giftexp"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class InstanceItemJson : ItemBaseJson
    {
        [JsonProperty("0")]
        public override int id { get; set; } 
        
        [JsonProperty("1")]
        public override int itemid { get; set; } 
        
        [JsonProperty("2")]
        public override string name { get; set; } 
        
        [JsonProperty("3")]
        public override string localizedname { get; set; } 
        
        [JsonProperty("4")]
        public override string description { get; set; } 
        
        [JsonProperty("5")]
        public override BagType bagtype { get; set; } 
        
        [JsonProperty("6")]
        public override ItemRarity rarity { get; set; } 
        
        [JsonProperty("7")]
        public override int requirelvl { get; set; } 
        
        [JsonProperty("8")]
        public override int casttime { get; set; } 
        
        [JsonProperty("9")]
        public override int sellprice { get; set; } 
        
        [JsonProperty("10")]
        public override string auction { get; set; } 
        
        //type ItemSortJson
        [JsonProperty("11")]
        public override int itemsort { get; set; } 
        
        [JsonProperty("12")]
        public override bool deposit { get; set; } 
        
        [JsonProperty("13")]
        public override string origin { get; set; } 
        
        [JsonProperty("14")]
        public override int dailyuselimit { get; set; } 
        
        [JsonProperty("15")]
        public override int weeklyuselimit { get; set; } 
        
        [JsonProperty("16")]
        public override int dailygetlimit { get; set; } 
        
        [JsonProperty("17")]
        public override int weeklygetlimit { get; set; } 
        
        [AssetData("sprite")]
        [JsonProperty("18")]
        public override string iconspritepath { get; set; } 
        
        [JsonProperty("19")]
        public override bool log { get; set; } 
        
        [JsonProperty("20")]
        public override bool tickermessage { get; set; } 
        
        [JsonProperty("21")]
        public override bool generateuid { get; set; } 
        
        public override ItemType itemtype { get { return ItemType.InstanceItem; } } 
        
        [JsonProperty("22")]
        public string coordinate { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            itemid = (int)vals["itemid"];
            name = (string)vals["name"];
            localizedname = (string)vals["localizedname"];
            description = (string)vals["description"];
            bagtype = (BagType)vals["bagtype"];
            rarity = (ItemRarity)vals["rarity"];
            requirelvl = (int)vals["requirelvl"];
            casttime = (int)vals["casttime"];
            sellprice = (int)vals["sellprice"];
            auction = (string)vals["auction"];
            itemsort = (int)vals["itemsort"];
            deposit = (bool)vals["deposit"];
            origin = (string)vals["origin"];
            dailyuselimit = (int)vals["dailyuselimit"];
            weeklyuselimit = (int)vals["weeklyuselimit"];
            dailygetlimit = (int)vals["dailygetlimit"];
            weeklygetlimit = (int)vals["weeklygetlimit"];
            iconspritepath = (string)vals["iconspritepath"];
            log = (bool)vals["log"];
            tickermessage = (bool)vals["tickermessage"];
            generateuid = (bool)vals["generateuid"];
            coordinate = (string)vals["coordinate"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class ExtraSideEffectJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public string name { get; set; } 
        
        [JsonProperty("2")]
        public string description { get; set; } 
        
        [JsonProperty("3")]
        public string sideeffect { get; set; } 
        
        [JsonProperty("4")]
        public ESERequireType reqtype { get; set; } 
        
        [JsonProperty("5")]
        public string requpgrade { get; set; } 
        
        [JsonProperty("6")]
        public string reqskill { get; set; } 
        
        [JsonProperty("7")]
        public string reqattribute { get; set; } 
        
        [JsonProperty("8")]
        public string requiregearset { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            name = (string)vals["name"];
            description = (string)vals["description"];
            sideeffect = (string)vals["sideeffect"];
            reqtype = (ESERequireType)vals["reqtype"];
            requpgrade = (string)vals["requpgrade"];
            reqskill = (string)vals["reqskill"];
            reqattribute = (string)vals["reqattribute"];
            requiregearset = (string)vals["requiregearset"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class EvolveGroupJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public string name { get; set; } 
        
        [JsonProperty("2")]
        public string groupid { get; set; } 
        
        [JsonProperty("3")]
        public int evolvestep { get; set; } 
        
        [JsonProperty("4")]
        public string sideeffect { get; set; } 
        
        [JsonProperty("5")]
        public string requirement { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            name = (string)vals["name"];
            groupid = (string)vals["groupid"];
            evolvestep = (int)vals["evolvestep"];
            sideeffect = (string)vals["sideeffect"];
            requirement = (string)vals["requirement"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class ItemMallItemJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public string name { get; set; } 
        
        [JsonProperty("2")]
        public int itemid { get; set; } 
        
        [JsonProperty("3")]
        public int price { get; set; } 
        
        [JsonProperty("4")]
        public int purchasecount { get; set; } 
        
        [JsonProperty("5")]
        public int limitcount { get; set; } 
        
        [JsonProperty("6")]
        public ItemMallCategory category { get; set; } 
        
        [JsonProperty("7")]
        public ItemMallShoppingType shoppingtype { get; set; } 
        
        [JsonProperty("8")]
        public int viplevel { get; set; } 
        
        [JsonProperty("9")]
        public string uptime { get; set; } 
        
        [JsonProperty("10")]
        public string downtime { get; set; } 
        
        [JsonProperty("11")]
        public int sortnumber { get; set; } 
        
        [JsonProperty("12")]
        public bool online { get; set; } 
        
        [JsonProperty("13")]
        public bool showtime { get; set; } 
        
        [JsonProperty("14")]
        public bool showlimited { get; set; } 
        
        [JsonProperty("15")]
        public bool showvip { get; set; } 
        
        [JsonProperty("16")]
        public JobType shoppingjob { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            name = (string)vals["name"];
            itemid = (int)vals["itemid"];
            price = (int)vals["price"];
            purchasecount = (int)vals["purchasecount"];
            limitcount = (int)vals["limitcount"];
            category = (ItemMallCategory)vals["category"];
            shoppingtype = (ItemMallShoppingType)vals["shoppingtype"];
            viplevel = (int)vals["viplevel"];
            uptime = (string)vals["uptime"];
            downtime = (string)vals["downtime"];
            sortnumber = (int)vals["sortnumber"];
            online = (bool)vals["online"];
            showtime = (bool)vals["showtime"];
            showlimited = (bool)vals["showlimited"];
            showvip = (bool)vals["showvip"];
            shoppingjob = (JobType)vals["shoppingjob"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class ShopItemMapTreasureJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public ItemMallShoppingType treasuretype { get; set; } 
        
        [JsonProperty("2")]
        public int points { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            treasuretype = (ItemMallShoppingType)vals["treasuretype"];
            points = (int)vals["points"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class StoreSetJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public int storeorder { get; set; } 
        
        [JsonProperty("2")]
        public string name { get; set; } 
        
        [JsonProperty("3")]
        public string localizedname { get; set; } 
        
        [JsonProperty("4")]
        public int heroid { get; set; } 
        
        [JsonProperty("5")]
        public UIStoreLinkType shoptype { get; set; } 
        
        [JsonProperty("6")]
        public int openlv { get; set; } 
        
        [JsonProperty("7")]
        public int commoditycount { get; set; } 
        
        [JsonProperty("8")]
        public bool refresh { get; set; } 
        
        [JsonProperty("9")]
        public string refreshtime1 { get; set; } 
        
        [JsonProperty("10")]
        public string refreshtime2 { get; set; } 
        
        [JsonProperty("11")]
        public string refreshtime3 { get; set; } 
        
        [JsonProperty("12")]
        public CurrencyType currencytype { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            storeorder = (int)vals["storeorder"];
            name = (string)vals["name"];
            localizedname = (string)vals["localizedname"];
            heroid = (int)vals["heroid"];
            shoptype = (UIStoreLinkType)vals["shoptype"];
            openlv = (int)vals["openlv"];
            commoditycount = (int)vals["commoditycount"];
            refresh = (bool)vals["refresh"];
            refreshtime1 = (string)vals["refreshtime1"];
            refreshtime2 = (string)vals["refreshtime2"];
            refreshtime3 = (string)vals["refreshtime3"];
            currencytype = (CurrencyType)vals["currencytype"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class ProductSettingJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public int itemid { get; set; } 
        
        [JsonProperty("2")]
        public int itemcount { get; set; } 
        
        [JsonProperty("3")]
        public int totalprice { get; set; } 
        
        [JsonProperty("4")]
        public int storeorder { get; set; } 
        
        [JsonProperty("5")]
        public int heroid { get; set; } 
        
        [JsonProperty("6")]
        public string shelvesnumber { get; set; } 
        
        [JsonProperty("7")]
        public int probability { get; set; } 
        
        [JsonProperty("8")]
        public int levelreq { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            itemid = (int)vals["itemid"];
            itemcount = (int)vals["itemcount"];
            totalprice = (int)vals["totalprice"];
            storeorder = (int)vals["storeorder"];
            heroid = (int)vals["heroid"];
            shelvesnumber = (string)vals["shelvesnumber"];
            probability = (int)vals["probability"];
            levelreq = (int)vals["levelreq"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class StoreRefreshJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public int storeorder { get; set; } 
        
        [JsonProperty("2")]
        public int refreshtime { get; set; } 
        
        [JsonProperty("3")]
        public int refreshprice { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            storeorder = (int)vals["storeorder"];
            refreshtime = (int)vals["refreshtime"];
            refreshprice = (int)vals["refreshprice"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class EquipmentUpgradeJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public int upgradelvl { get; set; } 
        
        [JsonProperty("2")]
        public ItemRarity rarity { get; set; } 
        
        [JsonProperty("3")]
        public EquipmentType type { get; set; } 
        
        [JsonProperty("4")]
        public int successweight { get; set; } 
        
        [JsonProperty("5")]
        public int failweight { get; set; } 
        
        [JsonProperty("6")]
        public int dropweight { get; set; } 
        
        [JsonProperty("7")]
        public float increase { get; set; } 
        
        [JsonProperty("8")]
        public string buff { get; set; } 
        
        [JsonProperty("9")]
        public int generalcost { get; set; } 
        
        [JsonProperty("10")]
        public int generalstone { get; set; } 
        
        [JsonProperty("11")]
        public int safecost { get; set; } 
        
        [JsonProperty("12")]
        public int safestone { get; set; } 
        
        [JsonProperty("13")]
        public int safegemamt { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            upgradelvl = (int)vals["upgradelvl"];
            rarity = (ItemRarity)vals["rarity"];
            type = (EquipmentType)vals["type"];
            successweight = (int)vals["successweight"];
            failweight = (int)vals["failweight"];
            dropweight = (int)vals["dropweight"];
            increase = Convert.ToSingle((double)vals["increase"]);
            buff = (string)vals["buff"];
            generalcost = (int)vals["generalcost"];
            generalstone = (int)vals["generalstone"];
            safecost = (int)vals["safecost"];
            safestone = (int)vals["safestone"];
            safegemamt = (int)vals["safegemamt"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class EquipmentReformGroupJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public string grpid { get; set; } 
        
        [JsonProperty("2")]
        public int reformstep { get; set; } 
        
        [JsonProperty("3")]
        public string sideeffect { get; set; } 
        
        [JsonProperty("4")]
        public string requirement { get; set; } 
        
        [JsonProperty("5")]
        public int cost { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            grpid = (string)vals["grpid"];
            reformstep = (int)vals["reformstep"];
            sideeffect = (string)vals["sideeffect"];
            requirement = (string)vals["requirement"];
            cost = (int)vals["cost"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class ExchangeShopItemJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public int exid { get; set; } 
        
        [JsonProperty("2")]
        public int categoryid { get; set; } 
        
        [JsonProperty("3")]
        public int sequence { get; set; } 
        
        [JsonProperty("4")]
        public bool show { get; set; } 
        
        [JsonProperty("5")]
        public int type { get; set; } 
        
        [JsonProperty("6")]
        public JobType jobsect { get; set; } 
        
        [JsonProperty("7")]
        public int rewarditem_reqlevel { get; set; } 
        
        [JsonProperty("8")]
        public int rewarditem_id { get; set; } 
        
        [JsonProperty("9")]
        public int rewarditem_count { get; set; } 
        
        [JsonProperty("10")]
        public int item1_id { get; set; } 
        
        [JsonProperty("11")]
        public int item1_count { get; set; } 
        
        [JsonProperty("12")]
        public int item2_id { get; set; } 
        
        [JsonProperty("13")]
        public int item2_count { get; set; } 
        
        [JsonProperty("14")]
        public int item3_id { get; set; } 
        
        [JsonProperty("15")]
        public int item3_count { get; set; } 
        
        [JsonProperty("16")]
        public int item4_id { get; set; } 
        
        [JsonProperty("17")]
        public int item4_count { get; set; } 
        
        [JsonProperty("18")]
        public int daily_exchange { get; set; } 
        
        [JsonProperty("19")]
        public bool daily_exchange_everyday { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            exid = (int)vals["exid"];
            categoryid = (int)vals["categoryid"];
            sequence = (int)vals["sequence"];
            show = (bool)vals["show"];
            type = (int)vals["type"];
            jobsect = (JobType)vals["jobsect"];
            rewarditem_reqlevel = (int)vals["rewarditem_reqlevel"];
            rewarditem_id = (int)vals["rewarditem_id"];
            rewarditem_count = (int)vals["rewarditem_count"];
            item1_id = (int)vals["item1_id"];
            item1_count = (int)vals["item1_count"];
            item2_id = (int)vals["item2_id"];
            item2_count = (int)vals["item2_count"];
            item3_id = (int)vals["item3_id"];
            item3_count = (int)vals["item3_count"];
            item4_id = (int)vals["item4_id"];
            item4_count = (int)vals["item4_count"];
            daily_exchange = (int)vals["daily_exchange"];
            daily_exchange_everyday = (bool)vals["daily_exchange_everyday"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class ExchangeShopCategoryJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public int categoryid { get; set; } 
        
        [JsonProperty("2")]
        public int sequence { get; set; } 
        
        [JsonProperty("3")]
        public string name { get; set; } 
        
        [JsonProperty("4")]
        public string localizedname { get; set; } 
        
        [JsonProperty("5")]
        public bool show { get; set; } 
        
        [JsonProperty("6")]
        public string opentime { get; set; } 
        
        [JsonProperty("7")]
        public string closetime { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            categoryid = (int)vals["categoryid"];
            sequence = (int)vals["sequence"];
            name = (string)vals["name"];
            localizedname = (string)vals["localizedname"];
            show = (bool)vals["show"];
            opentime = (string)vals["opentime"];
            closetime = (string)vals["closetime"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class CraftingJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public CraftingType categorytype { get; set; } 
        
        [JsonProperty("2")]
        public int ordering { get; set; } 
        
        [JsonProperty("3")]
        public int crafteditemid { get; set; } 
        
        [JsonProperty("4")]
        public int craftedcount { get; set; } 
        
        [JsonProperty("5")]
        public string itemid { get; set; } 
        
        [JsonProperty("6")]
        public string itemcount { get; set; } 
        
        [JsonProperty("7")]
        public int cost { get; set; } 
        
        //type CraftingCategoryJson
        [JsonProperty("8")]
        public int subcategoryname { get; set; } 
        
        [JsonProperty("9")]
        public string localizedname { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            categorytype = (CraftingType)vals["categorytype"];
            ordering = (int)vals["ordering"];
            crafteditemid = (int)vals["crafteditemid"];
            craftedcount = (int)vals["craftedcount"];
            itemid = (string)vals["itemid"];
            itemcount = (string)vals["itemcount"];
            cost = (int)vals["cost"];
            subcategoryname = (int)vals["subcategoryname"];
            localizedname = (string)vals["localizedname"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class CraftingCategoryJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public string subcategoryname { get; set; } 
        
        [JsonProperty("2")]
        public string localizedname { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            subcategoryname = (string)vals["subcategoryname"];
            localizedname = (string)vals["localizedname"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class JobsectJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public string name { get; set; } 
        
        [JsonProperty("2")]
        public string localizedname { get; set; } 
        
        [JsonProperty("3")]
        public JobType jobtype { get; set; } 
        
        [AssetData("mesh")]
        [JsonProperty("4")]
        public string malemeshpath { get; set; } 
        
        [AssetData("material")]
        [JsonProperty("5")]
        public string malematerialpath { get; set; } 
        
        [AssetData("mesh")]
        [JsonProperty("6")]
        public string femalemeshpath { get; set; } 
        
        [AssetData("material")]
        [JsonProperty("7")]
        public string femalematerialpath { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            name = (string)vals["name"];
            localizedname = (string)vals["localizedname"];
            jobtype = (JobType)vals["jobtype"];
            malemeshpath = (string)vals["malemeshpath"];
            malematerialpath = (string)vals["malematerialpath"];
            femalemeshpath = (string)vals["femalemeshpath"];
            femalematerialpath = (string)vals["femalematerialpath"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class GenderInfoJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public Gender gender { get; set; } 
        
        [AssetData("prefab")]
        [JsonProperty("2")]
        public string modelpath { get; set; } 
        
        [JsonProperty("3")]
        public string cameraposintalk { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            gender = (Gender)vals["gender"];
            modelpath = (string)vals["modelpath"];
            cameraposintalk = (string)vals["cameraposintalk"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class SurnameJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public string surname { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            surname = (string)vals["surname"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class MaleNameJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public string name { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            name = (string)vals["name"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class FemaleNameJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public string name { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            name = (string)vals["name"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class PortraitJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public int pid { get; set; } 
        
        [JsonProperty("2")]
        public CharPortraitType pclass { get; set; } 
        
        [JsonProperty("3")]
        public string path { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            pid = (int)vals["pid"];
            pclass = (CharPortraitType)vals["pclass"];
            path = (string)vals["path"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class LevelUpExpJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public int lvid { get; set; } 
        
        [JsonProperty("2")]
        public int expreq { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            lvid = (int)vals["lvid"];
            expreq = (int)vals["expreq"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class StatsJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public int lvid { get; set; } 
        
        [JsonProperty("2")]
        public int statspoint { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            lvid = (int)vals["lvid"];
            statspoint = (int)vals["statspoint"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class SkillPointJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public int minlvl { get; set; } 
        
        [JsonProperty("2")]
        public int maxlvl { get; set; } 
        
        [JsonProperty("3")]
        public int expreq { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            minlvl = (int)vals["minlvl"];
            maxlvl = (int)vals["maxlvl"];
            expreq = (int)vals["expreq"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class ExpMonsterLvDifferenceJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public int minlvl { get; set; } 
        
        [JsonProperty("2")]
        public bool minlvlequal { get; set; } 
        
        [JsonProperty("3")]
        public int maxlvl { get; set; } 
        
        [JsonProperty("4")]
        public bool maxlvlequal { get; set; } 
        
        [JsonProperty("5")]
        public int exppercent { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            minlvl = (int)vals["minlvl"];
            minlvlequal = (bool)vals["minlvlequal"];
            maxlvl = (int)vals["maxlvl"];
            maxlvlequal = (bool)vals["maxlvlequal"];
            exppercent = (int)vals["exppercent"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class JobCombatStatsJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public JobType jobsect { get; set; } 
        
        [JsonProperty("2")]
        public float hp { get; set; } 
        
        [JsonProperty("3")]
        public float attack { get; set; } 
        
        [JsonProperty("4")]
        public float defense { get; set; } 
        
        [JsonProperty("5")]
        public float critical { get; set; } 
        
        [JsonProperty("6")]
        public float cocritical { get; set; } 
        
        [JsonProperty("7")]
        public float criticaldamage { get; set; } 
        
        [JsonProperty("8")]
        public float cocriticaldamage { get; set; } 
        
        [JsonProperty("9")]
        public float evasion { get; set; } 
        
        [JsonProperty("10")]
        public float accuracy { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            jobsect = (JobType)vals["jobsect"];
            hp = Convert.ToSingle((double)vals["hp"]);
            attack = Convert.ToSingle((double)vals["attack"]);
            defense = Convert.ToSingle((double)vals["defense"]);
            critical = Convert.ToSingle((double)vals["critical"]);
            cocritical = Convert.ToSingle((double)vals["cocritical"]);
            criticaldamage = Convert.ToSingle((double)vals["criticaldamage"]);
            cocriticaldamage = Convert.ToSingle((double)vals["cocriticaldamage"]);
            evasion = Convert.ToSingle((double)vals["evasion"]);
            accuracy = Convert.ToSingle((double)vals["accuracy"]);
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class ExpRewardJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public int playerlv { get; set; } 
        
        [JsonProperty("2")]
        public int expreward01 { get; set; } 
        
        [JsonProperty("3")]
        public int expreward02 { get; set; } 
        
        [JsonProperty("4")]
        public int expreward04 { get; set; } 
        
        [JsonProperty("5")]
        public int expreward06 { get; set; } 
        
        [JsonProperty("6")]
        public int expreward08 { get; set; } 
        
        [JsonProperty("7")]
        public int expreward12 { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            playerlv = (int)vals["playerlv"];
            expreward01 = (int)vals["expreward01"];
            expreward02 = (int)vals["expreward02"];
            expreward04 = (int)vals["expreward04"];
            expreward06 = (int)vals["expreward06"];
            expreward08 = (int)vals["expreward08"];
            expreward12 = (int)vals["expreward12"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class LevelJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public string excelname { get; set; } 
        
        [JsonProperty("2")]
        public string localizedname { get; set; } 
        
        [JsonProperty("3")]
        public string unityscene { get; set; } 
        
        [AssetData("sprite")]
        [JsonProperty("4")]
        public string maptexture { get; set; } 
        
        [JsonProperty("5")]
        public string banitemtype { get; set; } 
        
        [JsonProperty("6")]
        public string banitemid { get; set; } 
        
        [AssetData("prefab")]
        [JsonProperty("7")]
        public string effectpath { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            excelname = (string)vals["excelname"];
            localizedname = (string)vals["localizedname"];
            unityscene = (string)vals["unityscene"];
            maptexture = (string)vals["maptexture"];
            banitemtype = (string)vals["banitemtype"];
            banitemid = (string)vals["banitemid"];
            effectpath = (string)vals["effectpath"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class MapCategoryJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public string categoryname { get; set; } 
        
        [JsonProperty("2")]
        public MapType maptype { get; set; } 
        
        [JsonProperty("3")]
        public string localizedname { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            categoryname = (string)vals["categoryname"];
            maptype = (MapType)vals["maptype"];
            localizedname = (string)vals["localizedname"];
        }
    }
    
    public class RealmJson
    {
        public virtual int id { get; set; } 
        
        public virtual RealmType type { get { return RealmType.RealmWorld; } } 
        
        public virtual string excelname { get; set; } 
        
        public virtual string localizedname { get; set; } 
        
        //type LevelJson
        public virtual int level { get; set; } 
        
        public virtual int reqlvl { get; set; } 
        
        public virtual LevelPVPType pvptype { get; set; } 
        
        public virtual int preparation { get; set; } 
        
        public virtual int timelimit { get; set; } 
        
        public virtual RespawnType respawntype { get; set; } 
        
        public virtual int respawncd { get; set; } 
        
        public virtual int minplayer { get; set; } 
        
        public virtual int maxplayer { get; set; } 
        
        public virtual MapType maptype { get; set; } 
        
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class RealmWorldJson : RealmJson
    {
        [JsonProperty("0")]
        public override int id { get; set; } 
        
        [JsonProperty("1")]
        public override string excelname { get; set; } 
        
        [JsonProperty("2")]
        public override string localizedname { get; set; } 
        
        //type LevelJson
        [JsonProperty("3")]
        public override int level { get; set; } 
        
        [JsonProperty("4")]
        public override int reqlvl { get; set; } 
        
        [JsonProperty("5")]
        public override LevelPVPType pvptype { get; set; } 
        
        [JsonProperty("6")]
        public override int preparation { get; set; } 
        
        [JsonProperty("7")]
        public override int timelimit { get; set; } 
        
        [JsonProperty("8")]
        public override RespawnType respawntype { get; set; } 
        
        [JsonProperty("9")]
        public override int respawncd { get; set; } 
        
        [JsonProperty("10")]
        public override int minplayer { get; set; } 
        
        [JsonProperty("11")]
        public override int maxplayer { get; set; } 
        
        [JsonProperty("12")]
        public override MapType maptype { get; set; } 
        
        public override RealmType type { get { return RealmType.RealmWorld; } } 
        
        [JsonProperty("13")]
        public int worldseq { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            excelname = (string)vals["excelname"];
            localizedname = (string)vals["localizedname"];
            level = (int)vals["level"];
            reqlvl = (int)vals["reqlvl"];
            pvptype = (LevelPVPType)vals["pvptype"];
            preparation = (int)vals["preparation"];
            timelimit = (int)vals["timelimit"];
            respawntype = (RespawnType)vals["respawntype"];
            respawncd = (int)vals["respawncd"];
            minplayer = (int)vals["minplayer"];
            maxplayer = (int)vals["maxplayer"];
            maptype = (MapType)vals["maptype"];
            worldseq = (int)vals["worldseq"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class DungeonStoryJson : RealmJson
    {
        [JsonProperty("0")]
        public override int id { get; set; } 
        
        [JsonProperty("1")]
        public override string excelname { get; set; } 
        
        [JsonProperty("2")]
        public override string localizedname { get; set; } 
        
        //type LevelJson
        [JsonProperty("3")]
        public override int level { get; set; } 
        
        [JsonProperty("4")]
        public override int reqlvl { get; set; } 
        
        [JsonProperty("5")]
        public override LevelPVPType pvptype { get; set; } 
        
        [JsonProperty("6")]
        public override int preparation { get; set; } 
        
        [JsonProperty("7")]
        public override int timelimit { get; set; } 
        
        [JsonProperty("8")]
        public override RespawnType respawntype { get; set; } 
        
        [JsonProperty("9")]
        public override int respawncd { get; set; } 
        
        [JsonProperty("10")]
        public override int minplayer { get; set; } 
        
        [JsonProperty("11")]
        public override int maxplayer { get; set; } 
        
        [JsonProperty("12")]
        public override MapType maptype { get; set; } 
        
        public override RealmType type { get { return RealmType.DungeonStory; } } 
        
        [JsonProperty("13")]
        public int sequence { get; set; } 
        
        [AssetData("sprite")]
        [JsonProperty("14")]
        public string iconpath { get; set; } 
        
        [JsonProperty("15")]
        public DungeonDifficulty difficulty { get; set; } 
        
        [JsonProperty("16")]
        public int dailyentry { get; set; } 
        
        //type RewardGroupJson
        [JsonProperty("17")]
        public int displaygrp { get; set; } 
        
        //type RewardGroupJson
        [JsonProperty("18")]
        public int rewardgrp { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            excelname = (string)vals["excelname"];
            localizedname = (string)vals["localizedname"];
            level = (int)vals["level"];
            reqlvl = (int)vals["reqlvl"];
            pvptype = (LevelPVPType)vals["pvptype"];
            preparation = (int)vals["preparation"];
            timelimit = (int)vals["timelimit"];
            respawntype = (RespawnType)vals["respawntype"];
            respawncd = (int)vals["respawncd"];
            minplayer = (int)vals["minplayer"];
            maxplayer = (int)vals["maxplayer"];
            maptype = (MapType)vals["maptype"];
            sequence = (int)vals["sequence"];
            iconpath = (string)vals["iconpath"];
            difficulty = (DungeonDifficulty)vals["difficulty"];
            dailyentry = (int)vals["dailyentry"];
            displaygrp = (int)vals["displaygrp"];
            rewardgrp = (int)vals["rewardgrp"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class DungeonDailySpecialJson : RealmJson
    {
        [JsonProperty("0")]
        public override int id { get; set; } 
        
        [JsonProperty("1")]
        public override string excelname { get; set; } 
        
        [JsonProperty("2")]
        public override string localizedname { get; set; } 
        
        //type LevelJson
        [JsonProperty("3")]
        public override int level { get; set; } 
        
        [JsonProperty("4")]
        public override int reqlvl { get; set; } 
        
        [JsonProperty("5")]
        public override LevelPVPType pvptype { get; set; } 
        
        [JsonProperty("6")]
        public override int preparation { get; set; } 
        
        [JsonProperty("7")]
        public override int timelimit { get; set; } 
        
        [JsonProperty("8")]
        public override RespawnType respawntype { get; set; } 
        
        [JsonProperty("9")]
        public override int respawncd { get; set; } 
        
        [JsonProperty("10")]
        public override int minplayer { get; set; } 
        
        [JsonProperty("11")]
        public override int maxplayer { get; set; } 
        
        [JsonProperty("12")]
        public override MapType maptype { get; set; } 
        
        public override RealmType type { get { return RealmType.DungeonDailySpecial; } } 
        
        [JsonProperty("13")]
        public DungeonType dungeontype { get; set; } 
        
        [JsonProperty("14")]
        public int sequence { get; set; } 
        
        [AssetData("sprite")]
        [JsonProperty("15")]
        public string iconpath { get; set; } 
        
        [JsonProperty("16")]
        public int dailyentry { get; set; } 
        
        //type RewardGroupJson
        [JsonProperty("17")]
        public int displaygrp { get; set; } 
        
        //type RewardGroupJson
        [JsonProperty("18")]
        public int rewardgrp { get; set; } 
        
        [JsonProperty("19")]
        public bool isopenday1 { get; set; } 
        
        [JsonProperty("20")]
        public bool isopenday2 { get; set; } 
        
        [JsonProperty("21")]
        public bool isopenday3 { get; set; } 
        
        [JsonProperty("22")]
        public bool isopenday4 { get; set; } 
        
        [JsonProperty("23")]
        public bool isopenday5 { get; set; } 
        
        [JsonProperty("24")]
        public bool isopenday6 { get; set; } 
        
        [JsonProperty("25")]
        public bool isopenday7 { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            excelname = (string)vals["excelname"];
            localizedname = (string)vals["localizedname"];
            level = (int)vals["level"];
            reqlvl = (int)vals["reqlvl"];
            pvptype = (LevelPVPType)vals["pvptype"];
            preparation = (int)vals["preparation"];
            timelimit = (int)vals["timelimit"];
            respawntype = (RespawnType)vals["respawntype"];
            respawncd = (int)vals["respawncd"];
            minplayer = (int)vals["minplayer"];
            maxplayer = (int)vals["maxplayer"];
            maptype = (MapType)vals["maptype"];
            dungeontype = (DungeonType)vals["dungeontype"];
            sequence = (int)vals["sequence"];
            iconpath = (string)vals["iconpath"];
            dailyentry = (int)vals["dailyentry"];
            displaygrp = (int)vals["displaygrp"];
            rewardgrp = (int)vals["rewardgrp"];
            isopenday1 = (bool)vals["isopenday1"];
            isopenday2 = (bool)vals["isopenday2"];
            isopenday3 = (bool)vals["isopenday3"];
            isopenday4 = (bool)vals["isopenday4"];
            isopenday5 = (bool)vals["isopenday5"];
            isopenday6 = (bool)vals["isopenday6"];
            isopenday7 = (bool)vals["isopenday7"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class RealmObjectiveJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public string excelname { get; set; } 
        
        [JsonProperty("2")]
        public string localizedname { get; set; } 
        
        [JsonProperty("3")]
        public string localizeddescription { get; set; } 
        
        [JsonProperty("4")]
        public RealmObjectiveType realmobjectivetype { get; set; } 
        
        [JsonProperty("5")]
        public int timelimit { get; set; } 
        
        //type CombatNPCJson
        [JsonProperty("6")]
        public int monsterid { get; set; } 
        
        [JsonProperty("7")]
        public int monsterkillcnt { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            excelname = (string)vals["excelname"];
            localizedname = (string)vals["localizedname"];
            localizeddescription = (string)vals["localizeddescription"];
            realmobjectivetype = (RealmObjectiveType)vals["realmobjectivetype"];
            timelimit = (int)vals["timelimit"];
            monsterid = (int)vals["monsterid"];
            monsterkillcnt = (int)vals["monsterkillcnt"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class WordFilterJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public string word { get; set; } 
        
        [JsonProperty("2")]
        public DirtyWordType dirtywordtype { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            word = (string)vals["word"];
            dirtywordtype = (DirtyWordType)vals["dirtywordtype"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class RewardListJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public int rewardgroupid { get; set; } 
        
        [JsonProperty("2")]
        public bool isexpgrade { get; set; } 
        
        [JsonProperty("3")]
        public int job { get; set; } 
        
        [JsonProperty("4")]
        public int experience { get; set; } 
        
        [JsonProperty("5")]
        public int jobexperience { get; set; } 
        
        [JsonProperty("6")]
        public int money { get; set; } 
        
        [JsonProperty("7")]
        public int donatept { get; set; } 
        
        [JsonProperty("8")]
        public int guildactivept { get; set; } 
        
        [JsonProperty("9")]
        public int itemid1 { get; set; } 
        
        [JsonProperty("10")]
        public int itemcount1 { get; set; } 
        
        [JsonProperty("11")]
        public int itemid2 { get; set; } 
        
        [JsonProperty("12")]
        public int itemcount2 { get; set; } 
        
        [JsonProperty("13")]
        public int itemid3 { get; set; } 
        
        [JsonProperty("14")]
        public int itemcount3 { get; set; } 
        
        [JsonProperty("15")]
        public int itemid4 { get; set; } 
        
        [JsonProperty("16")]
        public int itemcount4 { get; set; } 
        
        [JsonProperty("17")]
        public int itemid5 { get; set; } 
        
        [JsonProperty("18")]
        public int itemcount5 { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            rewardgroupid = (int)vals["rewardgroupid"];
            isexpgrade = (bool)vals["isexpgrade"];
            job = (int)vals["job"];
            experience = (int)vals["experience"];
            jobexperience = (int)vals["jobexperience"];
            money = (int)vals["money"];
            donatept = (int)vals["donatept"];
            guildactivept = (int)vals["guildactivept"];
            itemid1 = (int)vals["itemid1"];
            itemcount1 = (int)vals["itemcount1"];
            itemid2 = (int)vals["itemid2"];
            itemcount2 = (int)vals["itemcount2"];
            itemid3 = (int)vals["itemid3"];
            itemcount3 = (int)vals["itemcount3"];
            itemid4 = (int)vals["itemid4"];
            itemcount4 = (int)vals["itemcount4"];
            itemid5 = (int)vals["itemid5"];
            itemcount5 = (int)vals["itemcount5"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class ExperienceRateJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public int level { get; set; } 
        
        [JsonProperty("2")]
        public float exprate { get; set; } 
        
        [JsonProperty("3")]
        public float jexprate { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            level = (int)vals["level"];
            exprate = Convert.ToSingle((double)vals["exprate"]);
            jexprate = Convert.ToSingle((double)vals["jexprate"]);
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class ActivityRewardJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public int activitygroupid { get; set; } 
        
        [JsonProperty("2")]
        public int score { get; set; } 
        
        [JsonProperty("3")]
        public string rewardgroupid { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            activitygroupid = (int)vals["activitygroupid"];
            score = (int)vals["score"];
            rewardgroupid = (string)vals["rewardgroupid"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class RewardGroupJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public string name { get; set; } 
        
        [JsonProperty("2")]
        public string rewardids { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            name = (string)vals["name"];
            rewardids = (string)vals["rewardids"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class RandomBoxRewardJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public int boxcontinued { get; set; } 
        
        [JsonProperty("2")]
        public int boxtimemin { get; set; } 
        
        [JsonProperty("3")]
        public int boxtimemax { get; set; } 
        
        [JsonProperty("4")]
        public int rewardgroupid { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            boxcontinued = (int)vals["boxcontinued"];
            boxtimemin = (int)vals["boxtimemin"];
            boxtimemax = (int)vals["boxtimemax"];
            rewardgroupid = (int)vals["rewardgroupid"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class PrizeGuaranteeJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public string name { get; set; } 
        
        [JsonProperty("2")]
        public bool reset { get; set; } 
        
        [JsonProperty("3")]
        public PrizeGuaranteeType type { get; set; } 
        
        [JsonProperty("4")]
        public int itemid { get; set; } 
        
        //type ItemBaseJson
        [JsonProperty("5")]
        public int prizeid1 { get; set; } 
        
        [JsonProperty("6")]
        public int prizecount1 { get; set; } 
        
        [JsonProperty("7")]
        public int threshold1 { get; set; } 
        
        //type ItemBaseJson
        [JsonProperty("8")]
        public int prizeid2 { get; set; } 
        
        [JsonProperty("9")]
        public int prizecount2 { get; set; } 
        
        [JsonProperty("10")]
        public int threshold2 { get; set; } 
        
        //type ItemBaseJson
        [JsonProperty("11")]
        public int prizeid3 { get; set; } 
        
        [JsonProperty("12")]
        public int prizecount3 { get; set; } 
        
        [JsonProperty("13")]
        public int threshold3 { get; set; } 
        
        //type ItemBaseJson
        [JsonProperty("14")]
        public int prizeid4 { get; set; } 
        
        [JsonProperty("15")]
        public int prizecount4 { get; set; } 
        
        [JsonProperty("16")]
        public int threshold4 { get; set; } 
        
        //type ItemBaseJson
        [JsonProperty("17")]
        public int prizeid5 { get; set; } 
        
        [JsonProperty("18")]
        public int prizecount5 { get; set; } 
        
        [JsonProperty("19")]
        public int threshold5 { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            name = (string)vals["name"];
            reset = (bool)vals["reset"];
            type = (PrizeGuaranteeType)vals["type"];
            itemid = (int)vals["itemid"];
            prizeid1 = (int)vals["prizeid1"];
            prizecount1 = (int)vals["prizecount1"];
            threshold1 = (int)vals["threshold1"];
            prizeid2 = (int)vals["prizeid2"];
            prizecount2 = (int)vals["prizecount2"];
            threshold2 = (int)vals["threshold2"];
            prizeid3 = (int)vals["prizeid3"];
            prizecount3 = (int)vals["prizecount3"];
            threshold3 = (int)vals["threshold3"];
            prizeid4 = (int)vals["prizeid4"];
            prizecount4 = (int)vals["prizecount4"];
            threshold4 = (int)vals["threshold4"];
            prizeid5 = (int)vals["prizeid5"];
            prizecount5 = (int)vals["prizecount5"];
            threshold5 = (int)vals["threshold5"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class DonateRewardIdJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        //type RewardListJson
        [JsonProperty("1")]
        public int rewardlistid { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            rewardlistid = (int)vals["rewardlistid"];
        }
    }
    
    public class NPCJson
    {
        public virtual int id { get; set; } 
        
        public virtual NPCType npctype { get { return NPCType.Combat; } } 
        
        public virtual string archetype { get; set; } 
        
        public virtual string localizedname { get; set; } 
        
        public virtual string containerprefabpath { get; set; } 
        
        public virtual string modelprefabpath { get; set; } 
        
        public virtual string modelmaterialpath { get; set; } 
        
        public virtual float modelscalex { get; set; } 
        
        public virtual float modelscaley { get; set; } 
        
        public virtual float modelscalez { get; set; } 
        
        public virtual string speechbubbletext { get; set; } 
        
        public virtual string speechbubblebox { get; set; } 
        
        public virtual float speechbubbleheight { get; set; } 
        
        public virtual float speechbubbleduration { get; set; } 
        
        public virtual float speechbubbleminint { get; set; } 
        
        public virtual float speechbubblemaxint { get; set; } 
        
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class CombatNPCJson : NPCJson
    {
        [JsonProperty("0")]
        public override int id { get; set; } 
        
        [JsonProperty("1")]
        public override string archetype { get; set; } 
        
        [JsonProperty("2")]
        public override string localizedname { get; set; } 
        
        [AssetData("prefab")]
        [JsonProperty("3")]
        public override string containerprefabpath { get; set; } 
        
        [AssetData("prefab")]
        [JsonProperty("4")]
        public override string modelprefabpath { get; set; } 
        
        [JsonProperty("5")]
        public override string modelmaterialpath { get; set; } 
        
        [JsonProperty("6")]
        public override float modelscalex { get; set; } 
        
        [JsonProperty("7")]
        public override float modelscaley { get; set; } 
        
        [JsonProperty("8")]
        public override float modelscalez { get; set; } 
        
        [JsonProperty("9")]
        public override string speechbubbletext { get; set; } 
        
        [JsonProperty("10")]
        public override string speechbubblebox { get; set; } 
        
        [JsonProperty("11")]
        public override float speechbubbleheight { get; set; } 
        
        [JsonProperty("12")]
        public override float speechbubbleduration { get; set; } 
        
        [JsonProperty("13")]
        public override float speechbubbleminint { get; set; } 
        
        [JsonProperty("14")]
        public override float speechbubblemaxint { get; set; } 
        
        public override NPCType npctype { get { return NPCType.Combat; } } 
        
        [JsonProperty("15")]
        public MonsterClass monsterclass { get; set; } 
        
        [JsonProperty("16")]
        public Race race { get; set; } 
        
        [JsonProperty("17")]
        public Element element { get; set; } 
        
        [JsonProperty("18")]
        public AttackStyle weakness { get; set; } 
        
        [JsonProperty("19")]
        public float movespeed { get; set; } 
        
        [JsonProperty("20")]
        public float attackspeed { get; set; } 
        
        //type SkillJson
        [JsonProperty("21")]
        public int basicattack { get; set; } 
        
        //type SkillJson
        [JsonProperty("22")]
        public int basicattack2 { get; set; } 
        
        [JsonProperty("23")]
        public int camp { get; set; } 
        
        [JsonProperty("24")]
        public bool broadcast { get; set; } 
        
        [JsonProperty("25")]
        public int level { get; set; } 
        
        [JsonProperty("26")]
        public int healthmax { get; set; } 
        
        [JsonProperty("27")]
        public int attack { get; set; } 
        
        [JsonProperty("28")]
        public int armor { get; set; } 
        
        [JsonProperty("29")]
        public int strength { get; set; } 
        
        [JsonProperty("30")]
        public int agility { get; set; } 
        
        [JsonProperty("31")]
        public int dexterity { get; set; } 
        
        [JsonProperty("32")]
        public int constitution { get; set; } 
        
        [JsonProperty("33")]
        public int intelligence { get; set; } 
        
        [JsonProperty("34")]
        public int accuracy { get; set; } 
        
        [JsonProperty("35")]
        public int evasion { get; set; } 
        
        [JsonProperty("36")]
        public int critical { get; set; } 
        
        [JsonProperty("37")]
        public int criticaldamage { get; set; } 
        
        [JsonProperty("38")]
        public int cocritical { get; set; } 
        
        [JsonProperty("39")]
        public int cocriticaldamage { get; set; } 
        
        [JsonProperty("40")]
        public string immunities { get; set; } 
        
        [JsonProperty("41")]
        public bool dmgbyhitcount { get; set; } 
        
        [JsonProperty("42")]
        public bool recoveronreturn { get; set; } 
        
        [JsonProperty("43")]
        public int hpregenamt { get; set; } 
        
        [JsonProperty("44")]
        public int hpregenamtbypercent { get; set; } 
        
        [JsonProperty("45")]
        public float healthregeninterval { get; set; } 
        
        [JsonProperty("46")]
        public int experience { get; set; } 
        
        [JsonProperty("47")]
        public string loot { get; set; } 
        
        [JsonProperty("48")]
        public string eventloot { get; set; } 
        
        //type BossAIJson
        [JsonProperty("49")]
        public int bossai1 { get; set; } 
        
        //type BossAIJson
        [JsonProperty("50")]
        public int bossai2 { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            archetype = (string)vals["archetype"];
            localizedname = (string)vals["localizedname"];
            containerprefabpath = (string)vals["containerprefabpath"];
            modelprefabpath = (string)vals["modelprefabpath"];
            modelmaterialpath = (string)vals["modelmaterialpath"];
            modelscalex = Convert.ToSingle((double)vals["modelscalex"]);
            modelscaley = Convert.ToSingle((double)vals["modelscaley"]);
            modelscalez = Convert.ToSingle((double)vals["modelscalez"]);
            speechbubbletext = (string)vals["speechbubbletext"];
            speechbubblebox = (string)vals["speechbubblebox"];
            speechbubbleheight = Convert.ToSingle((double)vals["speechbubbleheight"]);
            speechbubbleduration = Convert.ToSingle((double)vals["speechbubbleduration"]);
            speechbubbleminint = Convert.ToSingle((double)vals["speechbubbleminint"]);
            speechbubblemaxint = Convert.ToSingle((double)vals["speechbubblemaxint"]);
            monsterclass = (MonsterClass)vals["monsterclass"];
            race = (Race)vals["race"];
            element = (Element)vals["element"];
            weakness = (AttackStyle)vals["weakness"];
            movespeed = Convert.ToSingle((double)vals["movespeed"]);
            attackspeed = Convert.ToSingle((double)vals["attackspeed"]);
            basicattack = (int)vals["basicattack"];
            basicattack2 = (int)vals["basicattack2"];
            camp = (int)vals["camp"];
            broadcast = (bool)vals["broadcast"];
            level = (int)vals["level"];
            healthmax = (int)vals["healthmax"];
            attack = (int)vals["attack"];
            armor = (int)vals["armor"];
            strength = (int)vals["strength"];
            agility = (int)vals["agility"];
            dexterity = (int)vals["dexterity"];
            constitution = (int)vals["constitution"];
            intelligence = (int)vals["intelligence"];
            accuracy = (int)vals["accuracy"];
            evasion = (int)vals["evasion"];
            critical = (int)vals["critical"];
            criticaldamage = (int)vals["criticaldamage"];
            cocritical = (int)vals["cocritical"];
            cocriticaldamage = (int)vals["cocriticaldamage"];
            immunities = (string)vals["immunities"];
            dmgbyhitcount = (bool)vals["dmgbyhitcount"];
            recoveronreturn = (bool)vals["recoveronreturn"];
            hpregenamt = (int)vals["hpregenamt"];
            hpregenamtbypercent = (int)vals["hpregenamtbypercent"];
            healthregeninterval = Convert.ToSingle((double)vals["healthregeninterval"]);
            experience = (int)vals["experience"];
            loot = (string)vals["loot"];
            eventloot = (string)vals["eventloot"];
            bossai1 = (int)vals["bossai1"];
            bossai2 = (int)vals["bossai2"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class StaticNPCJson : NPCJson
    {
        [JsonProperty("0")]
        public override int id { get; set; } 
        
        [JsonProperty("1")]
        public override string archetype { get; set; } 
        
        [JsonProperty("2")]
        public override string localizedname { get; set; } 
        
        [AssetData("prefab")]
        [JsonProperty("3")]
        public override string containerprefabpath { get; set; } 
        
        [AssetData("prefab")]
        [JsonProperty("4")]
        public override string modelprefabpath { get; set; } 
        
        [JsonProperty("5")]
        public override string modelmaterialpath { get; set; } 
        
        [JsonProperty("6")]
        public override float modelscalex { get; set; } 
        
        [JsonProperty("7")]
        public override float modelscaley { get; set; } 
        
        [JsonProperty("8")]
        public override float modelscalez { get; set; } 
        
        [JsonProperty("9")]
        public override string speechbubbletext { get; set; } 
        
        [JsonProperty("10")]
        public override string speechbubblebox { get; set; } 
        
        [JsonProperty("11")]
        public override float speechbubbleheight { get; set; } 
        
        [JsonProperty("12")]
        public override float speechbubbleduration { get; set; } 
        
        [JsonProperty("13")]
        public override float speechbubbleminint { get; set; } 
        
        [JsonProperty("14")]
        public override float speechbubblemaxint { get; set; } 
        
        public override NPCType npctype { get { return NPCType.Static; } } 
        
        [JsonProperty("15")]
        public string cameraposintalk { get; set; } 
        
        [JsonProperty("16")]
        public string talktext { get; set; } 
        
        [JsonProperty("17")]
        public string talktextalt { get; set; } 
        
        [JsonProperty("18")]
        public string talktextbox { get; set; } 
        
        [JsonProperty("19")]
        public string npcfunction { get; set; } 
        
        [JsonProperty("20")]
        public string questid { get; set; } 
        
        [JsonProperty("21")]
        public bool activeonstartup { get; set; } 
        
        [JsonProperty("22")]
        public string npccycletime { get; set; } 
        
        [JsonProperty("23")]
        public string npcopentime { get; set; } 
        
        [JsonProperty("24")]
        public string npcclosetime { get; set; } 
        
        [JsonProperty("25")]
        public StaticNPCInteractType interacttype { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            archetype = (string)vals["archetype"];
            localizedname = (string)vals["localizedname"];
            containerprefabpath = (string)vals["containerprefabpath"];
            modelprefabpath = (string)vals["modelprefabpath"];
            modelmaterialpath = (string)vals["modelmaterialpath"];
            modelscalex = Convert.ToSingle((double)vals["modelscalex"]);
            modelscaley = Convert.ToSingle((double)vals["modelscaley"]);
            modelscalez = Convert.ToSingle((double)vals["modelscalez"]);
            speechbubbletext = (string)vals["speechbubbletext"];
            speechbubblebox = (string)vals["speechbubblebox"];
            speechbubbleheight = Convert.ToSingle((double)vals["speechbubbleheight"]);
            speechbubbleduration = Convert.ToSingle((double)vals["speechbubbleduration"]);
            speechbubbleminint = Convert.ToSingle((double)vals["speechbubbleminint"]);
            speechbubblemaxint = Convert.ToSingle((double)vals["speechbubblemaxint"]);
            cameraposintalk = (string)vals["cameraposintalk"];
            talktext = (string)vals["talktext"];
            talktextalt = (string)vals["talktextalt"];
            talktextbox = (string)vals["talktextbox"];
            npcfunction = (string)vals["npcfunction"];
            questid = (string)vals["questid"];
            activeonstartup = (bool)vals["activeonstartup"];
            npccycletime = (string)vals["npccycletime"];
            npcopentime = (string)vals["npcopentime"];
            npcclosetime = (string)vals["npcclosetime"];
            interacttype = (StaticNPCInteractType)vals["interacttype"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class RealmNPCGroupJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public string name { get; set; } 
        
        //type RealmJson
        [JsonProperty("2")]
        public int realmid { get; set; } 
        
        //type CombatNPCJson
        [JsonProperty("3")]
        public int archetypeid { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            name = (string)vals["name"];
            realmid = (int)vals["realmid"];
            archetypeid = (int)vals["archetypeid"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class BossAIJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public string name { get; set; } 
        
        //type SkillJson
        [JsonProperty("2")]
        public int skillid { get; set; } 
        
        [JsonProperty("3")]
        public AISkillCondition condition { get; set; } 
        
        [JsonProperty("4")]
        public float conditiondata { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            name = (string)vals["name"];
            skillid = (int)vals["skillid"];
            condition = (AISkillCondition)vals["condition"];
            conditiondata = Convert.ToSingle((double)vals["conditiondata"]);
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class SpecialBossJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public string name { get; set; } 
        
        //type CombatNPCJson
        [JsonProperty("2")]
        public int archetypeid { get; set; } 
        
        //type LevelJson
        [JsonProperty("3")]
        public int location { get; set; } 
        
        [JsonProperty("4")]
        public BossCategory category { get; set; } 
        
        [JsonProperty("5")]
        public int sequence { get; set; } 
        
        [JsonProperty("6")]
        public BossSpawnType spawntype { get; set; } 
        
        [JsonProperty("7")]
        public string spawndaily { get; set; } 
        
        [JsonProperty("8")]
        public string spawnweekly { get; set; } 
        
        [JsonProperty("9")]
        public string spawnstart { get; set; } 
        
        [JsonProperty("10")]
        public string spawnend { get; set; } 
        
        [JsonProperty("11")]
        public int respawntime { get; set; } 
        
        [JsonProperty("12")]
        public int displayrewardgroupid { get; set; } 
        
        [AssetData("sprite")]
        [JsonProperty("13")]
        public string icon { get; set; } 
        
        [JsonProperty("14")]
        public float modelscale { get; set; } 
        
        [JsonProperty("15")]
        public float modelposx { get; set; } 
        
        [JsonProperty("16")]
        public float modelposy { get; set; } 
        
        [JsonProperty("17")]
        public float modelrotatey { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            name = (string)vals["name"];
            archetypeid = (int)vals["archetypeid"];
            location = (int)vals["location"];
            category = (BossCategory)vals["category"];
            sequence = (int)vals["sequence"];
            spawntype = (BossSpawnType)vals["spawntype"];
            spawndaily = (string)vals["spawndaily"];
            spawnweekly = (string)vals["spawnweekly"];
            spawnstart = (string)vals["spawnstart"];
            spawnend = (string)vals["spawnend"];
            respawntime = (int)vals["respawntime"];
            displayrewardgroupid = (int)vals["displayrewardgroupid"];
            icon = (string)vals["icon"];
            modelscale = Convert.ToSingle((double)vals["modelscale"]);
            modelposx = Convert.ToSingle((double)vals["modelposx"]);
            modelposy = Convert.ToSingle((double)vals["modelposy"]);
            modelrotatey = Convert.ToSingle((double)vals["modelrotatey"]);
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class LootCorrectionJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public LootCorrectionType type { get; set; } 
        
        [JsonProperty("2")]
        public int normalmonster { get; set; } 
        
        [JsonProperty("3")]
        public int boss { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            type = (LootCorrectionType)vals["type"];
            normalmonster = (int)vals["normalmonster"];
            boss = (int)vals["boss"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class LootItemGroupJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public int gid { get; set; } 
        
        [JsonProperty("2")]
        public int itemid { get; set; } 
        
        [JsonProperty("3")]
        public CurrencyType currency { get; set; } 
        
        [JsonProperty("4")]
        public bool grouptype { get; set; } 
        
        [JsonProperty("5")]
        public int probability { get; set; } 
        
        [JsonProperty("6")]
        public int weight { get; set; } 
        
        [JsonProperty("7")]
        public bool ignorelv { get; set; } 
        
        [JsonProperty("8")]
        public bool ignoretime { get; set; } 
        
        [JsonProperty("9")]
        public string amt { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            gid = (int)vals["gid"];
            itemid = (int)vals["itemid"];
            currency = (CurrencyType)vals["currency"];
            grouptype = (bool)vals["grouptype"];
            probability = (int)vals["probability"];
            weight = (int)vals["weight"];
            ignorelv = (bool)vals["ignorelv"];
            ignoretime = (bool)vals["ignoretime"];
            amt = (string)vals["amt"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class LootLinkJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public LootType loottype { get; set; } 
        
        [JsonProperty("2")]
        public string gids { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            loottype = (LootType)vals["loottype"];
            gids = (string)vals["gids"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class EventLootLinkJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public LootType loottype { get; set; } 
        
        [JsonProperty("2")]
        public string gids { get; set; } 
        
        [JsonProperty("3")]
        public string datestart { get; set; } 
        
        [JsonProperty("4")]
        public string dateend { get; set; } 
        
        [JsonProperty("5")]
        public string days { get; set; } 
        
        [JsonProperty("6")]
        public string weeks { get; set; } 
        
        [JsonProperty("7")]
        public string times { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            loottype = (LootType)vals["loottype"];
            gids = (string)vals["gids"];
            datestart = (string)vals["datestart"];
            dateend = (string)vals["dateend"];
            days = (string)vals["days"];
            weeks = (string)vals["weeks"];
            times = (string)vals["times"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class LimitedItemJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public int itemid { get; set; } 
        
        [JsonProperty("2")]
        public LootSourceType source { get; set; } 
        
        [JsonProperty("3")]
        public int amt { get; set; } 
        
        [JsonProperty("4")]
        public LimitItemCycle cycle { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            itemid = (int)vals["itemid"];
            source = (LootSourceType)vals["source"];
            amt = (int)vals["amt"];
            cycle = (LimitItemCycle)vals["cycle"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class GUILocalizedStringJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public string name { get; set; } 
        
        [JsonProperty("2")]
        public string localizedval { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            name = (string)vals["name"];
            localizedval = (string)vals["localizedval"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class SystemMessageJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public string name { get; set; } 
        
        [JsonProperty("2")]
        public string localizedval { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            name = (string)vals["name"];
            localizedval = (string)vals["localizedval"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class ChapterJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public int groupid { get; set; } 
        
        [JsonProperty("2")]
        public string name { get; set; } 
        
        [JsonProperty("3")]
        public string description { get; set; } 
        
        [JsonProperty("4")]
        public int questid { get; set; } 
        
        [JsonProperty("5")]
        public int sequence { get; set; } 
        
        [JsonProperty("6")]
        public string icon { get; set; } 
        
        [JsonProperty("7")]
        public string background { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            groupid = (int)vals["groupid"];
            name = (string)vals["name"];
            description = (string)vals["description"];
            questid = (int)vals["questid"];
            sequence = (int)vals["sequence"];
            icon = (string)vals["icon"];
            background = (string)vals["background"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class WonderfulJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public string name { get; set; } 
        
        [JsonProperty("2")]
        public string description { get; set; } 
        
        [JsonProperty("3")]
        public string path { get; set; } 
        
        [JsonProperty("4")]
        public string background { get; set; } 
        
        [JsonProperty("5")]
        public string questid { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            name = (string)vals["name"];
            description = (string)vals["description"];
            path = (string)vals["path"];
            background = (string)vals["background"];
            questid = (string)vals["questid"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class QuestJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public int questid { get; set; } 
        
        [JsonProperty("2")]
        public string questname { get; set; } 
        
        [JsonProperty("3")]
        public string subname { get; set; } 
        
        [JsonProperty("4")]
        public string description { get; set; } 
        
        [JsonProperty("5")]
        public int starttalkid { get; set; } 
        
        [JsonProperty("6")]
        public QuestType type { get; set; } 
        
        [JsonProperty("7")]
        public bool promptaccept { get; set; } 
        
        [JsonProperty("8")]
        public bool promptobj { get; set; } 
        
        [JsonProperty("9")]
        public int minlv { get; set; } 
        
        [JsonProperty("10")]
        public int requirementid { get; set; } 
        
        [JsonProperty("11")]
        public int frontquest { get; set; } 
        
        [JsonProperty("12")]
        public string nextquest { get; set; } 
        
        [JsonProperty("13")]
        public bool isopen { get; set; } 
        
        [JsonProperty("14")]
        public QuestRepeatType repeat { get; set; } 
        
        [JsonProperty("15")]
        public QuestTriggerType triggertype { get; set; } 
        
        [JsonProperty("16")]
        public int triggercaller { get; set; } 
        
        [JsonProperty("17")]
        public bool replyid { get; set; } 
        
        [JsonProperty("18")]
        public string objectiveid { get; set; } 
        
        [JsonProperty("19")]
        public string objgroup { get; set; } 
        
        [JsonProperty("20")]
        public int eventid { get; set; } 
        
        [JsonProperty("21")]
        public string reward { get; set; } 
        
        //type FeatureListJson
        [JsonProperty("22")]
        public int unlock { get; set; } 
        
        [JsonProperty("23")]
        public string unlockval { get; set; } 
        
        [JsonProperty("24")]
        public bool showae { get; set; } 
        
        [JsonProperty("25")]
        public bool teleport { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            questid = (int)vals["questid"];
            questname = (string)vals["questname"];
            subname = (string)vals["subname"];
            description = (string)vals["description"];
            starttalkid = (int)vals["starttalkid"];
            type = (QuestType)vals["type"];
            promptaccept = (bool)vals["promptaccept"];
            promptobj = (bool)vals["promptobj"];
            minlv = (int)vals["minlv"];
            requirementid = (int)vals["requirementid"];
            frontquest = (int)vals["frontquest"];
            nextquest = (string)vals["nextquest"];
            isopen = (bool)vals["isopen"];
            repeat = (QuestRepeatType)vals["repeat"];
            triggertype = (QuestTriggerType)vals["triggertype"];
            triggercaller = (int)vals["triggercaller"];
            replyid = (bool)vals["replyid"];
            objectiveid = (string)vals["objectiveid"];
            objgroup = (string)vals["objgroup"];
            eventid = (int)vals["eventid"];
            reward = (string)vals["reward"];
            unlock = (int)vals["unlock"];
            unlockval = (string)vals["unlockval"];
            showae = (bool)vals["showae"];
            teleport = (bool)vals["teleport"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class QuestObjectiveJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public int objid { get; set; } 
        
        [JsonProperty("2")]
        public string name { get; set; } 
        
        [JsonProperty("3")]
        public string localizedname { get; set; } 
        
        [JsonProperty("4")]
        public string description { get; set; } 
        
        [JsonProperty("5")]
        public int time { get; set; } 
        
        [JsonProperty("6")]
        public QuestObjectiveType type { get; set; } 
        
        [JsonProperty("7")]
        public int para1 { get; set; } 
        
        [JsonProperty("8")]
        public int para2 { get; set; } 
        
        [JsonProperty("9")]
        public int para3 { get; set; } 
        
        [JsonProperty("10")]
        public string para4 { get; set; } 
        
        [JsonProperty("11")]
        public int eventid { get; set; } 
        
        [JsonProperty("12")]
        public int requirementid { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            objid = (int)vals["objid"];
            name = (string)vals["name"];
            localizedname = (string)vals["localizedname"];
            description = (string)vals["description"];
            time = (int)vals["time"];
            type = (QuestObjectiveType)vals["type"];
            para1 = (int)vals["para1"];
            para2 = (int)vals["para2"];
            para3 = (int)vals["para3"];
            para4 = (string)vals["para4"];
            eventid = (int)vals["eventid"];
            requirementid = (int)vals["requirementid"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class QuestTalkDetailJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public int talkid { get; set; } 
        
        [JsonProperty("2")]
        public string name { get; set; } 
        
        [JsonProperty("3")]
        public int steps { get; set; } 
        
        [JsonProperty("4")]
        public int caller1 { get; set; } 
        
        [JsonProperty("5")]
        public string dialogue1 { get; set; } 
        
        [JsonProperty("6")]
        public int caller2 { get; set; } 
        
        [JsonProperty("7")]
        public string dialogue2 { get; set; } 
        
        [JsonProperty("8")]
        public int caller3 { get; set; } 
        
        [JsonProperty("9")]
        public string dialogue3 { get; set; } 
        
        [JsonProperty("10")]
        public int caller4 { get; set; } 
        
        [JsonProperty("11")]
        public string dialogue4 { get; set; } 
        
        [JsonProperty("12")]
        public int caller5 { get; set; } 
        
        [JsonProperty("13")]
        public string dialogue5 { get; set; } 
        
        [JsonProperty("14")]
        public int caller6 { get; set; } 
        
        [JsonProperty("15")]
        public string dialogue6 { get; set; } 
        
        [JsonProperty("16")]
        public int caller7 { get; set; } 
        
        [JsonProperty("17")]
        public string dialogue7 { get; set; } 
        
        [JsonProperty("18")]
        public int selectionid { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            talkid = (int)vals["talkid"];
            name = (string)vals["name"];
            steps = (int)vals["steps"];
            caller1 = (int)vals["caller1"];
            dialogue1 = (string)vals["dialogue1"];
            caller2 = (int)vals["caller2"];
            dialogue2 = (string)vals["dialogue2"];
            caller3 = (int)vals["caller3"];
            dialogue3 = (string)vals["dialogue3"];
            caller4 = (int)vals["caller4"];
            dialogue4 = (string)vals["dialogue4"];
            caller5 = (int)vals["caller5"];
            dialogue5 = (string)vals["dialogue5"];
            caller6 = (int)vals["caller6"];
            dialogue6 = (string)vals["dialogue6"];
            caller7 = (int)vals["caller7"];
            dialogue7 = (string)vals["dialogue7"];
            selectionid = (int)vals["selectionid"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class QuestSelectDetailJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public int groupid { get; set; } 
        
        [JsonProperty("2")]
        public string answer { get; set; } 
        
        //type QuestTalkDetailJson
        [JsonProperty("3")]
        public int questtalkid { get; set; } 
        
        [JsonProperty("4")]
        public bool isanswer { get; set; } 
        
        [JsonProperty("5")]
        public QuestSelectionActionType actiontype { get; set; } 
        
        [JsonProperty("6")]
        public int actionid { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            groupid = (int)vals["groupid"];
            answer = (string)vals["answer"];
            questtalkid = (int)vals["questtalkid"];
            isanswer = (bool)vals["isanswer"];
            actiontype = (QuestSelectionActionType)vals["actiontype"];
            actionid = (int)vals["actionid"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class QuestInteractiveDetailJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public int interactiveid { get; set; } 
        
        [JsonProperty("2")]
        public int interactivetime { get; set; } 
        
        [JsonProperty("3")]
        public int successrate { get; set; } 
        
        [JsonProperty("4")]
        public QuestInteractiveType type { get; set; } 
        
        [JsonProperty("5")]
        public int para { get; set; } 
        
        [JsonProperty("6")]
        public string iconid { get; set; } 
        
        [JsonProperty("7")]
        public string icontext { get; set; } 
        
        [JsonProperty("8")]
        public string emotionid { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            interactiveid = (int)vals["interactiveid"];
            interactivetime = (int)vals["interactivetime"];
            successrate = (int)vals["successrate"];
            type = (QuestInteractiveType)vals["type"];
            para = (int)vals["para"];
            iconid = (string)vals["iconid"];
            icontext = (string)vals["icontext"];
            emotionid = (string)vals["emotionid"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class QuestRequirementDetailJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public int requirementid { get; set; } 
        
        [JsonProperty("2")]
        public int groupid { get; set; } 
        
        [JsonProperty("3")]
        public QuestRequirementType type { get; set; } 
        
        [JsonProperty("4")]
        public int para1 { get; set; } 
        
        [JsonProperty("5")]
        public int para2 { get; set; } 
        
        [JsonProperty("6")]
        public int para3 { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            requirementid = (int)vals["requirementid"];
            groupid = (int)vals["groupid"];
            type = (QuestRequirementType)vals["type"];
            para1 = (int)vals["para1"];
            para2 = (int)vals["para2"];
            para3 = (int)vals["para3"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class QuestEventDetailJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public int groupid { get; set; } 
        
        [JsonProperty("2")]
        public EventTimingType timing { get; set; } 
        
        [JsonProperty("3")]
        public QuestEventType type { get; set; } 
        
        [JsonProperty("4")]
        public string para1 { get; set; } 
        
        [JsonProperty("5")]
        public string para2 { get; set; } 
        
        [JsonProperty("6")]
        public int questid { get; set; } 
        
        [JsonProperty("7")]
        public int objectiveid { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            groupid = (int)vals["groupid"];
            timing = (EventTimingType)vals["timing"];
            type = (QuestEventType)vals["type"];
            para1 = (string)vals["para1"];
            para2 = (string)vals["para2"];
            questid = (int)vals["questid"];
            objectiveid = (int)vals["objectiveid"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class QuestDestinyJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public int destinyid { get; set; } 
        
        [JsonProperty("2")]
        public int groupid { get; set; } 
        
        [JsonProperty("3")]
        public QuestDestinyType type { get; set; } 
        
        [JsonProperty("4")]
        public int sequence { get; set; } 
        
        [JsonProperty("5")]
        public int questid { get; set; } 
        
        [JsonProperty("6")]
        public string name { get; set; } 
        
        [JsonProperty("7")]
        public string path { get; set; } 
        
        [JsonProperty("8")]
        public int posx { get; set; } 
        
        [JsonProperty("9")]
        public int posy { get; set; } 
        
        [JsonProperty("10")]
        public string site { get; set; } 
        
        [JsonProperty("11")]
        public int uicolumn { get; set; } 
        
        [JsonProperty("12")]
        public int uirow { get; set; } 
        
        [JsonProperty("13")]
        public string next { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            destinyid = (int)vals["destinyid"];
            groupid = (int)vals["groupid"];
            type = (QuestDestinyType)vals["type"];
            sequence = (int)vals["sequence"];
            questid = (int)vals["questid"];
            name = (string)vals["name"];
            path = (string)vals["path"];
            posx = (int)vals["posx"];
            posy = (int)vals["posy"];
            site = (string)vals["site"];
            uicolumn = (int)vals["uicolumn"];
            uirow = (int)vals["uirow"];
            next = (string)vals["next"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class SkillGroupJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public string name { get; set; } 
        
        [JsonProperty("2")]
        public string localizedname { get; set; } 
        
        [JsonProperty("3")]
        public string weaponsrequired { get; set; } 
        
        [AssetData("sprite")]
        [JsonProperty("4")]
        public string icon { get; set; } 
        
        [JsonProperty("5")]
        public string action { get; set; } 
        
        [JsonProperty("6")]
        public string rtaction { get; set; } 
        
        [JsonProperty("7")]
        public float basicminrt { get; set; } 
        
        [JsonProperty("8")]
        public string costtype { get; set; } 
        
        [JsonProperty("9")]
        public bool costab { get; set; } 
        
        [JsonProperty("10")]
        public bool lvstacked { get; set; } 
        
        [JsonProperty("11")]
        public SkillClass skillclass { get; set; } 
        
        [JsonProperty("12")]
        public SkillType skilltype { get; set; } 
        
        [JsonProperty("13")]
        public SkillBehaviour skillbehavior { get; set; } 
        
        [JsonProperty("14")]
        public TargetType targettype { get; set; } 
        
        [JsonProperty("15")]
        public HitType hittype { get; set; } 
        
        [JsonProperty("16")]
        public Threatzone threatzone { get; set; } 
        
        [JsonProperty("17")]
        public bool moonwalk { get; set; } 
        
        [JsonProperty("18")]
        public bool canturn { get; set; } 
        
        [JsonProperty("19")]
        public float proctime { get; set; } 
        
        [JsonProperty("20")]
        public bool repeatproc { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            name = (string)vals["name"];
            localizedname = (string)vals["localizedname"];
            weaponsrequired = (string)vals["weaponsrequired"];
            icon = (string)vals["icon"];
            action = (string)vals["action"];
            rtaction = (string)vals["rtaction"];
            basicminrt = Convert.ToSingle((double)vals["basicminrt"]);
            costtype = (string)vals["costtype"];
            costab = (bool)vals["costab"];
            lvstacked = (bool)vals["lvstacked"];
            skillclass = (SkillClass)vals["skillclass"];
            skilltype = (SkillType)vals["skilltype"];
            skillbehavior = (SkillBehaviour)vals["skillbehavior"];
            targettype = (TargetType)vals["targettype"];
            hittype = (HitType)vals["hittype"];
            threatzone = (Threatzone)vals["threatzone"];
            moonwalk = (bool)vals["moonwalk"];
            canturn = (bool)vals["canturn"];
            proctime = Convert.ToSingle((double)vals["proctime"]);
            repeatproc = (bool)vals["repeatproc"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class SkillJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public string name { get; set; } 
        
        //type SkillGroupJson
        [JsonProperty("2")]
        public int skillgroupid { get; set; } 
        
        [JsonProperty("3")]
        public int level { get; set; } 
        
        [JsonProperty("4")]
        public string description { get; set; } 
        
        [JsonProperty("5")]
        public int cost { get; set; } 
        
        [JsonProperty("6")]
        public float cooldown { get; set; } 
        
        [JsonProperty("7")]
        public float globalcd { get; set; } 
        
        [JsonProperty("8")]
        public string actioneffect { get; set; } 
        
        [AssetData("prefab")]
        [JsonProperty("9")]
        public string effectgethit { get; set; } 
        
        [JsonProperty("10")]
        public float offsettime { get; set; } 
        
        [JsonProperty("11")]
        public float skillduration { get; set; } 
        
        [JsonProperty("12")]
        public float effect_dur { get; set; } 
        
        [JsonProperty("13")]
        public float rtfixedduration { get; set; } 
        
        [JsonProperty("14")]
        public float rtvarduration { get; set; } 
        
        [JsonProperty("15")]
        public float radius { get; set; } 
        
        [JsonProperty("16")]
        public float range { get; set; } 
        
        [JsonProperty("17")]
        public int maxtargets { get; set; } 
        
        [JsonProperty("18")]
        public string progressskill { get; set; } 
        
        [JsonProperty("19")]
        public int requiredlv { get; set; } 
        
        [JsonProperty("20")]
        public string requiredclass { get; set; } 
        
        [JsonProperty("21")]
        public int learningsp { get; set; } 
        
        [JsonProperty("22")]
        public int learningcost { get; set; } 
        
        [JsonProperty("23")]
        public int teacheritemid { get; set; } 
        
        [JsonProperty("24")]
        public int teacheritemcount { get; set; } 
        
        [JsonProperty("25")]
        public string repeatse { get; set; } 
        
        [JsonProperty("26")]
        public float repeattime { get; set; } 
        
        [JsonProperty("27")]
        public float chargeduration { get; set; } 
        
        [JsonProperty("28")]
        public string chargeeffect { get; set; } 
        
        [JsonProperty("29")]
        public int labelcount { get; set; } 
        
        public Dictionary<int,SideEffectJson> selfsideeffect = new Dictionary<int,SideEffectJson> ();
        
        public Dictionary<int,SideEffectJson> sideeffects = new Dictionary<int,SideEffectJson> ();
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            name = (string)vals["name"];
            skillgroupid = (int)vals["skillgroupid"];
            level = (int)vals["level"];
            description = (string)vals["description"];
            cost = (int)vals["cost"];
            cooldown = Convert.ToSingle((double)vals["cooldown"]);
            globalcd = Convert.ToSingle((double)vals["globalcd"]);
            actioneffect = (string)vals["actioneffect"];
            effectgethit = (string)vals["effectgethit"];
            offsettime = Convert.ToSingle((double)vals["offsettime"]);
            skillduration = Convert.ToSingle((double)vals["skillduration"]);
            effect_dur = Convert.ToSingle((double)vals["effect_dur"]);
            rtfixedduration = Convert.ToSingle((double)vals["rtfixedduration"]);
            rtvarduration = Convert.ToSingle((double)vals["rtvarduration"]);
            radius = Convert.ToSingle((double)vals["radius"]);
            range = Convert.ToSingle((double)vals["range"]);
            maxtargets = (int)vals["maxtargets"];
            progressskill = (string)vals["progressskill"];
            requiredlv = (int)vals["requiredlv"];
            requiredclass = (string)vals["requiredclass"];
            learningsp = (int)vals["learningsp"];
            learningcost = (int)vals["learningcost"];
            teacheritemid = (int)vals["teacheritemid"];
            teacheritemcount = (int)vals["teacheritemcount"];
            repeatse = (string)vals["repeatse"];
            repeattime = Convert.ToSingle((double)vals["repeattime"]);
            chargeduration = Convert.ToSingle((double)vals["chargeduration"]);
            chargeeffect = (string)vals["chargeeffect"];
            labelcount = (int)vals["labelcount"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class SkillTreeJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public JobType jobclass { get; set; } 
        
        //type SkillGroupJson
        [JsonProperty("2")]
        public int skillgroupid { get; set; } 
        
        [JsonProperty("3")]
        public string gridrow { get; set; } 
        
        [JsonProperty("4")]
        public int gridcol { get; set; } 
        
        [JsonProperty("5")]
        public string parent { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            jobclass = (JobType)vals["jobclass"];
            skillgroupid = (int)vals["skillgroupid"];
            gridrow = (string)vals["gridrow"];
            gridcol = (int)vals["gridcol"];
            parent = (string)vals["parent"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class SideEffectJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public string name { get; set; } 
        
        [JsonProperty("2")]
        public EffectType effecttype { get; set; } 
        
        [JsonProperty("3")]
        public float basicskilldamageperc { get; set; } 
        
        [JsonProperty("4")]
        public float max { get; set; } 
        
        [JsonProperty("5")]
        public float min { get; set; } 
        
        [JsonProperty("6")]
        public bool isrelative { get; set; } 
        
        [JsonProperty("7")]
        public bool usereferencetable { get; set; } 
        
        [JsonProperty("8")]
        public int step { get; set; } 
        
        [JsonProperty("9")]
        public float increase { get; set; } 
        
        [JsonProperty("10")]
        public string parameter { get; set; } 
        
        [JsonProperty("11")]
        public float duration { get; set; } 
        
        [JsonProperty("12")]
        public float interval { get; set; } 
        
        [JsonProperty("13")]
        public CriticalType criticaltype { get; set; } 
        
        [JsonProperty("14")]
        public int bonuscriticalchance { get; set; } 
        
        [JsonProperty("15")]
        public float procchance { get; set; } 
        
        [JsonProperty("16")]
        public bool persistentafterdeath { get; set; } 
        
        [JsonProperty("17")]
        public bool persistentonlogout { get; set; } 
        
        [JsonProperty("18")]
        public int rank { get; set; } 
        
        [AssetData("prefab")]
        [JsonProperty("19")]
        public string effectpath { get; set; } 
        
        [AssetData("sprite")]
        [JsonProperty("20")]
        public string icon { get; set; } 
        
        [JsonProperty("21")]
        public string localizedname { get; set; } 
        
        [JsonProperty("22")]
        public string description { get; set; } 
        
        [JsonProperty("23")]
        public ActorStatsType stat1 { get; set; } 
        
        [JsonProperty("24")]
        public ActorStatsType stat2 { get; set; } 
        
        [JsonProperty("25")]
        public ActorStatsType stat3 { get; set; } 
        
        //type SkillDescriptionGroupJson
        [JsonProperty("26")]
        public int sdg { get; set; } 
        
        [JsonProperty("27")]
        public bool stackable { get; set; } 
        
        [JsonProperty("28")]
        public int stackcount { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            name = (string)vals["name"];
            effecttype = (EffectType)vals["effecttype"];
            basicskilldamageperc = Convert.ToSingle((double)vals["basicskilldamageperc"]);
            max = Convert.ToSingle((double)vals["max"]);
            min = Convert.ToSingle((double)vals["min"]);
            isrelative = (bool)vals["isrelative"];
            usereferencetable = (bool)vals["usereferencetable"];
            step = (int)vals["step"];
            increase = Convert.ToSingle((double)vals["increase"]);
            parameter = (string)vals["parameter"];
            duration = Convert.ToSingle((double)vals["duration"]);
            interval = Convert.ToSingle((double)vals["interval"]);
            criticaltype = (CriticalType)vals["criticaltype"];
            bonuscriticalchance = (int)vals["bonuscriticalchance"];
            procchance = Convert.ToSingle((double)vals["procchance"]);
            persistentafterdeath = (bool)vals["persistentafterdeath"];
            persistentonlogout = (bool)vals["persistentonlogout"];
            rank = (int)vals["rank"];
            effectpath = (string)vals["effectpath"];
            icon = (string)vals["icon"];
            localizedname = (string)vals["localizedname"];
            description = (string)vals["description"];
            stat1 = (ActorStatsType)vals["stat1"];
            stat2 = (ActorStatsType)vals["stat2"];
            stat3 = (ActorStatsType)vals["stat3"];
            sdg = (int)vals["sdg"];
            stackable = (bool)vals["stackable"];
            stackcount = (int)vals["stackcount"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class SkillDescriptionGroupJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public string name { get; set; } 
        
        [JsonProperty("2")]
        public string description { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            name = (string)vals["name"];
            description = (string)vals["description"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class SideEffectGroupJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public string name { get; set; } 
        
        public Dictionary<int,SideEffectJson> sideeffects = new Dictionary<int,SideEffectJson> ();
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            name = (string)vals["name"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class NPCToSkillsLinkJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        //type CombatNPCJson
        [JsonProperty("1")]
        public int archetypeid { get; set; } 
        
        //type SkillJson
        [JsonProperty("2")]
        public int skillid { get; set; } 
        
        [JsonProperty("3")]
        public int priority { get; set; } 
        
        [JsonProperty("4")]
        public float chance { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            archetypeid = (int)vals["archetypeid"];
            skillid = (int)vals["skillid"];
            priority = (int)vals["priority"];
            chance = Convert.ToSingle((double)vals["chance"]);
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class ElementChartJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public Element elementid { get; set; } 
        
        [JsonProperty("2")]
        public int none { get; set; } 
        
        [JsonProperty("3")]
        public int metal { get; set; } 
        
        [JsonProperty("4")]
        public int wood { get; set; } 
        
        [JsonProperty("5")]
        public int earth { get; set; } 
        
        [JsonProperty("6")]
        public int water { get; set; } 
        
        [JsonProperty("7")]
        public int fire { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            elementid = (Element)vals["elementid"];
            none = (int)vals["none"];
            metal = (int)vals["metal"];
            wood = (int)vals["wood"];
            earth = (int)vals["earth"];
            water = (int)vals["water"];
            fire = (int)vals["fire"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class WeaknessChartJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public AttackStyle attacktype { get; set; } 
        
        [JsonProperty("2")]
        public int slice { get; set; } 
        
        [JsonProperty("3")]
        public int pierce { get; set; } 
        
        [JsonProperty("4")]
        public int smash { get; set; } 
        
        [JsonProperty("5")]
        public int god { get; set; } 
        
        [JsonProperty("6")]
        public int normal { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            attacktype = (AttackStyle)vals["attacktype"];
            slice = (int)vals["slice"];
            pierce = (int)vals["pierce"];
            smash = (int)vals["smash"];
            god = (int)vals["god"];
            normal = (int)vals["normal"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class HeroJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public int heroid { get; set; } 
        
        [JsonProperty("2")]
        public string name { get; set; } 
        
        [JsonProperty("3")]
        public string localizedname { get; set; } 
        
        [JsonProperty("4")]
        public string description { get; set; } 
        
        [JsonProperty("5")]
        public HeroRarity rarity { get; set; } 
        
        [JsonProperty("6")]
        public int unlockitemid { get; set; } 
        
        [JsonProperty("7")]
        public int unlockitemcount { get; set; } 
        
        [JsonProperty("8")]
        public int upgradeitemid { get; set; } 
        
        [JsonProperty("9")]
        public int upgradeitemcount { get; set; } 
        
        //type SkillJson
        [JsonProperty("10")]
        public int basicattack { get; set; } 
        
        //type SkillGroupJson
        [JsonProperty("11")]
        public int skill1grp { get; set; } 
        
        //type SkillGroupJson
        [JsonProperty("12")]
        public int skill2grp { get; set; } 
        
        //type SkillGroupJson
        [JsonProperty("13")]
        public int skill3grp { get; set; } 
        
        [JsonProperty("14")]
        public int resetskillmoney { get; set; } 
        
        [JsonProperty("15")]
        public int growthgroup { get; set; } 
        
        [JsonProperty("16")]
        public int interestgroup { get; set; } 
        
        [JsonProperty("17")]
        public float followdistance { get; set; } 
        
        [JsonProperty("18")]
        public float movespeed { get; set; } 
        
        [JsonProperty("19")]
        public float attackspeed { get; set; } 
        
        [JsonProperty("20")]
        public AttackStyle attackstyle { get; set; } 
        
        [JsonProperty("21")]
        public Element element { get; set; } 
        
        [JsonProperty("22")]
        public MainWeaponAttribute weaponattrib { get; set; } 
        
        [JsonProperty("23")]
        public int weaponattack { get; set; } 
        
        [JsonProperty("24")]
        public int summonlevel { get; set; } 
        
        [AssetData("prefab")]
        [JsonProperty("25")]
        public string t1modelpath { get; set; } 
        
        [AssetData("prefab")]
        [JsonProperty("26")]
        public string t2modelpath { get; set; } 
        
        [AssetData("prefab")]
        [JsonProperty("27")]
        public string t3modelpath { get; set; } 
        
        [AssetData("sprite")]
        [JsonProperty("28")]
        public string t1imagepath { get; set; } 
        
        [AssetData("sprite")]
        [JsonProperty("29")]
        public string t2imagepath { get; set; } 
        
        [AssetData("sprite")]
        [JsonProperty("30")]
        public string t3imagepath { get; set; } 
        
        [JsonProperty("31")]
        public float modelscalex { get; set; } 
        
        [JsonProperty("32")]
        public float modelscaley { get; set; } 
        
        [JsonProperty("33")]
        public float modelscalez { get; set; } 
        
        [JsonProperty("34")]
        public string summonaction { get; set; } 
        
        [JsonProperty("35")]
        public float summonduration { get; set; } 
        
        [AssetData("prefab")]
        [JsonProperty("36")]
        public string summoneffect { get; set; } 
        
        [AssetData("prefab")]
        [JsonProperty("37")]
        public string unlockshow { get; set; } 
        
        [JsonProperty("38")]
        public string lookbackshow { get; set; } 
        
        [JsonProperty("39")]
        public string exploreaction { get; set; } 
        
        [JsonProperty("40")]
        public string randomitemid { get; set; } 
        
        [AssetData("sprite")]
        [JsonProperty("41")]
        public string portraitpath { get; set; } 
        
        [JsonProperty("42")]
        public string questid { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            heroid = (int)vals["heroid"];
            name = (string)vals["name"];
            localizedname = (string)vals["localizedname"];
            description = (string)vals["description"];
            rarity = (HeroRarity)vals["rarity"];
            unlockitemid = (int)vals["unlockitemid"];
            unlockitemcount = (int)vals["unlockitemcount"];
            upgradeitemid = (int)vals["upgradeitemid"];
            upgradeitemcount = (int)vals["upgradeitemcount"];
            basicattack = (int)vals["basicattack"];
            skill1grp = (int)vals["skill1grp"];
            skill2grp = (int)vals["skill2grp"];
            skill3grp = (int)vals["skill3grp"];
            resetskillmoney = (int)vals["resetskillmoney"];
            growthgroup = (int)vals["growthgroup"];
            interestgroup = (int)vals["interestgroup"];
            followdistance = Convert.ToSingle((double)vals["followdistance"]);
            movespeed = Convert.ToSingle((double)vals["movespeed"]);
            attackspeed = Convert.ToSingle((double)vals["attackspeed"]);
            attackstyle = (AttackStyle)vals["attackstyle"];
            element = (Element)vals["element"];
            weaponattrib = (MainWeaponAttribute)vals["weaponattrib"];
            weaponattack = (int)vals["weaponattack"];
            summonlevel = (int)vals["summonlevel"];
            t1modelpath = (string)vals["t1modelpath"];
            t2modelpath = (string)vals["t2modelpath"];
            t3modelpath = (string)vals["t3modelpath"];
            t1imagepath = (string)vals["t1imagepath"];
            t2imagepath = (string)vals["t2imagepath"];
            t3imagepath = (string)vals["t3imagepath"];
            modelscalex = Convert.ToSingle((double)vals["modelscalex"]);
            modelscaley = Convert.ToSingle((double)vals["modelscaley"]);
            modelscalez = Convert.ToSingle((double)vals["modelscalez"]);
            summonaction = (string)vals["summonaction"];
            summonduration = Convert.ToSingle((double)vals["summonduration"]);
            summoneffect = (string)vals["summoneffect"];
            unlockshow = (string)vals["unlockshow"];
            lookbackshow = (string)vals["lookbackshow"];
            exploreaction = (string)vals["exploreaction"];
            randomitemid = (string)vals["randomitemid"];
            portraitpath = (string)vals["portraitpath"];
            questid = (string)vals["questid"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class HeroGrowthJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public int growthgroup { get; set; } 
        
        [JsonProperty("2")]
        public int herolevel { get; set; } 
        
        [JsonProperty("3")]
        public int levelupmoney { get; set; } 
        
        [JsonProperty("4")]
        public int strength { get; set; } 
        
        [JsonProperty("5")]
        public int agility { get; set; } 
        
        [JsonProperty("6")]
        public int dexterity { get; set; } 
        
        [JsonProperty("7")]
        public int constitution { get; set; } 
        
        [JsonProperty("8")]
        public int intelligence { get; set; } 
        
        [JsonProperty("9")]
        public int attackpower { get; set; } 
        
        [JsonProperty("10")]
        public int accuracy { get; set; } 
        
        [JsonProperty("11")]
        public int critical { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            growthgroup = (int)vals["growthgroup"];
            herolevel = (int)vals["herolevel"];
            levelupmoney = (int)vals["levelupmoney"];
            strength = (int)vals["strength"];
            agility = (int)vals["agility"];
            dexterity = (int)vals["dexterity"];
            constitution = (int)vals["constitution"];
            intelligence = (int)vals["intelligence"];
            attackpower = (int)vals["attackpower"];
            accuracy = (int)vals["accuracy"];
            critical = (int)vals["critical"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class HeroInterestGroupJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public int interestgroup { get; set; } 
        
        [JsonProperty("2")]
        public HeroInterestType interesttype { get; set; } 
        
        [JsonProperty("3")]
        public float probability { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            interestgroup = (int)vals["interestgroup"];
            interesttype = (HeroInterestType)vals["interesttype"];
            probability = Convert.ToSingle((double)vals["probability"]);
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class HeroInterestJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public HeroInterestType interesttype { get; set; } 
        
        [JsonProperty("2")]
        public string localizedname { get; set; } 
        
        [JsonProperty("3")]
        public string assigneditemid { get; set; } 
        
        [AssetData("sprite")]
        [JsonProperty("4")]
        public string iconpath { get; set; } 
        
        [JsonProperty("5")]
        public string description { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            interesttype = (HeroInterestType)vals["interesttype"];
            localizedname = (string)vals["localizedname"];
            assigneditemid = (string)vals["assigneditemid"];
            iconpath = (string)vals["iconpath"];
            description = (string)vals["description"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class HeroTrustJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public int trustlevel { get; set; } 
        
        [JsonProperty("2")]
        public int reqtrustexp { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            trustlevel = (int)vals["trustlevel"];
            reqtrustexp = (int)vals["reqtrustexp"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class HeroBondGroupJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public int sequence { get; set; } 
        
        [JsonProperty("2")]
        public string localizedname { get; set; } 
        
        [JsonProperty("3")]
        public int hero1 { get; set; } 
        
        [JsonProperty("4")]
        public int hero2 { get; set; } 
        
        [JsonProperty("5")]
        public int hero3 { get; set; } 
        
        [JsonProperty("6")]
        public int hero4 { get; set; } 
        
        [JsonProperty("7")]
        public int hero5 { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            sequence = (int)vals["sequence"];
            localizedname = (string)vals["localizedname"];
            hero1 = (int)vals["hero1"];
            hero2 = (int)vals["hero2"];
            hero3 = (int)vals["hero3"];
            hero4 = (int)vals["hero4"];
            hero5 = (int)vals["hero5"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class HeroBondJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public int bondgroupid { get; set; } 
        
        [JsonProperty("2")]
        public int bondlevel { get; set; } 
        
        [JsonProperty("3")]
        public HeroBondType bondtype1 { get; set; } 
        
        [JsonProperty("4")]
        public int bondvalue1 { get; set; } 
        
        [JsonProperty("5")]
        public HeroBondType bondtype2 { get; set; } 
        
        [JsonProperty("6")]
        public int bondvalue2 { get; set; } 
        
        //type SideEffectGroupJson
        [JsonProperty("7")]
        public int sideeffectgrp { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            bondgroupid = (int)vals["bondgroupid"];
            bondlevel = (int)vals["bondlevel"];
            bondtype1 = (HeroBondType)vals["bondtype1"];
            bondvalue1 = (int)vals["bondvalue1"];
            bondtype2 = (HeroBondType)vals["bondtype2"];
            bondvalue2 = (int)vals["bondvalue2"];
            sideeffectgrp = (int)vals["sideeffectgrp"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class ExplorationMapJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public int mapid { get; set; } 
        
        [JsonProperty("2")]
        public string localizedname { get; set; } 
        
        [JsonProperty("3")]
        public string localizedsubname { get; set; } 
        
        [JsonProperty("4")]
        public string localizeddescription { get; set; } 
        
        [JsonProperty("5")]
        public int sequence { get; set; } 
        
        [JsonProperty("6")]
        public int prevmapid { get; set; } 
        
        [JsonProperty("7")]
        public int reqherolevel { get; set; } 
        
        [JsonProperty("8")]
        public int reqherotrust { get; set; } 
        
        [JsonProperty("9")]
        public int reqmonsterlevel { get; set; } 
        
        [JsonProperty("10")]
        public int maxherocount { get; set; } 
        
        [JsonProperty("11")]
        public bool repeatable { get; set; } 
        
        [JsonProperty("12")]
        public int combattimecost { get; set; } 
        
        [JsonProperty("13")]
        public int completetime { get; set; } 
        
        [JsonProperty("14")]
        public int reqitemid { get; set; } 
        
        [JsonProperty("15")]
        public int reqitemcount { get; set; } 
        
        [JsonProperty("16")]
        public ChestRequirementType chestreqtype1 { get; set; } 
        
        [JsonProperty("17")]
        public int chestreqvalue1 { get; set; } 
        
        [JsonProperty("18")]
        public ChestRequirementType chestreqtype2 { get; set; } 
        
        [JsonProperty("19")]
        public int chestreqvalue2 { get; set; } 
        
        [JsonProperty("20")]
        public ChestRequirementType chestreqtype3 { get; set; } 
        
        [JsonProperty("21")]
        public int chestreqvalue3 { get; set; } 
        
        [JsonProperty("22")]
        public int baseefficiency { get; set; } 
        
        [JsonProperty("23")]
        public int levelrelvalue { get; set; } 
        
        [JsonProperty("24")]
        public int levelmaxefficiency { get; set; } 
        
        [JsonProperty("25")]
        public int trustrelvalue { get; set; } 
        
        [JsonProperty("26")]
        public int trustmaxefficiency { get; set; } 
        
        [JsonProperty("27")]
        public TerrainType terraintype { get; set; } 
        
        [JsonProperty("28")]
        public int exploregroupid { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            mapid = (int)vals["mapid"];
            localizedname = (string)vals["localizedname"];
            localizedsubname = (string)vals["localizedsubname"];
            localizeddescription = (string)vals["localizeddescription"];
            sequence = (int)vals["sequence"];
            prevmapid = (int)vals["prevmapid"];
            reqherolevel = (int)vals["reqherolevel"];
            reqherotrust = (int)vals["reqherotrust"];
            reqmonsterlevel = (int)vals["reqmonsterlevel"];
            maxherocount = (int)vals["maxherocount"];
            repeatable = (bool)vals["repeatable"];
            combattimecost = (int)vals["combattimecost"];
            completetime = (int)vals["completetime"];
            reqitemid = (int)vals["reqitemid"];
            reqitemcount = (int)vals["reqitemcount"];
            chestreqtype1 = (ChestRequirementType)vals["chestreqtype1"];
            chestreqvalue1 = (int)vals["chestreqvalue1"];
            chestreqtype2 = (ChestRequirementType)vals["chestreqtype2"];
            chestreqvalue2 = (int)vals["chestreqvalue2"];
            chestreqtype3 = (ChestRequirementType)vals["chestreqtype3"];
            chestreqvalue3 = (int)vals["chestreqvalue3"];
            baseefficiency = (int)vals["baseefficiency"];
            levelrelvalue = (int)vals["levelrelvalue"];
            levelmaxefficiency = (int)vals["levelmaxefficiency"];
            trustrelvalue = (int)vals["trustrelvalue"];
            trustmaxefficiency = (int)vals["trustmaxefficiency"];
            terraintype = (TerrainType)vals["terraintype"];
            exploregroupid = (int)vals["exploregroupid"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class ExplorationTargetJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public int exploregroupid { get; set; } 
        
        [JsonProperty("2")]
        public string localizedname { get; set; } 
        
        [AssetData("sprite")]
        [JsonProperty("3")]
        public string iconpath { get; set; } 
        
        [JsonProperty("4")]
        public int chestitemid { get; set; } 
        
        [JsonProperty("5")]
        public int rewardgroupid { get; set; } 
        
        [JsonProperty("6")]
        public string looktarget { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            exploregroupid = (int)vals["exploregroupid"];
            localizedname = (string)vals["localizedname"];
            iconpath = (string)vals["iconpath"];
            chestitemid = (int)vals["chestitemid"];
            rewardgroupid = (int)vals["rewardgroupid"];
            looktarget = (string)vals["looktarget"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class TerrainEfficiencyJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public TerrainType terraintype { get; set; } 
        
        [JsonProperty("2")]
        public HeroInterestType interesttype { get; set; } 
        
        [JsonProperty("3")]
        public float efficiency { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            terraintype = (TerrainType)vals["terraintype"];
            interesttype = (HeroInterestType)vals["interesttype"];
            efficiency = Convert.ToSingle((double)vals["efficiency"]);
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class TerrainJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public TerrainType terraintype { get; set; } 
        
        [JsonProperty("2")]
        public string terrainname { get; set; } 
        
        [AssetData("sprite")]
        [JsonProperty("3")]
        public string backgroundpath { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            terraintype = (TerrainType)vals["terraintype"];
            terrainname = (string)vals["terrainname"];
            backgroundpath = (string)vals["backgroundpath"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class GuildConstantJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public string name { get; set; } 
        
        [JsonProperty("2")]
        public int value { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            name = (string)vals["name"];
            value = (int)vals["value"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class GuildSMBossJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public string name { get; set; } 
        
        [JsonProperty("2")]
        public string localizedname { get; set; } 
        
        [JsonProperty("3")]
        public int level { get; set; } 
        
        [JsonProperty("4")]
        public int talentscissors { get; set; } 
        
        [JsonProperty("5")]
        public int talentstone { get; set; } 
        
        [JsonProperty("6")]
        public int talentcloth { get; set; } 
        
        [JsonProperty("7")]
        public int healthmax { get; set; } 
        
        [JsonProperty("8")]
        public int attack { get; set; } 
        
        [JsonProperty("9")]
        public int armor { get; set; } 
        
        [JsonProperty("10")]
        public int accuracy { get; set; } 
        
        [JsonProperty("11")]
        public int evasion { get; set; } 
        
        [JsonProperty("12")]
        public int critical { get; set; } 
        
        [JsonProperty("13")]
        public int criticaldamage { get; set; } 
        
        [JsonProperty("14")]
        public int cocritical { get; set; } 
        
        [JsonProperty("15")]
        public int cocriticaldamage { get; set; } 
        
        //type RewardListJson
        [JsonProperty("16")]
        public int enterrewardlist { get; set; } 
        
        //type RewardListJson
        [JsonProperty("17")]
        public int killrewardlist { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            name = (string)vals["name"];
            localizedname = (string)vals["localizedname"];
            level = (int)vals["level"];
            talentscissors = (int)vals["talentscissors"];
            talentstone = (int)vals["talentstone"];
            talentcloth = (int)vals["talentcloth"];
            healthmax = (int)vals["healthmax"];
            attack = (int)vals["attack"];
            armor = (int)vals["armor"];
            accuracy = (int)vals["accuracy"];
            evasion = (int)vals["evasion"];
            critical = (int)vals["critical"];
            criticaldamage = (int)vals["criticaldamage"];
            cocritical = (int)vals["cocritical"];
            cocriticaldamage = (int)vals["cocriticaldamage"];
            enterrewardlist = (int)vals["enterrewardlist"];
            killrewardlist = (int)vals["killrewardlist"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class GuildTechClassJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public string name { get; set; } 
        
        [JsonProperty("2")]
        public string localizedname { get; set; } 
        
        [JsonProperty("3")]
        public GuildTechType type { get; set; } 
        
        [JsonProperty("4")]
        public string description { get; set; } 
        
        [AssetData("sprite")]
        [JsonProperty("5")]
        public string icon { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            name = (string)vals["name"];
            localizedname = (string)vals["localizedname"];
            type = (GuildTechType)vals["type"];
            description = (string)vals["description"];
            icon = (string)vals["icon"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class GuildTechLevelJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public string name { get; set; } 
        
        [JsonProperty("2")]
        public GuildTechType type { get; set; } 
        
        [JsonProperty("3")]
        public int level { get; set; } 
        
        [JsonProperty("4")]
        public int fund { get; set; } 
        
        [JsonProperty("5")]
        public int requirelv { get; set; } 
        
        [JsonProperty("6")]
        public float stats { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            name = (string)vals["name"];
            type = (GuildTechType)vals["type"];
            level = (int)vals["level"];
            fund = (int)vals["fund"];
            requirelv = (int)vals["requirelv"];
            stats = Convert.ToSingle((double)vals["stats"]);
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class GuildQuestJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public string name { get; set; } 
        
        [JsonProperty("2")]
        public int probability { get; set; } 
        
        [JsonProperty("3")]
        public GuildQuestType rarity { get; set; } 
        
        [JsonProperty("4")]
        public string icon { get; set; } 
        
        [JsonProperty("5")]
        public int duration { get; set; } 
        
        [JsonProperty("6")]
        public int rewardcontribution { get; set; } 
        
        [JsonProperty("7")]
        public int rewardwealth { get; set; } 
        
        [JsonProperty("8")]
        public string itemid { get; set; } 
        
        [JsonProperty("9")]
        public int itemcount { get; set; } 
        
        [JsonProperty("10")]
        public int questlevel { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            name = (string)vals["name"];
            probability = (int)vals["probability"];
            rarity = (GuildQuestType)vals["rarity"];
            icon = (string)vals["icon"];
            duration = (int)vals["duration"];
            rewardcontribution = (int)vals["rewardcontribution"];
            rewardwealth = (int)vals["rewardwealth"];
            itemid = (string)vals["itemid"];
            itemcount = (int)vals["itemcount"];
            questlevel = (int)vals["questlevel"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class GuildDreamhouseJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public int progresscnt1 { get; set; } 
        
        //type RewardListJson
        [JsonProperty("2")]
        public int rewardlist1 { get; set; } 
        
        [JsonProperty("3")]
        public int progresscnt2 { get; set; } 
        
        //type RewardListJson
        [JsonProperty("4")]
        public int rewardlist2 { get; set; } 
        
        [JsonProperty("5")]
        public int progresscnt3 { get; set; } 
        
        //type RewardListJson
        [JsonProperty("6")]
        public int rewardlist3 { get; set; } 
        
        [JsonProperty("7")]
        public int progresscnt4 { get; set; } 
        
        //type RewardListJson
        [JsonProperty("8")]
        public int rewardlist4 { get; set; } 
        
        [JsonProperty("9")]
        public int progresscnt5 { get; set; } 
        
        //type RewardListJson
        [JsonProperty("10")]
        public int rewardlist5 { get; set; } 
        
        //type RewardListJson
        [JsonProperty("11")]
        public int smrewardlist1 { get; set; } 
        
        //type RewardListJson
        [JsonProperty("12")]
        public int smrewardlist2 { get; set; } 
        
        //type RewardListJson
        [JsonProperty("13")]
        public int smrewardlist3 { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            progresscnt1 = (int)vals["progresscnt1"];
            rewardlist1 = (int)vals["rewardlist1"];
            progresscnt2 = (int)vals["progresscnt2"];
            rewardlist2 = (int)vals["rewardlist2"];
            progresscnt3 = (int)vals["progresscnt3"];
            rewardlist3 = (int)vals["rewardlist3"];
            progresscnt4 = (int)vals["progresscnt4"];
            rewardlist4 = (int)vals["rewardlist4"];
            progresscnt5 = (int)vals["progresscnt5"];
            rewardlist5 = (int)vals["rewardlist5"];
            smrewardlist1 = (int)vals["smrewardlist1"];
            smrewardlist2 = (int)vals["smrewardlist2"];
            smrewardlist3 = (int)vals["smrewardlist3"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class GuildBadgeJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public PositionType type { get; set; } 
        
        [JsonProperty("2")]
        public int sortorder { get; set; } 
        
        [JsonProperty("3")]
        public string iconpath { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            type = (PositionType)vals["type"];
            sortorder = (int)vals["sortorder"];
            iconpath = (string)vals["iconpath"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class PartyLocationJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public LocationType locationtype { get; set; } 
        
        [JsonProperty("2")]
        public string realmid { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            locationtype = (LocationType)vals["locationtype"];
            realmid = (string)vals["realmid"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class SignInPrizeJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public int year { get; set; } 
        
        [JsonProperty("2")]
        public int month { get; set; } 
        
        [JsonProperty("3")]
        public int day { get; set; } 
        
        [JsonProperty("4")]
        public int itemid { get; set; } 
        
        [JsonProperty("5")]
        public int amount { get; set; } 
        
        [JsonProperty("6")]
        public int viplevel { get; set; } 
        
        [JsonProperty("7")]
        public int viptimes { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            year = (int)vals["year"];
            month = (int)vals["month"];
            day = (int)vals["day"];
            itemid = (int)vals["itemid"];
            amount = (int)vals["amount"];
            viplevel = (int)vals["viplevel"];
            viptimes = (int)vals["viptimes"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class OnlinePrizeJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public int serial { get; set; } 
        
        [JsonProperty("2")]
        public int itemid { get; set; } 
        
        [JsonProperty("3")]
        public int amount { get; set; } 
        
        [JsonProperty("4")]
        public int time { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            serial = (int)vals["serial"];
            itemid = (int)vals["itemid"];
            amount = (int)vals["amount"];
            time = (int)vals["time"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class RestrictionJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public Days category { get; set; } 
        
        [JsonProperty("2")]
        public int itemid { get; set; } 
        
        [JsonProperty("3")]
        public int itemcount { get; set; } 
        
        [JsonProperty("4")]
        public int displaypoints { get; set; } 
        
        [JsonProperty("5")]
        public int xenjopoints { get; set; } 
        
        [JsonProperty("6")]
        public int limitcount { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            category = (Days)vals["category"];
            itemid = (int)vals["itemid"];
            itemcount = (int)vals["itemcount"];
            displaypoints = (int)vals["displaypoints"];
            xenjopoints = (int)vals["xenjopoints"];
            limitcount = (int)vals["limitcount"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class NewServerActivitySubJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public string name { get; set; } 
        
        [JsonProperty("2")]
        public string localizedname { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            name = (string)vals["name"];
            localizedname = (string)vals["localizedname"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class NewServerActivityJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public Days maincategory { get; set; } 
        
        //type NewServerActivitySubJson
        [JsonProperty("2")]
        public int subcategory { get; set; } 
        
        [JsonProperty("3")]
        public NewServerActivityType type { get; set; } 
        
        [JsonProperty("4")]
        public string name { get; set; } 
        
        [JsonProperty("5")]
        public string description { get; set; } 
        
        [JsonProperty("6")]
        public int count1 { get; set; } 
        
        [JsonProperty("7")]
        public int count2 { get; set; } 
        
        //type RewardListJson
        [JsonProperty("8")]
        public int rewardlist { get; set; } 
        
        [JsonProperty("9")]
        public LinkUIType link { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            maincategory = (Days)vals["maincategory"];
            subcategory = (int)vals["subcategory"];
            type = (NewServerActivityType)vals["type"];
            name = (string)vals["name"];
            description = (string)vals["description"];
            count1 = (int)vals["count1"];
            count2 = (int)vals["count2"];
            rewardlist = (int)vals["rewardlist"];
            link = (LinkUIType)vals["link"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class GameConstantJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public string name { get; set; } 
        
        [JsonProperty("2")]
        public string value { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            name = (string)vals["name"];
            value = (string)vals["value"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class CurrencyWalletJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public CurrencyType currencytype { get; set; } 
        
        [JsonProperty("2")]
        public int rowindex { get; set; } 
        
        [JsonProperty("3")]
        public LinkUIType uitype { get; set; } 
        
        [JsonProperty("4")]
        public bool tick { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            currencytype = (CurrencyType)vals["currencytype"];
            rowindex = (int)vals["rowindex"];
            uitype = (LinkUIType)vals["uitype"];
            tick = (bool)vals["tick"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class ActivityOverviewJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public string name { get; set; } 
        
        [JsonProperty("2")]
        public string startdate { get; set; } 
        
        [JsonProperty("3")]
        public string enddate { get; set; } 
        
        [JsonProperty("4")]
        public string week { get; set; } 
        
        [JsonProperty("5")]
        public string starttime { get; set; } 
        
        [JsonProperty("6")]
        public string endtime { get; set; } 
        
        [AssetData("sprite")]
        [JsonProperty("7")]
        public string iconpath { get; set; } 
        
        [JsonProperty("8")]
        public string localizedname { get; set; } 
        
        [JsonProperty("9")]
        public string content { get; set; } 
        
        [JsonProperty("10")]
        public LinkUIType uitype { get; set; } 
        
        [JsonProperty("11")]
        public int reqlvl { get; set; } 
        
        [JsonProperty("12")]
        public string param1 { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            name = (string)vals["name"];
            startdate = (string)vals["startdate"];
            enddate = (string)vals["enddate"];
            week = (string)vals["week"];
            starttime = (string)vals["starttime"];
            endtime = (string)vals["endtime"];
            iconpath = (string)vals["iconpath"];
            localizedname = (string)vals["localizedname"];
            content = (string)vals["content"];
            uitype = (LinkUIType)vals["uitype"];
            reqlvl = (int)vals["reqlvl"];
            param1 = (string)vals["param1"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class CurrencyExchangeJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public int exchangetimes { get; set; } 
        
        [JsonProperty("2")]
        public int exchangegold { get; set; } 
        
        [JsonProperty("3")]
        public int exchangemoney { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            exchangetimes = (int)vals["exchangetimes"];
            exchangegold = (int)vals["exchangegold"];
            exchangemoney = (int)vals["exchangemoney"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class MailContentJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public string name { get; set; } 
        
        //type SystemMessageJson
        [JsonProperty("2")]
        public int hud { get; set; } 
        
        [JsonProperty("3")]
        public string body { get; set; } 
        
        [JsonProperty("4")]
        public string sender { get; set; } 
        
        [JsonProperty("5")]
        public string subject { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            name = (string)vals["name"];
            hud = (int)vals["hud"];
            body = (string)vals["body"];
            sender = (string)vals["sender"];
            subject = (string)vals["subject"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class TeleportNPCListJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public string archetype { get; set; } 
        
        [JsonProperty("2")]
        public string title { get; set; } 
        
        [JsonProperty("3")]
        public string message { get; set; } 
        
        [JsonProperty("4")]
        public string npcscale { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            archetype = (string)vals["archetype"];
            title = (string)vals["title"];
            message = (string)vals["message"];
            npcscale = (string)vals["npcscale"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class RareItemNotificationJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public int itemid { get; set; } 
        
        [JsonProperty("2")]
        public int broadcasttype { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            itemid = (int)vals["itemid"];
            broadcasttype = (int)vals["broadcasttype"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class PushNotificationJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public string name { get; set; } 
        
        [JsonProperty("2")]
        public int notificationid { get; set; } 
        
        [JsonProperty("3")]
        public PushNotificationType day { get; set; } 
        
        [JsonProperty("4")]
        public int hour { get; set; } 
        
        [JsonProperty("5")]
        public int minute { get; set; } 
        
        [JsonProperty("6")]
        public string title { get; set; } 
        
        [JsonProperty("7")]
        public string content { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            name = (string)vals["name"];
            notificationid = (int)vals["notificationid"];
            day = (PushNotificationType)vals["day"];
            hour = (int)vals["hour"];
            minute = (int)vals["minute"];
            title = (string)vals["title"];
            content = (string)vals["content"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class LotteryMainJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public int gold { get; set; } 
        
        [JsonProperty("2")]
        public int freetime { get; set; } 
        
        [JsonProperty("3")]
        public int point { get; set; } 
        
        [JsonProperty("4")]
        public string starttime { get; set; } 
        
        [JsonProperty("5")]
        public string endtime { get; set; } 
        
        [JsonProperty("6")]
        public string focus { get; set; } 
        
        //type LotteryItemJson
        [JsonProperty("7")]
        public int lotteryid { get; set; } 
        
        public Dictionary<int,LotteryPointRewardJson> lotterypointid = new Dictionary<int,LotteryPointRewardJson> ();
        
        [JsonProperty("8")]
        public int itemid { get; set; } 
        
        [JsonProperty("9")]
        public int itemidpoint { get; set; } 
        
        [JsonProperty("10")]
        public int sortid { get; set; } 
        
        [JsonProperty("11")]
        public string logopath { get; set; } 
        
        [JsonProperty("12")]
        public string name { get; set; } 
        
        [JsonProperty("13")]
        public int animationid { get; set; } 
        
        [JsonProperty("14")]
        public int ticketid { get; set; } 
        
        [JsonProperty("15")]
        public CurrencyType currencytype { get; set; } 
        
        [JsonProperty("16")]
        public int hot { get; set; } 
        
        [JsonProperty("17")]
        public int grandprize1 { get; set; } 
        
        [JsonProperty("18")]
        public int grandprize2 { get; set; } 
        
        [JsonProperty("19")]
        public int grandprize3 { get; set; } 
        
        [JsonProperty("20")]
        public int grandprize4 { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            gold = (int)vals["gold"];
            freetime = (int)vals["freetime"];
            point = (int)vals["point"];
            starttime = (string)vals["starttime"];
            endtime = (string)vals["endtime"];
            focus = (string)vals["focus"];
            lotteryid = (int)vals["lotteryid"];
            itemid = (int)vals["itemid"];
            itemidpoint = (int)vals["itemidpoint"];
            sortid = (int)vals["sortid"];
            logopath = (string)vals["logopath"];
            name = (string)vals["name"];
            animationid = (int)vals["animationid"];
            ticketid = (int)vals["ticketid"];
            currencytype = (CurrencyType)vals["currencytype"];
            hot = (int)vals["hot"];
            grandprize1 = (int)vals["grandprize1"];
            grandprize2 = (int)vals["grandprize2"];
            grandprize3 = (int)vals["grandprize3"];
            grandprize4 = (int)vals["grandprize4"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class LotteryItemJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public string name { get; set; } 
        
        [JsonProperty("2")]
        public int itemid1 { get; set; } 
        
        [JsonProperty("3")]
        public int count1 { get; set; } 
        
        [JsonProperty("4")]
        public int weight1 { get; set; } 
        
        [JsonProperty("5")]
        public bool award1 { get; set; } 
        
        [JsonProperty("6")]
        public int itemid2 { get; set; } 
        
        [JsonProperty("7")]
        public int count2 { get; set; } 
        
        [JsonProperty("8")]
        public int weight2 { get; set; } 
        
        [JsonProperty("9")]
        public bool award2 { get; set; } 
        
        [JsonProperty("10")]
        public int itemid3 { get; set; } 
        
        [JsonProperty("11")]
        public int count3 { get; set; } 
        
        [JsonProperty("12")]
        public int weight3 { get; set; } 
        
        [JsonProperty("13")]
        public bool award3 { get; set; } 
        
        [JsonProperty("14")]
        public int itemid4 { get; set; } 
        
        [JsonProperty("15")]
        public int count4 { get; set; } 
        
        [JsonProperty("16")]
        public int weight4 { get; set; } 
        
        [JsonProperty("17")]
        public bool award4 { get; set; } 
        
        [JsonProperty("18")]
        public int itemid5 { get; set; } 
        
        [JsonProperty("19")]
        public int count5 { get; set; } 
        
        [JsonProperty("20")]
        public int weight5 { get; set; } 
        
        [JsonProperty("21")]
        public bool award5 { get; set; } 
        
        [JsonProperty("22")]
        public int itemid6 { get; set; } 
        
        [JsonProperty("23")]
        public int count6 { get; set; } 
        
        [JsonProperty("24")]
        public int weight6 { get; set; } 
        
        [JsonProperty("25")]
        public bool award6 { get; set; } 
        
        [JsonProperty("26")]
        public int itemid7 { get; set; } 
        
        [JsonProperty("27")]
        public int count7 { get; set; } 
        
        [JsonProperty("28")]
        public int weight7 { get; set; } 
        
        [JsonProperty("29")]
        public bool award7 { get; set; } 
        
        [JsonProperty("30")]
        public int itemid8 { get; set; } 
        
        [JsonProperty("31")]
        public int count8 { get; set; } 
        
        [JsonProperty("32")]
        public int weight8 { get; set; } 
        
        [JsonProperty("33")]
        public bool award8 { get; set; } 
        
        [JsonProperty("34")]
        public int itemid9 { get; set; } 
        
        [JsonProperty("35")]
        public int count9 { get; set; } 
        
        [JsonProperty("36")]
        public int weight9 { get; set; } 
        
        [JsonProperty("37")]
        public bool award9 { get; set; } 
        
        [JsonProperty("38")]
        public int itemid10 { get; set; } 
        
        [JsonProperty("39")]
        public int count10 { get; set; } 
        
        [JsonProperty("40")]
        public int weight10 { get; set; } 
        
        [JsonProperty("41")]
        public bool award10 { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            name = (string)vals["name"];
            itemid1 = (int)vals["itemid1"];
            count1 = (int)vals["count1"];
            weight1 = (int)vals["weight1"];
            award1 = (bool)vals["award1"];
            itemid2 = (int)vals["itemid2"];
            count2 = (int)vals["count2"];
            weight2 = (int)vals["weight2"];
            award2 = (bool)vals["award2"];
            itemid3 = (int)vals["itemid3"];
            count3 = (int)vals["count3"];
            weight3 = (int)vals["weight3"];
            award3 = (bool)vals["award3"];
            itemid4 = (int)vals["itemid4"];
            count4 = (int)vals["count4"];
            weight4 = (int)vals["weight4"];
            award4 = (bool)vals["award4"];
            itemid5 = (int)vals["itemid5"];
            count5 = (int)vals["count5"];
            weight5 = (int)vals["weight5"];
            award5 = (bool)vals["award5"];
            itemid6 = (int)vals["itemid6"];
            count6 = (int)vals["count6"];
            weight6 = (int)vals["weight6"];
            award6 = (bool)vals["award6"];
            itemid7 = (int)vals["itemid7"];
            count7 = (int)vals["count7"];
            weight7 = (int)vals["weight7"];
            award7 = (bool)vals["award7"];
            itemid8 = (int)vals["itemid8"];
            count8 = (int)vals["count8"];
            weight8 = (int)vals["weight8"];
            award8 = (bool)vals["award8"];
            itemid9 = (int)vals["itemid9"];
            count9 = (int)vals["count9"];
            weight9 = (int)vals["weight9"];
            award9 = (bool)vals["award9"];
            itemid10 = (int)vals["itemid10"];
            count10 = (int)vals["count10"];
            weight10 = (int)vals["weight10"];
            award10 = (bool)vals["award10"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class LotteryPointRewardJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public string name { get; set; } 
        
        [JsonProperty("2")]
        public int point { get; set; } 
        
        [JsonProperty("3")]
        public int itemid1 { get; set; } 
        
        [JsonProperty("4")]
        public int count1 { get; set; } 
        
        [JsonProperty("5")]
        public int itemid2 { get; set; } 
        
        [JsonProperty("6")]
        public int count2 { get; set; } 
        
        [JsonProperty("7")]
        public int itemid3 { get; set; } 
        
        [JsonProperty("8")]
        public int count3 { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            name = (string)vals["name"];
            point = (int)vals["point"];
            itemid1 = (int)vals["itemid1"];
            count1 = (int)vals["count1"];
            itemid2 = (int)vals["itemid2"];
            count2 = (int)vals["count2"];
            itemid3 = (int)vals["itemid3"];
            count3 = (int)vals["count3"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class TopUpItemAndroidJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public string name { get; set; } 
        
        [JsonProperty("2")]
        public int uiorder { get; set; } 
        
        [JsonProperty("3")]
        public string icon { get; set; } 
        
        [JsonProperty("4")]
        public bool doublebonus { get; set; } 
        
        [JsonProperty("5")]
        public int gold { get; set; } 
        
        [JsonProperty("6")]
        public int lockgold { get; set; } 
        
        [JsonProperty("7")]
        public int vippoints { get; set; } 
        
        [JsonProperty("8")]
        public int price { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            name = (string)vals["name"];
            uiorder = (int)vals["uiorder"];
            icon = (string)vals["icon"];
            doublebonus = (bool)vals["doublebonus"];
            gold = (int)vals["gold"];
            lockgold = (int)vals["lockgold"];
            vippoints = (int)vals["vippoints"];
            price = (int)vals["price"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class TopUpItemAppleJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public string name { get; set; } 
        
        [JsonProperty("2")]
        public int uiorder { get; set; } 
        
        [JsonProperty("3")]
        public string icon { get; set; } 
        
        [JsonProperty("4")]
        public bool doublebonus { get; set; } 
        
        [JsonProperty("5")]
        public int gold { get; set; } 
        
        [JsonProperty("6")]
        public int lockgold { get; set; } 
        
        [JsonProperty("7")]
        public int vippoints { get; set; } 
        
        [JsonProperty("8")]
        public int price { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            name = (string)vals["name"];
            uiorder = (int)vals["uiorder"];
            icon = (string)vals["icon"];
            doublebonus = (bool)vals["doublebonus"];
            gold = (int)vals["gold"];
            lockgold = (int)vals["lockgold"];
            vippoints = (int)vals["vippoints"];
            price = (int)vals["price"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class TopUpItemMyCardJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public string name { get; set; } 
        
        [JsonProperty("2")]
        public int uiorder { get; set; } 
        
        [JsonProperty("3")]
        public string icon { get; set; } 
        
        [JsonProperty("4")]
        public bool doublebonus { get; set; } 
        
        [JsonProperty("5")]
        public int gold { get; set; } 
        
        [JsonProperty("6")]
        public int lockgold { get; set; } 
        
        [JsonProperty("7")]
        public int vippoints { get; set; } 
        
        [JsonProperty("8")]
        public int price { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            name = (string)vals["name"];
            uiorder = (int)vals["uiorder"];
            icon = (string)vals["icon"];
            doublebonus = (bool)vals["doublebonus"];
            gold = (int)vals["gold"];
            lockgold = (int)vals["lockgold"];
            vippoints = (int)vals["vippoints"];
            price = (int)vals["price"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class QuestExtraTaskJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public string name { get; set; } 
        
        [JsonProperty("2")]
        public string iconpath { get; set; } 
        
        [JsonProperty("3")]
        public int reqgold { get; set; } 
        
        [JsonProperty("4")]
        public string questname { get; set; } 
        
        [JsonProperty("5")]
        public QuestExtraType questtype { get; set; } 
        
        [JsonProperty("6")]
        public string questobj { get; set; } 
        
        [JsonProperty("7")]
        public int questcount { get; set; } 
        
        //type RewardListJson
        [JsonProperty("8")]
        public int rewardlist { get; set; } 
        
        [JsonProperty("9")]
        public LinkUIType linkui { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            name = (string)vals["name"];
            iconpath = (string)vals["iconpath"];
            reqgold = (int)vals["reqgold"];
            questname = (string)vals["questname"];
            questtype = (QuestExtraType)vals["questtype"];
            questobj = (string)vals["questobj"];
            questcount = (int)vals["questcount"];
            rewardlist = (int)vals["rewardlist"];
            linkui = (LinkUIType)vals["linkui"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class QuestExtraRewardJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public string name { get; set; } 
        
        [JsonProperty("2")]
        public int boxnum { get; set; } 
        
        [JsonProperty("3")]
        public int reqpts { get; set; } 
        
        //type RewardListJson
        [JsonProperty("4")]
        public int rewardlist { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            name = (string)vals["name"];
            boxnum = (int)vals["boxnum"];
            reqpts = (int)vals["reqpts"];
            rewardlist = (int)vals["rewardlist"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class RespawnJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public string name { get; set; } 
        
        [JsonProperty("2")]
        public int respawnid { get; set; } 
        
        [JsonProperty("3")]
        public bool siturespawn { get; set; } 
        
        [JsonProperty("4")]
        public string deductitem { get; set; } 
        
        [JsonProperty("5")]
        public string deductcurrency { get; set; } 
        
        [JsonProperty("6")]
        public ReturnType returntype { get; set; } 
        
        [JsonProperty("7")]
        public int countdown { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            name = (string)vals["name"];
            respawnid = (int)vals["respawnid"];
            siturespawn = (bool)vals["siturespawn"];
            deductitem = (string)vals["deductitem"];
            deductcurrency = (string)vals["deductcurrency"];
            returntype = (ReturnType)vals["returntype"];
            countdown = (int)vals["countdown"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class OnsenJson
    {
        [JsonProperty("0")]
        public int id { get; set; } 
        
        [JsonProperty("1")]
        public int waterid { get; set; } 
        
        [JsonProperty("2")]
        public int isActive { get; set; } 
        
        [JsonProperty("3")]
        public int onsenID { get; set; } 
        
        [JsonProperty("4")]
        public int onsenLV { get; set; } 
        
        [JsonProperty("5")]
        public int upkeep { get; set; } 
        
        [JsonProperty("6")]
        public int additionMultiplier { get; set; } 
        
        [JsonProperty("7")]
        public int maxBuildPerUser { get; set; } 
        
        [JsonProperty("8")]
        public int maxUser { get; set; } 
        
        [JsonProperty("9")]
        public int lifetime { get; set; } 
        
        [JsonProperty("10")]
        public int healCeiling { get; set; } 
        
        [JsonProperty("11")]
        public int nHealRate { get; set; } 
        
        [JsonProperty("12")]
        public int sHealRate { get; set; } 
        
        [JsonProperty("13")]
        public string enterTalk { get; set; } 
        
        [JsonProperty("14")]
        public int goldCount { get; set; } 
        
        [JsonProperty("15")]
        public int goldMax { get; set; } 
        
        [JsonProperty("16")]
        public int design { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            id = (int)vals["id"];
            waterid = (int)vals["waterid"];
            isActive = (int)vals["isActive"];
            onsenID = (int)vals["onsenID"];
            onsenLV = (int)vals["onsenLV"];
            upkeep = (int)vals["upkeep"];
            additionMultiplier = (int)vals["additionMultiplier"];
            maxBuildPerUser = (int)vals["maxBuildPerUser"];
            maxUser = (int)vals["maxUser"];
            lifetime = (int)vals["lifetime"];
            healCeiling = (int)vals["healCeiling"];
            nHealRate = (int)vals["nHealRate"];
            sHealRate = (int)vals["sHealRate"];
            enterTalk = (string)vals["enterTalk"];
            goldCount = (int)vals["goldCount"];
            goldMax = (int)vals["goldMax"];
            design = (int)vals["design"];
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class LotteryMain__lotterypointidJson
    {
        [JsonProperty("0")]
        public int lotterymainid { get; set; } 
        
        [JsonProperty("1")]
        public int lotterypointidid { get; set; } 
        
        public void Load(Dictionary<string, object> vals)
        {
            lotterymainid = (int)vals["lotterymainid"];
            lotterypointidid = (int)vals["lotterypointidid"];
        }
        }[JsonObject(MemberSerialization.OptIn)]
        public class Skill__selfsideeffectJson
        {
            [JsonProperty("0")]
            public int skillid { get; set; } 
            
            [JsonProperty("1")]
            public int selfsideeffectid { get; set; } 
            
            public void Load(Dictionary<string, object> vals)
            {
                skillid = (int)vals["skillid"];
                selfsideeffectid = (int)vals["selfsideeffectid"];
            }
            }[JsonObject(MemberSerialization.OptIn)]
            public class Skill__sideeffectsJson
            {
                [JsonProperty("0")]
                public int skillid { get; set; } 
                
                [JsonProperty("1")]
                public int sideeffectsid { get; set; } 
                
                public void Load(Dictionary<string, object> vals)
                {
                    skillid = (int)vals["skillid"];
                    sideeffectsid = (int)vals["sideeffectsid"];
                }
                }[JsonObject(MemberSerialization.OptIn)]
                public class SideEffectGroup__sideeffectsJson
                {
                    [JsonProperty("0")]
                    public int sideeffectgroupid { get; set; } 
                    
                    [JsonProperty("1")]
                    public int sideeffectsid { get; set; } 
                    
                    public void Load(Dictionary<string, object> vals)
                    {
                        sideeffectgroupid = (int)vals["sideeffectgroupid"];
                        sideeffectsid = (int)vals["sideeffectsid"];
                    }
                }


    [JsonObject(MemberSerialization.OptIn)]
    public class GameDBRepo
    {
        [JsonProperty]
        public uint GameDBHash { get; set; }

        [JsonProperty]
        public Dictionary<int, LinkUIJson> LinkUI = new Dictionary<int, LinkUIJson>();

        [JsonProperty]
        public Dictionary<int, ItemOriginJson> ItemOrigin = new Dictionary<int, ItemOriginJson>();

        [JsonProperty]
        public Dictionary<int, ItemSortJson> ItemSort = new Dictionary<int, ItemSortJson>();

        [JsonProperty]
        public Dictionary<int, PotionFoodJson> PotionFood = new Dictionary<int, PotionFoodJson>();

        [JsonProperty]
        public Dictionary<int, MaterialJson> Material = new Dictionary<int, MaterialJson>();

        [JsonProperty]
        public Dictionary<int, LuckyPickJson> LuckyPick = new Dictionary<int, LuckyPickJson>();

        [JsonProperty]
        public Dictionary<int, HenshinJson> Henshin = new Dictionary<int, HenshinJson>();

        [JsonProperty]
        public Dictionary<int, FeaturesJson> Features = new Dictionary<int, FeaturesJson>();

        [JsonProperty]
        public Dictionary<int, EquipmentJson> Equipment = new Dictionary<int, EquipmentJson>();

        [JsonProperty]
        public Dictionary<int, DNAJson> DNA = new Dictionary<int, DNAJson>();

        [JsonProperty]
        public Dictionary<int, RelicJson> Relic = new Dictionary<int, RelicJson>();

        [JsonProperty]
        public Dictionary<int, QuestItemJson> QuestItem = new Dictionary<int, QuestItemJson>();

        [JsonProperty]
        public Dictionary<int, HeroItemJson> HeroItem = new Dictionary<int, HeroItemJson>();

        [JsonProperty]
        public Dictionary<int, InstanceItemJson> InstanceItem = new Dictionary<int, InstanceItemJson>();

        [JsonProperty]
        public Dictionary<int, ExtraSideEffectJson> ExtraSideEffect = new Dictionary<int, ExtraSideEffectJson>();

        [JsonProperty]
        public Dictionary<int, EvolveGroupJson> EvolveGroup = new Dictionary<int, EvolveGroupJson>();

        [JsonProperty]
        public Dictionary<int, ItemMallItemJson> ItemMallItem = new Dictionary<int, ItemMallItemJson>();

        [JsonProperty]
        public Dictionary<int, ShopItemMapTreasureJson> ShopItemMapTreasure = new Dictionary<int, ShopItemMapTreasureJson>();

        [JsonProperty]
        public Dictionary<int, StoreSetJson> StoreSet = new Dictionary<int, StoreSetJson>();

        [JsonProperty]
        public Dictionary<int, ProductSettingJson> ProductSetting = new Dictionary<int, ProductSettingJson>();

        [JsonProperty]
        public Dictionary<int, StoreRefreshJson> StoreRefresh = new Dictionary<int, StoreRefreshJson>();

        [JsonProperty]
        public Dictionary<int, EquipmentUpgradeJson> EquipmentUpgrade = new Dictionary<int, EquipmentUpgradeJson>();

        [JsonProperty]
        public Dictionary<int, EquipmentReformGroupJson> EquipmentReformGroup = new Dictionary<int, EquipmentReformGroupJson>();

        [JsonProperty]
        public Dictionary<int, ExchangeShopItemJson> ExchangeShopItem = new Dictionary<int, ExchangeShopItemJson>();

        [JsonProperty]
        public Dictionary<int, ExchangeShopCategoryJson> ExchangeShopCategory = new Dictionary<int, ExchangeShopCategoryJson>();

        [JsonProperty]
        public Dictionary<int, CraftingJson> Crafting = new Dictionary<int, CraftingJson>();

        [JsonProperty]
        public Dictionary<int, CraftingCategoryJson> CraftingCategory = new Dictionary<int, CraftingCategoryJson>();

        [JsonProperty]
        public Dictionary<int, JobsectJson> Jobsect = new Dictionary<int, JobsectJson>();

        [JsonProperty]
        public Dictionary<int, GenderInfoJson> GenderInfo = new Dictionary<int, GenderInfoJson>();

        [JsonProperty]
        public Dictionary<int, SurnameJson> Surname = new Dictionary<int, SurnameJson>();

        [JsonProperty]
        public Dictionary<int, MaleNameJson> MaleName = new Dictionary<int, MaleNameJson>();

        [JsonProperty]
        public Dictionary<int, FemaleNameJson> FemaleName = new Dictionary<int, FemaleNameJson>();

        [JsonProperty]
        public Dictionary<int, PortraitJson> Portrait = new Dictionary<int, PortraitJson>();

        [JsonProperty]
        public Dictionary<int, LevelUpExpJson> LevelUpExp = new Dictionary<int, LevelUpExpJson>();

        [JsonProperty]
        public Dictionary<int, StatsJson> Stats = new Dictionary<int, StatsJson>();

        [JsonProperty]
        public Dictionary<int, SkillPointJson> SkillPoint = new Dictionary<int, SkillPointJson>();

        [JsonProperty]
        public Dictionary<int, ExpMonsterLvDifferenceJson> ExpMonsterLvDifference = new Dictionary<int, ExpMonsterLvDifferenceJson>();

        [JsonProperty]
        public Dictionary<int, JobCombatStatsJson> JobCombatStats = new Dictionary<int, JobCombatStatsJson>();

        [JsonProperty]
        public Dictionary<int, ExpRewardJson> ExpReward = new Dictionary<int, ExpRewardJson>();

        [JsonProperty]
        public Dictionary<int, LevelJson> Level = new Dictionary<int, LevelJson>();

        [JsonProperty]
        public Dictionary<int, MapCategoryJson> MapCategory = new Dictionary<int, MapCategoryJson>();

        [JsonProperty]
        public Dictionary<int, RealmWorldJson> RealmWorld = new Dictionary<int, RealmWorldJson>();

        [JsonProperty]
        public Dictionary<int, DungeonStoryJson> DungeonStory = new Dictionary<int, DungeonStoryJson>();

        [JsonProperty]
        public Dictionary<int, DungeonDailySpecialJson> DungeonDailySpecial = new Dictionary<int, DungeonDailySpecialJson>();

        [JsonProperty]
        public Dictionary<int, RealmObjectiveJson> RealmObjective = new Dictionary<int, RealmObjectiveJson>();

        [JsonProperty]
        public Dictionary<int, WordFilterJson> WordFilter = new Dictionary<int, WordFilterJson>();

        [JsonProperty]
        public Dictionary<int, RewardListJson> RewardList = new Dictionary<int, RewardListJson>();

        [JsonProperty]
        public Dictionary<int, ExperienceRateJson> ExperienceRate = new Dictionary<int, ExperienceRateJson>();

        [JsonProperty]
        public Dictionary<int, ActivityRewardJson> ActivityReward = new Dictionary<int, ActivityRewardJson>();

        [JsonProperty]
        public Dictionary<int, RewardGroupJson> RewardGroup = new Dictionary<int, RewardGroupJson>();

        [JsonProperty]
        public Dictionary<int, RandomBoxRewardJson> RandomBoxReward = new Dictionary<int, RandomBoxRewardJson>();

        [JsonProperty]
        public Dictionary<int, PrizeGuaranteeJson> PrizeGuarantee = new Dictionary<int, PrizeGuaranteeJson>();

        [JsonProperty]
        public Dictionary<int, DonateRewardIdJson> DonateRewardId = new Dictionary<int, DonateRewardIdJson>();

        [JsonProperty]
        public Dictionary<int, CombatNPCJson> CombatNPC = new Dictionary<int, CombatNPCJson>();

        [JsonProperty]
        public Dictionary<int, StaticNPCJson> StaticNPC = new Dictionary<int, StaticNPCJson>();

        [JsonProperty]
        public Dictionary<int, RealmNPCGroupJson> RealmNPCGroup = new Dictionary<int, RealmNPCGroupJson>();

        [JsonProperty]
        public Dictionary<int, BossAIJson> BossAI = new Dictionary<int, BossAIJson>();

        [JsonProperty]
        public Dictionary<int, SpecialBossJson> SpecialBoss = new Dictionary<int, SpecialBossJson>();

        [JsonProperty]
        public Dictionary<int, LootCorrectionJson> LootCorrection = new Dictionary<int, LootCorrectionJson>();

        [JsonProperty]
        public Dictionary<int, LootItemGroupJson> LootItemGroup = new Dictionary<int, LootItemGroupJson>();

        [JsonProperty]
        public Dictionary<int, LootLinkJson> LootLink = new Dictionary<int, LootLinkJson>();

        [JsonProperty]
        public Dictionary<int, EventLootLinkJson> EventLootLink = new Dictionary<int, EventLootLinkJson>();

        [JsonProperty]
        public Dictionary<int, LimitedItemJson> LimitedItem = new Dictionary<int, LimitedItemJson>();

        [JsonProperty]
        public Dictionary<int, GUILocalizedStringJson> GUILocalizedString = new Dictionary<int, GUILocalizedStringJson>();

        [JsonProperty]
        public Dictionary<int, SystemMessageJson> SystemMessage = new Dictionary<int, SystemMessageJson>();

        [JsonProperty]
        public Dictionary<int, ChapterJson> Chapter = new Dictionary<int, ChapterJson>();

        [JsonProperty]
        public Dictionary<int, WonderfulJson> Wonderful = new Dictionary<int, WonderfulJson>();

        [JsonProperty]
        public Dictionary<int, QuestJson> Quest = new Dictionary<int, QuestJson>();

        [JsonProperty]
        public Dictionary<int, QuestObjectiveJson> QuestObjective = new Dictionary<int, QuestObjectiveJson>();

        [JsonProperty]
        public Dictionary<int, QuestTalkDetailJson> QuestTalkDetail = new Dictionary<int, QuestTalkDetailJson>();

        [JsonProperty]
        public Dictionary<int, QuestSelectDetailJson> QuestSelectDetail = new Dictionary<int, QuestSelectDetailJson>();

        [JsonProperty]
        public Dictionary<int, QuestInteractiveDetailJson> QuestInteractiveDetail = new Dictionary<int, QuestInteractiveDetailJson>();

        [JsonProperty]
        public Dictionary<int, QuestRequirementDetailJson> QuestRequirementDetail = new Dictionary<int, QuestRequirementDetailJson>();

        [JsonProperty]
        public Dictionary<int, QuestEventDetailJson> QuestEventDetail = new Dictionary<int, QuestEventDetailJson>();

        [JsonProperty]
        public Dictionary<int, QuestDestinyJson> QuestDestiny = new Dictionary<int, QuestDestinyJson>();

        [JsonProperty]
        public Dictionary<int, SkillGroupJson> SkillGroup = new Dictionary<int, SkillGroupJson>();

        [JsonProperty]
        public Dictionary<int, SkillJson> Skill = new Dictionary<int, SkillJson>();

        [JsonProperty]
        public Dictionary<int, SkillTreeJson> SkillTree = new Dictionary<int, SkillTreeJson>();

        [JsonProperty]
        public Dictionary<int, SideEffectJson> SideEffect = new Dictionary<int, SideEffectJson>();

        [JsonProperty]
        public Dictionary<int, SkillDescriptionGroupJson> SkillDescriptionGroup = new Dictionary<int, SkillDescriptionGroupJson>();

        [JsonProperty]
        public Dictionary<int, SideEffectGroupJson> SideEffectGroup = new Dictionary<int, SideEffectGroupJson>();

        [JsonProperty]
        public Dictionary<int, NPCToSkillsLinkJson> NPCToSkillsLink = new Dictionary<int, NPCToSkillsLinkJson>();

        [JsonProperty]
        public Dictionary<int, ElementChartJson> ElementChart = new Dictionary<int, ElementChartJson>();

        [JsonProperty]
        public Dictionary<int, WeaknessChartJson> WeaknessChart = new Dictionary<int, WeaknessChartJson>();

        [JsonProperty]
        public Dictionary<int, HeroJson> Hero = new Dictionary<int, HeroJson>();

        [JsonProperty]
        public Dictionary<int, HeroGrowthJson> HeroGrowth = new Dictionary<int, HeroGrowthJson>();

        [JsonProperty]
        public Dictionary<int, HeroInterestGroupJson> HeroInterestGroup = new Dictionary<int, HeroInterestGroupJson>();

        [JsonProperty]
        public Dictionary<int, HeroInterestJson> HeroInterest = new Dictionary<int, HeroInterestJson>();

        [JsonProperty]
        public Dictionary<int, HeroTrustJson> HeroTrust = new Dictionary<int, HeroTrustJson>();

        [JsonProperty]
        public Dictionary<int, HeroBondGroupJson> HeroBondGroup = new Dictionary<int, HeroBondGroupJson>();

        [JsonProperty]
        public Dictionary<int, HeroBondJson> HeroBond = new Dictionary<int, HeroBondJson>();

        [JsonProperty]
        public Dictionary<int, ExplorationMapJson> ExplorationMap = new Dictionary<int, ExplorationMapJson>();

        [JsonProperty]
        public Dictionary<int, ExplorationTargetJson> ExplorationTarget = new Dictionary<int, ExplorationTargetJson>();

        [JsonProperty]
        public Dictionary<int, TerrainEfficiencyJson> TerrainEfficiency = new Dictionary<int, TerrainEfficiencyJson>();

        [JsonProperty]
        public Dictionary<int, TerrainJson> Terrain = new Dictionary<int, TerrainJson>();

        [JsonProperty]
        public Dictionary<int, GuildConstantJson> GuildConstant = new Dictionary<int, GuildConstantJson>();

        [JsonProperty]
        public Dictionary<int, GuildSMBossJson> GuildSMBoss = new Dictionary<int, GuildSMBossJson>();

        [JsonProperty]
        public Dictionary<int, GuildTechClassJson> GuildTechClass = new Dictionary<int, GuildTechClassJson>();

        [JsonProperty]
        public Dictionary<int, GuildTechLevelJson> GuildTechLevel = new Dictionary<int, GuildTechLevelJson>();

        [JsonProperty]
        public Dictionary<int, GuildQuestJson> GuildQuest = new Dictionary<int, GuildQuestJson>();

        [JsonProperty]
        public Dictionary<int, GuildDreamhouseJson> GuildDreamhouse = new Dictionary<int, GuildDreamhouseJson>();

        [JsonProperty]
        public Dictionary<int, GuildBadgeJson> GuildBadge = new Dictionary<int, GuildBadgeJson>();

        [JsonProperty]
        public Dictionary<int, PartyLocationJson> PartyLocation = new Dictionary<int, PartyLocationJson>();

        [JsonProperty]
        public Dictionary<int, SignInPrizeJson> SignInPrize = new Dictionary<int, SignInPrizeJson>();

        [JsonProperty]
        public Dictionary<int, OnlinePrizeJson> OnlinePrize = new Dictionary<int, OnlinePrizeJson>();

        [JsonProperty]
        public Dictionary<int, RestrictionJson> Restriction = new Dictionary<int, RestrictionJson>();

        [JsonProperty]
        public Dictionary<int, NewServerActivitySubJson> NewServerActivitySub = new Dictionary<int, NewServerActivitySubJson>();

        [JsonProperty]
        public Dictionary<int, NewServerActivityJson> NewServerActivity = new Dictionary<int, NewServerActivityJson>();

        [JsonProperty]
        public Dictionary<int, GameConstantJson> GameConstant = new Dictionary<int, GameConstantJson>();

        [JsonProperty]
        public Dictionary<int, CurrencyWalletJson> CurrencyWallet = new Dictionary<int, CurrencyWalletJson>();

        [JsonProperty]
        public Dictionary<int, ActivityOverviewJson> ActivityOverview = new Dictionary<int, ActivityOverviewJson>();

        [JsonProperty]
        public Dictionary<int, CurrencyExchangeJson> CurrencyExchange = new Dictionary<int, CurrencyExchangeJson>();

        [JsonProperty]
        public Dictionary<int, MailContentJson> MailContent = new Dictionary<int, MailContentJson>();

        [JsonProperty]
        public Dictionary<int, TeleportNPCListJson> TeleportNPCList = new Dictionary<int, TeleportNPCListJson>();

        [JsonProperty]
        public Dictionary<int, RareItemNotificationJson> RareItemNotification = new Dictionary<int, RareItemNotificationJson>();

        [JsonProperty]
        public Dictionary<int, PushNotificationJson> PushNotification = new Dictionary<int, PushNotificationJson>();

        [JsonProperty]
        public Dictionary<int, LotteryMainJson> LotteryMain = new Dictionary<int, LotteryMainJson>();

        [JsonProperty]
        public Dictionary<int, LotteryItemJson> LotteryItem = new Dictionary<int, LotteryItemJson>();

        [JsonProperty]
        public Dictionary<int, LotteryPointRewardJson> LotteryPointReward = new Dictionary<int, LotteryPointRewardJson>();

        [JsonProperty]
        public Dictionary<int, TopUpItemAndroidJson> TopUpItemAndroid = new Dictionary<int, TopUpItemAndroidJson>();

        [JsonProperty]
        public Dictionary<int, TopUpItemAppleJson> TopUpItemApple = new Dictionary<int, TopUpItemAppleJson>();

        [JsonProperty]
        public Dictionary<int, TopUpItemMyCardJson> TopUpItemMyCard = new Dictionary<int, TopUpItemMyCardJson>();

        [JsonProperty]
        public Dictionary<int, QuestExtraTaskJson> QuestExtraTask = new Dictionary<int, QuestExtraTaskJson>();

        [JsonProperty]
        public Dictionary<int, QuestExtraRewardJson> QuestExtraReward = new Dictionary<int, QuestExtraRewardJson>();

        [JsonProperty]
        public Dictionary<int, RespawnJson> Respawn = new Dictionary<int, RespawnJson>();

        [JsonProperty]
        public Dictionary<int, OnsenJson> Onsen = new Dictionary<int, OnsenJson>();

        [JsonProperty]
        public List<LotteryMain__lotterypointidJson> LotteryMain__lotterypointid = new List<LotteryMain__lotterypointidJson>();

        [JsonProperty]
        public List<Skill__selfsideeffectJson> Skill__selfsideeffect = new List<Skill__selfsideeffectJson>();

        [JsonProperty]
        public List<Skill__sideeffectsJson> Skill__sideeffects = new List<Skill__sideeffectsJson>();

        [JsonProperty]
        public List<SideEffectGroup__sideeffectsJson> SideEffectGroup__sideeffects = new List<SideEffectGroup__sideeffectsJson>();


        public bool IsValidSchemaHash()
        {
            return GameDBHash == GetSchemaHash();
        }

        static public uint GetSchemaHash()
        {
            uint res = 0;

            foreach (FieldInfo fi in typeof(GameDBRepo).GetFields())
            {
                Type fieldtype = fi.FieldType;
                if (fieldtype.IsGenericType && fieldtype.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                {
                    Type valueType = fieldtype.GetGenericArguments()[1];
                    res ^= GetHashCode(fieldtype.Name) ^ GetHash(valueType);
                }
            }

            return res;
        }

        static private uint GetHash(Type T)
        {
            uint res = 0;
            foreach (PropertyInfo pi in T.GetProperties())
            {
                res ^= GetHashCode(pi.PropertyType.Name) ^ GetHashCode(pi.Name);
            }

            return res;
        }

        static private uint GetHashCode(string name)
        {
            uint hash = 0;
            foreach(char c in name)
            {
                uint charcode = (uint)Char.GetNumericValue(c);
                hash = ((hash << 5) - hash) + charcode;
                hash |= 0;
            }
            return hash;
        }

#if SERVICECONTRACTS
        public void Add<T>(T item)
        {
            Type itemtype = typeof(T);
            switch (itemtype.Name)
            {
                case "LinkUIJson":
                LinkUI.Add((item as LinkUIJson).id, item as LinkUIJson);
                break;
                case "ItemOriginJson":
                ItemOrigin.Add((item as ItemOriginJson).id, item as ItemOriginJson);
                break;
                case "ItemSortJson":
                ItemSort.Add((item as ItemSortJson).id, item as ItemSortJson);
                break;
                case "PotionFoodJson":
                PotionFood.Add((item as PotionFoodJson).id, item as PotionFoodJson);
                break;
                case "MaterialJson":
                Material.Add((item as MaterialJson).id, item as MaterialJson);
                break;
                case "LuckyPickJson":
                LuckyPick.Add((item as LuckyPickJson).id, item as LuckyPickJson);
                break;
                case "HenshinJson":
                Henshin.Add((item as HenshinJson).id, item as HenshinJson);
                break;
                case "FeaturesJson":
                Features.Add((item as FeaturesJson).id, item as FeaturesJson);
                break;
                case "EquipmentJson":
                Equipment.Add((item as EquipmentJson).id, item as EquipmentJson);
                break;
                case "DNAJson":
                DNA.Add((item as DNAJson).id, item as DNAJson);
                break;
                case "RelicJson":
                Relic.Add((item as RelicJson).id, item as RelicJson);
                break;
                case "QuestItemJson":
                QuestItem.Add((item as QuestItemJson).id, item as QuestItemJson);
                break;
                case "HeroItemJson":
                HeroItem.Add((item as HeroItemJson).id, item as HeroItemJson);
                break;
                case "InstanceItemJson":
                InstanceItem.Add((item as InstanceItemJson).id, item as InstanceItemJson);
                break;
                case "ExtraSideEffectJson":
                ExtraSideEffect.Add((item as ExtraSideEffectJson).id, item as ExtraSideEffectJson);
                break;
                case "EvolveGroupJson":
                EvolveGroup.Add((item as EvolveGroupJson).id, item as EvolveGroupJson);
                break;
                case "ItemMallItemJson":
                ItemMallItem.Add((item as ItemMallItemJson).id, item as ItemMallItemJson);
                break;
                case "ShopItemMapTreasureJson":
                ShopItemMapTreasure.Add((item as ShopItemMapTreasureJson).id, item as ShopItemMapTreasureJson);
                break;
                case "StoreSetJson":
                StoreSet.Add((item as StoreSetJson).id, item as StoreSetJson);
                break;
                case "ProductSettingJson":
                ProductSetting.Add((item as ProductSettingJson).id, item as ProductSettingJson);
                break;
                case "StoreRefreshJson":
                StoreRefresh.Add((item as StoreRefreshJson).id, item as StoreRefreshJson);
                break;
                case "EquipmentUpgradeJson":
                EquipmentUpgrade.Add((item as EquipmentUpgradeJson).id, item as EquipmentUpgradeJson);
                break;
                case "EquipmentReformGroupJson":
                EquipmentReformGroup.Add((item as EquipmentReformGroupJson).id, item as EquipmentReformGroupJson);
                break;
                case "ExchangeShopItemJson":
                ExchangeShopItem.Add((item as ExchangeShopItemJson).id, item as ExchangeShopItemJson);
                break;
                case "ExchangeShopCategoryJson":
                ExchangeShopCategory.Add((item as ExchangeShopCategoryJson).id, item as ExchangeShopCategoryJson);
                break;
                case "CraftingJson":
                Crafting.Add((item as CraftingJson).id, item as CraftingJson);
                break;
                case "CraftingCategoryJson":
                CraftingCategory.Add((item as CraftingCategoryJson).id, item as CraftingCategoryJson);
                break;
                case "JobsectJson":
                Jobsect.Add((item as JobsectJson).id, item as JobsectJson);
                break;
                case "GenderInfoJson":
                GenderInfo.Add((item as GenderInfoJson).id, item as GenderInfoJson);
                break;
                case "SurnameJson":
                Surname.Add((item as SurnameJson).id, item as SurnameJson);
                break;
                case "MaleNameJson":
                MaleName.Add((item as MaleNameJson).id, item as MaleNameJson);
                break;
                case "FemaleNameJson":
                FemaleName.Add((item as FemaleNameJson).id, item as FemaleNameJson);
                break;
                case "PortraitJson":
                Portrait.Add((item as PortraitJson).id, item as PortraitJson);
                break;
                case "LevelUpExpJson":
                LevelUpExp.Add((item as LevelUpExpJson).id, item as LevelUpExpJson);
                break;
                case "StatsJson":
                Stats.Add((item as StatsJson).id, item as StatsJson);
                break;
                case "SkillPointJson":
                SkillPoint.Add((item as SkillPointJson).id, item as SkillPointJson);
                break;
                case "ExpMonsterLvDifferenceJson":
                ExpMonsterLvDifference.Add((item as ExpMonsterLvDifferenceJson).id, item as ExpMonsterLvDifferenceJson);
                break;
                case "JobCombatStatsJson":
                JobCombatStats.Add((item as JobCombatStatsJson).id, item as JobCombatStatsJson);
                break;
                case "ExpRewardJson":
                ExpReward.Add((item as ExpRewardJson).id, item as ExpRewardJson);
                break;
                case "LevelJson":
                Level.Add((item as LevelJson).id, item as LevelJson);
                break;
                case "MapCategoryJson":
                MapCategory.Add((item as MapCategoryJson).id, item as MapCategoryJson);
                break;
                case "RealmWorldJson":
                RealmWorld.Add((item as RealmWorldJson).id, item as RealmWorldJson);
                break;
                case "DungeonStoryJson":
                DungeonStory.Add((item as DungeonStoryJson).id, item as DungeonStoryJson);
                break;
                case "DungeonDailySpecialJson":
                DungeonDailySpecial.Add((item as DungeonDailySpecialJson).id, item as DungeonDailySpecialJson);
                break;
                case "RealmObjectiveJson":
                RealmObjective.Add((item as RealmObjectiveJson).id, item as RealmObjectiveJson);
                break;
                case "WordFilterJson":
                WordFilter.Add((item as WordFilterJson).id, item as WordFilterJson);
                break;
                case "RewardListJson":
                RewardList.Add((item as RewardListJson).id, item as RewardListJson);
                break;
                case "ExperienceRateJson":
                ExperienceRate.Add((item as ExperienceRateJson).id, item as ExperienceRateJson);
                break;
                case "ActivityRewardJson":
                ActivityReward.Add((item as ActivityRewardJson).id, item as ActivityRewardJson);
                break;
                case "RewardGroupJson":
                RewardGroup.Add((item as RewardGroupJson).id, item as RewardGroupJson);
                break;
                case "RandomBoxRewardJson":
                RandomBoxReward.Add((item as RandomBoxRewardJson).id, item as RandomBoxRewardJson);
                break;
                case "PrizeGuaranteeJson":
                PrizeGuarantee.Add((item as PrizeGuaranteeJson).id, item as PrizeGuaranteeJson);
                break;
                case "DonateRewardIdJson":
                DonateRewardId.Add((item as DonateRewardIdJson).id, item as DonateRewardIdJson);
                break;
                case "CombatNPCJson":
                CombatNPC.Add((item as CombatNPCJson).id, item as CombatNPCJson);
                break;
                case "StaticNPCJson":
                StaticNPC.Add((item as StaticNPCJson).id, item as StaticNPCJson);
                break;
                case "RealmNPCGroupJson":
                RealmNPCGroup.Add((item as RealmNPCGroupJson).id, item as RealmNPCGroupJson);
                break;
                case "BossAIJson":
                BossAI.Add((item as BossAIJson).id, item as BossAIJson);
                break;
                case "SpecialBossJson":
                SpecialBoss.Add((item as SpecialBossJson).id, item as SpecialBossJson);
                break;
                case "LootCorrectionJson":
                LootCorrection.Add((item as LootCorrectionJson).id, item as LootCorrectionJson);
                break;
                case "LootItemGroupJson":
                LootItemGroup.Add((item as LootItemGroupJson).id, item as LootItemGroupJson);
                break;
                case "LootLinkJson":
                LootLink.Add((item as LootLinkJson).id, item as LootLinkJson);
                break;
                case "EventLootLinkJson":
                EventLootLink.Add((item as EventLootLinkJson).id, item as EventLootLinkJson);
                break;
                case "LimitedItemJson":
                LimitedItem.Add((item as LimitedItemJson).id, item as LimitedItemJson);
                break;
                case "GUILocalizedStringJson":
                GUILocalizedString.Add((item as GUILocalizedStringJson).id, item as GUILocalizedStringJson);
                break;
                case "SystemMessageJson":
                SystemMessage.Add((item as SystemMessageJson).id, item as SystemMessageJson);
                break;
                case "ChapterJson":
                Chapter.Add((item as ChapterJson).id, item as ChapterJson);
                break;
                case "WonderfulJson":
                Wonderful.Add((item as WonderfulJson).id, item as WonderfulJson);
                break;
                case "QuestJson":
                Quest.Add((item as QuestJson).id, item as QuestJson);
                break;
                case "QuestObjectiveJson":
                QuestObjective.Add((item as QuestObjectiveJson).id, item as QuestObjectiveJson);
                break;
                case "QuestTalkDetailJson":
                QuestTalkDetail.Add((item as QuestTalkDetailJson).id, item as QuestTalkDetailJson);
                break;
                case "QuestSelectDetailJson":
                QuestSelectDetail.Add((item as QuestSelectDetailJson).id, item as QuestSelectDetailJson);
                break;
                case "QuestInteractiveDetailJson":
                QuestInteractiveDetail.Add((item as QuestInteractiveDetailJson).id, item as QuestInteractiveDetailJson);
                break;
                case "QuestRequirementDetailJson":
                QuestRequirementDetail.Add((item as QuestRequirementDetailJson).id, item as QuestRequirementDetailJson);
                break;
                case "QuestEventDetailJson":
                QuestEventDetail.Add((item as QuestEventDetailJson).id, item as QuestEventDetailJson);
                break;
                case "QuestDestinyJson":
                QuestDestiny.Add((item as QuestDestinyJson).id, item as QuestDestinyJson);
                break;
                case "SkillGroupJson":
                SkillGroup.Add((item as SkillGroupJson).id, item as SkillGroupJson);
                break;
                case "SkillJson":
                Skill.Add((item as SkillJson).id, item as SkillJson);
                break;
                case "SkillTreeJson":
                SkillTree.Add((item as SkillTreeJson).id, item as SkillTreeJson);
                break;
                case "SideEffectJson":
                SideEffect.Add((item as SideEffectJson).id, item as SideEffectJson);
                break;
                case "SkillDescriptionGroupJson":
                SkillDescriptionGroup.Add((item as SkillDescriptionGroupJson).id, item as SkillDescriptionGroupJson);
                break;
                case "SideEffectGroupJson":
                SideEffectGroup.Add((item as SideEffectGroupJson).id, item as SideEffectGroupJson);
                break;
                case "NPCToSkillsLinkJson":
                NPCToSkillsLink.Add((item as NPCToSkillsLinkJson).id, item as NPCToSkillsLinkJson);
                break;
                case "ElementChartJson":
                ElementChart.Add((item as ElementChartJson).id, item as ElementChartJson);
                break;
                case "WeaknessChartJson":
                WeaknessChart.Add((item as WeaknessChartJson).id, item as WeaknessChartJson);
                break;
                case "HeroJson":
                Hero.Add((item as HeroJson).id, item as HeroJson);
                break;
                case "HeroGrowthJson":
                HeroGrowth.Add((item as HeroGrowthJson).id, item as HeroGrowthJson);
                break;
                case "HeroInterestGroupJson":
                HeroInterestGroup.Add((item as HeroInterestGroupJson).id, item as HeroInterestGroupJson);
                break;
                case "HeroInterestJson":
                HeroInterest.Add((item as HeroInterestJson).id, item as HeroInterestJson);
                break;
                case "HeroTrustJson":
                HeroTrust.Add((item as HeroTrustJson).id, item as HeroTrustJson);
                break;
                case "HeroBondGroupJson":
                HeroBondGroup.Add((item as HeroBondGroupJson).id, item as HeroBondGroupJson);
                break;
                case "HeroBondJson":
                HeroBond.Add((item as HeroBondJson).id, item as HeroBondJson);
                break;
                case "ExplorationMapJson":
                ExplorationMap.Add((item as ExplorationMapJson).id, item as ExplorationMapJson);
                break;
                case "ExplorationTargetJson":
                ExplorationTarget.Add((item as ExplorationTargetJson).id, item as ExplorationTargetJson);
                break;
                case "TerrainEfficiencyJson":
                TerrainEfficiency.Add((item as TerrainEfficiencyJson).id, item as TerrainEfficiencyJson);
                break;
                case "TerrainJson":
                Terrain.Add((item as TerrainJson).id, item as TerrainJson);
                break;
                case "GuildConstantJson":
                GuildConstant.Add((item as GuildConstantJson).id, item as GuildConstantJson);
                break;
                case "GuildSMBossJson":
                GuildSMBoss.Add((item as GuildSMBossJson).id, item as GuildSMBossJson);
                break;
                case "GuildTechClassJson":
                GuildTechClass.Add((item as GuildTechClassJson).id, item as GuildTechClassJson);
                break;
                case "GuildTechLevelJson":
                GuildTechLevel.Add((item as GuildTechLevelJson).id, item as GuildTechLevelJson);
                break;
                case "GuildQuestJson":
                GuildQuest.Add((item as GuildQuestJson).id, item as GuildQuestJson);
                break;
                case "GuildDreamhouseJson":
                GuildDreamhouse.Add((item as GuildDreamhouseJson).id, item as GuildDreamhouseJson);
                break;
                case "GuildBadgeJson":
                GuildBadge.Add((item as GuildBadgeJson).id, item as GuildBadgeJson);
                break;
                case "PartyLocationJson":
                PartyLocation.Add((item as PartyLocationJson).id, item as PartyLocationJson);
                break;
                case "SignInPrizeJson":
                SignInPrize.Add((item as SignInPrizeJson).id, item as SignInPrizeJson);
                break;
                case "OnlinePrizeJson":
                OnlinePrize.Add((item as OnlinePrizeJson).id, item as OnlinePrizeJson);
                break;
                case "RestrictionJson":
                Restriction.Add((item as RestrictionJson).id, item as RestrictionJson);
                break;
                case "NewServerActivitySubJson":
                NewServerActivitySub.Add((item as NewServerActivitySubJson).id, item as NewServerActivitySubJson);
                break;
                case "NewServerActivityJson":
                NewServerActivity.Add((item as NewServerActivityJson).id, item as NewServerActivityJson);
                break;
                case "GameConstantJson":
                GameConstant.Add((item as GameConstantJson).id, item as GameConstantJson);
                break;
                case "CurrencyWalletJson":
                CurrencyWallet.Add((item as CurrencyWalletJson).id, item as CurrencyWalletJson);
                break;
                case "ActivityOverviewJson":
                ActivityOverview.Add((item as ActivityOverviewJson).id, item as ActivityOverviewJson);
                break;
                case "CurrencyExchangeJson":
                CurrencyExchange.Add((item as CurrencyExchangeJson).id, item as CurrencyExchangeJson);
                break;
                case "MailContentJson":
                MailContent.Add((item as MailContentJson).id, item as MailContentJson);
                break;
                case "TeleportNPCListJson":
                TeleportNPCList.Add((item as TeleportNPCListJson).id, item as TeleportNPCListJson);
                break;
                case "RareItemNotificationJson":
                RareItemNotification.Add((item as RareItemNotificationJson).id, item as RareItemNotificationJson);
                break;
                case "PushNotificationJson":
                PushNotification.Add((item as PushNotificationJson).id, item as PushNotificationJson);
                break;
                case "LotteryMainJson":
                LotteryMain.Add((item as LotteryMainJson).id, item as LotteryMainJson);
                break;
                case "LotteryItemJson":
                LotteryItem.Add((item as LotteryItemJson).id, item as LotteryItemJson);
                break;
                case "LotteryPointRewardJson":
                LotteryPointReward.Add((item as LotteryPointRewardJson).id, item as LotteryPointRewardJson);
                break;
                case "TopUpItemAndroidJson":
                TopUpItemAndroid.Add((item as TopUpItemAndroidJson).id, item as TopUpItemAndroidJson);
                break;
                case "TopUpItemAppleJson":
                TopUpItemApple.Add((item as TopUpItemAppleJson).id, item as TopUpItemAppleJson);
                break;
                case "TopUpItemMyCardJson":
                TopUpItemMyCard.Add((item as TopUpItemMyCardJson).id, item as TopUpItemMyCardJson);
                break;
                case "QuestExtraTaskJson":
                QuestExtraTask.Add((item as QuestExtraTaskJson).id, item as QuestExtraTaskJson);
                break;
                case "QuestExtraRewardJson":
                QuestExtraReward.Add((item as QuestExtraRewardJson).id, item as QuestExtraRewardJson);
                break;
                case "RespawnJson":
                Respawn.Add((item as RespawnJson).id, item as RespawnJson);
                break;
                case "OnsenJson":
                Onsen.Add((item as OnsenJson).id, item as OnsenJson);
                break;
                case "LotteryMain__lotterypointidJson":
                LotteryMain__lotterypointid.Add(item as LotteryMain__lotterypointidJson);
                break;
                case "Skill__selfsideeffectJson":
                Skill__selfsideeffect.Add(item as Skill__selfsideeffectJson);
                break;
                case "Skill__sideeffectsJson":
                Skill__sideeffects.Add(item as Skill__sideeffectsJson);
                break;
                case "SideEffectGroup__sideeffectsJson":
                SideEffectGroup__sideeffects.Add(item as SideEffectGroup__sideeffectsJson);
                break;

            }
        }
#endif
    }
}