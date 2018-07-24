using Kopio.JsonContracts;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Zealot.Common;
using Zealot.Repository;

public static class HeroData
{
    public static readonly int MAX_HEROES = 40;
    public static readonly int TIER1_UNLOCK = 1;
    public static readonly int TIER2_UNLOCK = 6;
    public static readonly int TIER3_UNLOCK = 10;
}

[JsonObject(MemberSerialization = MemberSerialization.OptIn)]
public class Hero
{
    [JsonProperty(PropertyName = "idx")]
    public int SlotIdx { get; set; }

    [JsonProperty(PropertyName = "id")]
    public int HeroId { get; set; }

    [JsonProperty(PropertyName = "lv")]
    public int Level { get; set; }

    [JsonProperty(PropertyName = "sk1lv")]
    public int Skill1Level { get; set; }

    [JsonProperty(PropertyName = "sk2lv")]
    public int Skill2Level { get; set; }

    [JsonProperty(PropertyName = "sk3lv")]
    public int Skill3Level { get; set; }

    [JsonProperty(PropertyName = "skpts")]
    public int SkillPoints { get; set; }

    [JsonProperty(PropertyName = "like")]
    public HeroInterestType Interest { get; set; }

    [JsonProperty(PropertyName = "tlv")]
    public int TrustLevel { get; set; }

    [JsonProperty(PropertyName = "texp")]
    public int TrustExp { get; set; }

    [JsonProperty(PropertyName = "tier")]
    public int ModelTier { get; set; }

    [JsonProperty(PropertyName = "away")]
    public bool IsAway { get; set; }

    //  Non Serialized
    public HeroJson HeroJson { get; set; }

    public Hero() { }

    public Hero(int id, HeroInterestType interestType, int index, HeroJson json)
    {
        SlotIdx = index;
        HeroId = id;
        Level = 1;
        Skill1Level = 1;
        Skill2Level = 1;
        Skill3Level = 1;
        SkillPoints = 1;
        Interest = interestType;
        TrustLevel = 1;
        TrustExp = 0;
        ModelTier = 1;
        IsAway = false;
        HeroJson = json;
    }

    public static Hero ToObject(string str)
    {
        return JsonConvertDefaultSetting.DeserializeObject<Hero>(str);
    }

    public override string ToString()
    {
        return JsonConvertDefaultSetting.SerializeObject(this);
    }

    public bool CanLevelUp(int playerLevel)
    {
        return Level < playerLevel && HeroRepo.GetHeroGrowthData(HeroJson.growthgroup, Level + 1) != null;
    }

    public int GetTotalSkillPoints()
    {
        return Skill1Level + Skill2Level + Skill3Level + SkillPoints;
    }

    public void ResetSkillPoints()
    {
        SkillPoints += Skill1Level - 1;
        SkillPoints += Skill2Level - 1;
        SkillPoints += Skill3Level - 1;
        Skill1Level = 1;
        Skill2Level = 1;
        Skill3Level = 1;
    }

    public bool CanLevelUpSkill(int skillNo)
    {
        int skillgroupid = 0, currentLevel = 0;
        switch (skillNo)
        {
            case 1:
                skillgroupid = HeroJson.skill1grp;
                currentLevel = Skill1Level;
                break;
            case 2:
                skillgroupid = HeroJson.skill2grp;
                currentLevel = Skill2Level;
                break;
            case 3:
                skillgroupid = HeroJson.skill3grp;
                currentLevel = Skill3Level;
                break;
        }
        return !SkillRepo.IsSkillMaxLevel(skillgroupid, currentLevel);
    }

    public bool IsModelTierUnlocked(int tier)
    {
        int reqPts = 0;
        switch (tier)
        {
            case 1: reqPts = HeroData.TIER1_UNLOCK; break;
            case 2: reqPts = HeroData.TIER2_UNLOCK; break;
            case 3: reqPts = HeroData.TIER3_UNLOCK; break;
        }
        return GetTotalSkillPoints() >= reqPts;
    }

