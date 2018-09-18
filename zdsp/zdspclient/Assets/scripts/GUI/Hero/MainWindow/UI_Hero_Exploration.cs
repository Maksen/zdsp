using Kopio.JsonContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;
using Zealot.Repository;

public class UI_Hero_Exploration : MonoBehaviour
{
    [Header("Left Side")]
    [SerializeField] Text mapNameText;
    [SerializeField] UI_CountdownTime mapTime;
    [SerializeField] Image backgroundImage;
    [SerializeField] Transform reqmtParent;
    [SerializeField] GameObject reqmtDataPrefab;

    [Header("Target")]
    [SerializeField] Button targetBtn;
    [SerializeField] Image targetIconImage;

    [Header("Heroes")]
    [SerializeField] Transform heroDataParent;
    [SerializeField] GameObject heroDataPrefab;
    [SerializeField] Text efficiencyText;

    [Header("Rewards")]
    [SerializeField] UI_RewardDisplay rewardDisplay;
    [SerializeField] ScrollRect rewardScrollRect;

    [Header("Bottom")]
    [SerializeField] Transform reqItemParent;
    [SerializeField] GameObject reqItemPrefab;
    [SerializeField] Text reqItemCount;
    [SerializeField] Button startButton;
    [SerializeField] Button claimButton;
    [SerializeField] Text tooltipText;

    [Header("Right Side")]
    [SerializeField] Text exploreLimitText;
    [SerializeField] ComboBoxA mapComboBox;
    [SerializeField] Transform mapDataParent;
    [SerializeField] GameObject mapDataPrefab;
    [SerializeField] ScrollRect mapListScrollRect;

    private HeroStatsClient heroStats;
    private bool initialized;
    private bool refresh;
    private string mapFilter = "all";
    private int selectedMapId = -1;
    private ExplorationMapJson selectedMapData;
    private ExploreMapData selectedOngoingMap;
    private int selectedTargetId = -1;
    private string backgroundPath = "";
    private List<Hero_MapRequirementData> requirementList = new List<Hero_MapRequirementData>();
    private List<Hero_MapHeroData> heroList = new List<Hero_MapHeroData>();
    private List<ExplorationMapJson> mapList = new List<ExplorationMapJson>();  // list of selectable maps 
    private GameIcon_MaterialConsumable requiredItem;
    private Dictionary<int, int> currentMapTargets = new Dictionary<int, int>(); // mapid -> targetid
    private int bindItemId, unbindItemId;
    private bool showSpendConfirmation;

    private class MapComparer : IComparer<ExplorationMapJson>
    {
        public HeroStatsClient heroStats { get; set; }

        public MapComparer(HeroStatsClient hstats)
        {
            heroStats = hstats;
        }

        public int Compare(ExplorationMapJson x, ExplorationMapJson y)
        {
            if (heroStats.IsMapCompleted(x.mapid) && !heroStats.IsMapCompleted(y.mapid))
                return -1;
            else if (!heroStats.IsMapCompleted(x.mapid) && heroStats.IsMapCompleted(y.mapid))
                return 1;
            else if (heroStats.IsExploringMap(x.mapid) && !heroStats.IsExploringMap(y.mapid))
                return -1;
            else if (!heroStats.IsExploringMap(x.mapid) && heroStats.IsExploringMap(y.mapid))
                return 1;
            else
                return x.sequence.CompareTo(y.sequence);
        }
    }

    private void Awake()
    {
        // create map requirements icons
        for (int i = 0; i < 3; i++)
        {
            GameObject obj = ClientUtils.CreateChild(reqmtParent, reqmtDataPrefab);
            Hero_MapRequirementData data = obj.GetComponent<Hero_MapRequirementData>();
            requirementList.Add(data);
        }

        // create hero icons
        for (int i = 0; i < 3; i++)
        {
            GameObject obj = ClientUtils.CreateChild(heroDataParent, heroDataPrefab);
            Hero_MapHeroData data = obj.GetComponent<Hero_MapHeroData>();
            data.Setup(i, OnClickAddHero);
            heroList.Add(data);
        }
    }

    private void OnEnable()
    {
        heroStats = GameInfo.gLocalPlayer.HeroStats;
        if (!initialized)
        {
            SetupDropdownList();  // setup all selectable maps first
            mapComboBox.SelectedIndex = 0;
            PopulateMapList();
            SetExploreLimitText();
            initialized = true;
        }
        else
            Refresh();
    }

