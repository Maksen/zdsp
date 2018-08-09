using System.Collections.Generic;
using Kopio.JsonContracts;
using System;
using System.Linq;
using Zealot.Common;

namespace Zealot.Repository
{
    public enum EquipUpgradeMatType
    {
        UpgradeStone,
        UpgradeDiamond,
        SafeGemGeneral,
        SafeGemAdvanced,
    }

    public class EquipUpgMaterial
    {
        public EquipUpgradeMatType mType;
        public EquipModMaterial mMat;

        public EquipUpgMaterial(EquipUpgradeMatType type, int amount)
        {
            mType = type;
            mMat = new EquipModMaterial(GetItemIdByMaterialType(type), amount);
        }

        private int GetItemIdByMaterialType(EquipUpgradeMatType type)
        {
            int itemId = 0;

            switch(type)
            {
                case EquipUpgradeMatType.UpgradeStone:
                    itemId = GameConstantRepo.GetConstantInt("upgrade_stone");
                    break;
                case EquipUpgradeMatType.UpgradeDiamond:
                    itemId = GameConstantRepo.GetConstantInt("upgrade_diamond");
                    break;
                case EquipUpgradeMatType.SafeGemGeneral:
                    itemId = GameConstantRepo.GetConstantInt("safe_gem_general");
                    break;
                case EquipUpgradeMatType.SafeGemAdvanced:
                    itemId = GameConstantRepo.GetConstantInt("safe_gem_advanced");
                    break;
            }

            return itemId;
        }
    }

    public enum EquipUpgProbResult
    {
        Error,
        Success,
        Fail,
        Drop,
    }

    public class EquipUpgradeProb
    {
        private List<int> _Weights;
        private List<EquipUpgProbResult> _Results;

        public EquipUpgradeProb(int successWeight, int failWeight, int dropWeight)
        {
            _Weights = new List<int>();
            _Weights.Add(successWeight);
            _Weights.Add(failWeight);
            _Weights.Add(dropWeight);

            _Results = new List<EquipUpgProbResult>();
            _Results.Add(EquipUpgProbResult.Success);
            _Results.Add(EquipUpgProbResult.Fail);
            _Results.Add(EquipUpgProbResult.Drop);
        }

        public EquipUpgProbResult RollUpgrade()
        {
            int totalWeight = GetFullWeight();
            List<int> probabilities = GetBuckets();

            Random rand = GameUtils.GetRandomGenerator();
            int pick = rand.Next(0, totalWeight);

            return GetResult(probabilities, pick);
        }

        private EquipUpgProbResult GetResult(List<int> prob, int pick)
        {
            int pos = -1;

            if (prob.Count == 0 || pick < 0 || pick > prob[prob.Count - 1])
            {
                return EquipUpgProbResult.Error;
            }

            if ((pick >= 0 && pick <= prob[0]))
            {
                pos = 0;
            }
            else
            {
                for (int i = 1; i < prob.Count; ++i)
                {
                    if (pick > prob[i - 1] && pick <= prob[i])
                    {
                        pos = i;
                    }
                }
            }

            return _Results[pos];
        }

        public int GetFullWeight()
        {
            int sum = 0;

            for (int i = 0; i < _Weights.Count; ++i)
            {
                sum += _Weights[i];
            }

            return sum;
        }

        private List<int> GetBuckets()
        {
            List<int> buckets = new List<int>();
            int curr = 0;

            for (int i = 0; i < _Weights.Count; ++i)
            {
                curr += _Weights[i];
                buckets.Add(curr);
            }

            return buckets;
        }
    }

    public class EquipModMaterial
    {
        public int mItemID;
        public int mAmount;

        public EquipModMaterial(int itemId, int amount)
        {
            mItemID = itemId;
            mAmount = amount;
        }
    }

    public class EquipReformData
    {
        public int mReformStep;
        public EquipmentReformGroupJson mReformData;

        public EquipReformData(int reformStep, EquipmentReformGroupJson reformData)
        {
            mReformStep = reformStep;
            mReformData = reformData;
        }

        public List<int> GetSideEffects()
        {
            List<int> seIds = new List<int>();
            List<string> seIdStrList = mReformData.sideeffect.Split(';').ToList();
            
            for(int i = 0; i < seIdStrList.Count; ++i)
            {
                int seId = 0;
                if(int.TryParse(seIdStrList[i], out seId))
                {
                    seIds.Add(seId);
                }
            }

            return seIds;
        }
    }

