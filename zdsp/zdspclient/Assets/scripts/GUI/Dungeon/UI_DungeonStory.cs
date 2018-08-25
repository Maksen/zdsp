using Kopio.JsonContracts;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;
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
    DungeonDifficulty currentDifficulty = DungeonDifficulty.None;
    List<RewardItem> lootRewardList = null;

    // Use this for initialization
    void Awake()
    {
        currentDungeonInfos = new List<Dictionary<DungeonDifficulty, DungeonJson>>();
        lootRewardList = new List<RewardItem>();
    }

    void OnEnable()
    {
        Init();
    }

    void OnDisable()
    {
        currentDungeonInfos.Clear();
    }

    public void Init()
    {      
        if (currentDayTab == DayOfWeek.Monday)
        {
            dungeonPanelIdx = 0;
            currentDifficulty = DungeonDifficulty.Easy;
            PopulateCurrentDungeonInfos();
            InitDungeonPanel();
        }
        else
            defaultToggleingrpDayTabs.GoToPage((int)DayOfWeek.Monday);
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

                byte daysOpen = dungeonDaysOpen[dungeonJson.sequence];
                if (GameUtils.IsBitSet(daysOpen, (int)currentDayTab))
                    currentDungeonInfos.Add(dungeonStoryDict);
            }
        }
    }

    void InitDungeonPanel()
    {
        int infoCount = currentDungeonInfos.Count;
        if (infoCount > 0)
        {
            Dictionary<DungeonDifficulty, DungeonJson> dungeonStoryDict = currentDungeonInfos[dungeonPanelIdx];
            bool noDifficulty = dungeonStoryDict.ContainsKey(DungeonDifficulty.None);
            if (noDifficulty)
                currentDifficulty = DungeonDifficulty.None;
            DungeonJson dungeonJson = dungeonStoryDict[currentDifficulty];

            txtDungeonName.text = dungeonJson.localizedname + " " + dungeonPanelIdx; // For test, remove later
            txtLevelReq.text = dungeonJson.reqlvl.ToString();
            txtLootRewardLimit.text = dungeonJson.lootlimit.ToString();
            for (int i = 0; i < 4; ++i)
                buttonDifficulty[i].interactable = (!noDifficulty && dungeonStoryDict.ContainsKey((DungeonDifficulty)i+1));

            InitLootReward(dungeonJson.lootdisplayids);
        }

        buttonLeftArrow.interactable = (infoCount > 0 && dungeonPanelIdx > 0);
        buttonRightArrow.interactable = (infoCount > 0 && dungeonPanelIdx < infoCount-1);
    }

    public void InitLootReward(string lootLinkIds)
    {
        lootRewardList.Clear();
        if (!string.IsNullOrEmpty(lootLinkIds))
        {
            List<int> lootLinkIdList = GameUtils.ParseStringToIntList(lootLinkIds, ';');
            int lootLinkIdsCount = lootLinkIdList.Count;
            for (int i = 0; i < lootLinkIdsCount; ++i)
            {
                LootLink lootLink = LootRepo.GetLootLink(lootLinkIdList[i]);
                if (lootLink != null)
                {
                    int lootLinkGIdCount = lootLink.gids.Count;
                    for (int j = 0; j < lootLinkGIdCount; ++j)
                    {
                        LootItemGroup lootItemGrp = LootRepo.GetLootItemGroup(lootLink.gids[j]);
                        if (lootItemGrp != null)
                        {
                            int lootItemCount = lootItemGrp.groupItems.Count;
                            for (int k = 0; k < lootItemCount; ++k)
                            {
                                LootItem lootItem = lootItemGrp.groupItems[k];
                                lootRewardList.Add(new RewardItem(lootItem.itemid, lootItem.max));
                            }
                            lootItemCount = lootItemGrp.nonGroupItems.Count;
                            for (int k = 0; k < lootItemCount; ++k)
                            {
                                LootItem lootItem = lootItemGrp.nonGroupItems[k];
                                lootRewardList.Add(new RewardItem(lootItem.itemid, lootItem.max));
                            }
                        }
                    }
                }
            }
        }
        uiRewardDisplay.Init(lootRewardList);
    }

    public void OnValueChangedDaysTab(int index)
    {
        if (currentDayTab == (DayOfWeek)index)
            return;

        currentDayTab = (DayOfWeek)index;

        dungeonPanelIdx = 0;
        currentDifficulty = DungeonDifficulty.Easy;
        PopulateCurrentDungeonInfos();
        InitDungeonPanel();
    }

	public void OnClickLeftArrow()
    {
        if (dungeonPanelIdx > 0)
        {
            --dungeonPanelIdx;
            currentDifficulty = DungeonDifficulty.Easy;
            InitDungeonPanel();
        }
    }

    public void OnClickRightArrow()
    {
        if (dungeonPanelIdx < currentDungeonInfos.Count-1)
        {
            ++dungeonPanelIdx;
            currentDifficulty = DungeonDifficulty.Easy;
            InitDungeonPanel();
        }
    }

    public void OnClickDungeonDifficulty(int difficulty)
    {
        if (currentDifficulty != (DungeonDifficulty)difficulty)
        {
            currentDifficulty = (DungeonDifficulty)difficulty;
            InitDungeonPanel();
        }
    }

    public void OnClickAutoClear()
    {
    }

    public void OnClickAutoClearAll()
    {
    }

    public void OnClickOpenParty()
    {

    }

    public void OnClickEnterDungeon()
    {
        DungeonJson dungeonJson = currentDungeonInfos[dungeonPanelIdx][currentDifficulty];
        RPCFactory.CombatRPC.CreateRealmByID(dungeonJson.id, false, false);
    }
}