    public void Refresh()
    {
        if (heroStats != null)
        {
            refresh = true;
            SetupDropdownList();
            PopulateMapList();
            SetExploreLimitText();
        }
    }

    public void CleanUp()
    {
        initialized = false;
        refresh = false;
        selectedMapId = -1;
        mapFilter = "all";
        selectedTargetId = -1;
        selectedMapData = null;
        selectedOngoingMap = null;
        backgroundPath = "";
        mapList.Clear();
        ClientUtils.DestroyChildren(mapDataParent);
        mapListScrollRect.verticalNormalizedPosition = 1f;
        mapComboBox.ClearItemList();
        rewardDisplay.Clear();
        rewardScrollRect.horizontalNormalizedPosition = 0f;
    }

    private void SetExploreLimitText()
    {
        exploreLimitText.text = heroStats.GetExplorationsDict().Count + "/" + HeroRepo.EXPLORE_LIMIT;
    }

    public void OnMapSelectionChanged(int index)
    {
        if (mapFilter != mapComboBox.SelectedValue)
        {
            mapFilter = mapComboBox.SelectedValue;
            PopulateMapList();
        }
    }

    private void SetupDropdownList()
    {
        mapList.Clear();
        List<ExplorationMapJson> allMapsList = HeroRepo.explorationMapsList;
        List<string> subNameList = new List<string>();
        int count = allMapsList.Count;
        for (int i = 0; i < count; i++)
        {
            ExplorationMapJson mapData = allMapsList[i];
            // if has already explored this map and it is not repeatable, do not show anymore
            if (heroStats.HasExploredMap(mapData.mapid) && !mapData.repeatable)
                continue;
            // do not show if have previous map but have not complete predecessor yet
            if (mapData.prevmapid > 0 && !heroStats.HasExploredMap(mapData.prevmapid))
                continue;
            mapList.Add(mapData);  // add to selectable map list
            if (!subNameList.Contains(mapData.localizedsubname))
                subNameList.Add(mapData.localizedsubname);  // add subname as option in dropdown list
        }

        mapComboBox.ClearItemList();
        mapComboBox.AddItem(GUILocalizationRepo.GetLocalizedString("hro_all_maps"), "all");  // all maps as first option
        for (int i = 0; i < subNameList.Count; i++)
            mapComboBox.AddItem(subNameList[i], subNameList[i]);

        bool needSorting = heroStats.GetExplorationsDict().Count > 0;  // need sorting if have ongoing maps
        if (needSorting)
            mapList = mapList.OrderBy(x => x, new MapComparer(heroStats)).ToList();
    }

    // Call this only after setting up dropdown list
    private void PopulateMapList()
    {
        ClientUtils.DestroyChildren(mapDataParent);
        mapListScrollRect.verticalNormalizedPosition = 1f;

        bool hasToggled = false;
        ToggleGroup toggleGroup = mapDataParent.GetComponent<ToggleGroup>();
        int count = mapList.Count;
        for (int i = 0; i < count; i++)
        {
            if (mapFilter == "all" || mapFilter == mapList[i].localizedsubname)
            {
                GameObject obj = ClientUtils.CreateChild(mapDataParent, mapDataPrefab);
                Hero_MapData data = obj.GetComponent<Hero_MapData>();
                data.Init(mapList[i], toggleGroup, OnMapSelected);
                if (heroStats.IsExploringMap(mapList[i].mapid)) // ongoing map
                    data.SetAsOngoing();
                else if (HeroRepo.IsPredecessorMap(mapList[i].mapid) && !heroStats.HasExploredMap(mapList[i].mapid))
                    data.SetAsPredecessor(); // set color only if have not explored
                if (mapList[i].mapid == selectedMapId)
                {
                    data.SetToggleOn(true);
                    hasToggled = true;
                }
            }
        }

        if (mapDataParent.childCount > 0 && !hasToggled)
            mapDataParent.GetChild(0).GetComponent<Hero_MapData>().SetToggleOn(true);
    }

