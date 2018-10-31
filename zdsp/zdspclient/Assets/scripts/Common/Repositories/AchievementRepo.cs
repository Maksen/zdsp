using Kopio.JsonContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using Zealot.Common;

namespace Zealot.Repository
{
    public abstract class BaseAchievementObjective
    {
        public int id;
        public string localizedName;
        public int exp;
        public AchievementRewardType rewardType;
        public int rewardId;
        public int rewardCount;
        public string rewardIconPath;
    }

    public class CollectionObjective : BaseAchievementObjective
    {
        public CollectionObjectiveJson json;
        public CollectionType type;
        public int targetId;
        public List<SideEffectJson> storeSEs = new List<SideEffectJson>();

        public CollectionObjective(CollectionObjectiveJson json)
        {
            this.json = json;
            id = json.cid;
            localizedName = json.localizedname;
            type = json.ctype;
            targetId = json.targetid;
            exp = json.exp;
            rewardType = json.rewardtype;
            rewardIconPath = json.rewardiconpath;
            if (rewardType != AchievementRewardType.None && !string.IsNullOrEmpty(json.reward))
            {
                string[] reward = json.reward.Split(';');
                rewardId = int.Parse(reward[0]);
                if (reward.Length > 1)
                    rewardCount = int.Parse(reward[1]);
            }
            if (type == CollectionType.Fashion || type == CollectionType.Relic)
            {
                if (!string.IsNullOrEmpty(json.collectse))
                {
                    string[] seArray = json.collectse.Split(';');
                    for (int i = 0; i < seArray.Length; ++i)
                    {
                        SideEffectJson se = SideEffectRepo.GetSideEffect(int.Parse(seArray[i]));
                        if (se != null)
                            storeSEs.Add(se);
                    }
                }
            }
        }
    }

    public class AchievementObjective : BaseAchievementObjective
    {
        public AchievementObjectiveJson json;
        public AchievementType mainType;
        public int subType;
        public AchievementObjectiveType objType;
        public string target;
        public int completeCount;
        public LISAFunction rewardFunction = LISAFunction.None;
        public int rewardFunctionValue;

        public AchievementObjective(AchievementObjectiveJson json, AchievementType maintype)
        {
            this.json = json;
            id = json.achid;
            localizedName = json.localizedname;
            mainType = maintype;
            subType = json.subtype;
            objType = json.objtype;
            target = json.target;
            completeCount = json.count;
            exp = json.exp;
            rewardType = json.rewardtype;
            rewardIconPath = json.rewardiconpath;
            if (rewardType != AchievementRewardType.None && !string.IsNullOrEmpty(json.reward))
            {
                string[] reward = json.reward.Split(';');
                rewardId = int.Parse(reward[0]);
                if (reward.Length > 1)
                    rewardCount = int.Parse(reward[1]);
            }
        }

        public void AddRewardFunction(LISAFunction function, int value)
        {
            if (function != LISAFunction.None)
            {
                rewardFunction = function;
                rewardFunctionValue = value;
            }
        }
    }

    public class AchievementLevel
    {
        public int level;
        public int expToNextLv;
        public string name;
        public string description;
        public bool hasReward;
        public List<ItemInfo> rewardItems = new List<ItemInfo>();
        public Dictionary<CurrencyType, int> currencies = new Dictionary<CurrencyType, int>();
        public List<SideEffectJson> sideEffects = new List<SideEffectJson>();
        public LISAFunction rewardFunction = LISAFunction.None;
        public int rewardFunctionValue;

