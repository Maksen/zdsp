using Kopio.JsonContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using Zealot.Common;

namespace Zealot.Repository
{
    public class CollectionObjective
    {
        public CollectionObjectiveJson json;
        public int id;
        public CollectionType type;
        public int targetId;
        public int exp;
        public AchievementRewardType rewardType;
        public int rewardId;
        public int rewardCount;
        public List<SideEffectJson> collectSEs = new List<SideEffectJson>();

        public CollectionObjective(CollectionObjectiveJson json)
        {
            this.json = json;
            id = json.cid;
            type = json.ctype;
            targetId = json.targetid;
            exp = json.exp;
            rewardType = json.rewardtype;
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
                            collectSEs.Add(se);
                    }
                }
            }
        }
    }

    public class AchievementObjective
    {
        public AchievementObjectiveJson json;
        public int id;
        public int slotIdx;
        public int subType;
        public AchievementObjectiveType objType;
        public string target;
        public int completeCount;
        public int exp;
        public AchievementRewardType rewardType;
        public int rewardId;
        public int rewardCount;

        public AchievementObjective(AchievementObjectiveJson json, int idx)
        {
            this.json = json;
            id = json.achid;
            slotIdx = idx;
            subType = json.subtype;
            objType = json.objtype;
            target = json.target;
            completeCount = json.count;
            exp = json.exp;
            rewardType = json.rewardtype;
            if (rewardType != AchievementRewardType.None && !string.IsNullOrEmpty(json.reward))
            {
                string[] reward = json.reward.Split(';');
                rewardId = int.Parse(reward[0]);
                if (reward.Length > 1)
                    rewardCount = int.Parse(reward[1]);
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
        public List<RewardItem> rewardItems = new List<RewardItem>();
        public Dictionary<CurrencyType, int> currencies = new Dictionary<CurrencyType, int>();
        public List<SideEffectJson> sideEffects = new List<SideEffectJson>();

        public AchievementLevel(AchievementLevelJson json)
        {
            level = json.achlevel;
            expToNextLv = json.experience;
            name = json.localizedname;
            description = json.localizeddescription;
            hasReward = json.hasreward;
            if (hasReward)
            {
                string[] itemArray = json.itemreward.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < itemArray.Length; ++i)
                {
                    string[] item = itemArray[i].Split(';');
                    rewardItems.Add(new RewardItem(int.Parse(item[0]), int.Parse(item[1])));
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
        public static Dictionary<int, AchievementMainTypeJson> achievementMainTypes;
        public static Dictionary<int, AchievementSubTypeJson> achievementSubTypes;
        public static Dictionary<int, List<AchievementSubTypeJson>> achievementMainToSubTypes; // main -> sorted sub
        public static Dictionary<int, AchievementObjective> achievementObjectives; // key = id
        public static Dictionary<int, List<AchievementObjective>> achievementSubTypeToObjectives; // subtype -> sorted
        public static Dictionary<Pair<AchievementObjectiveType, string>, List<AchievementObjective>> achievementKeyToObjectives; // server use
        public static Dictionary<int, AchievementLevel> achievementLevels;
        public static Dictionary<int, LISATransformTierJson> lisaTiers; // tier -> json
        public static List<LISARewardJson> lisaRewards;
        public static Dictionary<LISAMsgBehaviourType, LISAMessageDirectionGroup> lisaMsgBehaviours;
        public static Dictionary<LISAMsgDirectionType, LISAMessageGroup> lisaMsgGroups;

        static AchievementRepo()
        {
            collectionCategories = new Dictionary<CollectionType, CollectionCategoryJson>();
            collectionObjectives = new Dictionary<int, CollectionObjective>();
            collectionTypeToObjectives = new Dictionary<CollectionType, List<CollectionObjective>>();
            collectionKeyToObjective = new Dictionary<Pair<CollectionType, int>, CollectionObjective>();
            collectionPhotoDescriptions = new Dictionary<int, PhotoDescGroup>();

            achievementMainTypes = new Dictionary<int, AchievementMainTypeJson>();
            achievementSubTypes = new Dictionary<int, AchievementSubTypeJson>();
            achievementMainToSubTypes = new Dictionary<int, List<AchievementSubTypeJson>>();
            achievementObjectives = new Dictionary<int, AchievementObjective>();
            achievementSubTypeToObjectives = new Dictionary<int, List<AchievementObjective>>();
            achievementKeyToObjectives = new Dictionary<Pair<AchievementObjectiveType, string>, List<AchievementObjective>>();
            achievementLevels = new Dictionary<int, AchievementLevel>();
            lisaTiers = new Dictionary<int, LISATransformTierJson>();
            lisaRewards = new List<LISARewardJson>();
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
            achievementLevels.Clear();
            lisaTiers.Clear();
            lisaRewards.Clear();
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
                if (!collectionCategories.ContainsKey(entry.Value.ctype))
                    continue;
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

            achievementMainTypes = gameData.AchievementMainType;

            // for main type to collectionhandler index (in case type id deleted or not consecutive)
            Dictionary<int, int> achieveMainTypeToIndexMap = new Dictionary<int, int>();
            int index = 0;
            foreach (var entry in achievementMainTypes)
                achieveMainTypeToIndexMap[entry.Value.id] = index++;

            foreach (var entry in gameData.AchievementSubType)
            {
                if (!achievementMainTypes.ContainsKey(entry.Value.maintype))
                    continue;
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
                AchievementObjective obj = new AchievementObjective(entry.Value, achieveMainTypeToIndexMap[GetAchievementMainTypeBySubType(entry.Value.subtype)]);
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
            }
            foreach (var type in achievementSubTypeToObjectives.Keys.ToList())
                achievementSubTypeToObjectives[type] = achievementSubTypeToObjectives[type].OrderBy(x => x.json.sortorder).ToList(); // stable sort
            foreach (var key in achievementKeyToObjectives.Keys.ToList())
                achievementKeyToObjectives[key] = achievementKeyToObjectives[key].OrderBy(x => x.json.step).ToList(); // stable sort

            foreach (var entry in gameData.AchievementLevel)
                achievementLevels.Add(entry.Value.achlevel, new AchievementLevel(entry.Value));

            foreach (var entry in gameData.LISATransformTier)
                lisaTiers.Add(entry.Value.tierid, entry.Value);

            lisaRewards = gameData.LISAReward.Values.OrderBy(x => x.sortorder).ToList();

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

        public static AchievementObjective GetAchievementObjectiveById(int id)
        {
            AchievementObjective obj;
            achievementObjectives.TryGetValue(id, out obj);
            return obj;
        }

        public static List<AchievementObjective> GetAchievementObjectivesByKey(AchievementObjectiveType type, string target)
        {
            var key = Pair.Create(type, target);
            List<AchievementObjective> objList;
            achievementKeyToObjectives.TryGetValue(key, out objList);
            return objList;
        }

        public static string GetRandomPhotoDescription(int memberCount)
        {
            PhotoDescGroup pdg;
            if (collectionPhotoDescriptions.TryGetValue(memberCount, out pdg))
                return pdg.GetRandomDescription();
            return "";
        }

        public static int GetAchievementMainTypeBySubType(int subType)
        {
            foreach(var kvp in achievementMainToSubTypes)
            {
                if (kvp.Value.Exists(x => x.id == subType))
                    return kvp.Key;
            }
            return 0;
        }
    }
}