    private void OnMapSelected(int mapId)
    {
        if (selectedMapId != mapId)
        {
            selectedMapId = mapId;
            selectedMapData = HeroRepo.GetExplorationMapById(mapId);
            if (selectedMapData != null)
            {
                selectedOngoingMap = heroStats.GetExploringMap(mapId); // null if not currently exploring this map
                // below independent of whether ongoing
                mapNameText.text = selectedMapData.localizedname;
                SetBackgroundImage();
                SetMapRequirements();
                SetToolTipText();
                // below dependent on whether ongoing
                SetExplorationTime();
                SetHeroList();
                SetBottomButton();
                if (!currentMapTargets.ContainsKey(mapId))
                    UpdateTarget(0);
                else
                    UpdateTarget(currentMapTargets[mapId]);
            }
        }
        else if (refresh)  // already currently selected map, just need to refresh
        {
            selectedOngoingMap = heroStats.GetExploringMap(mapId); // null if not currently exploring this map
            SetExplorationTime();
            SetHeroList();
            SetBottomButton();
            if (!currentMapTargets.ContainsKey(mapId))
                UpdateTarget(0);
            else
                UpdateTarget(currentMapTargets[mapId]);
        }

        refresh = false;
    }

    private void SetBackgroundImage()
    {
        TerrainJson terrainData = HeroRepo.GetTerrainByType(selectedMapData.terraintype);
        if (terrainData != null && backgroundPath != terrainData.backgroundpath)
        {
            ClientUtils.LoadIconAsync(terrainData.backgroundpath, OnBackgroundImageLoaded);
            backgroundPath = terrainData.backgroundpath;
        }
    }

    private void OnBackgroundImageLoaded(Sprite sprite)
    {
        if (sprite != null)
            backgroundImage.sprite = sprite;
    }

    private void SetMapRequirements()
    {
        requirementList[0].Init(selectedMapData.chestreqtype1, selectedMapData.chestreqvalue1);
        requirementList[1].Init(selectedMapData.chestreqtype2, selectedMapData.chestreqvalue2);
        requirementList[2].Init(selectedMapData.chestreqtype3, selectedMapData.chestreqvalue3);
    }

    private void SetToolTipText()
    {
        Dictionary<string, string> param = new Dictionary<string, string>();
        param.Add("time", selectedMapData.battletimecost.ToString());
        tooltipText.text = GUILocalizationRepo.GetLocalizedString("hro_exploration_tooltip", param);
    }

    private void SetExplorationTime()
    {
        if (selectedOngoingMap == null)
        {
            double totalSeconds = selectedMapData.completetime * 60; // completetime in min
            mapTime.SetTime(totalSeconds);
        }
        else // ongoing exploration
        {
            if (selectedOngoingMap.Completed)
                mapTime.SetTime(0);
            else
            {
                DateTime endTime = selectedOngoingMap.EndTime;
                double remainingSeconds = Math.Max(0, endTime.Subtract(GameInfo.GetSynchronizedServerDT()).TotalSeconds);
                mapTime.SetTime(remainingSeconds);
                if (remainingSeconds > 0)
                    mapTime.StartCountdown();
            }
        }
    }

    private void SetBottomButton()
    {
        if (requiredItem == null)
        {
            GameObject obj = ClientUtils.CreateChild(reqItemParent, reqItemPrefab);
            requiredItem = obj.GetComponent<GameIcon_MaterialConsumable>();
        }

        if (selectedOngoingMap == null)
        {
            startButton.gameObject.SetActive(true);
            requiredItem.gameObject.SetActive(true);
            claimButton.gameObject.SetActive(false);

            string[] itemids = selectedMapData.reqitemid.Split(';');
            if (itemids.Length > 0 && int.TryParse(itemids[0], out bindItemId))
            {
                requiredItem.InitWithToolTipView(bindItemId, 1);
                int bindCount = GameInfo.gLocalPlayer.clientItemInvCtrl.itemInvData.GetTotalStackCountByItemId((ushort)bindItemId);
                int unbindCount = 0;
                if (itemids.Length > 1 && int.TryParse(itemids[1], out unbindItemId))
                    unbindCount = GameInfo.gLocalPlayer.clientItemInvCtrl.itemInvData.GetTotalStackCountByItemId((ushort)unbindItemId);
                int totalCount = bindCount + unbindCount;
                bool enough = totalCount >= selectedMapData.reqitemcount;
                reqItemCount.text = "x" + selectedMapData.reqitemcount;
                if (!enough)
                    reqItemCount.text = string.Format("<color=red>{0}</color>", reqItemCount.text);
                showSpendConfirmation = enough && bindCount < selectedMapData.reqitemcount;
            }
        }
        else  // ongoing exploration
        {
            startButton.gameObject.SetActive(false);
            requiredItem.gameObject.SetActive(false);
            claimButton.gameObject.SetActive(true);
            claimButton.interactable = selectedOngoingMap.Completed;
        }
    }

