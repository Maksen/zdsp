using Kopio.JsonContracts;
using System.Collections.Generic;
using Zealot.Common.Datablock;
using Zealot.Repository;

namespace Zealot.Common.Entities
{
    public class RealmInfo
    {
        public int Sequence = 0;
        public int LootRewardLimit = 0;
        public int LocalObjIdx = 0;

        public RealmInfo(int sequence, int lootRewardLimit, int localObjIdx)
        {
            Sequence = sequence;
            LootRewardLimit = lootRewardLimit;
            LocalObjIdx = localObjIdx;
        }

        public RealmInfo(string info, int localObjIdx)
        {
            string[] strArray = info.Split(';');
            if (strArray.Length == 2)
            {
                int.TryParse(strArray[0], out Sequence);
                int.TryParse(strArray[1], out LootRewardLimit);
            }
            LocalObjIdx = localObjIdx;
        }

        public bool HasLootReward()
        {
            return LootRewardLimit > 0;
        }

        public override string ToString()
        {
            return string.Format("{0};{1}", Sequence, LootRewardLimit);
        }  
    }

    public class RealmStats : LocalObject
    {     
        public RealmStats() : base(LOTYPE.RealmStats)
        {
            DungeonStory = new CollectionHandler<object>(40);
            DungeonStory.SetParent(this, "DungeonStory");
            dungeonStoryInfos = new Dictionary<int, RealmInfo>();
        }

        public CollectionHandler<object> DungeonStory { get; set; }
        public Dictionary<int, RealmInfo> GetDungeonStoryInfos() { return dungeonStoryInfos; }
        private Dictionary<int, RealmInfo> dungeonStoryInfos; //sequence <- RealmInfo

        public void ResetOnNewDay()
        {
            List<int> seqsToRemove = new List<int>();
            foreach (KeyValuePair<int, RealmInfo> kvp in dungeonStoryInfos)
            {
                RealmInfo realmInfo = kvp.Value;
                Dictionary<DungeonDifficulty, DungeonJson> dungeonInfo = RealmRepo.GetDungeonByTypeAndSeq(DungeonType.Story, realmInfo.Sequence);
                if (dungeonInfo != null)
                {
                    DungeonJson dungeonJson = dungeonInfo.ContainsKey(DungeonDifficulty.Easy)
                        ? dungeonInfo[DungeonDifficulty.Easy] : dungeonInfo[DungeonDifficulty.None];
                    if (dungeonJson.lootresettype == LootResetType.Daily)
                    {
                        seqsToRemove.Add(realmInfo.Sequence);
                        DungeonStory[realmInfo.LocalObjIdx] = null;
                    }
                }
            }
            seqsToRemove.ForEach(seq => dungeonStoryInfos.Remove(seq));
        }

        public void Init(RealmInventoryData realmInvData)
        {
            List<RealmData> realmDataList = realmInvData.DungeonStory;
            int count = realmDataList.Count;
            for (int i = 0; i < count; ++i)
            {
                RealmData data = realmDataList[i];
                int seq = data.Sequence;
                RealmInfo info = new RealmInfo(seq, data.LootRewardLimit, i);
                dungeonStoryInfos[seq] = info;
                DungeonStory[i] = info.ToString();
            }
        }

        public int DecreaseLootRewardCount(DungeonJson dungeonJson, bool clearAll)
        {
            int seq = dungeonJson.sequence;
            if (dungeonStoryInfos.ContainsKey(seq))
            {
                RealmInfo info = dungeonStoryInfos[seq];
                if (info.HasLootReward())
                {
                    int amt = clearAll ? info.LootRewardLimit : 1;
                    info.LootRewardLimit -= amt;
                    DungeonStory[info.LocalObjIdx] = info.ToString();
                    return amt;
                }
            }
            else
            {
                int count = DungeonStory.Count;
                for (int i = 0; i < count; ++i)
                {
                    if (DungeonStory[i] == null)
                    {
                        int amt = clearAll ? dungeonJson.lootlimit : 1;
                        RealmInfo info = new RealmInfo(seq, dungeonJson.lootlimit-amt, i);
                        dungeonStoryInfos[seq] = info;
                        DungeonStory[i] = info.ToString();
                        return amt;
                    }
                }
            }
            return 0;
        }
    }
}