    public class EquipmentModdingRepo
    {
        // Equipment Upgrade
        private static Dictionary<EquipmentType, Dictionary<ItemRarity, Dictionary<int, EquipmentUpgradeJson>>> equipUpgradeTypeRarityIDJsonMap;    // Equipment Type -> Rarity -> Upgrade Level -> Json

        // Equipment Reform
        private static Dictionary<string, Dictionary<int, List<EquipmentReformGroupJson>>>  equipReformJsonMap;     // Reform Group -> Reform Step -> Json List
        private static Dictionary<string, int>                                              equipReformMaxLvlMap;   // Reform Group -> Max Level

        static EquipmentModdingRepo()
        {
            // Equipment Upgrade
            equipUpgradeTypeRarityIDJsonMap = new Dictionary<EquipmentType, Dictionary<ItemRarity, Dictionary<int, EquipmentUpgradeJson>>>();

            // Equipment Reform
            equipReformJsonMap      = new Dictionary<string, Dictionary<int, List<EquipmentReformGroupJson>>>();
            equipReformMaxLvlMap    = new Dictionary<string, int>();
        }

        public static void Init(GameDBRepo gameData)
        {
            InitUpgrade(gameData);
            InitReform(gameData);
        }

        public static void InitUpgrade(GameDBRepo gameData)
        {
            foreach(KeyValuePair<int, EquipmentUpgradeJson> entry in gameData.EquipmentUpgrade)
            {
                EquipmentType equipType = entry.Value.type;
                ItemRarity rarity = entry.Value.rarity;
                int upgradeLvl = entry.Value.upgradelvl;
                if(!equipUpgradeTypeRarityIDJsonMap.ContainsKey(equipType))
                {
                    Dictionary<ItemRarity, Dictionary<int, EquipmentUpgradeJson>> newTypeMap = new Dictionary<ItemRarity, Dictionary<int, EquipmentUpgradeJson>>();
                    equipUpgradeTypeRarityIDJsonMap.Add(equipType, newTypeMap);
                }

                if(!equipUpgradeTypeRarityIDJsonMap[equipType].ContainsKey(rarity))
                {
                    Dictionary<int, EquipmentUpgradeJson> newRarityMap = new Dictionary<int, EquipmentUpgradeJson>();
                    equipUpgradeTypeRarityIDJsonMap[equipType].Add(rarity, newRarityMap);
                }

                if(!equipUpgradeTypeRarityIDJsonMap[equipType][rarity].ContainsKey(upgradeLvl))
                {
                    equipUpgradeTypeRarityIDJsonMap[equipType][rarity].Add(upgradeLvl, entry.Value);
                }
            }
        }

        public static void InitReform(GameDBRepo gameData)
        {
            string prevGrpId = "";
            int maxLvl = 0;
            foreach(KeyValuePair<int, EquipmentReformGroupJson> entry in gameData.EquipmentReformGroup)
            {
                string grpId = entry.Value.grpid;
                int reformStep = entry.Value.reformstep;

                if(!string.IsNullOrEmpty(prevGrpId) && !string.Equals(grpId, prevGrpId))
                {
                    if(!equipReformMaxLvlMap.ContainsKey(prevGrpId))
                    {
                        equipReformMaxLvlMap.Add(prevGrpId, maxLvl);
                        maxLvl = 0;
                    }
                }

                if(!equipReformJsonMap.ContainsKey(grpId))
                {
                    Dictionary<int, List<EquipmentReformGroupJson>> newStepJsonMap = new Dictionary<int, List<EquipmentReformGroupJson>>();
                    equipReformJsonMap.Add(grpId, newStepJsonMap);
                }

                if(!equipReformJsonMap[grpId].ContainsKey(reformStep))
                {
                    List<EquipmentReformGroupJson> reformGrpStepsList = new List<EquipmentReformGroupJson>();
                    reformGrpStepsList.Add(entry.Value);
                    equipReformJsonMap[grpId].Add(reformStep, reformGrpStepsList);
                }
                else
                {
                    equipReformJsonMap[grpId][reformStep].Add(entry.Value);
                }

                prevGrpId = grpId;
                ++maxLvl;
            }

            equipReformMaxLvlMap.Add(prevGrpId, maxLvl);
        }

