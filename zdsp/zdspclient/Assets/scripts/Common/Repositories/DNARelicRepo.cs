using Kopio.JsonContracts;
using System.Collections.Generic;
using System;
using System.Linq;
using Zealot.Common;

namespace Zealot.Repository
{
    public class DNARelicRepo
    {
        // DNA
        public static Dictionary<ItemRarity, Dictionary<int, Dictionary<int, DNAUpgradeJson>>> rarityStageLvlUpgradeJsonMap;        // Rarity -> Stage -> Level -> DNAUpgradeJson
        public static Dictionary<DNARollResType, DNAUpgradeRollJson> dnaUpgradeRollResMap;                                          // Roll Result -> DNAUpgradeRollJson
        public static Dictionary<ItemRarity, Dictionary<string, Dictionary<int, DNAEvolveJson>>> rarityTypeStageUpgradeJsonMap;     // Rarity -> Stage -> Level -> DNAEvolveJson

        static DNARelicRepo()
        {
            rarityStageLvlUpgradeJsonMap    = new Dictionary<ItemRarity, Dictionary<int, Dictionary<int, DNAUpgradeJson>>>();
            dnaUpgradeRollResMap            = new Dictionary<DNARollResType, DNAUpgradeRollJson>();
            rarityTypeStageUpgradeJsonMap   = new Dictionary<ItemRarity, Dictionary<string, Dictionary<int, DNAEvolveJson>>>();
        }

        public static void Init(GameDBRepo gameData)
        {
            InitDNA(gameData);
            InitRelic(gameData);
        }

        public static void InitDNA(GameDBRepo gameData)
        {
            foreach(KeyValuePair<int, DNAUpgradeJson> entry in gameData.DNAUpgrade)
            {
                ItemRarity rarity = entry.Value.rarity;
                int stage = entry.Value.stage;
                int level = entry.Value.level;

                if(!rarityStageLvlUpgradeJsonMap.ContainsKey(rarity))
                {
                    Dictionary<int, Dictionary<int, DNAUpgradeJson>> stageLevelJsonMap = new Dictionary<int, Dictionary<int, DNAUpgradeJson>>();
                    rarityStageLvlUpgradeJsonMap.Add(rarity, stageLevelJsonMap);
                }

                if(!rarityStageLvlUpgradeJsonMap[rarity].ContainsKey(stage))
                {
                    Dictionary<int, DNAUpgradeJson> levelJsonMap = new Dictionary<int, DNAUpgradeJson>();
                    rarityStageLvlUpgradeJsonMap[rarity].Add(stage, levelJsonMap);
                }

                if(!rarityStageLvlUpgradeJsonMap[rarity][stage].ContainsKey(level))
                {
                    rarityStageLvlUpgradeJsonMap[rarity][stage].Add(level, entry.Value);
                }
            }
            
            foreach(KeyValuePair<int, DNAUpgradeRollJson> entry in gameData.DNAUpgradeRoll)
            {
                DNARollResType res = entry.Value.result;
                if(!dnaUpgradeRollResMap.ContainsKey(res))
                {
                    dnaUpgradeRollResMap.Add(res, entry.Value);
                }
            }

            foreach(KeyValuePair<int, DNAEvolveJson> entry in gameData.DNAEvolve)
            {
                ItemRarity rarity = entry.Value.rarity;
                string evoType = entry.Value.evotype;
                int stage = entry.Value.stage;

                if(!rarityTypeStageUpgradeJsonMap.ContainsKey(rarity))
                {
                    Dictionary<string, Dictionary<int, DNAEvolveJson>> typeStageJsonMap = new Dictionary<string, Dictionary<int, DNAEvolveJson>>();
                    rarityTypeStageUpgradeJsonMap.Add(rarity, typeStageJsonMap);
                }

                if(!rarityTypeStageUpgradeJsonMap[rarity].ContainsKey(evoType))
                {
                    Dictionary<int, DNAEvolveJson> stageJsonMap = new Dictionary<int, DNAEvolveJson>();
                    rarityTypeStageUpgradeJsonMap[rarity].Add(evoType, stageJsonMap);
                }

                if(!rarityTypeStageUpgradeJsonMap[rarity][evoType].ContainsKey(stage))
                {
                    rarityTypeStageUpgradeJsonMap[rarity][evoType].Add(stage, entry.Value);
                }
            }
        }

        public static void InitRelic(GameDBRepo gameData)
        {

        }

        public static DNAUpgradeJson GetDNAUpgradeData(ItemRarity rarity, int stage, int level)
        {
            if(!rarityStageLvlUpgradeJsonMap.ContainsKey(rarity))
            {
                return null;
            }
            
            if(!rarityStageLvlUpgradeJsonMap[rarity].ContainsKey(stage))
            {
                return null;
            }
            
            if(!rarityStageLvlUpgradeJsonMap[rarity][stage].ContainsKey(level))
            {
                return null;
            }

            return rarityStageLvlUpgradeJsonMap[rarity][stage][level];
        }

        public static DNAEvolveJson GetDNAEvolveData(ItemRarity rarity, string evoType, int stage)
        {
            if(!rarityTypeStageUpgradeJsonMap.ContainsKey(rarity))
            {
                return null;
            }

            if(!rarityTypeStageUpgradeJsonMap[rarity].ContainsKey(evoType))
            {
                return null;
            }

            if(!rarityTypeStageUpgradeJsonMap[rarity][evoType].ContainsKey(stage))
            {
                return null;
            }

            return rarityTypeStageUpgradeJsonMap[rarity][evoType][stage];
        }
    }
}
