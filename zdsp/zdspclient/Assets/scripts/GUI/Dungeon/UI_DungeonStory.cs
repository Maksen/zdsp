using Kopio.JsonContracts;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Client.Entities;
using Zealot.Common;
using Zealot.Common.Entities;
using Zealot.Repository;

public class UI_DungeonStory : MonoBehaviour
{
    [SerializeField]
    DefaultToggleInGroup defaultToggleingrpDayTabs = null;

    [Header("Dungeon Panel")]
    [SerializeField]
    Image imgPreview = null;
    [SerializeField]
    Image imgDifficulty = null;
    [SerializeField]
    Sprite[] difficultyIcons = null;
    [SerializeField]
    Text txtDungeonName = null;
    [SerializeField]
    Text txtLevelReq = null;
    [SerializeField]
    Text txtLootRewardLimit = null;
    [SerializeField]
    UI_RewardDisplay uiRewardDisplay = null;
    [SerializeField]
    Button[] buttonDifficulty = null;
    [SerializeField]
    Button buttonLeftArrow = null;
    [SerializeField]
    Button buttonRightArrow = null;

    DayOfWeek currentDayTab = DayOfWeek.Monday;
    List<Dictionary<DungeonDifficulty, DungeonJson>> currentDungeonInfos = null;
    int dungeonPanelIdx = 0;
    int onInitSequence = 1;
    DungeonDifficulty currentDifficulty = DungeonDifficulty.None;
    DungeonJson currentDungeonInfo = null;

    [NonSerialized]
    public bool InitOnEnable = true;

    // Use this for initialization
    void Awake()
    {
        currentDungeonInfos = new List<Dictionary<DungeonDifficulty, DungeonJson>>();
    }

    void OnEnable()
    {
        if (InitOnEnable)
            Init(1);
    }

    void OnDisable()
    {
        currentDungeonInfos.Clear();

        InitOnEnable = true;
    }

    public void Init(int sequence)
    {
        DayOfWeek todayDayOfWeek = DateTime.Today.DayOfWeek;
        onInitSequence = sequence;
        if (currentDayTab == todayDayOfWeek)
        {
            PopulateCurrentDungeonInfos();
            InitDungeonPanelIndexBySequence();
            InitDungeonPanel(DungeonDifficulty.Easy);
        }
        defaultToggleingrpDayTabs.GoToPage((int)todayDayOfWeek);
    }

    void InitDungeonPanelIndexBySequence()
    {
        dungeonPanelIdx = 0;
        if (onInitSequence != 0)
        {
            int dungeonInfosCount = currentDungeonInfos.Count;
            for (int i = 0; i < dungeonInfosCount; ++i)
            {
                Dictionary<DungeonDifficulty, DungeonJson> dungeonStoryDict = currentDungeonInfos[i];
                DungeonJson dungeonJson = dungeonStoryDict.ContainsKey(DungeonDifficulty.Easy)
                        ? dungeonStoryDict[DungeonDifficulty.Easy] : dungeonStoryDict[DungeonDifficulty.None];
                if (dungeonJson.sequence == onInitSequence)
                {
                    dungeonPanelIdx = i;
                    break;
                }
            }
        }
        onInitSequence = 0;
    }

    void PopulateCurrentDungeonInfos()
    {
        currentDungeonInfos.Clear();
        List<Dictionary<DungeonDifficulty, DungeonJson>> dungeonList = null;
        if (RealmRepo.mDungeons.TryGetValue(DungeonType.Story, out dungeonList))
        {
            Dictionary<int, byte> dungeonDaysOpen = RealmRepo.mDungeonDaysOpen[DungeonType.Story];
            int count = dungeonList.Count;
            for (int i = 0; i < count; ++i)
            {
                Dictionary<DungeonDifficulty, DungeonJson> dungeonStoryDict = dungeonList[i];
                DungeonJson dungeonJson = dungeonStoryDict.ContainsKey(DungeonDifficulty.Easy)
                    ? dungeonStoryDict[DungeonDifficulty.Easy] : dungeonStoryDict[DungeonDifficulty.None];

                if (GameUtils.IsBitSet(dungeonDaysOpen[dungeonJson.sequence], (int)currentDayTab))
                    currentDungeonInfos.Add(dungeonStoryDict);
            }
        }
    }