    public int GetTriggeredQuest()
    {
        string[] questStr = HeroJson.questid.Split(';');
        for (int i = 0; i < questStr.Length; i++)
        {
            string[] detail = questStr[i].Split(',');
            int level, questId;
            if (int.TryParse(detail[0], out level) && int.TryParse(detail[1], out questId))
            {
                if (level == TrustLevel)
                    return questId;
            }
        }
        return 0;
    }

    public bool CanSummon(int playerLevel)
    {
        return playerLevel >= HeroJson.summonlevel;
    }
}

[JsonObject(MemberSerialization = MemberSerialization.OptIn)]
public class ExploreMapData
{
    [JsonProperty(PropertyName = "map")]
    public int MapId { get; set; }

    [JsonProperty(PropertyName = "tgt")]
    public int TargetId { get; set; }

    [JsonProperty(PropertyName = "hero")]
    public List<int> HeroIdList { get; set; }

    [JsonProperty(PropertyName = "end")]
    public DateTime EndTime { get; set; }

    //  Non Serialized
    public ExplorationMapJson MapData { get; set; }

    public ExploreMapData() { }

    public ExploreMapData(int map, int target, List<int> heroIds, DateTime end, ExplorationMapJson mapJson)
    {
        MapId = map;
        TargetId = target;
        HeroIdList = heroIds;
        EndTime = end;
        MapData = mapJson;
    }

    public static ExploreMapData ToObject(string str)
    {
        return JsonConvertDefaultSetting.DeserializeObject<ExploreMapData>(str);
    }

    public override string ToString()
    {
        return JsonConvertDefaultSetting.SerializeObject(this);
    }

    public bool IsCompleted()
    {
        return DateTime.Now >= EndTime;
    }

    public float GetHeroEfficiency(Hero hero)
    {
        float terrainEff = HeroRepo.GetTerrainEfficiency(MapData.terraintype, hero.Interest) * 0.01f;
        float levelEff = Math.Min((float)hero.Level * MapData.levelmaxefficiency / MapData.levelrelvalue, MapData.levelmaxefficiency);
        float trustEff = Math.Min((float)hero.TrustLevel * MapData.trustmaxefficiency / MapData.trustrelvalue, MapData.trustmaxefficiency);
        return (levelEff + trustEff) * terrainEff * 0.01f;
    }

    public int GetFulfilledChestCount(List<Hero> heroesList)
    {
        int count = 0;
        if (IsChestRequirementFulfilled(MapData.chestreqtype1, MapData.chestreqvalue1, heroesList))
            count++;
        if (IsChestRequirementFulfilled(MapData.chestreqtype2, MapData.chestreqvalue2, heroesList))
            count++;
        if (IsChestRequirementFulfilled(MapData.chestreqtype3, MapData.chestreqvalue3, heroesList))
            count++;
        return count;
    }

    private bool IsChestRequirementFulfilled(ChestRequirementType type, int value, List<Hero> heroes)
    {
        bool fulfilled = false;
        switch (type)
        {
            case ChestRequirementType.HeroID:
                fulfilled = heroes.Exists(x => x.HeroId == value);
                break;
            case ChestRequirementType.HeroInterest:
                fulfilled = heroes.Exists(x => x.Interest == (HeroInterestType)value);
                break;
            case ChestRequirementType.HeroTrust:
                fulfilled = heroes.Exists(x => x.TrustLevel == value);
                break;
        }
        return fulfilled;
    }
    
}

[JsonObject(MemberSerialization = MemberSerialization.OptIn)]
public class HeroInvData
{
    [JsonProperty(PropertyName = "heroes")]
    public List<Hero> HeroesList = new List<Hero>();

    [JsonProperty(PropertyName = "summon")]
    public int SummonedHero { get; set; }

    [JsonProperty(PropertyName = "inprogress")]
    public List<ExploreMapData> InProgressMaps = new List<ExploreMapData>();

    [JsonProperty(PropertyName = "explored")]
    public string ExploredMaps { get; set; }

}