    private void SetHeroList()
    {
        int maxHeroCount = selectedMapData.maxherocount;
        for (int i = 0; i < heroList.Count; i++)
            heroList[i].gameObject.SetActive(i < maxHeroCount);

        if (selectedOngoingMap == null) // not exploring, clear hero list
        {
            for (int i = 0; i < heroList.Count; i++)
                heroList[i].Clear();
        }
        else // ongoing exploration
        {
            List<int> heroIds = selectedOngoingMap.HeroIdList;
            for (int i = 0; i < heroList.Count; i++)
            {
                if (i < heroIds.Count)
                    heroList[i].Init(heroIds[i], false);
                else
                    heroList[i].Init(0, false);  // empty but not interactable
            }
        }

        UpdateEfficiency();
    }

    private void UpdateEfficiency()
    {
        float totalEfficiency = selectedMapData.baseefficiency * 0.01f;
        for (int i = 0; i < heroList.Count; i++)
        {
            int heroId = heroList[i].GetHeroId();
            if (heroId > 0)
            {
                Hero hero = heroStats.GetHero(heroId);
                if (hero != null) // should always have
                    totalEfficiency += hero.GetTotalExploreEfficiency(selectedMapData);
            }
        }
        efficiencyText.text = (totalEfficiency * 100).ToString("F0") + "%";
    }

    public void OnClickSelectTarget()
    {
        UIManager.OpenDialog(WindowType.DialogHeroTargetList,
            (window) => window.GetComponent<UI_Hero_MapTargetDialog>().Init(selectedMapData, UpdateTarget));
    }

    private void UpdateTarget(int targetId)
    {
        currentMapTargets[selectedMapId] = targetId;

        if (selectedTargetId != targetId)
        {
            selectedTargetId = targetId;
            //print("selected target: " + selectedTargetId);
            if (selectedTargetId == 0)  // all
                targetIconImage.sprite = ClientUtils.LoadIcon("UI_ZDSP_Icons/Portraits/zzz_Test.png");  // temp
            else
            {
                ExplorationTargetJson targetData = HeroRepo.GetExplorationTargetById(selectedTargetId);
                if (targetData != null)
                    targetIconImage.sprite = ClientUtils.LoadIcon(targetData.iconpath);
            }
        }

        targetBtn.interactable = selectedOngoingMap == null; // already ongoing, cannot change target

        UpdateRewardList();
    }

    private void UpdateRewardList()  // after update target or update hero
    {
        List<RewardItem> rewardList = new List<RewardItem>();
        if (selectedOngoingMap != null && selectedOngoingMap.Completed)  // show actual rewards when complete
        {
            var itemList = selectedOngoingMap.Rewards.items;
            int count = itemList.Count;
            for (int i = 0; i < count; i++)
                rewardList.Add(new RewardItem(itemList[i].itemId, itemList[i].stackCount));
        }
        else
        {
            Dictionary<int, int> rewardDict = new Dictionary<int, int>();
            // determine chest count
            List<int> heroIds = new List<int>();
            for (int i = 0; i < heroList.Count; i++)
            {
                if (heroList[i].GetHeroId() > 0)
                    heroIds.Add(heroList[i].GetHeroId());
            }
            int chestCount = heroStats.GetFulfilledChestCount(selectedMapData, heroIds);
            rewardDict.Add(selectedMapData.chestitemid, chestCount);

            // determine target rewards
            if (selectedTargetId == 0)  // all
            {
                List<ExplorationTargetJson> targets = HeroRepo.GetExplorationTargetsByGroup(selectedMapData.exploregroupid);
                for (int i = 0; i < targets.Count; i++)
                    AddTargetReward(targets[i], rewardDict);
            }
            else  // specific target
            {
                ExplorationTargetJson targetData = HeroRepo.GetExplorationTargetById(selectedTargetId);
                AddTargetReward(targetData, rewardDict);
            }

            foreach (var item in rewardDict)
                rewardList.Add(new RewardItem(item.Key, item.Value));
        }
        rewardDisplay.Init(rewardList);
        rewardScrollRect.horizontalNormalizedPosition = 0f;
    }