    void InitDungeonPanel(DungeonDifficulty difficulty)
    {
        currentDifficulty = difficulty;
        int infoCount = currentDungeonInfos.Count;
        if (infoCount > 0)
        {
            Dictionary<DungeonDifficulty, DungeonJson> dungeonStoryDict = currentDungeonInfos[dungeonPanelIdx];
            bool noDifficulty = dungeonStoryDict.ContainsKey(DungeonDifficulty.None);
            if (noDifficulty)
                currentDifficulty = DungeonDifficulty.None;

            currentDungeonInfo = dungeonStoryDict[currentDifficulty];
            txtDungeonName.text = currentDungeonInfo.localizedname + " " + dungeonPanelIdx; // For test, remove later
            txtLevelReq.text = currentDungeonInfo.reqlvl.ToString();
            UpdateLootRewardLimit(GameInfo.gLocalPlayer);

            if (!noDifficulty)
            {
                imgDifficulty.gameObject.SetActive(true);
                imgDifficulty.sprite = difficultyIcons[(int)currentDifficulty-1];
            }
            else
                imgDifficulty.gameObject.SetActive(false);
            for (int i = 0; i < 4; ++i)
                buttonDifficulty[i].interactable = (!noDifficulty && dungeonStoryDict.ContainsKey((DungeonDifficulty)i+1));

            InitLootReward(currentDungeonInfo.lootdisplayids);
        }

        buttonLeftArrow.interactable = (infoCount > 0 && dungeonPanelIdx > 0);
        buttonRightArrow.interactable = (infoCount > 0 && dungeonPanelIdx < infoCount-1);
    }

    public void UpdateLootRewardLimit(PlayerGhost player)
    {
        Dictionary<int, RealmInfo> dungeonStoryinfos = player.RealmStats.GetDungeonStoryInfos();
        int seq = currentDungeonInfo.sequence;
        txtLootRewardLimit.text = dungeonStoryinfos.ContainsKey(seq)
            ? dungeonStoryinfos[seq].LootRewardLimit.ToString() : currentDungeonInfo.lootlimit.ToString();
    }

    public void InitLootReward(string lootLinkIds)
    {     
        List<RewardItem> lootRewardList = new List<RewardItem>();
        if (!string.IsNullOrEmpty(lootLinkIds))
        {
            Dictionary<int, int> compiledLootItems = new Dictionary<int, int>();
            List<int> lootLinkIdList = GameUtils.ParseStringToIntList(lootLinkIds, ';');
            int lootLinkIdsCount = lootLinkIdList.Count;
            for (int i = 0; i < lootLinkIdsCount; ++i)
            {
                LootLink lootLink = LootRepo.GetLootLink(lootLinkIdList[i]);
                if (lootLink != null)
                {
                    List<int> lootLinkGrpIds = lootLink.gids;
                    int lootLinkGrpIdCount = lootLinkGrpIds.Count;
                    for (int j = 0; j < lootLinkGrpIdCount; ++j)
                    {
                        LootItemGroup lootItemGrp = LootRepo.GetLootItemGroup(lootLinkGrpIds[j]);
                        if (lootItemGrp != null)
                        {
                            int lootItemCount = lootItemGrp.groupItems.Count;
                            for (int k = 0; k < lootItemCount; ++k)
                            {
                                LootItem lootItem = lootItemGrp.groupItems[k];
                                int itemId = lootItem.itemid;
                                if (compiledLootItems.ContainsKey(itemId))
                                    compiledLootItems[itemId] += lootItem.max;
                                else
                                    compiledLootItems[itemId] = lootItem.max;

                            }
                            lootItemCount = lootItemGrp.nonGroupItems.Count;
                            for (int k = 0; k < lootItemCount; ++k)
                            {
                                LootItem lootItem = lootItemGrp.nonGroupItems[k];
                                int itemId = lootItem.itemid;
                                if (compiledLootItems.ContainsKey(itemId))
                                    compiledLootItems[itemId] += lootItem.max;
                                else
                                    compiledLootItems[itemId] = lootItem.max;
                            }
                        }
                    }
                }
            }
            foreach (KeyValuePair<int, int> kvp in compiledLootItems)
                lootRewardList.Add(new RewardItem(kvp.Key, kvp.Value));
        }
        uiRewardDisplay.Init(lootRewardList);
    }

    public void OnValueChangedDaysTab(int index)
    {
        if (currentDayTab == (DayOfWeek)index)
            return;

        currentDayTab = (DayOfWeek)index;     
        PopulateCurrentDungeonInfos();
        InitDungeonPanelIndexBySequence();
        InitDungeonPanel(DungeonDifficulty.Easy);
    }

	public void OnClickLeftArrow()
    {
        if (dungeonPanelIdx > 0)
        {
            --dungeonPanelIdx;
            InitDungeonPanel(DungeonDifficulty.Easy);
        }
    }

    public void OnClickRightArrow()
    {
        if (dungeonPanelIdx < currentDungeonInfos.Count-1)
        {
            ++dungeonPanelIdx;
            InitDungeonPanel(DungeonDifficulty.Easy);
        }
    }

    public void OnClickDungeonDifficulty(int difficulty)
    {
        if (currentDifficulty != (DungeonDifficulty)difficulty)
            InitDungeonPanel((DungeonDifficulty)difficulty);
    }

    public void OnClickAutoClear()
    {
        RPCFactory.CombatRPC.DungeonAutoClear(currentDungeonInfo.id, false);
    }

    public void OnClickAutoClearAll()
    {
        RPCFactory.CombatRPC.DungeonAutoClear(currentDungeonInfo.id, true);
    }

    public void OnClickEnterDungeon()
    {
        RPCFactory.CombatRPC.CreateRealmByID(currentDungeonInfo.id, false, false);
    }
}