        public static EquipmentUpgradeJson GetEquipmentUpgradeData(EquipmentType equipType, ItemRarity equipRarity, int upgradeLvl)
        {
            if(!equipUpgradeTypeRarityIDJsonMap.ContainsKey(equipType))
            {
                return null;
            }
            
            if(!equipUpgradeTypeRarityIDJsonMap[equipType].ContainsKey(equipRarity))
            {
                return null;
            }
            
            if(!equipUpgradeTypeRarityIDJsonMap[equipType][equipRarity].ContainsKey(upgradeLvl))
            {
                return null;
            }

            return equipUpgradeTypeRarityIDJsonMap[equipType][equipRarity][upgradeLvl];
        }

        public static EquipUpgMaterial GetEquipmentUpgradeMaterial(EquipmentType equipType, ItemRarity equipRarity, int upgradeLvl, bool isUseGenMaterial, bool isSafeUpgrade, 
            bool isSafeUseEquip)
        {
            EquipmentUpgradeJson upgradeData = GetEquipmentUpgradeData(equipType, equipRarity, upgradeLvl);
            if (upgradeData == null)
            {
                return null;
            }

            EquipUpgMaterial equipMaterial = null;
            if(!isSafeUpgrade)
            {
                if(upgradeData.generalstone > 0)
                {
                    if(isUseGenMaterial)
                    {
                        equipMaterial = new EquipUpgMaterial(EquipUpgradeMatType.UpgradeStone, upgradeData.generalstone);
                    }
                    else
                    {
                        equipMaterial = new EquipUpgMaterial(EquipUpgradeMatType.UpgradeDiamond, upgradeData.generalstone);
                    }
                }
            }
            else
            {
                if(upgradeData.safestone > 0)
                {
                    if (isUseGenMaterial)
                    {
                        equipMaterial = new EquipUpgMaterial(EquipUpgradeMatType.SafeGemGeneral, upgradeData.safestone);
                    }
                    else
                    {
                        equipMaterial = new EquipUpgMaterial(EquipUpgradeMatType.SafeGemAdvanced, upgradeData.safestone);
                    }
                }
            }

            return equipMaterial;
        }

        public static List<EquipUpgMaterial> GetEquipmentUpgradeMaterials(EquipmentType equipType, ItemRarity equipRarity, int upgradeLvl, bool isSafeUpgrade)
        {
            EquipmentUpgradeJson upgradeData = GetEquipmentUpgradeData(equipType, equipRarity, upgradeLvl);
            if (upgradeData == null)
            {
                return null;
            }

            List<EquipUpgMaterial> equipMaterials = new List<EquipUpgMaterial>();
            if (!isSafeUpgrade)
            {
                if (upgradeData.generalstone > 0)
                {
                    equipMaterials.Add(new EquipUpgMaterial(EquipUpgradeMatType.UpgradeStone, upgradeData.generalstone));
                    equipMaterials.Add(new EquipUpgMaterial(EquipUpgradeMatType.UpgradeDiamond, upgradeData.generalstone));
                }
            }
            else
            {
                if (upgradeData.safestone > 0)
                {
                    equipMaterials.Add(new EquipUpgMaterial(EquipUpgradeMatType.SafeGemGeneral, upgradeData.safestone));
                    equipMaterials.Add(new EquipUpgMaterial(EquipUpgradeMatType.SafeGemAdvanced, upgradeData.safestone));
                }
            }

            return equipMaterials;
        }

        public static EquipUpgMaterial GetEquipmentUpgradeSafeMaterial(EquipmentType equipType, ItemRarity equipRarity, int upgradeLvl, bool isGenMat)
        {
            EquipmentUpgradeJson upgradeData = GetEquipmentUpgradeData(equipType, equipRarity, upgradeLvl);
            if (upgradeData == null)
            {
                return null;
            }

            EquipUpgradeMatType safeMatType = isGenMat ? EquipUpgradeMatType.SafeGemGeneral : EquipUpgradeMatType.SafeGemAdvanced;

            return new EquipUpgMaterial(safeMatType, upgradeData.safestone);
        }

        public static int GetEquipmentUpgradeCost(EquipmentType equipType, ItemRarity equipRarity, int upgradeLvl, bool isSafeUpgrade)
        {
            EquipmentUpgradeJson upgradeData = GetEquipmentUpgradeData(equipType, equipRarity, upgradeLvl);
            if (upgradeData == null)
            {
                return -1;
            }

            return !isSafeUpgrade ? upgradeData.generalcost : upgradeData.safecost;
        }