        public AchievementLevel(AchievementLevelJson json)
        {
            level = json.achlevel;
            expToNextLv = json.expreq;
            name = json.localizedname;
            description = json.localizeddescription;
            hasReward = json.hasreward;
            if (hasReward)
            {
                string[] itemArray = json.itemreward.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < itemArray.Length; ++i)
                {
                    string[] item = itemArray[i].Split(';');
                    rewardItems.Add(new ItemInfo() { itemId = ushort.Parse(item[0]), stackCount = int.Parse(item[1]) });
                }
                string[] currencyArray = json.currencyreward.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < currencyArray.Length; ++i)
                {
                    string[] currency = currencyArray[i].Split(';');
                    currencies.Add((CurrencyType)int.Parse(currency[0]), int.Parse(currency[1]));
                }
                string[] seArray = json.sereward.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < seArray.Length; ++i)
                {
                    SideEffectJson se = SideEffectRepo.GetSideEffect(int.Parse(seArray[i]));
                    if (se != null)
                        sideEffects.Add(se);
                }
            }
        }

        public void AddRewardFunction(LISAFunction function, int value)
        {
            if (function != LISAFunction.None)
            {
                rewardFunction = function;
                rewardFunctionValue = value;
            }
        }
    }

    public class PhotoDescGroup
    {
        public int totalWeight;
        public List<CollectionPhotoJson> jsons = new List<CollectionPhotoJson>();

        public void AddToGroup(CollectionPhotoJson json)
        {
            if (json.weight > 0)
            {
                totalWeight += json.weight;
                jsons.Add(json);
            }
        }

        public string GetRandomDescription()
        {
            int count = jsons.Count;
            int randWeight = GameUtils.RandomInt(1, totalWeight);
            for (int i = 0; i < count; ++i)
            {
                if (randWeight <= jsons[i].weight)
                    return jsons[i].localizeddescription;
                randWeight -= jsons[i].weight;
            }
            return "";
        }
    }

    public class LISAMessageDirectionGroup
    {
        public int totalWeight;
        public List<LISAMsgBehaviourJson> jsons = new List<LISAMsgBehaviourJson>();

        public void AddToGroup(LISAMsgBehaviourJson json)
        {
            if (json.weight > 0)
            {
                jsons.Add(json);
            }
        }

        public LISAMsgDirectionType GetRandomMsgDirection(bool systemOpen, bool rewardTip, bool changeTier)
        {
            totalWeight = 0;
            int count = jsons.Count;
            for (int i = 0; i < count; ++i)
            {
                if (jsons[i].directiontype == LISAMsgDirectionType.SystemOpen && !systemOpen)
                    continue;
                else if (jsons[i].directiontype == LISAMsgDirectionType.RewardTip && !rewardTip)
                    continue;
                else if (jsons[i].directiontype == LISAMsgDirectionType.ChangeTier && !changeTier)
                    continue;
                totalWeight += jsons[i].weight;
            }

            int randWeight = GameUtils.RandomInt(1, totalWeight);
            for (int i = 0; i < count; ++i)
            {
                if (jsons[i].directiontype == LISAMsgDirectionType.SystemOpen && !systemOpen)
                    continue;
                else if (jsons[i].directiontype == LISAMsgDirectionType.RewardTip && !rewardTip)
                    continue;
                else if (jsons[i].directiontype == LISAMsgDirectionType.ChangeTier && !changeTier)
                    continue;
                if (randWeight <= jsons[i].weight)
                    return jsons[i].directiontype;
                randWeight -= jsons[i].weight;
            }
            return LISAMsgDirectionType.Idle;
        }
    }

    public class LISAMessageGroup
    {
        public int totalWeight;
        public List<LISAMsgJson> jsons = new List<LISAMsgJson>();

        public void AddToGroup(LISAMsgJson json)
        {
            if (json.weight > 0)
            {
                totalWeight += json.weight;
                jsons.Add(json);
            }
        }

        public string GetRandomMsg()
        {
            int count = jsons.Count;
            int randWeight = GameUtils.RandomInt(1, totalWeight);
            for (int i = 0; i < count; ++i)
            {
                if (randWeight <= jsons[i].weight)
                    return jsons[i].localizeddescription;
                randWeight -= jsons[i].weight;
            }
            return "";
        }
    }

    public static class AchievementRepo
    {
        // Collections
        public static Dictionary<CollectionType, CollectionCategoryJson> collectionCategories;
        public static Dictionary<int, CollectionObjective> collectionObjectives; // key = id
        public static Dictionary<CollectionType, List<CollectionObjective>> collectionTypeToObjectives; // type -> sorted
        public static Dictionary<Pair<CollectionType, int>, CollectionObjective> collectionKeyToObjective; // server use 
        public static Dictionary<int, PhotoDescGroup> collectionPhotoDescriptions; // party member count -> descriptions

        // Achievements
        public static Dictionary<AchievementType, AchievementMainTypeJson> achievementMainTypes;
        public static Dictionary<int, AchievementSubTypeJson> achievementSubTypes;
        public static Dictionary<AchievementType, List<AchievementSubTypeJson>> achievementMainToSubTypes; // main -> sorted sub
        public static Dictionary<int, AchievementObjective> achievementObjectives; // key = id
        public static Dictionary<int, List<AchievementObjective>> achievementSubTypeToObjectives; // subtype -> sorted
        public static Dictionary<Pair<AchievementObjectiveType, string>, List<AchievementObjective>> achievementKeyToObjectives; // server use
        public static Dictionary<AchievementObjectiveType, List<string>> achievementObjTypeToTargets; // list of targets for each obj type

        public static Dictionary<int, AchievementLevel> achievementLevels;
        public static Dictionary<int, LISATransformTierJson> lisaTiers; // tier -> json
        public static Dictionary<LISAFunctionTriggerType, List<LISAExternalFunctionJson>> lisaExternalFunctions;
        public static Dictionary<LISAFunction, List<LISAExternalFunctionJson>> lisaFunctionTypeToDataList;
        public static Dictionary<LISAMsgBehaviourType, LISAMessageDirectionGroup> lisaMsgBehaviours;
        public static Dictionary<LISAMsgDirectionType, LISAMessageGroup> lisaMsgGroups;

        public static int ACHIEVEMENT_MAX_LEVEL;

        static AchievementRepo()
        {
            collectionCategories = new Dictionary<CollectionType, CollectionCategoryJson>();
            collectionObjectives = new Dictionary<int, CollectionObjective>();
            collectionTypeToObjectives = new Dictionary<CollectionType, List<CollectionObjective>>();
            collectionKeyToObjective = new Dictionary<Pair<CollectionType, int>, CollectionObjective>();
            collectionPhotoDescriptions = new Dictionary<int, PhotoDescGroup>();

            achievementMainTypes = new Dictionary<AchievementType, AchievementMainTypeJson>();
            achievementSubTypes = new Dictionary<int, AchievementSubTypeJson>();
            achievementMainToSubTypes = new Dictionary<AchievementType, List<AchievementSubTypeJson>>();
            achievementObjectives = new Dictionary<int, AchievementObjective>();
            achievementSubTypeToObjectives = new Dictionary<int, List<AchievementObjective>>();
            achievementKeyToObjectives = new Dictionary<Pair<AchievementObjectiveType, string>, List<AchievementObjective>>();
            achievementObjTypeToTargets = new Dictionary<AchievementObjectiveType, List<string>>();

            achievementLevels = new Dictionary<int, AchievementLevel>();
            lisaTiers = new Dictionary<int, LISATransformTierJson>();
            lisaExternalFunctions = new Dictionary<LISAFunctionTriggerType, List<LISAExternalFunctionJson>>();
            lisaFunctionTypeToDataList = new Dictionary<LISAFunction, List<LISAExternalFunctionJson>>();
            lisaMsgBehaviours = new Dictionary<LISAMsgBehaviourType, LISAMessageDirectionGroup>();
            lisaMsgGroups = new Dictionary<LISAMsgDirectionType, LISAMessageGroup>();
        }

        private static void Clear()
        {
            collectionCategories.Clear();
            collectionObjectives.Clear();
            collectionTypeToObjectives.Clear();
            collectionKeyToObjective.Clear();
            collectionPhotoDescriptions.Clear();

            achievementMainTypes.Clear();
            achievementSubTypes.Clear();
            achievementMainToSubTypes.Clear();
            achievementObjectives.Clear();
            achievementSubTypeToObjectives.Clear();
            achievementKeyToObjectives.Clear();
            achievementObjTypeToTargets.Clear();

            achievementLevels.Clear();
            lisaTiers.Clear();
            lisaExternalFunctions.Clear();
            lisaFunctionTypeToDataList.Clear();
            lisaMsgBehaviours.Clear();
            lisaMsgGroups.Clear();
        }

        public static void Init(GameDBRepo gameData)
        {
            Clear();

            foreach (var entry in gameData.CollectionCategory)
                collectionCategories.Add(entry.Value.ctype, entry.Value);

            foreach (var entry in gameData.CollectionObjective)
            {
                CollectionObjective obj = new CollectionObjective(entry.Value);
                collectionObjectives.Add(obj.id, obj);

                if (collectionTypeToObjectives.ContainsKey(obj.type))
                    collectionTypeToObjectives[obj.type].Add(obj);
                else
                    collectionTypeToObjectives.Add(obj.type, new List<CollectionObjective>() { obj });

                collectionKeyToObjective.Add(Pair.Create(obj.type, obj.targetId), obj);
            }
            foreach (var type in collectionTypeToObjectives.Keys.ToList())
                collectionTypeToObjectives[type] = collectionTypeToObjectives[type].OrderBy(x => x.json.sortorder).ToList(); // stable sort

            foreach (var entry in gameData.CollectionPhoto)
            {
                if (!collectionPhotoDescriptions.ContainsKey(entry.Value.membercount))
                    collectionPhotoDescriptions.Add(entry.Value.membercount, new PhotoDescGroup());
                collectionPhotoDescriptions[entry.Value.membercount].AddToGroup(entry.Value);
            }

            foreach (var entry in gameData.AchievementMainType)
                achievementMainTypes.Add(entry.Value.maintype, entry.Value);

            foreach (var entry in gameData.AchievementSubType)
            {
                achievementSubTypes.Add(entry.Key, entry.Value);

                if (achievementMainToSubTypes.ContainsKey(entry.Value.maintype))
                    achievementMainToSubTypes[entry.Value.maintype].Add(entry.Value);
                else
                    achievementMainToSubTypes.Add(entry.Value.maintype, new List<AchievementSubTypeJson>() { entry.Value });
            }
            foreach (var type in achievementMainToSubTypes.Keys.ToList())
                achievementMainToSubTypes[type] = achievementMainToSubTypes[type].OrderBy(x => x.sequence).ToList(); // stable sort

            foreach (var entry in gameData.AchievementObjective)
            {
                if (!achievementSubTypes.ContainsKey(entry.Value.subtype))
                    continue;
                AchievementType mainType = GetAchievementMainTypeBySubType(entry.Value.subtype);
                AchievementObjective obj = new AchievementObjective(entry.Value, mainType);
                achievementObjectives.Add(obj.id, obj);

                if (achievementSubTypeToObjectives.ContainsKey(obj.subType))
                    achievementSubTypeToObjectives[obj.subType].Add(obj);
                else
                    achievementSubTypeToObjectives.Add(obj.subType, new List<AchievementObjective>() { obj });

                var key = Pair.Create(obj.objType, obj.target);
                if (achievementKeyToObjectives.ContainsKey(key))
                    achievementKeyToObjectives[key].Add(obj);
                else
                    achievementKeyToObjectives.Add(key, new List<AchievementObjective>() { obj });

                if (achievementObjTypeToTargets.ContainsKey(obj.objType))
                {
                    if (!achievementObjTypeToTargets[obj.objType].Contains(obj.target))
                        achievementObjTypeToTargets[obj.objType].Add(obj.target);
                }
                else
                    achievementObjTypeToTargets.Add(obj.objType, new List<string>() { obj.target });
            }
            foreach (var type in achievementSubTypeToObjectives.Keys.ToList())
                achievementSubTypeToObjectives[type] = achievementSubTypeToObjectives[type].OrderBy(x => x.json.sortorder).ToList(); // stable sort
            foreach (var key in achievementKeyToObjectives.Keys.ToList())
                achievementKeyToObjectives[key] = achievementKeyToObjectives[key].OrderBy(x => x.json.step).ToList(); // stable sort

            foreach (var entry in gameData.AchievementLevel)
            {
                achievementLevels.Add(entry.Value.achlevel, new AchievementLevel(entry.Value));
                if (entry.Value.achlevel > ACHIEVEMENT_MAX_LEVEL)
                    ACHIEVEMENT_MAX_LEVEL = entry.Value.achlevel;
            }

            foreach (var entry in gameData.LISATransformTier)
                lisaTiers.Add(entry.Value.tierid, entry.Value);

            foreach (var entry in gameData.LISAExternalFunction)
            {
                if (lisaExternalFunctions.ContainsKey(entry.Value.triggertype))
                    lisaExternalFunctions[entry.Value.triggertype].Add(entry.Value);
                else
                    lisaExternalFunctions.Add(entry.Value.triggertype, new List<LISAExternalFunctionJson>() { entry.Value });

                if (entry.Value.triggertype == LISAFunctionTriggerType.AchievementLV)
                {
                    var levelinfo = GetAchievementLevelInfo(entry.Value.triggervalue);
                    if (levelinfo != null)
                        levelinfo.AddRewardFunction(entry.Value.functiontype, entry.Value.functionvalue);
                }
                else // AchievementID
                {
                    var achObj = GetAchievementObjectiveById(entry.Value.triggervalue);
                    if (achObj != null)
                        achObj.AddRewardFunction(entry.Value.functiontype, entry.Value.functionvalue);
                }

                if (lisaFunctionTypeToDataList.ContainsKey(entry.Value.functiontype))
                    lisaFunctionTypeToDataList[entry.Value.functiontype].Add(entry.Value);
                else
                    lisaFunctionTypeToDataList.Add(entry.Value.functiontype, new List<LISAExternalFunctionJson>() { entry.Value });
            }
            foreach (var type in lisaExternalFunctions.Keys.ToList())
                lisaExternalFunctions[type] = lisaExternalFunctions[type].OrderBy(x => x.sortorder).ToList(); // stable sort
            foreach (var type in lisaFunctionTypeToDataList.Keys.ToList())
                lisaFunctionTypeToDataList[type] = lisaFunctionTypeToDataList[type].OrderBy(x => x.sortorder).ToList(); // stable sort

            foreach (var entry in gameData.LISAMsgBehaviour)
            {
                if (!lisaMsgBehaviours.ContainsKey(entry.Value.behaviourtype))
                    lisaMsgBehaviours.Add(entry.Value.behaviourtype, new LISAMessageDirectionGroup());
                lisaMsgBehaviours[entry.Value.behaviourtype].AddToGroup(entry.Value);
            }

            foreach (var entry in gameData.LISAMsg)
            {
                if (!lisaMsgGroups.ContainsKey(entry.Value.directiontype))
                    lisaMsgGroups.Add(entry.Value.directiontype, new LISAMessageGroup());
                lisaMsgGroups[entry.Value.directiontype].AddToGroup(entry.Value);
            }
        }

        public static BaseAchievementObjective GetObjectiveByTypeAndId(AchievementKind type, int id)
        {
            if (type == AchievementKind.Collection)
                return GetCollectionObjectiveById(id);
            else
                return GetAchievementObjectiveById(id);
        }

        public static CollectionObjective GetCollectionObjectiveById(int id)
        {
            CollectionObjective obj;
            collectionObjectives.TryGetValue(id, out obj);
            return obj;
        }

        public static CollectionObjective GetCollectionObjectiveByKey(CollectionType objtype, int target)
        {
            var key = Pair.Create(objtype, target);
            CollectionObjective obj;
            collectionKeyToObjective.TryGetValue(key, out obj);
            return obj;
        }

        public static List<CollectionObjective> GetCollectionObjectivesByType(CollectionType objtype)
        {
            List<CollectionObjective> objList;
            if (collectionTypeToObjectives.TryGetValue(objtype, out objList))
                return objList;
            return new List<CollectionObjective>();
        }

        public static string GetRandomPhotoDescription(int memberCount)
        {
            PhotoDescGroup pdg;
            if (collectionPhotoDescriptions.TryGetValue(memberCount, out pdg))
                return pdg.GetRandomDescription();
            return "";
        }

        public static AchievementObjective GetAchievementObjectiveById(int id)
        {
            AchievementObjective obj;
            achievementObjectives.TryGetValue(id, out obj);
            return obj;
        }

        public static List<AchievementObjective> GetAchievementObjectivesByKey(AchievementObjectiveType objType, string target)
        {
            var key = Pair.Create(objType, target);
            List<AchievementObjective> objList;
            achievementKeyToObjectives.TryGetValue(key, out objList);
            return objList;
        }

        public static List<AchievementObjective> GetAchievementObjectivesBySubType(int subTypeId)
        {
            List<AchievementObjective> list;
            if (achievementSubTypeToObjectives.TryGetValue(subTypeId, out list))
                return list;
            return new List<AchievementObjective>();
        }

        public static int GetAchievementObjectiveCountByMainType(AchievementType mainType)
        {
            return achievementObjectives.Values.Count(x => x.mainType == mainType);
        }

        public static List<string> GetTargetsByAchievementObjType(AchievementObjectiveType objType)
        {
            List<string> list;
            if (achievementObjTypeToTargets.TryGetValue(objType, out list))
                return list;
            return new List<string>();
        }

        public static AchievementSubTypeJson GetAchievementSubTypeByID(int subTypeId)
        {
            AchievementSubTypeJson json;
            achievementSubTypes.TryGetValue(subTypeId, out json);
            return json;
        }

        public static string GetAchievementSubTypeLocalizedName(int subTypeId)
        {
            AchievementSubTypeJson json;
            if (achievementSubTypes.TryGetValue(subTypeId, out json))
                return json.localizedname;
            return "";
        }

        public static List<AchievementSubTypeJson> GetAchievementSubTypesByMainType(AchievementType mainType)
        {
            List<AchievementSubTypeJson> list;
            if (achievementMainToSubTypes.TryGetValue(mainType, out list))
                return list;
            return new List<AchievementSubTypeJson>();
        }



        public static AchievementType GetAchievementMainTypeBySubType(int subType)
        {
            foreach(var kvp in achievementMainToSubTypes)
            {
                if (kvp.Value.Exists(x => x.id == subType))
                    return kvp.Key;
            }
            return AchievementType.Others;
        }

        public static AchievementLevel GetAchievementLevelInfo(int currentLevel)
        {
            AchievementLevel info;
            achievementLevels.TryGetValue(currentLevel, out info);
            return info;
        }

        public static LISATransformTierJson GetLISATierInfoByTier(int tier)
        {
            LISATransformTierJson json;
            lisaTiers.TryGetValue(tier, out json);
            return json;
        }

        public static LISATransformTierJson GetLISATierInfoByLevel(int level)
        {
            LISATransformTierJson highest = null;
            foreach (var entry in lisaTiers.Values)
            {
                if (highest == null || (level >= entry.reqlvl && entry.reqlvl > highest.reqlvl))
                    highest = entry;
            }
            return highest;
        }

        public static List<LISAExternalFunctionJson> GetExternalFunctionsByTriggerType(LISAFunctionTriggerType triggerType)
        {
            List<LISAExternalFunctionJson> list;
            if (lisaExternalFunctions.TryGetValue(triggerType, out list))
                return list;
            return new List<LISAExternalFunctionJson>();
        }

        public static List<LISAExternalFunctionJson> GetExternalFunctionsByFunctionType(LISAFunction functionType)
        {
            List<LISAExternalFunctionJson> list;
            lisaFunctionTypeToDataList.TryGetValue(functionType, out list);
            return list;
        }
    }
}