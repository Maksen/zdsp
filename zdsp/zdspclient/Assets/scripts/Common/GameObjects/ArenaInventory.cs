using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.ComponentModel;

namespace Zealot.Common
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class ArenaInventoryData
    {
        #region serializable properties
        [DefaultValue(500)]
        [JsonProperty(PropertyName = "rank")]
        public int ArenaRankHighest { get; set; } //歷史最高排名

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "entry")]
        public int Entries { get; set; }

        [JsonProperty(PropertyName = "battledt")]
        public DateTime LastBattleDT { get; set; }

        [JsonProperty(PropertyName = "rewarddt")]
        public DateTime LastRewardDT { get; set; }   
        #endregion

        public ArenaInventoryData()
        {
            ArenaRankHighest = 500;
        }
    }

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class BonusCombatStats
    {
        [DefaultValue(0)]
        [JsonProperty(PropertyName = "hp")]
        public int HealthBonus { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "hppct")]
        public int HealthPercentBonus { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "atk")]
        public int AttackBonus { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "atkpct")]
        public int AttackPercentBonus { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "amr")]
        public int ArmorBonus { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "amrpct")]
        public int ArmorPercentBonus { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "acc")]
        public int AccuracyBonus { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "accpct")]
        public int AccuracyPercentBonus { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "eva")]
        public int EvasionBonus { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "evapct")]
        public int EvasionPercentBonus { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "cri")]
        public int CriticalBonus { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "cripct")]
        public int CriticalPercentBonus { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "cocri")]
        public int CoCriticalBonus { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "cocripct")]
        public int CoCriticalPercentBonus { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "cridmg")]
        public int CriticalDmgBonus { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "cridmgpct")]
        public int CriticalDmgPercentBonus { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "cocridmg")]
        public int CoCriticalDmgBonus { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "cocridmgpct")]
        public int CoCriticalDmgPercentBonus { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "absorb")]
        public int AbsorbDmgBonus { get; set; }
    }

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class ArenaTalentStats
    {
        [DefaultValue(0)]
        [JsonProperty(PropertyName = "sc")]
        public int TalentScissor { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "rk")]
        public int TalentRock { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "pp")]
        public int TalentPaper { get; set; }
    }

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class ArenaSkillLevel
    {
        [DefaultValue(1)]
        [JsonProperty(PropertyName = "rd")]
        public int RedLvl { get; set; }

        [DefaultValue(1)]
        [JsonProperty(PropertyName = "rds")]
        public int RedSubLvl { get; set; }

        [DefaultValue(1)]
        [JsonProperty(PropertyName = "gr")]
        public int GreenLvl { get; set; }

        [DefaultValue(1)]
        [JsonProperty(PropertyName = "grs")]
        public int GreenSubLvl { get; set; }

        [DefaultValue(1)]
        [JsonProperty(PropertyName = "bl")]
        public int BlueLvl { get; set; }

        [DefaultValue(1)]
        [JsonProperty(PropertyName = "bls")]
        public int BlueSubLvl { get; set; }
    }

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class ArenaPlayerRecord
    {
        [DefaultValue(false)]
        [JsonProperty(PropertyName = "fake")]
        public bool Fake { get; set; }

        [JsonProperty(PropertyName = "data")]
        public CharacterCreationData CharacterCreationData { get; set; }

        [JsonProperty(PropertyName = "combatstats")]
        public BonusCombatStats BonusCombatStats { get; set; }

        [JsonProperty(PropertyName = "skillInv")]
        public SkillInventoryData SkillInventory { get; set; }

        [JsonProperty(PropertyName = "skilllv")]
        public ArenaSkillLevel ArenaSkillLevel { get; set; }

        [JsonProperty(PropertyName = "talent")]
        public ArenaTalentStats ArenaTalentStats { get; set; }

        public ArenaPlayerRecord()
        {
            CharacterCreationData = new CharacterCreationData();
            BonusCombatStats = new BonusCombatStats();
            SkillInventory = new SkillInventoryData();
            ArenaSkillLevel = new ArenaSkillLevel();
            ArenaTalentStats = new ArenaTalentStats();
        }

        public string GetDisplayString()
        {
            if (Fake)
                return "";
            return CharacterCreationData.SerializeForCharCreation();
        }
    }

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class ArenaRankingRecords
    {
        [JsonProperty(PropertyName = "records")]
        public List<ArenaPlayerRecord> PlayerRecords = new List<ArenaPlayerRecord>(); //max count of 500;

        public List<string> RecordString;
        public bool IsDirty;

        public ArenaRankingRecords()
        {
            RecordString = new List<string>();
            IsDirty = false;
        }

        public string SerializeForDB(bool formatIndented = false)
        {
            JsonSerializerSettings jsonSetting = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None, DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore };
            Formatting formatting = formatIndented ? Formatting.Indented : Formatting.None;
            jsonSetting.Converters.Add(new DBInventoryItemConverter());
            return JsonConvert.SerializeObject(this, formatting, jsonSetting);
        }

        public static ArenaRankingRecords DeserializeFromDB(string data)
        {
            JsonSerializerSettings jsonSetting = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None, DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore };
            jsonSetting.Converters.Add(new DBInventoryItemConverter());
            return JsonConvert.DeserializeObject<ArenaRankingRecords>(data, jsonSetting);
        }

        public void Add(ArenaPlayerRecord record)
        {
            PlayerRecords.Add(record);
            string recordString = record.GetDisplayString();
            RecordString.Add(recordString);
            IsDirty = true;
        } 
    }

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class ArenaChallengers
    {
        [JsonProperty(PropertyName = "rank")]
        public List<int> Ranks = new List<int>(); //max count of 6;

        [JsonProperty(PropertyName = "infos")]
        public List<string> Infos = new List<string>(); //max count of 6

        public string Serialize(bool formatIndented = false)
        {
            JsonSerializerSettings jsonSetting = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None, DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore };
            Formatting formatting = formatIndented ? Formatting.Indented : Formatting.None;
            return JsonConvert.SerializeObject(this, formatting, jsonSetting);
        }

        public static ArenaChallengers Deserialize(string data)
        {
            JsonSerializerSettings jsonSetting = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None, DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore };
            return JsonConvert.DeserializeObject<ArenaChallengers>(data, jsonSetting);
        }
    }
}