        public static List<int> GetEquipmentUpgradeBuff(EquipmentType equipType, ItemRarity equipRarity, int upgradeLvl)
        {
            EquipmentUpgradeJson upgradeData = GetEquipmentUpgradeData(equipType, equipRarity, upgradeLvl);
            if (upgradeData == null)
            {
                return null;
            }

            if(string.IsNullOrEmpty(upgradeData.buff))
            {
                return new List<int>();
            }
            else
            {
                List<string> seStrList = upgradeData.buff.Split(';').ToList();
                List<int> seList = new List<int>();
                for(int i = 0; i < seStrList.Count; ++i)
                {
                    int seId = 0;
                    if(int.TryParse(seStrList[i], out seId))
                    {
                        seList.Add(seId);
                    }
                }

                return seList;
            }
        }

        public static List<int> GetEquipmentUpgradeBuffFromData(EquipmentUpgradeJson upgradeData)
        {
            if (upgradeData == null)
            {
                return null;
            }

            if (string.IsNullOrEmpty(upgradeData.buff))
            {
                return null;
            }
            else
            {
                List<string> seStrList = upgradeData.buff.Split(';').ToList();
                List<int> seList = new List<int>();
                for (int i = 0; i < seStrList.Count; ++i)
                {
                    int seId = 0;
                    if (int.TryParse(seStrList[i], out seId))
                    {
                        seList.Add(seId);
                    }
                }

                return seList;
            }
        }

        public static float GetEquipmentUpgradeSuccessProb(EquipmentType equipType, ItemRarity equipRarity, int upgradeLvl)
        {
            EquipmentUpgradeJson upgradeData = GetEquipmentUpgradeData(equipType, equipRarity, upgradeLvl);
            if (upgradeData == null)
            {
                return -1f;
            }

            EquipUpgradeProb prob = new EquipUpgradeProb(upgradeData.successweight, upgradeData.failweight, upgradeData.dropweight);
            int probTotalWeight = prob.GetFullWeight();

            return ((float)upgradeData.successweight / (float)probTotalWeight) * 100f;
        }

        public static EquipUpgProbResult GetEquipmentUpgradeRoll(EquipmentType equipType, ItemRarity equipRarity, int upgradeLvl)
        {
            EquipmentUpgradeJson upgradeData = GetEquipmentUpgradeData(equipType, equipRarity, upgradeLvl);
            if(upgradeData == null)
            {
                return EquipUpgProbResult.Error;
            }

            EquipUpgradeProb prob = new EquipUpgradeProb(upgradeData.successweight, upgradeData.failweight, upgradeData.dropweight);

            return prob.RollUpgrade();
        }

        public static Dictionary<int, List<EquipmentReformGroupJson>> GetEquipmentReformDataByGroup(string reformGrp)
        {
            if (!equipReformJsonMap.ContainsKey(reformGrp))
            {
                return null;
            }

            return equipReformJsonMap[reformGrp];
        }

        public static List<EquipmentReformGroupJson> GetEquipmentReformDataByGroupStep(string reformGrp, int reformStep)
        {
            if(!equipReformJsonMap.ContainsKey(reformGrp))
            {
                return null;
            }

            if(!equipReformJsonMap[reformGrp].ContainsKey(reformStep))
            {
                return null;
            }

            return equipReformJsonMap[reformGrp][reformStep];
        }

        private static Dictionary<int, List<EquipmentReformGroupJson>> GetEquipmentReformDataToStep(string reformGrp, int reformStep)
        {
            Dictionary<int, List<EquipmentReformGroupJson>> reformGroup = GetEquipmentReformDataByGroup(reformGrp);

            if(reformGroup == null)
            {
                return null;
            }

            Dictionary<int, List<EquipmentReformGroupJson>> relevantData = new Dictionary<int, List<EquipmentReformGroupJson>>();
            int i = 0;
            foreach(KeyValuePair<int, List<EquipmentReformGroupJson>> entry in reformGroup)
            {
                if(i < reformStep)
                {
                    relevantData.Add(entry.Key, entry.Value);
                }
                
                ++i;
            }

            return relevantData;
        }