    private void AddTargetReward(ExplorationTargetJson targetData, Dictionary<int, int> rewardDict)
    {
        if (targetData == null)
            return;
        List<RewardItem> itemList = RewardListRepo.GetRewardItemsByGrpIDJobID(targetData.rewardgroupid, GameInfo.gLocalPlayer.PlayerSynStats.jobsect);
        if (itemList != null)
        {
            for (int i = 0; i < itemList.Count; i++)
            {
                if (itemList[i].count == 0)
                    continue;
                if (!rewardDict.ContainsKey(itemList[i].itemId))
                    rewardDict.Add(itemList[i].itemId, 1);
            }
        }

        LootLink lootLink = LootRepo.GetLootLink(targetData.lootlinkid);
        if (lootLink != null)
        {
            for (int index = 0; index < lootLink.gids.Count; index++)
            {
                LootItemGroup itemGrp = LootRepo.GetLootItemGroup(lootLink.gids[index]);
                if (itemGrp != null)
                {
                    for (int i = 0; i < itemGrp.groupItems.Count; i++)
                    {
                        if (!rewardDict.ContainsKey(itemGrp.groupItems[i].itemid))
                            rewardDict.Add(itemGrp.groupItems[i].itemid, 1);
                    }
                    for (int i = 0; i < itemGrp.nonGroupItems.Count; i++)
                    {
                        if (!rewardDict.ContainsKey(itemGrp.nonGroupItems[i].itemid))
                            rewardDict.Add(itemGrp.nonGroupItems[i].itemid, 1);
                    }
                }
            }
        }
    }

    private void OnClickAddHero(int index)
    {
        UIManager.OpenDialog(WindowType.DialogHeroList,
            (window) => window.GetComponent<UI_Hero_MapHeroListDialog>().Init(selectedMapData, index, UpdateHeroSelected));
    }

    private void UpdateHeroSelected(int index, int heroId)
    {
        // check whether same hero is already set in other slots
        for (int i = 0; i < 3; i++)
        {
            if (i == index)
                continue;
            if (heroList[i].GetHeroId() == heroId)
            {
                heroList[i].Clear();  // reset to empty
                break;
            }
        }
        heroList[index].Init(heroId, true);  // set hero in this slot
        UpdateEfficiency();
        UpdateRewardList();
    }

    public void OnClickShowEfficiency()
    {
        List<int> heroIds = new List<int>();
        for (int i = 0; i < heroList.Count; i++)
        {
            if (heroList[i].GetHeroId() > 0)
                heroIds.Add(heroList[i].GetHeroId());
        }
        UIManager.OpenDialog(WindowType.DialogHeroEfficiency,
            (window) => window.GetComponent<UI_Hero_MapEfficiencyDialog>().Init(selectedMapData, heroIds));
    }

    public void OnClickStartExplore()
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < heroList.Count; i++)
        {
            if (heroList[i].GetHeroId() > 0)
                sb.AppendFormat("{0};", heroList[i].GetHeroId());
        }
        string heroesStr = sb.ToString().TrimEnd(';');
        if (!string.IsNullOrEmpty(heroesStr))
        {
            if (showSpendConfirmation)
            {
                IInventoryItem bindItem = requiredItem.inventoryItem;
                IInventoryItem unbindItem = GameRepo.ItemFactory.GetInventoryItem(unbindItemId);
                if (bindItem != null && unbindItem != null)
                {
                    Dictionary<string, string> param = new Dictionary<string, string>();
                    param.Add("bind", bindItem.JsonObject.localizedname);
                    param.Add("unbind", unbindItem.JsonObject.localizedname);
                    string message = GUILocalizationRepo.GetLocalizedString("hro_confirmUseUnbindToExplore", param);
                    UIManager.OpenYesNoDialog(message, () => OnConfirmExplore(heroesStr));
                }
            }
            else
                OnConfirmExplore(heroesStr);
        }
        else
            UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("sys_hero_ExplorationNoHeroSelected"));
    }

    private void OnConfirmExplore(string heroesStr)
    {
        RPCFactory.CombatRPC.ExploreMap(selectedMapId, selectedTargetId, heroesStr);

    }

    public void OnClickClaimReward()
    {
        RPCFactory.CombatRPC.ClaimExplorationReward(selectedMapId);
    }
}