        public static EquipReformData GetEquipmentReformDataSgl(Equipment equipment)
        {
            EquipReformData reformData = null;

            string reformGrp = equipment.EquipmentJson.evolvegrp;
            int reformStep = equipment.ReformStep;

            if (reformStep == 0)
            {
                return null;
            }

            Dictionary<int, List<EquipmentReformGroupJson>> reformGrpData = GetEquipmentReformDataToStep(reformGrp, reformStep);
            if (reformGrpData == null)
            {
                return null;
            }

            int i = 0;
            foreach (KeyValuePair<int, List<EquipmentReformGroupJson>> entry in reformGrpData)
            {
                List<EquipmentReformGroupJson> reformGrpStepList = entry.Value;

                if (reformGrpStepList.Count == 1)
                {
                    reformData = new EquipReformData(entry.Key, entry.Value[0]);
                }
                else
                {
                    int selection = equipment.Selection[i];
                    reformData = new EquipReformData(entry.Key, entry.Value[selection]);
                }

                ++i;
            }

            return reformData;
        }

        public static List<EquipReformData> GetEquipmentReformData(Equipment equipment)
        {
            List<EquipReformData> reformDataList = new List<EquipReformData>();

            string reformGrp = equipment.EquipmentJson.evolvegrp;
            int reformStep = equipment.ReformStep;

            if(reformStep == 0)
            {
                return null;
            }

            Dictionary<int, List<EquipmentReformGroupJson>> reformGrpData = GetEquipmentReformDataToStep(reformGrp, reformStep);
            if(reformGrpData == null)
            {
                return null;
            }

            int i = 0;
            foreach(KeyValuePair<int, List<EquipmentReformGroupJson>> entry in reformGrpData)
            {
                List<EquipmentReformGroupJson> reformGrpStepList = entry.Value;

                if(reformGrpStepList.Count == 1)
                {
                    reformDataList.Add(new EquipReformData(entry.Key, entry.Value[0]));
                }
                else
                {
                    int selection = equipment.GetSelectionsList()[i];
                    reformDataList.Add(new EquipReformData(entry.Key, entry.Value[selection]));
                }

                ++i;
            }

            return reformDataList;
        }

        public static List<EquipModMaterial> GetEquipmentReformMaterials(string reformGrp, int reformStep, int selection)
        {
            List<EquipmentReformGroupJson> equipReformData = GetEquipmentReformDataByGroupStep(reformGrp, reformStep);
            if(equipReformData == null)
            {
                return null;
            }

            if (selection < 0 || selection >= equipReformData.Count)
            {
                return null;
            }

            List<string> reformMaterialsDataStrList = equipReformData[selection].requirement.Split(';').ToList();
            List<EquipModMaterial> reformMaterialsList = new List<EquipModMaterial>();
            for(int i = 0; i < reformMaterialsDataStrList.Count; ++i)
            {
                List<string> reformMatData = reformMaterialsDataStrList[i].Split('#').ToList();
                int itemId = 0;
                int amount = 0;
                if(int.TryParse(reformMatData[0], out itemId) && int.TryParse(reformMatData[1], out amount))
                {
                    reformMaterialsList.Add(new EquipModMaterial(itemId, amount));
                }
            }

            return reformMaterialsList;
        }

        public static int GetEquipmentReformCost(string reformGrp, int reformStep, int selection)
        {
            List<EquipmentReformGroupJson> equipReformData = GetEquipmentReformDataByGroupStep(reformGrp, reformStep);
            if (equipReformData == null)
            {
                return -1;
            }

            if (selection < 0 || selection >= equipReformData.Count)
            {
                return -1;
            }

            return equipReformData[selection].cost;
        }

        public static int GetEquipmentReformGroupMaxLevel(string reformGrp)
        {
            if(equipReformMaxLvlMap.ContainsKey(reformGrp))
            {
                return equipReformMaxLvlMap[reformGrp];
            }

            return -1;
        }

        public static List<EquipModMaterial> GetModdingMaterialsFromStr(string matStr)
        {
            if(string.IsNullOrEmpty(matStr))
            {
                return null;
            }

            List<string> matStrList = matStr.Split(';').ToList();
            List<EquipModMaterial> materialList = new List<EquipModMaterial>();
            for(int i = 0; i < matStrList.Count; ++i)
            {
                string currStr = matStrList[i];
                if(string.IsNullOrEmpty(currStr))
                {
                    continue;
                }

                List<string> matDataList = currStr.Split('#').ToList();
                if(matDataList.Count != 2)
                {
                    continue;
                }

                int itemId = 0;
                int amount = 0;
                if(int.TryParse(matDataList[0], out itemId) && int.TryParse(matDataList[1], out amount))
                {
                    materialList.Add(new EquipModMaterial(itemId, amount));
                }
            }

            return materialList;
        }
    }
}
